//-----------------------------------------------------------------------
// <copyright file="PhotoAlbumControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Control that displays album UI.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using ScePhoto.Data;
    using ScePhoto;
    using System.ComponentModel;
    using ScePhoto.Controls;

    /// <summary>
    /// Control that displays album UI.
    /// </summary>
    public class PhotoAlbumControl : SizeTemplateControl
    {
        #region Fields
        /// <summary>
        /// DependencyProperty for <see cref="Album" /> property.
        /// </summary>
        public static readonly DependencyProperty AlbumProperty =
                DependencyProperty.Register(
                        "Album",
                        typeof(PhotoAlbum),
                        typeof(PhotoAlbumControl),
                        new FrameworkPropertyMetadata(null));
        #endregion

        #region Constructors

        /// <summary>
        /// PhotoAlbumControl Constructor.
        /// </summary>
        public PhotoAlbumControl()
        {
        }

        #endregion Constructors

        #region Public Properties

        /// <summary>
        /// Gets or sets the Album object used to generate the content of this control.
        /// </summary>
        public PhotoAlbum Album
        {
            get { return (PhotoAlbum)GetValue(AlbumProperty); }
            set { SetValue(AlbumProperty, value); }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Focuses the control when initialized and sets up handlers so that the control is refocused when the album changes.
        /// </summary>
        /// <param name="e">Arguments describing the event.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.Focus();
            this.Loaded += new RoutedEventHandler(this.OnPhotoAlbumControlLoaded);
            this.Unloaded += new RoutedEventHandler(this.OnPhotoAlbumControlUnloaded);   
        }

        /// <summary>
        /// PhotoAlbumControl has special handling for directional keys since the behavior depends on the element currently in focus.
        /// </summary>
        /// <param name="e">Arguments describing the event.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!e.Handled)
            {
                if (e.KeyboardDevice.Modifiers == ModifierKeys.None)
                {
                    switch (e.Key)
                    {
                        case Key.Enter:
                            this.OnEnterKeyPress(e);
                            break;
                        case Key.Escape:
                            this.OnEscapeKeyPress(e);
                            break;
                        default:
                            break;
                    }
                }
            }

            if (!e.Handled)
            {
                base.OnKeyDown(e);
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// On Enter key, enter tab mode.
        /// </summary>
        /// <param name="e">EventArgs describing the event.</param>
        private void OnEnterKeyPress(KeyEventArgs e)
        {
            // Move focus only if there is keyboard focus on this control, but not within it (otherwise handled as tab or traversal requests)
            // Also, ensure that no element has mouse capture, focus should not move while the mouse is captured
            if (this.IsKeyboardFocused && Mouse.Captured == null)
            {
                this.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
            }
        }

        /// <summary>
        /// If keyboard focus is within, move focus to the main control to get out of directional navigation mode.
        /// </summary>
        /// <param name="e">EventArgs describing the event.</param>
        private void OnEscapeKeyPress(KeyEventArgs e)
        {
            // Move focus only if there is keyboard focus within 
            // Also, ensure that no element has mouse  capture, focus should not move while the mouse is captured
            if (!IsKeyboardFocused && IsKeyboardFocusWithin && Mouse.Captured == null)
            {
                this.Focus();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Establishes a handler for ViewManager's property changed so that it can refocus the control when the photo album changes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Arguments describing the event.</param>
        private void OnPhotoAlbumControlLoaded(object sender, RoutedEventArgs e)
        {
            ServiceProvider.ViewManager.PropertyChanged += new PropertyChangedEventHandler(this.OnViewManagerPropertyChanged);
        }

        /// <summary>
        /// Focuses the PhotoAlbumControl when the current photo album changes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Arguments describing the event.</param>
        private void OnViewManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActivePhotoAlbum")
            {
                this.Focus();
            }
        }

        /// <summary>
        /// Removes the handler from ViewManager's property changed event when the control is unloaded.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private void OnPhotoAlbumControlUnloaded(object sender, RoutedEventArgs e)
        {
            ServiceProvider.ViewManager.PropertyChanged -= new PropertyChangedEventHandler(this.OnViewManagerPropertyChanged);
        }
        #endregion
    }
}