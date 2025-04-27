//-----------------------------------------------------------------------
// <copyright file="CommandButton.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Button with which resets its Command on being unloaded.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Button with which resets its Command on being unloaded.
    /// </summary>
    public class CommandButton : Button
    {
        #region Protected Methods

        /// <summary>
        /// Called when the element is initialized.
        /// </summary>
        /// <param name="e">Arguments of the event.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // Register for Unloaded event to properly cleanup the state of the element.
            Unloaded += new RoutedEventHandler(this.OnUnloaded);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// EventHandler for Unloaded event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event details.</param>
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // Reset the command binding to avoid memory leaks.
            Command = null;
        }

        #endregion
    }
}
