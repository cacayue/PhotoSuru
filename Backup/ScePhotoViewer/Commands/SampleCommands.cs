//-----------------------------------------------------------------------
// <copyright file="SampleCommands.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Sample commands used by the ScePhotoViewer sample.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using ScePhoto.View;

    /// <summary>
    /// Sample commands used by the ScePhotoViewer.
    /// </summary>
    public class SampleCommands
    {
        /// <summary>
        /// The command that initiates an application data sync.
        /// </summary>
        private SampleStartSyncCommand sampleStartSyncCommand;

        /// <summary>
        /// SampleCommands constructor.
        /// </summary>
        /// <param name="viewManager">The ViewManager executing the command.</param>
        public SampleCommands(SampleViewManager viewManager)
        {
            this.sampleStartSyncCommand = new SampleStartSyncCommand(viewManager);
        }

        /// <summary>
        /// Gets the command that syncs the application data.
        /// </summary>
        public SampleStartSyncCommand SampleStartSyncCommand
        {
            get
            {
                return this.sampleStartSyncCommand;
            }
        }
    }

    /// <summary>
    /// Command that initiates an application data sync.
    /// </summary>
    public class SampleStartSyncCommand : ViewCommand
    {
        /// <summary>
        /// Command constructor.
        /// </summary>
        /// <param name="viewManager">The ViewManager associated with this command.</param>
        public SampleStartSyncCommand(SampleViewManager viewManager) 
            : base(viewManager) 
        { 
        }

        /// <summary>
        /// Override implementation that determines whether this command can be executed.
        /// </summary>
        /// <param name="parameter">Command parameter.</param>
        /// <returns>True if the command can execute.</returns>
        protected override bool CanExecuteInternal(object parameter)
        {
            return (!SampleServiceProvider.SubscriptionServiceManager.IsServiceUpdateInProgress
            && ViewManager.SyncCommands.StartSyncCommand.CanExecute(parameter));
        }

        /// <summary>
        /// Override implementation that initiates the sync of application data.
        /// </summary>
        /// <param name="parameter">Command parameter.</param>
        protected override void ExecuteInternal(object parameter)
        {
            if (ViewManager.SyncCommands.StartSyncCommand.CanExecute(parameter) &&
                !SampleServiceProvider.SubscriptionServiceManager.IsServiceUpdateInProgress)
            {
                ViewManager.SyncCommands.StartSyncCommand.Execute(parameter);
            }
        }
    }
}
