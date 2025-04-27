//-----------------------------------------------------------------------
// <copyright file="SubscriptionServiceManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Management class for working with the subscription center.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Channels.Ipc;
    using System.Runtime.Serialization.Formatters;
    using System.Threading;
    using System.Windows.Threading;
    using Microsoft.SubscriptionCenter.Sync;
    using Microsoft.SubscriptionCenter.Ipc;
    using ScePhoto;

    /// <summary>
    /// This class is responsible for connecting to the subscription service and for synchronizing
    /// application's data updates with those performed by subscription service.
    /// </summary>
    public class SubscriptionServiceManager : IDisposable
    {
        #region Fields
        /// <summary>
        /// Indicates whether the application is waiting for service (initially false).
        /// </summary>
        private bool waitingForService;

        /// <summary>
        /// The IPC channel used to communicate with the subscription service.
        /// </summary>
        private IpcChannel channel;

        /// <summary>
        /// The service channel used by the subscription service.
        /// </summary>
        private ISubscriptionServiceChannel serviceChannel;

        /// <summary>
        /// The dispatcher used for threading.
        /// </summary>
        private Dispatcher dispatcher;

        /// <summary>
        /// The handle for the service channel.
        /// </summary>
        private EventWaitHandle serviceChannelHandle;

        /// <summary>
        /// Whether or not a sync is in progress (initally false).
        /// </summary>
        private bool active;

        /// <summary>
        /// A value indicating whether this type has been disposed or not.
        /// </summary>
        private bool disposed; 
        #endregion

        #region Destructor
        /// <summary>
        /// Destructor, for completeness in implementing IDisposable.
        /// </summary>
        ~SubscriptionServiceManager()
        {
            this.Dispose(false);
        } 
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets a value indicating whether the subscription service is currently updating this application's data.
        /// </summary>
        public bool IsServiceUpdateInProgress
        {
            get
            {
                bool updateInProgress = false;
                try
                {
                    if (this.serviceChannel != null)
                    {
                        updateInProgress = this.serviceChannel.IsSubscriptionUpdateInProgress(SampleScePhotoSettings.ApplicationName);
                    }
                }
                catch (Exception e)
                {
                    updateInProgress = false;
                    ServiceProvider.Logger.Error(Resources.Strings.SubscriptionServiceManagerServiceError, e.Message);
                    this.HandleRemotingException(e);
                }

                return updateInProgress;
            }
        }

        /// <summary>
        ///  Register client channel, connect to subscription server's channel.
        /// </summary>
        public void Initialize()
        {
            if (!this.disposed)
            {
                // Initialize dispatcher
                this.dispatcher = Dispatcher.CurrentDispatcher;
                this.active = true;

                ServiceProvider.DataManager.UpdateStarted += this.DataManager_UpdateStarted;
                ServiceProvider.DataManager.UpdateCompleted += this.DataManager_UpdateCompleted;

                // Register client IPC channel
                this.RegisterChannelServices();

                // Wait for subscription service to activate its channel
                this.WaitForServiceActivation();
            }
            else
            {
                throw new ObjectDisposedException("SubscriptionServiceManager");
            }
        }

        /// <summary>
        /// Shut down remote communication.
        /// </summary>
        public void Shutdown()
        {
            this.active = false;

            ServiceProvider.DataManager.UpdateStarted -= this.DataManager_UpdateStarted;
            ServiceProvider.DataManager.UpdateCompleted -= this.DataManager_UpdateCompleted;

            // Disconnect subscription from the service
            this.DisconnectFromServiceChannel();
            if (this.serviceChannelHandle != null)
            {
                this.serviceChannelHandle.Reset();
            }

            this.waitingForService = false;
            this.dispatcher = null;

            this.UnregisterChannelServices();
        }

        /// <summary>
        /// Implements IDisposable to dispose of the IDisposable types it creates.
        /// <remarks>Since this object has already done its own disposal, it no longer needs to be finalized by the framework.</remarks>
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        } 
        #endregion

        #region Private Methods
        /// <summary>
        /// Registers IPC chnnael for remote communication.
        /// </summary>
        private void RegisterChannelServices()
        {
            BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
            BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();
            serverProvider.TypeFilterLevel = TypeFilterLevel.Full;
            System.Collections.IDictionary props = new Dictionary<string, string>();

            props["name"] = String.Concat(ScePhotoSettings.CompanyName, ScePhotoSettings.ApplicationName, Environment.UserName);
            props["portName"] = SampleScePhotoSettings.ChannelPortName;
            props["secure"] = "true";
            props["tokenImpersonationLevel"] = "Impersonation";
            props["exclusiveAddressUse"] = "false";

            // If channel is already registered - may happen if there is not enough time to unregister it on suspend, etc -
            // unregister and re-register it
            if (this.ChannelRegistered())
            {
                this.UnregisterChannelServices();
            }

            this.channel = new IpcChannel(props, clientProvider, serverProvider);
            ChannelServices.RegisterChannel(this.channel, true);
        }

        /// <summary>
        /// Disconects IPC channel for remote communication.
        /// </summary>
        private void UnregisterChannelServices()
        {
            ChannelServices.UnregisterChannel(this.channel);
            this.channel = null;
        }

        /// <summary>
        /// Check if the current channel is registered with channel services.
        /// </summary>
        /// <returns>True if the current channel is registered with channel services.</returns>
        private bool ChannelRegistered()
        {
            bool registered = false;
            if (this.channel != null)
            {
                foreach (IChannel remotingChannel in ChannelServices.RegisteredChannels)
                {
                    if (remotingChannel == this.channel)
                    {
                        registered = true;
                    }
                }
            }

            return registered;
        }

        /// <summary>
        /// Spins new thread to wait for subscription service channel activation.
        /// </summary>
        private void WaitForServiceActivation()
        {
            if (!this.waitingForService && this.active)
            {
                this.waitingForService = true;
                ThreadPool.QueueUserWorkItem(this.WaitForServiceActivationCallback);
            }
        }

        /// <summary>
        /// Callback to wait for service activation.
        /// </summary>
        /// <param name="state">State of the service.</param>
        private void WaitForServiceActivationCallback(object state)
        {
            if (this.waitingForService && this.active)
            {
                this.serviceChannel = null;
                string subscriptionServiceSignalName = SampleScePhotoSettings.SubscriptionServiceSignalName;
                bool createdNew = false;
                this.serviceChannelHandle = new EventWaitHandle(false, EventResetMode.ManualReset, subscriptionServiceSignalName, out createdNew);

                // Wait for service to signal activation
                this.serviceChannelHandle.WaitOne();

                // Queue dispatcher item to begin communication when unblocked
                // When using '_dispatcher' we need to check that it is not null, if the application is shutting down dispatcher may no longer be active
                // It is safe to do nothing if dispatcher is null
                if (this.dispatcher != null)
                {
                    this.dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(this.SubscriptionServiceChannelActivatedCallback), null);
                }
                else
                {
                    this.waitingForService = false;
                }
            }
        }

        /// <summary>
        /// Connects to channel exposed by subscription service. Returns true if connection succeeded.
        /// </summary>
        /// <returns>True if the channel connects.</returns>
        private bool ConnectToServiceChannel()
        {
            bool connected = false;
            try
            {
                if (this.serviceChannel != null)
                {
                    connected = this.serviceChannel.ConnectSubscription(SampleScePhotoSettings.ApplicationName);
                }
            }
            catch (Exception e)
            {
                connected = false;
                ServiceProvider.Logger.Error(Resources.Strings.SubscriptionServiceManagerServiceError, e.Message);
                this.HandleRemotingException(e);
            }

            return connected;
        }

        /// <summary>
        /// Disconnects from service channel.
        /// </summary>
        private void DisconnectFromServiceChannel()
        {
            try
            {
                if (this.serviceChannel != null)
                {
                    this.serviceChannel.DisconnectSubscription(SampleScePhotoSettings.ApplicationName);
                }
            }
            catch (Exception e)
            {
                // On disconnecting there is no need to provide further exception handling such as waiting for service reactivation             
                ServiceProvider.Logger.Error(Resources.Strings.SubscriptionServiceManagerServiceError, e.Message);
            }

            this.serviceChannel = null;
        }

        /// <summary>
        /// Subscribe for subscription service events.
        /// </summary>
        private void AddServiceChannelHandlers()
        {
            try
            {
                // Add handler for subscription service's update completed event
                if (this.serviceChannel != null)
                {
                    this.serviceChannel.AddSubscriptionUpdateCompletedHandler(
                        SampleScePhotoSettings.ApplicationName,
                        SubscriptionServiceEventHandlerShim.Create(this.SubscriptionService_UpdateCompleted));
                    this.serviceChannel.AddSubscriptionServiceDisconnectedHandler(
                        SampleScePhotoSettings.ApplicationName,
                        SubscriptionServiceEventHandlerShim.Create(this.SubscriptionService_Disconnected));
                }
            }
            catch (Exception e)
            {
                ServiceProvider.Logger.Error(Resources.Strings.SubscriptionServiceManagerServiceError, e.Message);
                this.HandleRemotingException(e);
            }
        }

        /// <summary>
        /// Handler for subscription service's update completed event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private void SubscriptionService_UpdateCompleted(object sender, EventArgs e)
        {
            // Queue dispatcher item to load cached data
            // When using '_dispatcher' we need to check that it is not null, if the application is shutting down dispatcher may no longer be active
            // It is safe to do nothing if dispatcher is null
            if (this.dispatcher != null)
            {
                this.dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(this.ServiceUpdateCompletedCallback), null);
            }
        }

        /// <summary>
        /// Handler for subscription service's disconnected event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private void SubscriptionService_Disconnected(object sender, EventArgs e)
        {
            // Queue dispatcher item to wait for service reconnection
            // When using '_dispatcher' we need to check that it is not null, if the application is shutting down dispatcher may no longer be active
            // It is safe to do nothing if dispatcher is null
            if (this.dispatcher != null)
            {
                this.dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(this.ServiceDisconnectedCallback), null);
            }
            else
            {
                // Set service channel to null, no need to wait for service activation since there is no dispatcher. 
                this.serviceChannel = null;
            }
        }

        /// <summary>
        /// Handler for DataManager's UpdateCompleted event. Notifies subscription service that update is completed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private void DataManager_UpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                // Notify subscription service of update completed
                if (this.serviceChannel != null)
                {
                    this.serviceChannel.OnSubscriptionUpdateCompleted(SampleScePhotoSettings.ApplicationName);
                }
            }
            catch (Exception exception)
            {
                ServiceProvider.Logger.Error(Resources.Strings.SubscriptionServiceManagerServiceError, exception.Message);
                this.HandleRemotingException(exception);
            }
        }

        /// <summary>
        /// Handler for DataManager's UpdateStarted event. Notifies subscription service that update has started.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private void DataManager_UpdateStarted(object sender, EventArgs e)
        {
            this.NotifyUpdateStarted();
        }

        /// <summary>
        /// When communicating with the service over IPC Channel, exceptions are logged. In the case of
        /// RemotingExceptions, the server may be down. In this case, client should block and attempt to
        /// wait for the server to re-signal.
        /// </summary>
        /// <param name="e">The exception to be handled.</param>
        private void HandleRemotingException(Exception e)
        {
            if (e is RemotingException)
            {
                this.serviceChannel = null;

                // Reset service channel handle. If the service is still online, it should set the handle at regular intervals
                if (this.serviceChannelHandle != null)
                {
                    this.serviceChannelHandle.Reset();
                }

                this.WaitForServiceActivation();
            }
        }

        /// <summary>
        /// Callback worker for subscription service activation. Attempts to connect to service.
        /// </summary>
        /// <param name="arg">The callback details.</param>
        /// <returns>Always null.</returns>
        private object SubscriptionServiceChannelActivatedCallback(object arg)
        {
            if (!this.disposed)
            {
                // Reset waiting for service flag
                if (this.waitingForService && this.active)
                {
                    this.waitingForService = false;
                    this.serviceChannel = (ISubscriptionServiceChannel)Activator.GetObject(
                        typeof(ISubscriptionServiceChannel), SampleScePhotoSettings.SubscriptionServiceChannelName);

                    // Try to connect to the service channel, if connection fails, continue waiting
                    if (this.ConnectToServiceChannel())
                    {
                        // If syncing, notify service channel of update started
                        if (ServiceProvider.DataManager.IsUpdateInProgress)
                        {
                            this.NotifyUpdateStarted();
                        }

                        this.AddServiceChannelHandlers();
                    }
                    else
                    {
                        this.serviceChannel = null;
                    }
                }
            }
            else
            {
                throw new ObjectDisposedException("SubscriptionServiceManager");
            }

            return null;
        }

        /// <summary>
        ///  Notify the subscription service that an update has started.
        /// </summary>
        private void NotifyUpdateStarted()
        {
            // Notify subscription service of update completed
            try
            {
                if (this.serviceChannel != null)
                {
                    this.serviceChannel.OnSubscriptionUpdateStarted(SampleScePhotoSettings.ApplicationName);
                }
            }
            catch (Exception exception)
            {
                ServiceProvider.Logger.Error(Resources.Strings.SubscriptionServiceManagerServiceError, exception.Message);
                this.HandleRemotingException(exception);
            }
        }

        /// <summary>
        /// Callback worker for subscription service update completed event. 
        /// When subscription service completes an update, data is reloaded from the cache if no other
        /// updates are already in progress.
        /// </summary>
        /// <param name="arg">The callback argument.</param>
        /// <returns>Always null.</returns>
        private object ServiceUpdateCompletedCallback(object arg)
        {
            // If no update is in progress, load cached data
            if (!ServiceProvider.DataManager.IsUpdateInProgress)
            {
                ServiceProvider.DataManager.LoadCachedDataAsync();
            }

#if DEBUG
            ServiceProvider.Logger.Information(Resources.Strings.SubscriptionServiceManagerHandlerExecuted, "UpdateCompleted");
#endif
            return null;
        }

        /// <summary>
        /// Callback worker for subscription service disconnected event. 
        /// </summary>
        /// <param name="arg">The callback argument.</param>
        /// <returns>Always null.</returns>
        private object ServiceDisconnectedCallback(object arg)
        {
            // Wait for service to reconnect
            this.serviceChannel = null;
            this.WaitForServiceActivation();

#if DEBUG
            ServiceProvider.Logger.Information(Resources.Strings.SubscriptionServiceManagerHandlerExecuted, "ServiceDisconnected");
#endif
            return null;
        }

        /// <summary>
        /// Implements IDisposable to dispose of the IDisposable types the SubscriptionServiceProvider creates.
        /// </summary>
        /// <remarks>Dispose(bool disposing) is called in two situations.  When disposing is true, it has been called from
        /// user code and can dispose of other managed objects; when false, it has been called automatically from the runtime
        /// finalizer and other managed resources may or may not have already been finalized.</remarks>
        /// <param name="disposing">Whether this method is being called by user code.</param>
        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    ((IDisposable)this.serviceChannelHandle).Dispose();
                }

                this.disposed = true;
            }
        } 
        #endregion
    }
}
