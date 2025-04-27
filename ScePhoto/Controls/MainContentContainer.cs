//-----------------------------------------------------------------------
// <copyright file="MainContentContainer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Container element for the primary content of a ScePhoto application.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Navigation;
    using ScePhoto.View;

    /// <summary>
    /// Container element for the primary content of a ScePhoto application. For example, when a reader is viewing a <see cref="Photo"/> object,
    /// MainContentContainer contains a <see cref="PhotoViewer"/> control, the default display for Photos. When displaying a Photo Album from the data
    /// feed, MainContentContainer contains a <see cref="PhotoSectionFrontControl"/>.
    /// </summary>
    public class MainContentContainer : ContentControl
    {
        #region Private Fields

        /// <summary>
        /// The content state to save in the journal.
        /// </summary>
        private CustomContentState contentStateToSave;

        #endregion

        #region Constructor

        /// <summary>
        /// Static ctor registers command binding for Refresh command.
        /// </summary>
        static MainContentContainer()
        {
            CommandManager.RegisterClassCommandBinding(typeof(MainContentContainer), new CommandBinding(System.Windows.Input.NavigationCommands.Refresh, new ExecutedRoutedEventHandler(OnRefreshCommand), null));
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Called when the element is initialized.
        /// </summary>
        /// <param name="e">Arguments of the event.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Loaded += this.OnLoaded;
            Unloaded += this.OnUnloaded;
        }

        /// <summary>
        /// Virtual handler for loaded event.
        /// </summary>
        protected virtual void OnLoaded()
        {
            ServiceProvider.ViewManager.Navigated += this.OnViewManagerNavigated;
            NavigationService navigationService = NavigationService.GetNavigationService(this);
            if (navigationService != null)
            {
                navigationService.Navigating += this.OnNavigationServiceNavigating;
            }

            // If ViewManager's current navigator is not null at load time, navigate to it
            // TODO: this is a workaround for the case when the application is minimized to system tray and reopened
            // To restore navigation state, we must do this navigation
            // Once ViewManager is removed in this Scenario, this should not be needed because Viewmanager will navigate to the home
            // page on startup, which is the correct behavior. NavigateByRefresh method on ViewManager should be removed.
            if (ServiceProvider.ViewManager.CurrentNavigator != null)
            {
                ServiceProvider.ViewManager.NavigateByRefresh(ServiceProvider.ViewManager.CurrentNavigator);
            }
        }

        /// <summary>
        /// Virtual handler for unloaded event.
        /// </summary>
        protected virtual void OnUnloaded()
        {
            ServiceProvider.ViewManager.Navigated -= this.OnViewManagerNavigated;
            NavigationService navigationService = NavigationService.GetNavigationService(this);
            if (navigationService != null)
            {
                navigationService.Navigating -= this.OnNavigationServiceNavigating;
            }
        }

        /// <summary>
        /// Virtual handler for the view manager navigation event.
        /// </summary>
        /// <param name="e">Details of the navigation event.</param>
        protected virtual void OnViewManagerNavigated(ViewManagerNavigatedEventArgs e)
        {
            this.DoNavigation(e);
        }

        /// <summary>
        /// Does actual navigation - adds journal entry if necessary, etc.
        /// </summary>
        /// <param name="e">Details of the navigation event.</param>
        protected virtual void DoNavigation(ViewManagerNavigatedEventArgs e)
        {
            if (this.contentStateToSave != null && this.IsNewNavigation(e.NavigationMode))
            {
                NavigationService navigationService = NavigationService.GetNavigationService(this);
                if (navigationService != null)
                {
                    CustomContentState contentStateToSave = this.contentStateToSave;
                    this.contentStateToSave = null;
                    navigationService.AddBackEntry(contentStateToSave);
                }
            }

            if (e.ContentStateToSave != null)
            {
                this.contentStateToSave = e.ContentStateToSave;
            }

            Content = e.Content;
        }

        /// <summary>
        /// Determines whether this container considers a navigation to be "new" or not. New navigations cause a back entry
        /// to be added to the journal's back stack.
        /// </summary>
        /// <param name="navigationMode">Mode of the navigation, determines if it will be to a new Navigator or not.</param>
        /// <returns>True if the navigation mode represents a "new" navigation, i.e. not from the journal stack.</returns>
        protected virtual bool IsNewNavigation(ScePhotoNavigationMode navigationMode)
        {
            return ((navigationMode != ScePhotoNavigationMode.Back) && (navigationMode != ScePhotoNavigationMode.Forward) &&
                (navigationMode != ScePhotoNavigationMode.Refresh));
        }

        /// <summary>
        /// Virtual handler for Refresh command. Executes SyncCommand on ViewManager.
        /// </summary>
        /// <param name="e">Details of the routed event.</param>
        protected virtual void OnRefreshCommand(RoutedEventArgs e)
        {
            e.Handled = true;
            if (ServiceProvider.ViewManager.SyncCommands.StartSyncCommand.CanExecute(null))
            {
                ServiceProvider.ViewManager.SyncCommands.StartSyncCommand.Execute(null);
            }
        }

        /// <summary>
        /// Creates AutomationPeer for MainContentContainer.
        /// </summary>
        /// <returns>AutomationPeer for this object.</returns>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new MainContentContainerAutomationPeer(this);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Command handler for Refresh navigation command.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">EventArgs describing the Eexcuted event.</param>
        private static void OnRefreshCommand(object sender, ExecutedRoutedEventArgs e)
        {
            MainContentContainer frame = sender as MainContentContainer;
            if (frame != null && !e.Handled)
            {
                frame.OnRefreshCommand(e);
            }
        }

        /// <summary>
        /// When navigating via the journal, the last viewed content state has not been
        /// saved to the journal. We can listen for the navigating event on the navigation service to store the content state.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Details of the cancelled navigation.</param>
        private void OnNavigationServiceNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (this.contentStateToSave != null)
            {
                e.ContentStateToSave = this.contentStateToSave;
                this.contentStateToSave = null;
            }
        }

        /// <summary>
        /// Event handler for ViewManager's navigated event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Details of the navigation.</param>
        private void OnViewManagerNavigated(object sender, ViewManagerNavigatedEventArgs e)
        {
            this.OnViewManagerNavigated(e);
        }

        /// <summary>
        /// Handler for unloaded event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Details of the unload.</param>
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.OnUnloaded();
        }

        /// <summary>
        /// Handler for loaded event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Details of the load.</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.OnLoaded();
        }

        #endregion

        #region Internal Classes
        /// <summary>
        /// The constructor for the MainContentContainerAutomationPeer.
        /// </summary>
        private class MainContentContainerAutomationPeer : System.Windows.Automation.Peers.FrameworkElementAutomationPeer
        {
            /// <summary>
            /// Constructor for MainContentContainerAutomationPeer.
            /// </summary>
            /// <param name="owner">FrameworkElement associated with this AutomationPeer.</param>
            public MainContentContainerAutomationPeer(FrameworkElement owner)
                : base(owner)
            {
            }

            /// <summary>
            /// Gets the control type for the element associated with this AutomationPeer. This is of type
            /// Pane for MainContentContainer.
            /// </summary>
            /// <returns>AutomationControlType for the element associated with this AutomationPeer.</returns>
            protected override System.Windows.Automation.Peers.AutomationControlType GetAutomationControlTypeCore()
            {
                return System.Windows.Automation.Peers.AutomationControlType.Pane;
            }

            /// <summary>
            /// Gets the class name for the element associated with this AutomationPeer.
            /// </summary>
            /// <returns>String containing the name of the element associated with this AutomationPeer.</returns>
            protected override string GetClassNameCore()
            {
                return "Frame";
            }
        }
        #endregion
    }
}
