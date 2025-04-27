//-----------------------------------------------------------------------
// <copyright file="IntroWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Interaction logic for IntroWindow.xaml
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using ScePhoto;

    /// <summary>
    /// Interaction logic for IntroWindow.xaml
    /// </summary>
    public partial class IntroWindow : UserControl
    {
        /// <summary>
        /// IntroWindow Constructor.
        /// </summary>
        public IntroWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Close button click handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            ServiceProvider.ViewManager.EndDialog(this);
        }
    }
}
