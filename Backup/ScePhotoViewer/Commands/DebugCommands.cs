//-----------------------------------------------------------------------
// <copyright file="DebugCommands.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Commands useful when debugging.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using System;
    using System.Windows.Input;

#if DEBUG   
    /// <summary>
    /// Debug-only commands.
    /// </summary>
    public sealed class DebugCommands
    {
        /// <summary>
        /// Command to force garbage collection.
        /// </summary>
        private static GarbageCollectCommand garbageCollectCommand = new GarbageCollectCommand();

        /// <summary>
        /// Private constructor to meet FxCop guidelines.
        /// </summary>
        private DebugCommands()
        {
        }

        /// <summary>
        /// Gets the Garbage Collection command.
        /// </summary>
        public static GarbageCollectCommand GarbageCollectCommand
        {
            get { return garbageCollectCommand; } 
        }
    }

    /// <summary>
    /// A generic command used for debugging.
    /// </summary>
    public abstract class DebugCommand : ICommand
    {
        #region ICommand Members

        /// <summary>
        /// Event fired when the availability of this command changes.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Whether this command can be executed.
        /// </summary>
        /// <param name="parameter">The object upon which the command would potentially act.</param>
        /// <returns>True when the command can be executed.</returns>
        public bool CanExecute(object parameter)
        {
            return this.CanExecuteInternal(parameter);
        }

        /// <summary>
        /// Runs the command.
        /// </summary>
        /// <param name="parameter">The object upon which the command might act.</param>
        public void Execute(object parameter)
        {
            this.ExecuteInternal(parameter);
        }

        #endregion

        /// <summary>
        /// Specific logic for the execution of the command is provided by the classes that inherit from this one.
        /// </summary>
        /// <param name="parameter">The object upon which the command might act.</param>
        protected virtual void ExecuteInternal(object parameter)
        {
        }

        /// <summary>
        /// Specific test to see if the command can be executed is provided by the classes that inherit from this one.
        /// </summary>
        /// <param name="parameter">The object upon which the command would potentially act.</param>
        /// <returns>True if the command can be executed.</returns>
        protected virtual bool CanExecuteInternal(object parameter)
        {
            return true;
        }
    }

    /// <summary>
    /// Forces garbage collection.
    /// </summary>
    public sealed class GarbageCollectCommand : DebugCommand
    {
        /// <summary>
        /// The garbage collection command logic.
        /// </summary>
        /// <param name="parameter">The object upon which this command could act (but doesn't).</param>
        protected override void ExecuteInternal(object parameter)
        {
            GC.Collect(2);
            GC.WaitForPendingFinalizers();
            GC.Collect(2);
            GC.WaitForPendingFinalizers();
        }
    }

#endif
}
