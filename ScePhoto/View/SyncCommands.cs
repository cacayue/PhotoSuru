//-----------------------------------------------------------------------
// <copyright file="SyncCommands.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Commands controlling sync process and gallery selection
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.View
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;
    using System.Text;
    using ScePhoto.Data;

    /// <summary>
    /// Commands controlling sync process and gallery selection.
    /// </summary>
    public class SyncCommands
    {
        #region Fields
        /// <summary>
        /// Gets the command to start the sync/data update process.
        /// </summary>
        private StartSyncCommand startSyncCommand;

        /// <summary>
        /// Gets the command to stop a sync in progress.
        /// </summary>
        private StopSyncCommand stopSyncCommand;

        /// <summary>
        /// Gets the command to select a gallery (PhotoGallery) from the collection of PhotoGallery files present on the master feed.
        /// </summary>
        private SelectGalleryCommand selectGalleryCommand; 
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor initializes all sync commands.
        /// </summary>
        /// <param name="viewManager">
        /// ViewManager on which all ViewCommands are dependent.
        /// </param>
        public SyncCommands(ViewManager viewManager)
        {
            this.startSyncCommand = new StartSyncCommand(viewManager);
            this.stopSyncCommand = new StopSyncCommand(viewManager);
            this.selectGalleryCommand = new SelectGalleryCommand(viewManager);
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets the command to start the sync/data update process.
        /// </summary>
        public StartSyncCommand StartSyncCommand
        {
            get { return this.startSyncCommand; }
        }

        /// <summary>
        /// Gets the command to stop a sync in progress.
        /// </summary>
        public StopSyncCommand StopSyncCommand
        {
            get { return this.stopSyncCommand; }
        }

        /// <summary>
        /// Gets the command to select a gallery (PhotoGallery) from the collection of PhotoGallery files present on the master feed.
        /// </summary>
        public SelectGalleryCommand SelectGalleryCommand
        {
            get { return this.selectGalleryCommand; }
        } 
        #endregion
    }

    /// <summary>
    /// Starts a data update process.
    /// </summary>
    public class StartSyncCommand : ViewCommand
    {
        /// <summary>
        /// Constructor for StartSyncCommand.
        /// </summary>
        /// <param name="viewManager">ViewManager associated with this command.</param>
        public StartSyncCommand(ViewManager viewManager) : base(viewManager) 
        {
        }

        /// <summary>
        /// CanExecute logic for ViewCommand that can be overridden by derived classes.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for this command.
        /// </param>
        /// <returns>True when Sync is not already in progress.</returns>
        protected override bool CanExecuteInternal(object parameter)
        {
            return (ViewManager.SyncState != SyncState.SyncInProgress);
        }

        /// <summary>
        /// Execution logic for ViewCommand that can be overridden by derived classes.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for this command.
        /// </param>
        protected override void ExecuteInternal(object parameter)
        {
            if (ServiceProvider.ViewManager.SyncState != SyncState.SyncInProgress)
            {
                ServiceProvider.DataManager.UpdateAsync();
            }
        }
    }

    /// <summary>
    /// Stops a sync operation currently in progress.
    /// </summary>
    public class StopSyncCommand : ViewCommand
    {
        /// <summary>
        /// Constructor for StopSyncCommand.
        /// </summary>
        /// <param name="viewManager">ViewManager associated with this command.</param>
        public StopSyncCommand(ViewManager viewManager) : base(viewManager) 
        {
        }

        /// <summary>
        /// CanExecute logic for ViewCommand that can be overridden by derived classes.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for this command.
        /// </param>
        /// <returns>True when sync in progress.</returns>
        protected override bool CanExecuteInternal(object parameter)
        {
            return (ViewManager.SyncState == SyncState.SyncInProgress);
        }

        /// <summary>
        /// Execution logic for ViewCommand that can be overridden by derived classes.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for this command.
        /// </param>
        protected override void ExecuteInternal(object parameter)
        {
            ServiceProvider.DataManager.CancelAsync(null);
        }
    }

    /// <summary>
    /// Given a PhotoGallery as parameter, selects it as the currently viewed PhotoGallery. Albums and photos viewed
    /// come from this PhotoGallery.
    /// </summary>
    public class SelectGalleryCommand : ViewCommand
    {
        /// <summary>
        /// Constructor for SelectArchiveCommand.
        /// </summary>
        /// <param name="viewManager">ViewManager associated with this command.</param>
        public SelectGalleryCommand(ViewManager viewManager) : base(viewManager) 
        {
        }

        /// <summary>
        /// CanExecute logic for ViewCommand that can be overridden by derived classes.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for this command.
        /// </param>
        /// <returns>Always returns true.</returns>
        protected override bool CanExecuteInternal(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Execution logic for ViewCommand that can be overridden by derived classes.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for this command.
        /// </param>
        protected override void ExecuteInternal(object parameter)
        {
            PhotoGallery photoGallery = parameter as PhotoGallery;
            if (photoGallery != null && !Object.ReferenceEquals(photoGallery, ViewManager.PhotoGallery))
            {
                ViewManager.SwitchToPhotoGallery(photoGallery, false);
            }
        }
    }
}
