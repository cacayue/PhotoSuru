//-----------------------------------------------------------------------
// <copyright file="PhotoGalleryAlbumSelector.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Control to display the photo gallery and album selectors.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Controls
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using ScePhoto.Data;
    using ScePhoto.View;

    /// <summary>
    /// Control to display the photo gallery and album selectors.
    /// </summary>
    [TemplatePart(Name = "PART_AlbumBreadcrumbBox", Type = typeof(BreadcrumbBox)), TemplatePart(Name = "PART_GalleryBreadcrumbBox", Type = typeof(BreadcrumbBox))]
    public class PhotoGalleryAlbumSelector : Control
    {
        #region Fields
        /// <summary>
        /// DependencyProperty backing store for PhotoAlbumNavigators.
        /// </summary>
        public static readonly DependencyProperty PhotoAlbumNavigatorsProperty =
            DependencyProperty.Register("PhotoAlbumNavigators", typeof(IList<Navigator>), typeof(PhotoGalleryAlbumSelector), new UIPropertyMetadata(null));

        /// <summary>
        /// DependencyProperty backing store for PhotoAlbumNavigator.
        /// </summary>
        public static readonly DependencyProperty PhotoAlbumNavigatorProperty =
            DependencyProperty.Register("PhotoAlbumNavigator", typeof(PhotoAlbumNavigator), typeof(PhotoGalleryAlbumSelector), new UIPropertyMetadata(null, new PropertyChangedCallback(PhotoAlbumNavigatorChangedCallback)));

        /// <summary>
        /// The photo album breadcrumb box.
        /// </summary>
        private BreadcrumbBox albumBox;

        /// <summary>
        /// The photo gallery breadcrumb box.
        /// </summary>
        private BreadcrumbBox galleryBox;
        #endregion

        #region Constructor
        /// <summary>
        /// PhotoGalleryAlbum constructor; registers for property change events on the view manager.
        /// </summary>
        public PhotoGalleryAlbumSelector()
        {
            ServiceProvider.ViewManager.PropertyChanged += new PropertyChangedEventHandler(this.OnViewManagerPropertyChanged);
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets the list of navigators for all photo albums in the current gallery.
        /// </summary>
        public IList<Navigator> PhotoAlbumNavigators
        {
            get { return (IList<Navigator>)GetValue(PhotoAlbumNavigatorsProperty); }
            protected set { SetValue(PhotoAlbumNavigatorsProperty, value); }
        }

        /// <summary>
        /// Gets or sets the navigator fot the album currently displayed.
        /// </summary>
        public PhotoAlbumNavigator PhotoAlbumNavigator
        {
            get { return (PhotoAlbumNavigator)GetValue(PhotoAlbumNavigatorProperty); }
            set { SetValue(PhotoAlbumNavigatorProperty, value); }
        } 
        #endregion

        #region Methods

        /// <summary>
        /// Open (or close, if already open) the album menu.
        /// </summary>
        public void OpenAlbumMenu()
        {
            if (this.albumBox != null)
            {
                this.albumBox.IsDropDownOpen = !this.albumBox.IsDropDownOpen;
            }
        }

        /// <summary>
        /// Open (or close, if already open) the gallery menu.
        /// </summary>
        public void OpenGalleryMenu()
        {
            if (this.galleryBox != null)
            {
                this.galleryBox.IsDropDownOpen = !this.galleryBox.IsDropDownOpen;
            }
        }

        /// <summary>
        /// Locates the photo album breadcrumb box when the template is loaded.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.albumBox = this.Template.FindName("PART_AlbumBreadcrumbBox", this) as BreadcrumbBox;
            this.galleryBox = this.Template.FindName("PART_GalleryBreadcrumbBox", this) as BreadcrumbBox;
        }

        /// <summary>
        /// Handles the Enter, Left, and Right keys to allow for navigation.
        /// </summary>
        /// <param name="e">Arguments describing the event.</param>
        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (!e.Handled)
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        NavigateToSelectedAlbum(e);
                        break;
                    case Key.Left:
                        this.SwitchBoxesLeft(e);
                        break;
                    case Key.Right:
                        this.SwitchBoxesRight(e);
                        break;
                    case Key.Down:
                    case Key.Up:
                        ControlMenu(e);
                        break;
                    default:
                        break;
                }
            }            
            
            base.OnPreviewKeyDown(e);
        }

        /// <summary>
        /// Since the navigation to an album is actually done by a button in the template to prevent navigation requests from being
        /// generated by this control when other controls change ViewManager.ActivePhotoAlbum, we have to handle the enter key
        /// and do the navigation as well.
        /// </summary>
        /// <param name="e">Arguments describing the key event.</param>
        private static void NavigateToSelectedAlbum(KeyEventArgs e)
        {
            ComboBoxItem selectedItem = e.OriginalSource as ComboBoxItem;
            if (selectedItem != null)
            {
                Navigator selectedNavigator = selectedItem.Content as Navigator;
                if (selectedNavigator != null)
                {
                    if (selectedNavigator is PhotoAlbumNavigator)
                    {
                        ServiceProvider.ViewManager.NavigationCommands.NavigateToPhotoAlbumCommand.Execute(selectedNavigator);
                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Controls the display of the drop-down menus when their toggle buttons have been selected with the keyboard.
        /// </summary>
        /// <param name="e">Arguments describing the key event.</param>
        private static void ControlMenu(KeyEventArgs e)
        {
            ToggleButton button = e.OriginalSource as ToggleButton;
            if (button != null)
            {
                if (e.Key == Key.Down && button.IsChecked == false)
                {
                    button.IsChecked = true;
                }
                else if (e.Key == Key.Up && button.IsChecked == true)
                {
                    button.IsChecked = false;
                }

                // Always handle the event so that the behaviour of the drop-down menu is appropriate.
                e.Handled = true;
            }
        }

        /// <summary>
        /// Callback function for the PhotoAlbumNavigator dependency property.
        /// </summary>
        /// <param name="element">The source element of the event.</param>
        /// <param name="args">Event arguments describing the event.</param>
        private static void PhotoAlbumNavigatorChangedCallback(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            ((PhotoGalleryAlbumSelector)element).OnPhotoAlbumNavigatorChanged();
        }

        /// <summary>
        /// Switches from the photo album box to the photo gallery box.
        /// </summary>
        /// <param name="e">Arguments describing the key event.</param>
        private void SwitchBoxesLeft(KeyEventArgs e)
        {
            if (this.albumBox.IsKeyboardFocusWithin)
            {
                this.albumBox.IsDropDownOpen = false;
                this.OpenGalleryMenu();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Switches from the photo gallery box to the photo album box.
        /// </summary>
        /// <param name="e">Arguments describing the key event.</param>
        private void SwitchBoxesRight(KeyEventArgs e)
        {
            if (this.galleryBox.IsKeyboardFocusWithin)
            {
                this.galleryBox.IsDropDownOpen = false;
                this.OpenAlbumMenu();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Property changed handler executed when a ViewManager property changes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private void OnViewManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PhotoGallery")
            {
                this.OnPhotoGalleryChanged();
            }
            else if (e.PropertyName == "ActivePhotoAlbum")
            {
                this.OnPhotoAlbumChanged();
            }
        }

        /// <summary>
        /// Updates PhotoAlbumNavigator to match the current photo album.
        /// </summary>
        private void OnPhotoAlbumChanged()
        {
            PhotoAlbum activePhotoAlbum = ServiceProvider.ViewManager.ActivePhotoAlbum;
            foreach (PhotoAlbumNavigator albumNavigator in this.PhotoAlbumNavigators)
            {
                if (activePhotoAlbum == albumNavigator.Content)
                {
                    this.PhotoAlbumNavigator = albumNavigator;
                }
            }
        }

        /// <summary>
        /// Refreshes the photo album navigators when the photo gallery changes.
        /// </summary>
        private void OnPhotoGalleryChanged()
        {
            this.PhotoAlbumNavigators = ServiceProvider.ViewManager.MasterNavigator.GetTopLevelNavigators();
            if (this.PhotoAlbumNavigators.Count > 0)
            {
                this.PhotoAlbumNavigator = (PhotoAlbumNavigator)this.PhotoAlbumNavigators[0];
            }

            ServiceProvider.ViewManager.NavigationCommands.NavigateToFirstPhotoAlbumCommand.Execute(null);
        }

        /// <summary>
        /// Return focus to the selector so that the drop down closes.
        /// </summary>
        private void OnPhotoAlbumNavigatorChanged()
        {
            this.Focus();
        } 
        #endregion
    }
}
