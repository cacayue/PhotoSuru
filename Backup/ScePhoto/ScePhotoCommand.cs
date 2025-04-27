//-----------------------------------------------------------------------
// <copyright file="ScePhotoCommand.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Abstract base class for all commands that exist on ViewManager.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;
    using System.Text;

    /// <summary>
    /// Abstract base class for all commands that exist on ViewManager. Registers event handlers for CanExecuteChanged as
    /// weak references to prevent the commands from holding on to objects past their lifetime.
    /// </summary>
    public abstract class ScePhotoCommand : ICommand
    {
        #region ICommand Members

        /// <summary>
        /// Event handler for CanExecuteChanged. This event is not raised by any ScePhotoCommand.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// CanExecute logic.
        /// </summary>
        /// <param name="parameter">Command parameter.</param>
        /// <returns>True if command can execute.</returns>
        public bool CanExecute(object parameter)
        {
            return this.CanExecuteInternal(parameter);
        }

        /// <summary>
        /// Command execution logic.
        /// </summary>
        /// <param name="parameter">Command parameter.</param>
        public void Execute(object parameter)
        {
            this.ExecuteInternal(parameter);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Derived classes implement Execute functionality here.
        /// </summary>
        /// <param name="parameter">Command parameter.</param>
        protected abstract void ExecuteInternal(object parameter);

        /// <summary>
        /// Derived classes implement CanEexecute logic here.
        /// </summary>
        /// <param name="parameter">Command parameter.</param>
        /// <returns>True if command can execute.</returns>
        protected abstract bool CanExecuteInternal(object parameter);

        #endregion
    }
}
