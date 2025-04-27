//-----------------------------------------------------------------------
// <copyright file="GalleryHomeControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Control used to display a multi-templated gallery home view.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using ScePhoto.Data;
    using ScePhoto.View;
    using ScePhoto.Controls;
    using ScePhoto;

    /// <summary>
    /// Control used to display a multi-templated gallery home view.
    /// </summary>
    public class GalleryHomeControl : SizeTemplateControl
    {
        #region Fields
        /// <summary>
        /// Dependency property backing store for RightListPhotoAlbumNavigators.
        /// </summary>
        public static readonly DependencyProperty RightListPhotoAlbumNavigatorsProperty =
            DependencyProperty.Register("RightListPhotoAlbumNavigators", typeof(IList<Navigator>), typeof(GalleryHomeControl), new UIPropertyMetadata(null));

        /// <summary>
        /// Dependency pro[erty backing store for FirstAlbumPhotoNavigators.
        /// </summary>
        public static readonly DependencyProperty FirstAlbumPhotoThumbnailsProperty =
            DependencyProperty.Register("FirstAlbumPhotoThumbnails", typeof(IList<Photo>), typeof(GalleryHomeControl), new UIPropertyMetadata(null));

        /// <summary>
        /// Dependency propety backing store for SecondAlbumPhotoThumbnails.
        /// </summary>
        public static readonly DependencyProperty SecondAlbumPhotoThumbnailsProperty =
            DependencyProperty.Register("SecondAlbumPhotoThumbnails", typeof(IList<Photo>), typeof(GalleryHomeControl), new UIPropertyMetadata(null));

        /// <summary>
        /// Dependency property backing store for TemplateSwitchText.
        /// </summary>
        public static readonly DependencyProperty TemplateSwitchTextProperty =
            DependencyProperty.Register("TemplateSwitchText", typeof(string), typeof(GalleryHomeControl), new UIPropertyMetadata(null));

        /// <summary>
        /// The routed command to switch the template between home and list view.
        /// </summary>
        private static RoutedCommand switchTemplateCommand = new RoutedCommand("SwitchTemplateCommand", typeof(GalleryHomeControl));

        /// <summary>
        /// The template used to display the GalleryHomeControl when the window is large.
        /// </summary>
        private ControlTemplate galleryHomeLargeTemplate;

        /// <summary>
        /// The template used to display the GalleryHomeControl when the window is medium.
        /// </summary>
        private ControlTemplate galleryHomeMediumTemplate;

        /// <summary>
        /// The template used to display the GalleryHomeControl when the window is small but very wide.
        /// </summary>
        private ControlTemplate galleryHomeWideTemplate;

        /// <summary>
        /// The template used to displate the GalleryHomeControl in list mode.
        /// </summary>
        private ControlTemplate galleryListViewTemplate;

        /// <summary>
        /// The template mode for the gallery control; using home templates or the list template.
        /// </summary>
        private GalleryTemplateMode currentGalleryTemplateMode = GalleryTemplateMode.HomeTemplates;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor for GalleryHomeControl, binds the routed commands.
        /// </summary>
        public GalleryHomeControl()
            : base()
        {
            this.CommandBindings.Add(new CommandBinding(switchTemplateCommand, new ExecutedRoutedEventHandler(OnSwitchTemplateCommand)));
            this.Unloaded += new RoutedEventHandler(this.OnGalleryHomeControlUnloaded);
        } 
        #endregion

        #region Enums
        /// <summary>
        /// The template mode for the gallery control; using home templates or the list template.
        /// </summary>
        private enum GalleryTemplateMode
        {
            /// <summary>
            /// Show the home templates and change them based on the window size.
            /// </summary>
            HomeTemplates,

            /// <summary>
            /// Show the list template only.
            /// </summary>
            ListTemplate
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets the command to switch templates.
        /// </summary>
        public static RoutedCommand SwitchTemplateCommand
        {
            get { return GalleryHomeControl.switchTemplateCommand; }
        }

        /// <summary>
        /// Gets the list of photo album navigators to display on the right of the screen.
        /// </summary>
        public IList<Navigator> RightListPhotoAlbumNavigators
        {
            get { return (IList<Navigator>)GetValue(RightListPhotoAlbumNavigatorsProperty); }
            protected set { SetValue(RightListPhotoAlbumNavigatorsProperty, value); }
        }

        /// <summary>
        /// Gets the list of photos to display for the first photo album.
        /// </summary>
        public IList<Photo> FirstAlbumPhotoThumbnails
        {
            get { return (IList<Photo>)GetValue(FirstAlbumPhotoThumbnailsProperty); }
            protected set { SetValue(FirstAlbumPhotoThumbnailsProperty, value); }
        }

        /// <summary>
        /// Gets the list of photos to display for the second photo album.
        /// </summary>
        public IList<Photo> SecondAlbumPhotoThumbnails
        {
            get { return (IList<Photo>)GetValue(SecondAlbumPhotoThumbnailsProperty); }
            protected set { SetValue(SecondAlbumPhotoThumbnailsProperty, value); }
        }

        /// <summary>
        /// Gets the text to display to switch the current template.
        /// </summary>
        public string TemplateSwitchText
        {
            get { return (string)GetValue(TemplateSwitchTextProperty); }
            protected set { SetValue(TemplateSwitchTextProperty, value); }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// On application of a new template, adjusts the various back-end collections so that the right number of photos are shown.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.currentGalleryTemplateMode == GalleryTemplateMode.ListTemplate)
            {
                this.TemplateSwitchText = "Switch to Home View";
            }
            else if (this.currentGalleryTemplateMode == GalleryTemplateMode.HomeTemplates)
            {
                this.TemplateSwitchText = "Switch to List View";
            }

            this.UpdateCollections();
        } 
        #endregion

        #region Protected Methods
        /// <summary>
        /// Signs the GalleryHomeControl up for notification when the current PhotoGallery changes.
        /// </summary>
        /// <param name="e">Event arguments describing the event.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            
            ServiceProvider.ViewManager.PropertyChanged += new PropertyChangedEventHandler(this.OnViewManagerPropertyChanged);
            ServiceProvider.DataManager.UpdateCompleted += new EventHandler<AsyncCompletedEventArgs>(this.OnDataManagerUpdateCompleted);

            try
            {
                this.galleryHomeLargeTemplate = this.FindResource("GalleryHome_Large") as ControlTemplate;
                this.galleryHomeMediumTemplate = this.FindResource("GalleryHome_Medium") as ControlTemplate;
                this.galleryHomeWideTemplate = this.FindResource("GalleryHome_Wide") as ControlTemplate;
                this.galleryListViewTemplate = this.FindResource("GalleryListTemplate") as ControlTemplate;
            }
            catch (ResourceReferenceKeyNotFoundException exception)
            {
                ServiceProvider.Logger.Warning(exception.Message);
            }

            if (SampleScePhotoSettings.GalleryHomeInListView)
            {
                this.SwitchTemplate();
            }

            this.Focus();
        }

        /// <summary>
        /// Updates the current template with the best template for the current size.  Overriden as we want to always use the list view template 
        /// when it is selected; when it's not, we choose the best template as in the base class.
        /// </summary>
        /// <param name="size">The size for which a template should be chosen.</param>
        protected override void UpdateCurrentTemplate(Size size)
        {
            if (this.currentGalleryTemplateMode == GalleryTemplateMode.HomeTemplates)
            {
                base.UpdateCurrentTemplate(size);
            }
        }

        /// <summary>
        /// GalleryHomeControl has special handling for directional keys since the behavior depends on the element currently in focus.
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
                else if (e.KeyboardDevice.Modifiers == ModifierKeys.Alt)
                {
                    if (e.Key == Key.System && e.SystemKey == Key.S)
                    {
                        SwitchTemplateCommand.Execute(null, this);
                        e.Handled = true;
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
        /// Event handler for the SwitchTemplateCommand.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Arguments describing the event.</param>
        private static void OnSwitchTemplateCommand(object sender, ExecutedRoutedEventArgs e)
        {
            GalleryHomeControl galleryHomeControl = sender as GalleryHomeControl;
            if (galleryHomeControl != null)
            {
                galleryHomeControl.SwitchTemplate();
            }
        }

        /// <summary>
        /// Removes items from the beginning of a collection.
        /// </summary>
        /// <typeparam name="T">The type of items contained within the collection.</typeparam>
        /// <param name="list">The collection of items.</param>
        /// <param name="numberOfItems">The number of items to remove.</param>
        /// <returns>A collection with the specified number of items removed from the beginning.</returns>
        private static IList<T> RemoveItemsFromBeginning<T>(IList<T> list, int numberOfItems)
        {
            if (list.Count > numberOfItems)
            {
                for (int i = 0; i < numberOfItems; i++)
                {
                    list.RemoveAt(0);
                }
            }
            else
            {
                list.Clear();
            }

            return list;
        }

        /// <summary>
        /// Updates the control's collections when the viewed photo gallery changes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private void OnViewManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PhotoGallery")
            {
                this.UpdateCollections();
            }
        }

        /// <summary>
        /// Updates the control's collections to match the current gallery and template.
        /// </summary>
        private void UpdateCollections()
        {
            if (ServiceProvider.ViewManager.PhotoGallery != null && ServiceProvider.ViewManager.PhotoGallery.PhotoAlbums.Count > 0)
            {
                this.FirstAlbumPhotoThumbnails = RemoveItemsFromBeginning(new List<Photo>(ServiceProvider.ViewManager.PhotoGallery.PhotoAlbums[0].Photos), 1);

                if (ServiceProvider.ViewManager.PhotoGallery.PhotoAlbums.Count > 1)
                {
                    this.SecondAlbumPhotoThumbnails = RemoveItemsFromBeginning(new List<Photo>(ServiceProvider.ViewManager.PhotoGallery.PhotoAlbums[1].Photos), 1);
                }
                else
                {
                    this.SecondAlbumPhotoThumbnails = null;
                }
            }

            int albumsToRemove = 1;
            if ((this.galleryHomeLargeTemplate == this.Template) || (this.galleryHomeMediumTemplate == this.Template))
            {
                albumsToRemove = 4;
            }
            else if (this.galleryHomeWideTemplate == this.Template)
            {
                albumsToRemove = 3;
            }

            this.RightListPhotoAlbumNavigators = RemoveItemsFromBeginning(new List<Navigator>(ServiceProvider.ViewManager.MasterNavigator.GetTopLevelNavigators()), albumsToRemove);
        }

        /// <summary>
        /// Switches the current template to the home or list view template.
        /// </summary>
        private void SwitchTemplate()
        {
            if (this.galleryListViewTemplate != null)
            {
                switch (this.currentGalleryTemplateMode)
                {
                    case GalleryTemplateMode.HomeTemplates:
                        this.currentGalleryTemplateMode = GalleryTemplateMode.ListTemplate;
                        this.Template = this.galleryListViewTemplate;
                        SampleScePhotoSettings.GalleryHomeInListView = true;
                        break;
                    case GalleryTemplateMode.ListTemplate:
                        this.currentGalleryTemplateMode = GalleryTemplateMode.HomeTemplates;
                        this.InvalidateArrange();
                        SampleScePhotoSettings.GalleryHomeInListView = false;
                        break;
                    default:
                        break;
                }

                this.Focus();
            }
        }

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
                if (this.currentGalleryTemplateMode == GalleryTemplateMode.ListTemplate)
                {
                    UIElement albumList = this.Template.FindName("GalleryListBox", this) as UIElement;
                    if (albumList != null)
                    {
                        albumList.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        this.SetValue(KeyboardNavigation.DirectionalNavigationProperty, KeyboardNavigationMode.Contained);
                        e.Handled = true;
                    }
                }
                else if (this.currentGalleryTemplateMode == GalleryTemplateMode.HomeTemplates)
                {
                    this.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                    this.SetValue(KeyboardNavigation.DirectionalNavigationProperty, KeyboardNavigationMode.Contained);
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// If keyboard focus is within, move focus to the main control to get out of directional navigation mode.
        /// </summary>
        /// <param name="e">EventArgs describing the event.</param>
        private void OnEscapeKeyPress(KeyEventArgs e)
        {
            // Move focus only if there is keyboard focus within 
            // Also, ensure that no element has mouse capture, focus should not move while the mouse is captured
            if (!IsKeyboardFocused && IsKeyboardFocusWithin && Mouse.Captured == null)
            {
                this.SetValue(KeyboardNavigation.DirectionalNavigationProperty, KeyboardNavigationMode.Continue);
                this.Focus();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Update the gallery home collections when an update has occurred.
        /// </summary>
        /// <remarks>
        /// A more accurate event would be ViewManager.PhotoGallery.CollectionChanged, but this is fired before photos exist in the new albums;
        /// creating navigators for these items would only produce an empty collection.  
        /// </remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Arguments describing the event.</param>
        private void OnDataManagerUpdateCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.UpdateCollections();
        }

        /// <summary>
        /// Removed the UpdateCompleted and PropertyChanged event handlers when this control unloads.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Arguments describing the event.</param>
        private void OnGalleryHomeControlUnloaded(object sender, RoutedEventArgs e)
        {
            ServiceProvider.DataManager.UpdateCompleted -= new EventHandler<AsyncCompletedEventArgs>(this.OnDataManagerUpdateCompleted);
            ServiceProvider.ViewManager.PropertyChanged -= new PropertyChangedEventHandler(this.OnViewManagerPropertyChanged);
        }
        #endregion
    }
}
