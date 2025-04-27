//-----------------------------------------------------------------------
// <copyright file="MasterNavigator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Handles navigation across galleries of MasterFeedContent.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.View
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.ComponentModel;
    using System.Collections.ObjectModel;
    using ScePhoto.Data;

    /// <summary>
    /// Handles navigation across galleries of MasterFeedContent.
    /// </summary>
    public class MasterNavigator : NavigatorBase
    {
        #region Private Fields
        
        /// <summary>
        /// Navigator for the PhotoGallery currently selected in the view/UI.
        /// </summary>
        private PhotoGalleryNavigator currentFeedNavigator;

        /// <summary>
        /// ViewManager with which MasterNavigator is associated.
        /// </summary>
        private ViewManager viewManager;

        /// <summary>
        /// The search navigator currently in use.
        /// </summary>
        private SearchNavigator searchNavigator;

        /// <summary>
        /// A dictionary of Guids for all navigators currently active under MasterNavigator.
        /// </summary>
        private NavigationGuidStore guidStore;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor - MasterNavigator is constructed with a ViewManager and listens to its PropertyChanged events to monitor
        /// changes in the current PhotoGallery. If overriding classes wish to skip this logic, they may call the parameterless
        /// protected constructor.
        /// </summary>
        /// <param name="viewManager">The ViewManager to be monitored.</param>
        public MasterNavigator(ViewManager viewManager)
        {
            if (viewManager == null)
            {
                throw new ArgumentNullException("viewManager");
            }

            this.viewManager = viewManager;
            this.viewManager.PropertyChanged += this.OnViewManagerPropertyChanged;
            this.guidStore = new NavigationGuidStore();
        }

        /// <summary>
        /// Parameterless constructors in case overrides wish to bypass constructor initialization with ViewManager.
        /// </summary>
        protected MasterNavigator()
        {
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the ViewManager with which MasterNavigator is associated. MasterNavigator may need access to APIs or information
        /// available on ViewManager, such as in retrieving navigators from journaled paths. If a journal path refers to
        /// data in another PhotoGallery photo gallery, retrieving this object must produce a change in the currently selected 
        /// photo gallery in ViweManager, so access to ViewManager APIs is necessary.
        /// </summary>
        protected virtual ViewManager ViewManager
        {
            get { return this.viewManager; }
        }

        /// <summary>
        /// Gets the Navigator for the PhotoGallery currently selected in the view/UI.
        /// </summary>
        protected virtual PhotoGalleryNavigator CurrentPhotoGalleryNavigator
        {
            get { return this.currentFeedNavigator; }
        }

        /// <summary>
        /// Gets the Navigation GuidStore maintains a dictionary of Guids for all navigators currently active under MasterNavigator
        /// for quick lookup of navigators by guid.
        /// </summary>
        protected virtual NavigationGuidStore GuidStore
        {
            get { return this.guidStore; }
        }

        /// <summary>
        /// Gets the search navigator currently in use, which can be refreshed for a new search query. This
        /// navigator is either created newly or refreshed by the GetSearchNavigator API.
        /// </summary>
        protected virtual SearchNavigator CurrentSearchNavigator
        {
            get { return this.searchNavigator; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get top level navigators, which represent navigation items accessible from the main page and NavPanel.
        /// </summary>
        /// <returns>A ReadOnlyCollection of top level navigators.</returns>
        public virtual ReadOnlyCollection<Navigator> GetTopLevelNavigators()
        {
            // Top level navigators is not cached because these navigators changed depending on the data feed content currently
            // in view - i.e. if the user selects a different gallery this collection will need to be reset.
            // Rather than resetting in multiple locations it is better to
            // compose this collection each time
            List<Navigator> topLevelNavigators = new List<Navigator>();
            this.AddDataFeedNavigators(topLevelNavigators);
            return new ReadOnlyCollection<Navigator>(topLevelNavigators);
        }

        /// <summary>
        /// Gets a list of all of the album navigators in the application, used for searches.
        /// </summary>
        /// <returns>A ReadOnlyCollection of album navigators.</returns>
        public ReadOnlyCollection<Navigator> GetAllAlbumNavigators()
        {
            List<Navigator> albumNavigators = new List<Navigator>();

            foreach (PhotoGallery gallery in ViewManager.PhotoGalleries)
            {
                PhotoGalleryNavigator galleryNavigator = new PhotoGalleryNavigator(gallery);
                IList<PhotoAlbumNavigator> photoAlbums = galleryNavigator.GetPhotoAlbums();
                foreach (PhotoAlbumNavigator navigator in photoAlbums)
                {
                    albumNavigators.Add(navigator);
                }
            }

            return new ReadOnlyCollection<Navigator>(albumNavigators);
        }

        /// <summary>
        /// Retrieve a child navigator from the specified path, if there is a match.
        /// </summary>
        /// <param name="parentPath">The path to find the navigator for.</param>
        /// <returns>The child navigator for the specified path, or null if no navigator exists.</returns>
        public override Navigator GetChildNavigatorFromPath(string parentPath)
        {
            Navigator navigator = null;
            if (!String.IsNullOrEmpty(parentPath))
            {
                // Search custom navigators
                navigator = this.FindCustomNavigator(parentPath);
                if (navigator == null)
                {
                    // No match. Try to match feed navigators
                    navigator = this.FindFeedNavigator(parentPath);
                }
            }

            return navigator;
        }

        /// <summary>
        /// Retrieves a child navigator for the specified guid.
        /// </summary>
        /// <param name="guid">The child element's guid.</param>
        /// <returns>The child navigator for the specified guid, null if no child exists.</returns>
        public Navigator GetChildNavigatorFromGuid(string guid)
        {
            guid = Uri.UnescapeDataString(guid);
            if (!String.IsNullOrEmpty(guid))
            {
                if (ViewManager.PhotoGallery == null || guid != ViewManager.PhotoGallery.Guid)
                {
                    if (ViewManager.SelectPhotoGalleryByGuid(guid))
                    {
                        if (this.CurrentPhotoGalleryNavigator != null)
                        {
                            return this.CurrentPhotoGalleryNavigator;
                        }
                    }
                    else
                    {
                        // It's not a gallery reference; go digging and try to find it elsewhere.
                        foreach (PhotoGallery gallery in ViewManager.PhotoGalleries)
                        {
                            foreach (PhotoAlbum album in gallery.PhotoAlbums)
                            {
                                if (album.Guid == guid)
                                {
                                    return this.GetChildNavigatorFromPath(gallery.Guid + this.ChildSeparator + album.Guid);
                                }
                                else
                                {
                                    foreach (Photo photo in album.Photos)
                                    {
                                        if (photo.Guid == guid)
                                        {
                                            return this.GetChildNavigatorFromPath(gallery.Guid + this.ChildSeparator + album.Guid + this.ChildSeparator + photo.Guid);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get the navigator for the "home page". In this implementation, this is the first photo album of current data feed content navigator.
        /// </summary>
        /// <returns>The AlbumNavigator for the home page.</returns>
        public virtual PhotoAlbumNavigator GetFirstPhotoAlbumNavigator()
        {
            if (this.currentFeedNavigator != null)
            {
                return this.currentFeedNavigator.GetFirstPhotoAlbum();
            }

            return null;
        }

        /// <summary>
        /// Get the navigator for the "end page". In this implementation, this is the last photo album of current data feed content navigator.
        /// </summary>
        /// <returns>The AlbumNavigator for the end page.</returns>
        public virtual PhotoAlbumNavigator GetLastPhotoAlbumNavigator()
        {
            if (this.currentFeedNavigator != null)
            {
                return this.currentFeedNavigator.GetLastPhotoAlbum();
            }

            return null;
        }

        /// <summary>
        /// Generates SearchNavigator given a SearchPhotoAlbum.
        /// </summary>
        /// <param name="searchPhotoAlbum">The search photo album to generate a navigator for.</param>
        /// <returns>The SearchNavigator for the provided search photo album.</returns>
        public virtual SearchNavigator GetSearchNavigator(SearchPhotoAlbum searchPhotoAlbum)
        {
            // Assume that all search navigators have a common search path, which is the case for standard starter kit search where search photo album
            // has a single guid
            SearchNavigator albumSearchNavigator = null;

            // Search navigator cannot be created with no photo album or no PhotoGallery, since we cannot retrieve it for journaled navigation
            // in such a case. This may change once we support multiple-photo gallery search
            if (searchPhotoAlbum != null)
            {
                if (this.CurrentSearchNavigator == null)
                {
                    this.searchNavigator = new SearchNavigator(searchPhotoAlbum);
                }
                else
                {
                    this.CurrentSearchNavigator.RefreshSearchPhotoAlbum(searchPhotoAlbum);
                }

                if (!this.GuidStore.ContainsKey(this.CurrentSearchNavigator.Path))
                {
                    this.GuidStore.Add(this.CurrentSearchNavigator.Path, this.CurrentSearchNavigator);
                }
                else
                {
                    this.GuidStore[this.CurrentSearchNavigator.Path] = this.CurrentSearchNavigator;
                }

                albumSearchNavigator = this.CurrentSearchNavigator;
            }

            return albumSearchNavigator;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Find a navigator from the provided navigator path from the data feed navigators.
        /// </summary>
        /// <param name="path">The path to find a feed navigator for.</param>
        /// <returns>A Navigator for the provided path, null if not available.</returns>
        protected virtual Navigator FindFeedNavigator(string path)
        {
            Navigator navigator = null;
            string itemPath = String.Empty;
            string topLevelGuid = ExtractFirstChildPath(path, true, out itemPath);
            if (!String.IsNullOrEmpty(topLevelGuid))
            {
                if (ViewManager.PhotoGallery == null || topLevelGuid != ViewManager.PhotoGallery.Guid)
                {
                    if (ViewManager.SelectPhotoGalleryByGuid(topLevelGuid))
                    {
                        if (this.CurrentPhotoGalleryNavigator != null)
                        {
                            navigator = this.CurrentPhotoGalleryNavigator.GetChildNavigatorFromPath(itemPath);
                        }
                    }
                }
                else if (ViewManager.PhotoGallery.Guid == topLevelGuid)
                {
                    if (this.CurrentPhotoGalleryNavigator != null)
                    {
                        navigator = this.CurrentPhotoGalleryNavigator.GetChildNavigatorFromPath(itemPath);
                    }
                }
            }

            return navigator;
        }

        /// <summary>
        /// Looks for a navigator in the custom navigators collection (navigation guid store) that matches the path.
        /// </summary>
        /// <param name="path">The path to find a custom navigator for.</param>
        /// <returns>A Navigator for the provided path, null if not available.</returns>
        protected virtual Navigator FindCustomNavigator(string path)
        {
            Navigator navigator = null;
            string itemPath = String.Empty;
            string topLevelGuid = ExtractFirstChildPath(path, false, out itemPath);
            if (!String.IsNullOrEmpty(topLevelGuid))
            {
                // Check if guid is present in navigation guid store
                if (this.GuidStore.ContainsKey(topLevelGuid))
                {
                    navigator = this.GuidStore[topLevelGuid];
                    if (Object.ReferenceEquals(navigator, this.CurrentSearchNavigator))
                    {
                        navigator = this.GetSearchNavigatorFromPath(itemPath);
                    }
                    else
                    {
                        navigator = this.GetUserNavigatorFromPath(navigator, itemPath);
                    }
                }
                else if (PhotoSlideShowNavigator.SlideShowGuid == topLevelGuid)
                {
                    navigator = this.GetPhotoSlideShowNavigatorFromPath(itemPath);
                }
            }

            return navigator;
        }

        /// <summary>
        /// Dynamically generate a search navigator from the path supplied.
        /// </summary>
        /// <param name="path">The path to find a search navigator for.</param>
        /// <returns>A Navigator for the provided path, null if not available.</returns>
        protected virtual Navigator GetSearchNavigatorFromPath(string path)
        {
            Navigator pathSearchNavigator = null;
            SearchPhotoAlbum searchPhotoAlbum = ViewManager.GenerateSearchPhotoAlbum(this.CurrentSearchNavigator.SearchText);
            if (searchPhotoAlbum != null)
            {
                // Path may point to a photo in search, check for one
                this.CurrentSearchNavigator.RefreshSearchPhotoAlbum(searchPhotoAlbum);
                pathSearchNavigator = this.CurrentSearchNavigator;
                PhotoNavigator searchPhotoNavigator = pathSearchNavigator.GetChildNavigatorFromPath(path) as PhotoNavigator;
                if (searchPhotoNavigator != null)
                {
                    pathSearchNavigator = searchPhotoNavigator;
                }
            }

            return pathSearchNavigator;
        }

        /// <summary>
        /// Dynamically generate a PhotoSlideShowNavigator from the path supplied.
        /// </summary>
        /// <param name="path">The path to find a navigator for.</param>
        /// <returns>A Navigator for the provided path, null if not available.</returns>
        protected virtual Navigator GetPhotoSlideShowNavigatorFromPath(string path)
        {
            string albumPath;
            string galleryPath = ExtractFirstChildPath(path, false, out albumPath);
            PhotoAlbumNavigator navigator = null;
            if (ViewManager.SelectPhotoGalleryByGuid(galleryPath))
            {
                if (this.CurrentPhotoGalleryNavigator != null)
                {
                    navigator = this.CurrentPhotoGalleryNavigator.GetChildNavigatorFromPath(albumPath) as PhotoAlbumNavigator;
                }

                if (navigator != null)
                {
                    PhotoSlideShow photoSlideShow = new PhotoSlideShow(navigator);
                    PhotoSlideShowNavigator photoSlideShowNavigator = new PhotoSlideShowNavigator(photoSlideShow);
                    return photoSlideShowNavigator;
                }
            }
            
            return null;
        }

        /// <summary>
        /// In addition to the Search photo album, users may add their own navigators via
        /// ViewManager.UserNavigators. These are part of the navigation guid store. To extract nested navigators from
        /// such a navigator, GetChildNavigator from path on the navigator is called and if it returns a non-null value,
        /// that value is used. If the value is null, the top level navigator is returned.
        /// </summary>
        /// <param name="userNavigator">The parent user navigator.</param>
        /// <param name="itemPath">The path to search.</param>
        /// <returns>The user navigator for the provided path.</returns>
        protected virtual Navigator GetUserNavigatorFromPath(Navigator userNavigator, string itemPath)
        {
            Navigator navigator = userNavigator;
            if (userNavigator != null)
            {
                Navigator child = userNavigator.GetChildNavigatorFromPath(itemPath);
                if (child != null)
                {
                    navigator = child;
                }
            }

            return navigator;
        }

        /// <summary>
        /// Virtual handler for ViewManager's PropertyChanged event.
        /// </summary>
        /// <param name="e">Event Arguments describing the event.</param>
        protected virtual void OnViewManagerPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PhotoGallery")
            {
                this.OnPhotoGalleryChanged();
            }
        }

        /// <summary>
        /// Called when ViewManager's current photo gallery changes. Navigator must be regenerated for the new photo gallery.
        /// </summary>
        protected virtual void OnPhotoGalleryChanged()
        {
            // The master navigator may change the ViewManager's current photo gallery itself on navigating to a different
            // photo gallery via the journal. In this case, it should not regenerate the current navigator. Check that the
            // current navigator's content is different from ViewManager's current photo gallery before regenerating
            if (ViewManager.PhotoGallery == null)
            {
                this.currentFeedNavigator = null;
            }
            else
            {
                this.currentFeedNavigator = new PhotoGalleryNavigator(ViewManager.PhotoGallery);
            }
        }

        /// <summary>
        /// Adds top level navigators from the data feed content navigator to the top level navigators collection
        /// In the default implementation, these are navigators for top-level galleries in the PhotoGallery.
        /// </summary>
        /// <param name="topLevelNavigators">The top level navigator collection to add to.</param>
        protected virtual void AddDataFeedNavigators(IList<Navigator> topLevelNavigators)
        {
            if (this.CurrentPhotoGalleryNavigator != null)
            {
                IList<PhotoAlbumNavigator> photoAlbums = this.CurrentPhotoGalleryNavigator.GetPhotoAlbums();
                foreach (PhotoAlbumNavigator navigator in photoAlbums)
                {
                    topLevelNavigators.Add(navigator);
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Event handler for ViewManager's PropertyChanged event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments accompanying the event.</param>
        private void OnViewManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.OnViewManagerPropertyChanged(e);
        }

        #endregion
    }
}
