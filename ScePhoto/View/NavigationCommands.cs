//-----------------------------------------------------------------------
// <copyright file="NavigationCommands.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Navigation commands exposed by ViewManager.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.View
{
    using System;
    using ScePhoto.Data;

    /// <summary>
    /// Navigation commands exposed by <see cref="ViewManager"/>. Includes commands for navigating to specific photos or albums,
    /// next/previous navigation, navigating to a Search album, navigating by Guid or by <see cref="Navigator"/> and others. 
    /// </summary>
    public class NavigationCommands
    {
        #region Fields
        /// <summary>
        /// Navigates to the next photo album.
        /// </summary>
        private NextPhotoAlbumCommand nextPhotoAlbumCommand;

        /// <summary>
        /// Navigates to the previous photo album.
        /// </summary>
        private PreviousPhotoAlbumCommand previousPhotoAlbumCommand;

        /// <summary>
        /// Navigtates to the next photo.
        /// </summary>
        private NextPhotoCommand nextPhotoCommand;

        /// <summary>
        /// Navigates to the previous photo.
        /// </summary>
        private PreviousPhotoCommand previousPhotoCommand;

        /// <summary>
        /// Navigates to a specific photo album.
        /// </summary>
        private NavigateToPhotoAlbumCommand navigateToPhotoAlbumCommand;

        /// <summary>
        /// Navigates to a specific photo.
        /// </summary>
        private NavigateToPhotoCommand navigateToPhotoCommand;

        /// <summary>
        /// Navigates to the photo's parent album.
        /// </summary>
        private NavigateToParentPhotoAlbumCommand navigateToParentPhotoAlbumCommand;

        /// <summary>
        /// Navigates to the NavigatableObject's next sibling.
        /// </summary>
        private NextSiblingCommand nextSiblingCommand;

        /// <summary>
        /// Navigates to the NavigatableObject's previous sibling.
        /// </summary>
        private PreviousSiblingCommand previousSiblingCommand;

        /// <summary>
        /// Navigates to the first photo album in the current gallery.
        /// </summary>
        private NavigateToFirstPhotoAlbumCommand navigateToFirstPhotoAlbumCommand;

        /// <summary>
        /// Navigates to the first photo gallery.
        /// </summary>
        private NavigateToFirstPhotoGalleryCommand navigateToFirstPhotoGalleryCommand;

        /// <summary>
        /// Navigats to the last photo album in the current gallery.
        /// </summary>
        private NavigateToLastPhotoAlbumCommand navigateToLastPhotoAlbumCommand;

        /// <summary>
        /// Navigates to photo slideshow.
        /// </summary>
        private NavigateToPhotoSlideShowCommand navigateToPhotoSlideShowCommand;

        /// <summary>
        /// Navigates to the search results.
        /// </summary>
        private SearchCommand searchCommand; 
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor for NavigationCommands.
        /// </summary>
        /// <param name="viewManager">ViewManager associated with each ViewCommand in NavigationCommands.</param>
        public NavigationCommands(ViewManager viewManager)
        {
            this.nextPhotoAlbumCommand = new NextPhotoAlbumCommand(viewManager);
            this.previousPhotoAlbumCommand = new PreviousPhotoAlbumCommand(viewManager);
            this.nextPhotoCommand = new NextPhotoCommand(viewManager);
            this.previousPhotoCommand = new PreviousPhotoCommand(viewManager);
            this.navigateToPhotoAlbumCommand = new NavigateToPhotoAlbumCommand(viewManager);
            this.navigateToPhotoCommand = new NavigateToPhotoCommand(viewManager);
            this.navigateToParentPhotoAlbumCommand = new NavigateToParentPhotoAlbumCommand(viewManager);
            this.nextSiblingCommand = new NextSiblingCommand(viewManager);
            this.previousSiblingCommand = new PreviousSiblingCommand(viewManager);
            this.navigateToFirstPhotoAlbumCommand = new NavigateToFirstPhotoAlbumCommand(viewManager);
            this.navigateToLastPhotoAlbumCommand = new NavigateToLastPhotoAlbumCommand(viewManager);
            this.searchCommand = new SearchCommand(viewManager);
            this.navigateToFirstPhotoGalleryCommand = new NavigateToFirstPhotoGalleryCommand(viewManager);
            this.navigateToPhotoSlideShowCommand = new NavigateToPhotoSlideShowCommand(viewManager);
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets the navigator to the next Photo Album from current content, if enabled.
        /// </summary>
        public NextPhotoAlbumCommand NextPhotoAlbumCommand
        {
            get
            {
                return this.nextPhotoAlbumCommand;
            }
        }

        /// <summary>
        /// Gets the navigator to the previous Photo Album from current content, if enabled.
        /// </summary>
        public PreviousPhotoAlbumCommand PreviousPhotoAlbumCommand
        {
            get
            {
                return this.previousPhotoAlbumCommand;
            }
        }

        /// <summary>
        /// Gets the navigator to the next photo from current content.
        /// </summary>
        public NextPhotoCommand NextPhotoCommand
        {
            get
            {
                return this.nextPhotoCommand;
            }
        }

        /// <summary>
        /// Gets the navigator to the previous photo from current content.
        /// </summary>
        public PreviousPhotoCommand PreviousPhotoCommand
        {
            get
            {
                return this.previousPhotoCommand;
            }
        }

        /// <summary>
        /// Gets the navigator to an arbitrary photo album, given a PhotoAlbumNavigator as parameter.
        /// </summary>
        public NavigateToPhotoAlbumCommand NavigateToPhotoAlbumCommand
        {
            get
            {
                return this.navigateToPhotoAlbumCommand;
            }
        }

        /// <summary>
        /// Gets the navigator to an arbitrary photo, given a PhotoAlbumNavigator as parameter.
        /// </summary>
        public NavigateToPhotoCommand NavigateToPhotoCommand
        {
            get
            {
                return this.navigateToPhotoCommand;
            }
        }

        /// <summary>
        /// Gets the navigator to the parent Photo Album of the current content.
        /// </summary>
        public NavigateToParentPhotoAlbumCommand NavigateToParentPhotoAlbumCommand
        {
            get
            {
                return this.navigateToParentPhotoAlbumCommand;
            }
        }

        /// <summary>
        /// Gets the navigator to the next sibling of current content, whether PhotoAlbum or Photo.
        /// </summary>
        public NextSiblingCommand NextSiblingCommand
        {
            get
            {
                return this.nextSiblingCommand;
            }
        }

        /// <summary>
        /// Gets the navigator to the previous sibling of current content, whether PhotoAlbum or Photo.
        /// </summary>
        public PreviousSiblingCommand PreviousSiblingCommand
        {
            get
            {
                return this.previousSiblingCommand;
            }
        }

        /// <summary>
        /// Gets the navigator to the first top-level PhotoAlbum of the currently selected PhotoGallery.
        /// </summary>
        public NavigateToFirstPhotoAlbumCommand NavigateToFirstPhotoAlbumCommand
        {
            get
            {
                return this.navigateToFirstPhotoAlbumCommand;
            }
        }

        /// <summary>
        /// Gets the navigator to the last top-level PhotoAlbum of the currently selected PhotoGallery.
        /// </summary>
        public NavigateToLastPhotoAlbumCommand NavigateToLastPhotoAlbumCommand
        {
            get
            {
                return this.navigateToLastPhotoAlbumCommand;
            }
        }

        /// <summary>
        /// Gets the navigator for a SearchPhotoAlbum with photos relevant to the search text and
        /// navigates to it.
        /// </summary>
        public SearchCommand SearchCommand
        {
            get
            {
                return this.searchCommand;
            }
        }

        /// <summary>
        /// Gets the navigator to the first photo album of the first photo gallery among all galleries in the current data.
        /// </summary>
        public NavigateToFirstPhotoGalleryCommand NavigateToFirstPhotoGalleryCommand
        {
            get
            {
                return this.navigateToFirstPhotoGalleryCommand;
            }
        }

        /// <summary>
        /// Gets the navigator to a photo slideshow.
        /// </summary>
        public NavigateToPhotoSlideShowCommand NavigateToPhotoSlideShowCommand
        {
            get
            {
                return this.navigateToPhotoSlideShowCommand;
            }
        } 
        #endregion
    }

    /// <summary>
    /// Base class for navigation commands. Defines the PerformNavigate method for subclasses to override to
    /// do the actual work of navigation.
    /// </summary>
    /// <remarks>
    /// Subclasses should not override NavigationCommand's ExecuteInternal method, since it checks that a transition
    /// is not in progress before performing navigation. Navigating during a transition produces unreliable results
    /// since the transition's animation may not be complete, but navigation can cause instantation of new UI for the navigated data,
    /// changing the transition animation's targets and causing the transition to freeze. Since navigation and transitions are
    /// closely linked, navigation should not proceed unless a transition has been completed. For customization, NavigationCommand provides
    /// the <see cref="PerformNavigate"/> method, subclasses should override this to customize behavior.
    /// </remarks>
    public abstract class NavigationCommand : ViewCommand
    {
        /// <summary>
        /// Type initializer for NavigationCommand.
        /// </summary>
        /// <param name="viewManager">
        /// ViewManager associated with ViewCommand subclasses.
        /// </param>
        protected NavigationCommand(ViewManager viewManager) : base(viewManager) 
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
            this.PerformNavigate(parameter);
        }

        /// <summary>
        /// Navigation-specific logic that can be overridden by subclasses of NavigationCommand to provide custom navigation behavior.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for NavigationCommand.
        /// </param>
        /// <remarks>
        /// PerformNavigate is a navigation-specific method that NavigationCommand subclasses may override to customize behavior
        /// instead of overriding ExecuteInternal on ViewCommand. This is useful because the NavigationCommand class overrides
        /// ExecuteInternal to add special interaction logic with transitions, it is not desirable to remove this logic by overriding.
        /// </remarks>
        protected virtual void PerformNavigate(object parameter)
        {
        }

        /// <summary>
        /// Gets a PhotoNavigator's parent PhotoAlbumNavigator.
        /// </summary>
        /// <param name="photoNavigator">PhotoNavigator whose parent is needed.</param>
        /// <returns>PhotoAlbumNavigator that is the parent of the navigator.</returns>
        protected virtual PhotoAlbumNavigator GetParentPhotoAlbumNavigator(PhotoNavigator photoNavigator)
        {
            PhotoAlbumNavigator parentNavigator = null;
            if (photoNavigator != null)
            {
                parentNavigator = photoNavigator.GetParent() as PhotoAlbumNavigator;
            }

            return parentNavigator;
        }

        /// <summary>
        /// Gets a PhotoAlbumNavigator's next sibling PhotoAlbumNavigator.
        /// </summary>
        /// <param name="photoAlbumNavigator">PhotoAlbumNavigator whose previous sibling is needed.</param>
        /// <returns>PhotoAlbumNavigator that is previous sibling of the navigator.</returns>
        protected virtual PhotoAlbumNavigator GetNextSiblingPhotoAlbumNavigator(PhotoAlbumNavigator photoAlbumNavigator)
        {
            PhotoAlbumNavigator nextPhotoAlbumNavigator = null;
            if (photoAlbumNavigator != null)
            {
                nextPhotoAlbumNavigator = photoAlbumNavigator.GetNextSibling() as PhotoAlbumNavigator;
            }

            return nextPhotoAlbumNavigator;
        }

        /// <summary>
        /// Gets a PhotoAlbumNavigator's previous sibling PhotoAlbumNavigator.
        /// </summary>
        /// <param name="photoAlbumNavigator">PhotoAlbumNavigator whose previous sibling is needed.</param>
        /// <returns>PhotoAlbumNavigator that is previous sibling of the navigator.</returns>
        protected virtual PhotoAlbumNavigator GetPreviousSiblingPhotoAlbumNavigator(PhotoAlbumNavigator photoAlbumNavigator)
        {
            PhotoAlbumNavigator previousPhotoAlbumNavigator = null;
            if (photoAlbumNavigator != null)
            {
                previousPhotoAlbumNavigator = photoAlbumNavigator.GetPreviousSibling() as PhotoAlbumNavigator;
            }

            return previousPhotoAlbumNavigator;
        }

        /// <summary>
        /// Gets a PhotoNavigator's next sibling PhotoNavigator.
        /// </summary>
        /// <param name="photoNavigator">PhotoNavigator whose sibling is needed.</param>
        /// <returns>PhotoNavigator that is next sibling of the navigator.</returns>
        protected virtual PhotoNavigator GetNextSiblingPhotoNavigator(PhotoNavigator photoNavigator)
        {
            if (photoNavigator != null)
            {
                return photoNavigator.GetNextSibling() as PhotoNavigator;
            }

            return null;
        }

        /// <summary>
        /// Gets a PhotoNavigator's previous sibling PhotoNavigator.
        /// </summary>
        /// <param name="photoNavigator">PhotoNavigator whose sibling is needed.</param>
        /// <returns>PhotoNavigator that is previous sibling of the navigator.</returns>
        protected virtual PhotoNavigator GetPreviousSiblingPhotoNavigator(PhotoNavigator photoNavigator)
        {
            if (photoNavigator != null)
            {
                return photoNavigator.GetPreviousSibling() as PhotoNavigator;
            }

            return null;
        }

        /// <summary>
        /// Gets a PhotoAlbumNavigator's first child PhotoNavigator.
        /// </summary>
        /// <param name="photoAlbumNavigator">The element's Parent <see cref="PhotoAlbumNavigator"/>.</param>
        /// <returns>PhotoNavigator for the the first <see cref="Photo"/> in photoAlbumNavigator's <see cref="PhotoAlbum"/> content.</returns>
        protected virtual PhotoNavigator GetFirstChildPhotoNavigator(PhotoAlbumNavigator photoAlbumNavigator)
        {
            if (photoAlbumNavigator != null)
            {
                return photoAlbumNavigator.GetFirstPhoto();
            }

            return null;
        }

        /// <summary>
        /// Gets a PhotoAlbumNavigator's last child PhotoNavigator.
        /// </summary>
        /// <param name="photoAlbumNavigator">Parent PhotoAlbumNavigator.</param>
        /// <returns>PhotoNavigator for the the last <see cref="Photo"/> in this  <see cref="PhotoAlbum"/>.</returns>
        protected virtual PhotoNavigator GetLastChildPhotoNavigator(PhotoAlbumNavigator photoAlbumNavigator)
        {
            if (photoAlbumNavigator != null)
            {
                return photoAlbumNavigator.GetLastPhoto();
            }

            return null;
        }

        /// <summary>
        /// Gets a PhotoAlbumNavigator's next PhotoAlbumNavigator. This is not necessarily a sibling as it may return
        /// a parent photo album navigator if the appropriate parameters are passed. 
        /// </summary>
        /// <param name="photoAlbumNavigator">
        /// PhotoAlbumNavigator for which next navigator is required.
        /// </param>
        /// <returns>
        /// The PhotoAlbumNavigator found in accordance with the search parameters above, or null if none was found.
        /// </returns>
        protected virtual PhotoAlbumNavigator GetNextPhotoAlbumNavigator(PhotoAlbumNavigator photoAlbumNavigator)
        {
            PhotoAlbumNavigator nextPhotoAlbumNavigator = null;
            if (photoAlbumNavigator != null)
            {
                    nextPhotoAlbumNavigator = this.GetNextSiblingPhotoAlbumNavigator(photoAlbumNavigator);
            }

            return nextPhotoAlbumNavigator;
        }

        /// <summary>
        /// Gets a PhotoAlbumNavigator's previous PhotoAlbumNavigator. This is not necessarily a sibling as it may return
        /// a parent photo album navigator if the appropriate parameters are passed. 
        /// </summary>
        /// <param name="photoAlbumNavigator">
        /// PhotoAlbumNavigator for which previous navigator is required.
        /// </param>
        /// <returns>
        /// The PhotoAlbumNavigator found in accordance with the search parameters above, or null if none was found.
        /// </returns>
        protected virtual PhotoAlbumNavigator GetPreviousPhotoAlbumNavigator(PhotoAlbumNavigator photoAlbumNavigator)
        {
            PhotoAlbumNavigator previousPhotoAlbumNavigator = null;
            if (photoAlbumNavigator != null)
            {
                    previousPhotoAlbumNavigator = this.GetPreviousSiblingPhotoAlbumNavigator(photoAlbumNavigator);
            }

            return previousPhotoAlbumNavigator;
        }

        /// <summary>
        /// Gets the next Navigator for a Navigator, whether PhotoAlbum or Photo. This is a deep-pad navigation scenario to enable
        /// navigating through all content by just hitting the Next/Previous key. The next navigator is not necessarily a sibling 
        /// since navigation may happen across parent photo albums, if the parameters are set appropriately .
        /// </summary>
        /// <param name="navigator">
        /// Navigator for which next navigator is required.
        /// </param>
        /// <param name="searchParent">
        /// True if the search includes parents - then the ancestors are searched after siblings, i.e. if no siblings remain.
        /// </param>
        /// <returns>
        /// The Navigator found in accordance with the search parameters above, or null if none was found.
        /// </returns>
        protected virtual Navigator GetNextNavigator(Navigator navigator, bool searchParent)
        {
            Navigator nextNavigator = null;
            PhotoNavigator photoNavigator = navigator as PhotoNavigator;
            if (photoNavigator != null)
            {
                nextNavigator = this.GetNextSiblingPhotoNavigator(photoNavigator);
                if ((nextNavigator == null) && searchParent)
                {
                    PhotoAlbumNavigator parent = photoNavigator.GetParent() as PhotoAlbumNavigator;
                    if (parent != null)
                    {
                        // Further navigation up/down the tree is possible. If parent is the PhotoGallery navigator, no further navigation is possible
                        nextNavigator = this.GetNextPhotoAlbumNavigator(parent);
                    }
                }
            }
            else
            {
                // There's no active Photo, only an active photo album, so go to it's first photo
                PhotoAlbumNavigator photoAlbumNavigator = navigator as PhotoAlbumNavigator;
                if (photoAlbumNavigator != null)
                {
                    nextNavigator = this.GetFirstChildPhotoNavigator(photoAlbumNavigator);
                    if ((nextNavigator == null))
                    {
                        nextNavigator = this.GetNextPhotoAlbumNavigator(photoAlbumNavigator);
                    }
                }
            }

            return nextNavigator;
        }

        /// <summary>
        /// Gets the previous Navigator for a Navigator, whether PhotoAlbum or Photo. This is a deep-pad navigation scenario to enable
        /// navigating through all content by just hitting the Next/Previous key. The previous navigator is not necessarily a sibling 
        /// since navigation may happen across parent photo albums, if the parameters are set appropriately .
        /// </summary>
        /// <param name="navigator">
        /// Navigator for which previous navigator is required.
        /// </param>
        /// <param name="searchParent">
        /// True if the search includes parents - then the ancestors are searched after siblings, i.e. if no siblings remain.
        /// </param>
        /// <returns>
        /// The Navigator found in accordance with the search parameters above, or null if none was found.
        /// </returns>
        protected virtual Navigator GetPreviousNavigator(Navigator navigator, bool searchParent)
        {
            Navigator previousNavigator = null;
            PhotoNavigator photoNavigator = navigator as PhotoNavigator;
            if (photoNavigator != null)
            {
                previousNavigator = this.GetPreviousSiblingPhotoNavigator(photoNavigator);
                if ((previousNavigator == null))
                {
                    Navigator parent = photoNavigator.GetParent();
                    if (parent is PhotoAlbumNavigator)
                    {
                        previousNavigator = parent;
                    }
                }
            }
            else
            {
                PhotoAlbumNavigator photoAlbumNavigator = navigator as PhotoAlbumNavigator;
                if (photoAlbumNavigator != null)
                {
                    PhotoAlbumNavigator previousPhotoAlbumNavigator = this.GetPreviousPhotoAlbumNavigator(photoAlbumNavigator);
                    previousNavigator = this.GetLastChildPhotoNavigator(previousPhotoAlbumNavigator);
                    if (previousNavigator == null)
                    {
                        previousNavigator = previousPhotoAlbumNavigator;
                    }
                }
            }

            return previousNavigator;
        }

        /// <summary>
        /// Get navigator to first main photo album in the current PhotoGallery on ViewManager. In ScePhoto, this is the default home page.
        /// </summary>
        /// <returns>The first PhotoAlbum in the gallery.</returns>
        protected virtual PhotoAlbumNavigator GetFirstPhotoAlbumNavigator()
        {
            return ViewManager.MasterNavigator.GetFirstPhotoAlbumNavigator();
        }

        /// <summary>
        /// Get navigator to the last main photo album on the currently selected PhotoGallery in ViewManager. In ScePhoto, this is.
        /// the default "End" page.
        /// </summary>
        /// <returns>The last PhotoAlbum in the gallery.</returns>
        protected virtual PhotoAlbumNavigator GetLastPhotoAlbumNavigator()
        {
            return ViewManager.MasterNavigator.GetLastPhotoAlbumNavigator();
        }

        /// <summary>
        /// Extracts a PhotoAlbumNavigator for the given photo album from the given navigator context.
        /// </summary>
        /// <param name="photoAlbum">The desired PhotoAlbum.</param>
        /// <param name="context">The current navigation context.</param>
        /// <returns>The PhotoAlbumNavigator relative to the current album.</returns>
        protected virtual PhotoAlbumNavigator GetPhotoAlbumNavigatorRelative(PhotoAlbum photoAlbum, Navigator context)
        {
            PhotoAlbumNavigator navigator = null;
            if (photoAlbum != null && context != null)
            {
                // Navigators only deal with escaped paths. Make sure photo album guid is escaped
                navigator = context.GetChildNavigatorFromPath(Uri.EscapeDataString(photoAlbum.Guid)) as PhotoAlbumNavigator;
            }

            return navigator;
        }

        /// <summary>
        /// Extracts a PhotoNavigator for the given photo from the given navigator context.
        /// </summary>
        /// <param name="photo">The desired Photo.</param>
        /// <param name="context">The current navigation context.</param>
        /// <returns>The PhotoNavigator relative to the current photo.</returns>
        protected virtual PhotoNavigator GetPhotoNavigatorRelative(Photo photo, Navigator context)
        {
            PhotoNavigator navigator = null;
            if (photo != null && context != null)
            {
                navigator = context.GetChildNavigatorFromPath(Uri.EscapeDataString(photo.Guid)) as PhotoNavigator;
            }

            return navigator;
        }
    }

    /// <summary>
    /// NavigationCommand that navigates to the next photo album from the one currently displayed. This command will navigate to
    /// subphoto albums if NestPhotoAlbumNavigation is selected on ViewManager.
    /// </summary>
    public class NextPhotoAlbumCommand : NavigationCommand
    {
        /// <summary>
        /// Type initializer for NextPhotoAlbumCommand.
        /// </summary>
        /// <param name="viewManager">
        /// ViewManager associated with ViewCommand subclasses.
        /// </param>
        public NextPhotoAlbumCommand(ViewManager viewManager) : base(viewManager) 
        { 
        }

        /// <summary>
        /// CanExecute logic for ViewCommand that can be overridden by derived classes.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for this command.
        /// </param>
        /// <returns>True when the next photo album is not empty.</returns>
        protected override bool CanExecuteInternal(object parameter)
        {
            bool foundNextAlbumOrGallery = false;
            if (ViewManager.CurrentNavigator is PhotoAlbumNavigator)
            {
                PhotoAlbumNavigator nextPhotoAlbumNavigator = GetNextPhotoAlbumNavigator(ViewManager.CurrentNavigator as PhotoAlbumNavigator);
                PhotoGallery nextGallery = null;
                if (nextPhotoAlbumNavigator == null)
                {
                    nextGallery = this.GetNextPhotoGallery();
                }

                foundNextAlbumOrGallery = (nextGallery != null || nextPhotoAlbumNavigator != null);
            }

            return foundNextAlbumOrGallery;
        }

        /// <summary>
        /// Navigation-specific logic that can be overridden by subclasses of NavigationCommand to provide custom navigation behavior.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for NavigationCommand.
        /// </param>
        protected override void PerformNavigate(object parameter)
        {
            // Navigation mode is Next
            if (ViewManager.CurrentNavigator is PhotoAlbumNavigator)
            {
                PhotoAlbumNavigator nextPhotoAlbum = GetNextPhotoAlbumNavigator(ViewManager.CurrentNavigator as PhotoAlbumNavigator);
                if (nextPhotoAlbum != null)
                {
                    ViewManager.NavigateByCommand(nextPhotoAlbum, ScePhotoNavigationMode.Next);
                }
                else
                {
                    PhotoGallery nextGallery = this.GetNextPhotoGallery();
                    if (nextGallery != null)
                    {
                        ViewManager.SwitchToPhotoGallery(nextGallery, false);
                    }
                }
            }
        }

        /// <summary>
        /// If no next sibling album can be found, locates the next gallery from the current one
        /// </summary>
        /// <returns>Next gallery</returns>
        private PhotoGallery GetNextPhotoGallery()
        {
            PhotoGallery nextGallery = null;
            PhotoGallery gallery = ViewManager.PhotoGallery;
            if (ViewManager.PhotoGalleries != null)
            {
                int galleryIndex = ViewManager.PhotoGalleries.IndexOf(gallery);
                if (galleryIndex < ViewManager.PhotoGalleries.Count - 1)
                {
                    nextGallery = ViewManager.PhotoGalleries[galleryIndex + 1];
                }
            }

            return nextGallery;
        }
    }

    /// <summary>
    /// NavigationCommand that navigates to the previous photo album from the one currently displayed.
    /// </summary>
    public class PreviousPhotoAlbumCommand : NavigationCommand
    {
        /// <summary>
        /// Type initializer for PreviousPhotoAlbumCommand.
        /// </summary>
        /// <param name="viewManager">
        /// ViewManager associated with ViewCommand subclasses.
        /// </param>
        public PreviousPhotoAlbumCommand(ViewManager viewManager) : base(viewManager) 
        { 
        }

        /// <summary>
        /// CanExecute logic for ViewCommand that can be overridden by derived classes.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for this command.
        /// </param>
        /// <returns>True when the previous photo album exists.</returns>
        protected override bool CanExecuteInternal(object parameter)
        {
            bool foundPreviousAlbumOrGallery = false;
            if (ViewManager.CurrentNavigator is PhotoAlbumNavigator)
            {
                PhotoAlbumNavigator previousPhotoAlbum = GetPreviousPhotoAlbumNavigator(ViewManager.CurrentNavigator as PhotoAlbumNavigator);
                PhotoGallery previousGallery = null;
                if (previousPhotoAlbum == null)
                {
                    previousGallery = this.GetPreviousPhotoGallery();
                }

                foundPreviousAlbumOrGallery = (previousGallery != null || previousPhotoAlbum != null);
            }

            return foundPreviousAlbumOrGallery;
        }

        /// <summary>
        /// Navigation-specific logic that can be overridden by subclasses of NavigationCommand to provide custom navigation behavior.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for NavigationCommand.
        /// </param>
        protected override void PerformNavigate(object parameter)
        {
            if (ViewManager.CurrentNavigator is PhotoAlbumNavigator)
            {
                PhotoAlbumNavigator previousPhotoAlbum = GetPreviousPhotoAlbumNavigator(ViewManager.CurrentNavigator as PhotoAlbumNavigator);
                if (previousPhotoAlbum != null)
                {
                    ViewManager.NavigateByCommand(previousPhotoAlbum, ScePhotoNavigationMode.Previous);
                }
                else
                {
                    PhotoGallery previousGallery = this.GetPreviousPhotoGallery();
                    if (previousGallery != null)
                    {
                        ViewManager.SwitchToPhotoGallery(previousGallery, true);
                    }
                }
            }
        }
        
        /// <summary>
        /// If no previous sibling album can be found, locates the previous gallery from the current one
        /// </summary>
        /// <returns>Previous gallery</returns>
        private PhotoGallery GetPreviousPhotoGallery()
        {
            PhotoGallery previousGallery = null;
            PhotoGallery gallery = ViewManager.PhotoGallery;
            if (ViewManager.PhotoGalleries != null)
            {
                int galleryIndex = ViewManager.PhotoGalleries.IndexOf(gallery);
                if (galleryIndex > 0)
                {
                    previousGallery = ViewManager.PhotoGalleries[galleryIndex - 1];
                }
            }

            return previousGallery;
        }
    }

    /// <summary>
    /// NavigationCommand that navigates to the next photo from the one currently displayed. This command navigates
    /// across photo albums if NestPhotoNavigation is set on ViewManager, i.e. if there are no more photos to navigate to in
    /// the current photo album, it will navigate to the next photo album.
    /// </summary>
    public class NextPhotoCommand : NavigationCommand
    {
        /// <summary>
        /// Type initializer for NextPhotoCommand.
        /// </summary>
        /// <param name="viewManager">
        /// ViewManager associated with ViewCommand subclasses.
        /// </param>
        public NextPhotoCommand(ViewManager viewManager) : base(viewManager) 
        { 
        }

        /// <summary>
        /// CanExecute logic for ViewCommand that can be overridden by derived classes.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for this command.
        /// </param>
        /// <returns>True when the next photo exists.</returns>
        protected override bool CanExecuteInternal(object parameter)
        {
            Navigator nextItem = GetNextNavigator(ViewManager.CurrentNavigator, true);
            return (nextItem == null) ? false : true;
        }

        /// <summary>
        /// Navigation-specific logic that can be overridden by subclasses of NavigationCommand to provide custom navigation behavior.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for NavigationCommand.
        /// </param>
        protected override void PerformNavigate(object parameter)
        {
            Navigator nextItem = GetNextNavigator(ViewManager.CurrentNavigator, true);
            ViewManager.NavigateByCommand(nextItem, ScePhotoNavigationMode.Next);
        }
    }

    /// <summary>
    /// NavigationCommand that navigates to the previous photo from the one currently displayed. 
    /// </summary>
    public class PreviousPhotoCommand : NavigationCommand
    {
        /// <summary>
        /// Type initializer for PreviousPhotoCommand.
        /// </summary>
        /// <param name="viewManager">
        /// ViewManager associated with ViewCommand subclasses.
        /// </param>
        public PreviousPhotoCommand(ViewManager viewManager) : base(viewManager) 
        {
        }

        /// <summary>
        /// CanExecute logic for ViewCommand that can be overridden by derived classes.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for this command.
        /// </param>
        /// <returns>True when the previous photo exists.</returns>
        protected override bool CanExecuteInternal(object parameter)
        {
            Navigator previousItem = GetPreviousNavigator(ViewManager.CurrentNavigator, true);
            return (previousItem == null) ? false : true;
        }

        /// <summary>
        /// Navigation-specific logic that can be overridden by subclasses of NavigationCommand to provide custom navigation behavior.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for NavigationCommand.
        /// </param>
        protected override void PerformNavigate(object parameter)
        {
            Navigator previousItem = GetPreviousNavigator(ViewManager.CurrentNavigator, true);
            ViewManager.NavigateByCommand(previousItem, ScePhotoNavigationMode.Previous);
        }
    }

    /// <summary>
    /// Navigates to the parent of the current content - for a Photo, it's containing PhotoAlbum if it exists, for a PhotoAlbum,
    /// it's parent PhotoAlbum if it is a sub section.
    /// </summary>
    public class NavigateToParentPhotoAlbumCommand : NavigationCommand
    {
        /// <summary>
        /// Type initializer for NavigateToParentPhotoAlbumCommand.
        /// </summary>
        /// <param name="viewManager">
        /// ViewManager associated with ViewCommand subclasses.
        /// </param>
        public NavigateToParentPhotoAlbumCommand(ViewManager viewManager) : base(viewManager) 
        {
        }

        /// <summary>
        /// CanExecute logic for ViewCommand that can be overridden by derived classes.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for this command.
        /// </param>
        /// <returns>True when the item's parent exists.</returns>
        protected override bool CanExecuteInternal(object parameter)
        {
            Navigator parent = null;
            NavigatableObjectNavigator itemNavigator = ViewManager.CurrentNavigator as NavigatableObjectNavigator;
            if (itemNavigator != null)
            {
                parent = itemNavigator.GetParent();
            }

            return (parent == null) ? false : true;
        }

        /// <summary>
        /// Navigation-specific logic that can be overridden by subclasses of NavigationCommand to provide custom navigation behavior.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for NavigationCommand.
        /// </param>
        protected override void PerformNavigate(object parameter)
        {
            Navigator parent = null;
            NavigatableObjectNavigator itemNavigator = ViewManager.CurrentNavigator as NavigatableObjectNavigator;
            if (itemNavigator != null)
            {
                parent = itemNavigator.GetParent();
            }

            ViewManager.NavigateByCommand(parent, ScePhotoNavigationMode.Normal);
        }
    }

    /// <summary>
    /// Navigates to the next sibling of the current content, whether PhotoAlbum or Photo.
    /// </summary>
    public class NextSiblingCommand : NavigationCommand
    {
        /// <summary>
        /// Type initializer for NextSiblingCommand.
        /// </summary>
        /// <param name="viewManager">
        /// ViewManager associated with ViewCommand subclasses.
        /// </param>
        public NextSiblingCommand(ViewManager viewManager) : base(viewManager) 
        {
        }

        /// <summary>
        /// CanExecute logic for ViewCommand that can be overridden by derived classes.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for this command.
        /// </param>
        /// <returns>True when the next sibling exists.</returns>
        protected override bool CanExecuteInternal(object parameter)
        {
            Navigator nextSibling = null;
            if (ViewManager.CurrentNavigator != null)
            {
                nextSibling = ViewManager.CurrentNavigator.GetNextSibling();
            }

            return (nextSibling == null) ? false : true;
        }

        /// <summary>
        /// Navigation-specific logic that can be overridden by subclasses of NavigationCommand to provide custom navigation behavior.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for NavigationCommand.
        /// </param>
        protected override void PerformNavigate(object parameter)
        {
            Navigator nextSibling = null;
            if (ViewManager.CurrentNavigator != null)
            {
                nextSibling = ViewManager.CurrentNavigator.GetNextSibling();
            }

            ViewManager.NavigateByCommand(nextSibling, ScePhotoNavigationMode.Next);
        }
    }

    /// <summary>
    /// Navigates to the previous sibling of the current content, whether PhotoAlbum or Photo.
    /// </summary>
    public class PreviousSiblingCommand : NavigationCommand
    {
        /// <summary>
        /// Type initializer for PreviousSiblingCommand.
        /// </summary>
        /// <param name="viewManager">
        /// ViewManager associated with ViewCommand subclasses.
        /// </param>
        public PreviousSiblingCommand(ViewManager viewManager) : base(viewManager) 
        {
        }

        /// <summary>
        /// CanExecute logic for ViewCommand that can be overridden by derived classes.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for this command.
        /// </param>
        /// <returns>True when the previous sibling exists.</returns>
        protected override bool CanExecuteInternal(object parameter)
        {
            Navigator previousSibling = null;
            if (ViewManager.CurrentNavigator != null)
            {
                previousSibling = ViewManager.CurrentNavigator.GetPreviousSibling();
            }

            return (previousSibling == null) ? false : true;
        }

        /// <summary>
        /// Navigation-specific logic that can be overridden by subclasses of NavigationCommand to provide custom navigation behavior.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for NavigationCommand.
        /// </param>
        protected override void PerformNavigate(object parameter)
        {
            Navigator previousSibling = null;
            if (ViewManager.CurrentNavigator != null)
            {
                previousSibling = ViewManager.CurrentNavigator.GetPreviousSibling();
            }

            ViewManager.NavigateByCommand(previousSibling, ScePhotoNavigationMode.Previous);
        }
    }

    /// <summary>
    /// Given a <see cref="PhotoAlbumNavigator"/>, navigates to the PhotoAlbum represented by that navigator.
    /// </summary>
    public class NavigateToPhotoAlbumCommand : NavigationCommand
    {
        /// <summary>
        /// Type initializer for NavigateToPhotoAlbumCommand.
        /// </summary>
        /// <param name="viewManager">
        /// ViewManager associated with ViewCommand subclasses.
        /// </param>
        public NavigateToPhotoAlbumCommand(ViewManager viewManager) : base(viewManager) 
        {
        }

        /// <summary>
        /// Navigation-specific logic that can be overridden by subclasses of NavigationCommand to provide custom navigation behavior.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for NavigationCommand.
        /// </param>
        protected override void PerformNavigate(object parameter)
        {
            PhotoAlbumNavigator photoAlbumNavigator = parameter as PhotoAlbumNavigator;
            if (photoAlbumNavigator != null)
            {
                ViewManager.NavigateByCommand(photoAlbumNavigator, ScePhotoNavigationMode.Normal);
            }
            else
            {
                PhotoAlbum photoAlbum = parameter as PhotoAlbum;
                if (photoAlbum != null)
                {
                    photoAlbumNavigator = GetPhotoAlbumNavigatorRelative(photoAlbum, ViewManager.CurrentNavigator);
                    if (photoAlbumNavigator == null)
                    {
                        foreach (Navigator album in ViewManager.MasterNavigator.GetTopLevelNavigators())
                        {
                            PhotoAlbumNavigator albumNavigator = album as PhotoAlbumNavigator;
                            if (albumNavigator != null && albumNavigator.Content == photoAlbum)
                            {
                                photoAlbumNavigator = albumNavigator;
                                break;
                            }
                        }
                    }

                    ViewManager.NavigateByCommand(photoAlbumNavigator, ScePhotoNavigationMode.Normal);
                }
            }
        }
    }

    /// <summary>
    /// Given a <see cref="PhotoNavigator"/>, navigates to the Photo represented by that navigator.
    /// </summary>
    public class NavigateToPhotoCommand : NavigationCommand
    {
        /// <summary>
        /// Type initializer for NavigateToPhotoCommand.
        /// </summary>
        /// <param name="viewManager">
        /// ViewManager associated with ViewCommand subclasses.
        /// </param>
        public NavigateToPhotoCommand(ViewManager viewManager) : base(viewManager) 
        { 
        }

        /// <summary>
        /// Navigation-specific logic that can be overridden by subclasses of NavigationCommand to provide custom navigation behavior.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for NavigationCommand.
        /// </param>
        protected override void PerformNavigate(object parameter)
        {
            PhotoNavigator photoNavigator = parameter as PhotoNavigator;
            if (photoNavigator != null)
            {
                ViewManager.NavigateByCommand(photoNavigator, ScePhotoNavigationMode.Normal);
            }
            else
            {
                Photo photo = parameter as Photo;
                if (photo != null)
                {
                    // If we've got a HomePhotoAlbumNavigator, then we don't have this photo's parent!
                    if (ViewManager.CurrentNavigator.GetType() == typeof(HomePhotoAlbumNavigator))
                    {
                        foreach (Navigator photoAlbumNavigator in ViewManager.MasterNavigator.GetTopLevelNavigators())
                        {
                            PhotoAlbum photoAlbum = photoAlbumNavigator.Content as PhotoAlbum;
                            if (photoAlbum.Photos.Contains(photo))
                            {
                                photoNavigator = GetPhotoNavigatorRelative(photo, photoAlbumNavigator);                               
                                break;
                            }
                        }
                    }
                    else
                    {
                        photoNavigator = GetPhotoNavigatorRelative(photo, ViewManager.CurrentNavigator);
                    }

                    if (photoNavigator == null)
                    {
                        photoNavigator = GetPhotoNavigatorRelative(photo, GetParentPhotoAlbumNavigator(ViewManager.CurrentNavigator as PhotoNavigator));
                    }

                    ViewManager.NavigateByCommand(photoNavigator, ScePhotoNavigationMode.Normal);
                }
            }
        }
    }

    /// <summary>
    /// Given a <see cref="PhotoSlideShowNavigator"/>, navigates to the PhotoSlideShow represented by that navigator.
    /// If not given a <see cref="PhotoSlideShowNavigator"/>, it will navigate to a slideshow based on the current Album. 
    /// </summary>
    public class NavigateToPhotoSlideShowCommand : NavigationCommand
    {
        /// <summary>
        /// Type initializer for NavigateToPhotoCommand.
        /// </summary>
        /// <param name="viewManager">
        /// ViewManager associated with ViewCommand subclasses.
        /// </param>
        public NavigateToPhotoSlideShowCommand(ViewManager viewManager)
            : base(viewManager)
        {
        }

        /// <summary>
        /// Navigation-specific logic that can be overridden by subclasses of NavigationCommand to provide custom navigation behavior.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for NavigationCommand.
        /// </param>
        protected override void PerformNavigate(object parameter)
        {
            PhotoSlideShowNavigator photoSlideShowNavigator = parameter as PhotoSlideShowNavigator;
            PhotoSlideShow slideShow = null;
            PhotoAlbumNavigator albumNavigator = null;

            if (photoSlideShowNavigator == null)
            {
                albumNavigator = parameter as PhotoAlbumNavigator;
                if (albumNavigator != null)
                {
                    slideShow = new PhotoSlideShow(albumNavigator);
                    photoSlideShowNavigator = new PhotoSlideShowNavigator(slideShow);
                }
            }

            if (photoSlideShowNavigator == null)
            {
                albumNavigator = ServiceProvider.ViewManager.CurrentNavigator as PhotoAlbumNavigator;
                if (albumNavigator != null && albumNavigator.GetPhotos().Count > 0)
                {
                    slideShow = new PhotoSlideShow(albumNavigator);
                    photoSlideShowNavigator = new PhotoSlideShowNavigator(slideShow);
                }

                if (albumNavigator == null)
                {
                    PhotoNavigator photoNavigator = ServiceProvider.ViewManager.CurrentNavigator as PhotoNavigator;
                    if (photoNavigator != null)
                    {
                        slideShow = new PhotoSlideShow(photoNavigator);
                        photoSlideShowNavigator = new PhotoSlideShowNavigator(slideShow);
                    }
                }
            }

            if (photoSlideShowNavigator != null)
            {
                ViewManager.NavigateByCommand(photoSlideShowNavigator, ScePhotoNavigationMode.Normal);
            }
        }
    }

    /// <summary>
    /// Navigates to the first top-level photo album in the current PhotoGallery on display.
    /// </summary>
    public class NavigateToFirstPhotoAlbumCommand : NavigationCommand
    {
        /// <summary>
        /// Type initializer for NavigateToFirstPhotoAlbumCommand.
        /// </summary>
        /// <param name="viewManager">
        /// ViewManager associated with ViewCommand subclasses.
        /// </param>
        public NavigateToFirstPhotoAlbumCommand(ViewManager viewManager) : base(viewManager) 
        { 
        }

        /// <summary>
        /// Navigation-specific logic that can be overridden by subclasses of NavigationCommand to provide custom navigation behavior.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for NavigationCommand.
        /// </param>
        protected override void PerformNavigate(object parameter)
        {
            PhotoAlbumNavigator firstPhotoAlbum = GetFirstPhotoAlbumNavigator();
            if (firstPhotoAlbum != null)
            {
                ViewManager.NavigateByCommand(firstPhotoAlbum, ScePhotoNavigationMode.Normal);
            }
        }
    }

    /// <summary>
    /// Navigates to the last top-level photo album in the current PhotoGallery on display.
    /// </summary>
    public class NavigateToLastPhotoAlbumCommand : NavigationCommand
    {
        /// <summary>
        /// Type initializer for NavigateToLastPhotoAlbumCommand.
        /// </summary>
        /// <param name="viewManager">
        /// ViewManager associated with ViewCommand subclasses.
        /// </param>
        public NavigateToLastPhotoAlbumCommand(ViewManager viewManager) : base(viewManager) 
        { 
        }

        /// <summary>
        /// Navigation-specific logic that can be overridden by subclasses of NavigationCommand to provide custom navigation behavior.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for NavigationCommand.
        /// </param>
        protected override void PerformNavigate(object parameter)
        {
            PhotoAlbumNavigator lastPhotoAlbum = GetLastPhotoAlbumNavigator();
            if (lastPhotoAlbum != null)
            {
                ViewManager.NavigateByCommand(lastPhotoAlbum, ScePhotoNavigationMode.Normal);
            }
        }
    }

    /// <summary>
    /// Navigates to the first photo album of the first photo gallery among all photo galleries on ViewManager.
    /// </summary>
    public class NavigateToFirstPhotoGalleryCommand : NavigationCommand
    {
        /// <summary>
        /// Type initializer for NavigateToFirstPhotoGalleryCommand.
        /// </summary>
        /// <param name="viewManager">
        /// ViewManager associated with ViewCommand subclasses.
        /// </param>
        public NavigateToFirstPhotoGalleryCommand(ViewManager viewManager) : base(viewManager) 
        { 
        }

        /// <summary>
        /// CanExecute logic for ViewCommand that can be overridden by derived classes.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for this command.
        /// </param>
        /// <returns>True as long as at least one gallery is present.</returns>
        protected override bool CanExecuteInternal(object parameter)
        {
            return (ViewManager.PhotoGalleries != null && ViewManager.PhotoGalleries.Count > 0);
        }

        /// <summary>
        /// Navigation-specific logic that can be overridden by subclasses of NavigationCommand to provide custom navigation behavior.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for NavigationCommand.
        /// </param>
        protected override void PerformNavigate(object parameter)
        {
            if (ViewManager.PhotoGalleries != null && ViewManager.PhotoGalleries.Count > 0)
            {
                PhotoGallery photoGallery = ViewManager.PhotoGalleries[0];
                ViewManager.SwitchToPhotoGallery(photoGallery, false);
            }
        }
    }

    /// <summary>
    /// Given search text, queries for a <see cref="Navigator"/> for a Search photo album for that text, and navigates to the Search photo album.
    /// This may result in the creation of a new Search photo album and Navigator for this search term, a consequence of navigation.
    /// </summary>
    public class SearchCommand : NavigationCommand
    {
        /// <summary>
        /// Type initializer for SearchCommand.
        /// </summary>
        /// <param name="viewManager">
        /// ViewManager associated with ViewCommand subclasses.
        /// </param>
        public SearchCommand(ViewManager viewManager) : base(viewManager) 
        { 
        }

        /// <summary>
        /// Navigation-specific logic that can be overridden by subclasses of NavigationCommand to provide custom navigation behavior.
        /// </summary>
        /// <param name="parameter">
        /// Execution parameter for NavigationCommand.
        /// </param>
        protected override void PerformNavigate(object parameter)
        {
            string searchText = parameter as string;
            if (searchText != null)
            {
                // Generate search photo album and get navigator
                SearchPhotoAlbum searchPhotoAlbum = ViewManager.GenerateSearchPhotoAlbum(searchText);
                PhotoAlbumNavigator searchNavigator = ViewManager.MasterNavigator.GetSearchNavigator(searchPhotoAlbum);
                ViewManager.NavigateByCommand(searchNavigator, ScePhotoNavigationMode.Normal);
            }
        }
    }
}
