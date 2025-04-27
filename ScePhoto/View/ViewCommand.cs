//-----------------------------------------------------------------------
// <copyright file="ViewCommand.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Abstract base class for all commands that exist on ViewManager.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.View
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;
    using System.Text;

    /// <summary>
    /// Abstract base class for all commands that exist on ViewManager. These commands are initialized with ViewManager in the ctor.
    /// </summary>
    public abstract class ViewCommand : ScePhotoCommand
    {
        #region Private Fields
        /// <summary>
        /// ViewManager associated with this command.
        /// </summary>
        private ViewManager viewManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Contructor for ViewCommand.
        /// </summary>
        /// <param name="viewManager">ViewManager associated with this command.</param>
        protected ViewCommand(ViewManager viewManager)
        {
            this.viewManager = viewManager;
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the ViewManager associated with this command. ViewCommands must have access to a ViewManager since they may call APIs or
        /// require state information from it.
        /// </summary>
        protected ViewManager ViewManager
        {
            get { return this.viewManager; }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Execution logic for ViewCommand that can be overridden by derived classes.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for this command.
        /// </param>
        protected override void ExecuteInternal(object parameter)
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

        #endregion
    }
}
