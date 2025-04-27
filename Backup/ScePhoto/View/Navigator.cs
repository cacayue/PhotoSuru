//-----------------------------------------------------------------------
// <copyright file="Navigator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Classes for all navigators.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.View
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using ScePhoto.Data;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;

    /// <summary>
    /// NavigationGuidStore class is used by master navigator to store top-level navigators that are not part of the data feed but need
    /// top-level lookup.
    /// </summary>
    public class NavigationGuidStore : Dictionary<string, Navigator>
    {
        #region Constructor

        /// <summary>
        /// Constructor for NavigationGuidStore.
        /// </summary>
        public NavigationGuidStore()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        #endregion
    }

    /// <summary>
    /// Base class for all navigators. Navigators are used to provide journaling support for ScePhoto applications. The navigation history
    /// of ScePhoto applications is persisted to the navigation journal as strings indicating the path of objects. Since ScePhoto data
    /// objects such as <see cref="PhotoAlbum"/> and <see cref="Photo"/> themselves do not provide serializable paths or navigation context,
    /// Navigator objects provide a navigation layer that wraps data objects and exposes information necessary for journaling, such as
    /// a path for the wrapped object that provides enough information to retrieve the object when access by the journal, and APIs to 
    /// retrieve a navigator, and it's wrapped content, from the journaled path.
    /// </summary>
    public abstract class NavigatorBase
    {
        #region Properties
        /// <summary>
        /// Gets the path separator.  When composing a path, the separator is used between parts added by different navigators.
        /// </summary>
        protected virtual string ChildSeparator
        {
            get { return "/"; }
        } 
        #endregion

        #region Methods
        /// <summary>
        /// Retrieves a navigator from the specified path. Used to walk the navigation tree when navigating by journal and a lookup from path is
        /// required.
        /// </summary>
        /// <param name="parentPath">The path to retrieve the navigator for.</param>
        /// <returns>Always returns null.</returns>
        public virtual Navigator GetChildNavigatorFromPath(string parentPath)
        {
            return null;
        } 

        /// <summary>
        /// Extract first child path, i.e. text before first separator.
        /// </summary>
        /// <remarks>
        /// If the "unEscape" parameter is set to true, calls Uri.UnescapedataString on the child path before returning it
        /// Does NOT call it on the remainder because the remainder string may be used for another lookup. 
        /// This is used by callers who are aware that the navigators they are querying escape path data strings. For example,
        /// when PhotoAlbumNavigator searches for children by guid, it knows that child guids are escaped before being added to the path
        /// and can request the unescaped guids.
        /// </remarks>
        /// <param name="path">The inital path.</param>
        /// <param name="unescape">When true, calls Uri.UnescapedataString on the child path before returning it.</param>
        /// <param name="remainder">The remainder of the path after extraction.</param>
        /// <returns>The text before the first separator.</returns>
        protected virtual string ExtractFirstChildPath(string path, bool unescape, out string remainder)
        {
            string childPath = path;
            remainder = String.Empty;
            if (!String.IsNullOrEmpty(path))
            {
                int separatorIndex = path.IndexOf(this.ChildSeparator, StringComparison.OrdinalIgnoreCase);
                if (separatorIndex >= 0 && separatorIndex < path.Length)
                {
                    // Separator was found, remainder of the path is passed to the matching child for further lookup
                    childPath = path.Substring(0, separatorIndex);
                    if (separatorIndex + 1 < path.Length)
                    {
                        remainder = path.Substring(separatorIndex + 1, path.Length - (separatorIndex + 1));
                    }
                }
            }

            if (!String.IsNullOrEmpty(childPath) && unescape)
            {
                childPath = Uri.UnescapeDataString(childPath);
            }

            return childPath;
        }
        #endregion
    }

    /// <summary>
    /// The Navigator class overrides <see cref="NavigatorBase"/>. Navigator forms the base class for all navigators that actually wrap content
    /// or datra objects. NavigatorBase is also the base class for MasterNavigator, which doesn't wrap any content but is the "root" Navigator
    /// for all content, used to access other navigators at various levels of nesting.
    /// </summary>
    public abstract class Navigator : NavigatorBase
    {
        #region Fields
        /// <summary>
        /// The actual content that the navigator is used to locate.
        /// </summary>
        private object content;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor for Navigator objects.
        /// </summary>
        /// <param name="content">Navigator's content.</param>
        protected Navigator(object content)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            this.content = content;
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets the actual data object that the navigator is used to locate.
        /// </summary>
        public object Content
        {
            get { return this.content; }
            protected set { this.content = value; }
        }

        /// <summary>
        /// Gets the path for the Navigator.  Navigators must provide a serializable path or locator for their object that can be stored in the navigation journal.
        /// </summary>
        public abstract string Path
        {
            get;
        } 
        #endregion

        #region Methods
        /// <summary>
        /// Gets the next sibling Navigator for this navigator.
        /// </summary>
        /// <returns>Base implementation always returns null.</returns>
        public virtual Navigator GetNextSibling()
        {
            return null;
        }

        /// <summary>
        /// Gets the previous sibling Navigator for this navigator.
        /// </summary>
        /// <returns>Base implementation always returns null.</returns>
        public virtual Navigator GetPreviousSibling()
        {
            return null;
        }

        /// <summary>
        /// Gets the next child navigator if this navigator has children. Base implementation assumes
        /// the navigator has no children and returns null.
        /// </summary>
        /// <param name="child">The current child.</param>
        /// <returns>Base implementation always returns null.</returns> 
        public virtual Navigator GetNextChild(Navigator child)
        {
            return null;
        }

        /// <summary>
        /// Gets the previous child navigator if this navigator has children. Base implementation assumes
        /// the navigator has no children and returns null.
        /// </summary>
        /// <param name="child">The current child.</param>
        /// <returns>Base implementation always returns null.</returns> 
        public virtual Navigator GetPreviousChild(Navigator child)
        {
            return null;
        } 
        #endregion
    }

    /// <summary>
    /// Navigator for objects deriving from GuidObject (Item, Channel). Includes a Parent navigator to give context to the item.
    /// </summary>
    public abstract class NavigatableObjectNavigator : Navigator
    {
        #region Fields
        /// <summary>
        /// Navigator for the next item.
        /// </summary>
        private Navigator nextItem;

        /// <summary>
        /// Navigator for the previous item.
        /// </summary>
        private Navigator previousItem;

        /// <summary>
        /// Navigator for the parent item.
        /// </summary>
        private Navigator parent;

        /// <summary>
        /// Navigator path.
        /// </summary>
        private string path;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor for GuidObjectNavigator.
        /// </summary>
        /// <param name="guidObject">GuidObject that is Navigator's content.</param>
        /// <param name="parent">GuidObject's parent to provide navigation context (may be null if the item is not parented).</param>
        protected NavigatableObjectNavigator(NavigatableObject guidObject, Navigator parent)
            : base(guidObject)
        {
            this.parent = parent;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets a path/locator for the Navigator.  Navigators must provide a serializable path or locator for their object that can be stored in the navigation journal.
        /// </summary>
        public override string Path
        {
            get
            {
                if (this.path == null)
                {
                    // Assume parent's path is escaped, escape own guid
                    NavigatableObject item = Content as NavigatableObject;
                    if (this.parent != null)
                    {
                        this.path = this.parent.Path + ChildSeparator + Uri.EscapeDataString(item.Guid);
                    }
                    else
                    {
                        this.path = Uri.EscapeDataString(item.Guid);
                    }
                }

                return this.path;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets next sibling navigator.
        /// </summary>
        /// <returns>The next sibling navigator.</returns>
        public override Navigator GetNextSibling()
        {
            if (this.nextItem == null)
            {
                if (this.parent != null)
                {
                    this.nextItem = this.parent.GetNextChild(this);
                }
            }

            return this.nextItem;
        }

        /// <summary>
        /// Gets previous sibling navigator.
        /// </summary>
        /// <returns>The previous sibling navigator.</returns>
        public override Navigator GetPreviousSibling()
        {
            if (this.previousItem == null)
            {
                if (this.parent != null)
                {
                    this.previousItem = this.parent.GetPreviousChild(this);
                }
            }

            return this.previousItem;
        }

        /// <summary>
        /// Parent navigator for this item - item navigators may be parented, for example, PhotoNavigators may be parented
        /// by their containing photo album navigator, etc.
        /// </summary>
        /// <returns>The parent navigator, if present.</returns>
        public Navigator GetParent()
        {
            return this.parent;
        } 
        #endregion  
    }

    /// <summary>
    /// PhotoGalleryNavigator wraps a PhotoGallery object, and exposes Navigator collections for the top-level PhotoAlbum
    /// collection of the data feed and for it's global Photo collection.
    /// </summary>
    public class PhotoGalleryNavigator : Navigator
    {
        #region Fields
        /// <summary>
        /// The first photo album in the data source.
        /// </summary>
        private PhotoAlbumNavigator firstPhotoAlbum;

        /// <summary>
        /// The last phot album in the data source.
        /// </summary>
        private PhotoAlbumNavigator lastPhotoAlbum;

        /// <summary>
        /// The path to the data source.
        /// </summary>
        private string path;

        /// <summary>
        /// The guid store for this data source.
        /// </summary>
        private NavigationGuidStore guidStore;

        /// <summary>
        /// The data source's photo albums.
        /// </summary>
        private List<PhotoAlbumNavigator> photoAlbums;

        /// <summary>
        /// The data source's photos.
        /// </summary>
        private List<PhotoNavigator> photos;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor for PhotoGalleryNavigator.
        /// </summary>
        /// <param name="photoGallery">
        /// PhotoGallery object that is Navgiator's content.
        /// </param>
        public PhotoGalleryNavigator(PhotoGallery photoGallery)
            : base(photoGallery)
        {
            if (photoGallery == null)
            {
                throw new ArgumentNullException("photoGallery");
            }

            // Escape the guid string in case it contains the guid separator character
            this.path = Uri.EscapeDataString(photoGallery.Guid);
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets the Navigator path.  Navigators must provide a serializable path or locator for their object that can be stored in the navigation journal.
        /// </summary>
        public override string Path
        {
            get { return this.path; }
        }

        /// <summary>
        ///  Gets the navigation guid store, which can be used to look up child navigators by item guid.
        /// </summary>
        protected NavigationGuidStore GuidStore
        {
            get
            {
                if (this.guidStore == null)
                {
                    this.CreateChildNavigators();
                }

                return this.guidStore;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the navigator for the first photo album of the PhotoGallery.
        /// </summary>
        /// <returns>The first photo album in the data feed.</returns>
        public virtual PhotoAlbumNavigator GetFirstPhotoAlbum()
        {
            if (this.firstPhotoAlbum == null)
            {
                IList<PhotoAlbumNavigator> photoAlbumNavigators = this.GetPhotoAlbums();
                if (photoAlbumNavigators.Count > 0)
                {
                    this.firstPhotoAlbum = photoAlbumNavigators[0];
                }
            }

            return this.firstPhotoAlbum;
        }

        /// <summary>
        /// Gets the navigator for the last photo album of the PhotoGallery.
        /// </summary>
        /// <returns>The last photo album in the data feed.</returns>
        public PhotoAlbumNavigator GetLastPhotoAlbum()
        {
            if (this.lastPhotoAlbum == null)
            {
                IList<PhotoAlbumNavigator> photoAlbumNavigators = this.GetPhotoAlbums();
                if (photoAlbumNavigators.Count > 0)
                {
                    this.lastPhotoAlbum = photoAlbumNavigators[photoAlbumNavigators.Count - 1];
                }
            }

            return this.lastPhotoAlbum;
        }

        /// <summary>
        /// Gets navigators for PhotoAlbums in the PhotoGallery.
        /// </summary>
        /// <returns>A collection of PhotoAlbums.</returns>
        public ReadOnlyCollection<PhotoAlbumNavigator> GetPhotoAlbums()
        {
            // If main photo album navigators have not been created and cached, call CreateChildNavigators
            // to populate photo albums, photos and the guid store
            if (this.photoAlbums == null)
            {
                this.CreateChildNavigators();
            }

            return new ReadOnlyCollection<PhotoAlbumNavigator>(this.photoAlbums);
        }

        /// <summary>
        /// Gets the photo navigator collection for the data feed content.
        /// </summary>
        /// <returns>A collection of Photos.</returns>
        public ReadOnlyCollection<PhotoNavigator> GetPhotos()
        {
            // If photo navigators have not been created and cached, call CreateChildNavigators
            // to populate photo albums, photos and the guid store
            if (this.photos == null)
            {
                // Call CreateChildNavigators to initialize photo navigators
                this.CreateChildNavigators();
            }

            return new ReadOnlyCollection<PhotoNavigator>(this.photos);
        }

        /// <summary>
        /// Extracts the navigator for the object indicated by the path.
        /// </summary>
        /// <param name="parentPath">The path for the navigator.</param>
        /// <returns>The child navigator, if present.</returns>
        public override Navigator GetChildNavigatorFromPath(string parentPath)
        {
            Navigator childNavigator = null;
            if (!String.IsNullOrEmpty(parentPath))
            {
                // Extract the first guid in the path and match it to a child navigator. If there is no child separator,
                // treat the entire path as the child's guid
                string childGuid = String.Empty;
                string remainder = String.Empty;
                childGuid = ExtractFirstChildPath(parentPath, true, out remainder);

                if (!String.IsNullOrEmpty(childGuid))
                {
                    this.GuidStore.TryGetValue(childGuid, out childNavigator);
                }

                if (childNavigator != null && !String.IsNullOrEmpty(remainder))
                {
                    // Look up the remainder of the path in the child navigator
                    childNavigator = childNavigator.GetChildNavigatorFromPath(remainder);
                }
            }

            return childNavigator;
        }

        /// <summary>
        /// Get next child navigator.
        /// </summary>
        /// <param name="child">The current child.</param>
        /// <returns>The next child navigator.</returns>
        public override Navigator GetNextChild(Navigator child)
        {
            PhotoAlbumNavigator photoAlbumNavigator = child as PhotoAlbumNavigator;
            if (photoAlbumNavigator != null)
            {
                return this.GetNextPhotoAlbum(photoAlbumNavigator);
            }
            else
            {
                PhotoNavigator photoNavigator = child as PhotoNavigator;
                if (photoNavigator != null)
                {
                    return this.GetNextPhoto(photoNavigator);
                }
            }

            return null;
        }

        /// <summary>
        /// Get previous child navigator.
        /// </summary>
        /// <param name="child">The current child.</param>
        /// <returns>The previous child navigator.</returns>
        public override Navigator GetPreviousChild(Navigator child)
        {
            PhotoAlbumNavigator photoAlbumNavigator = child as PhotoAlbumNavigator;
            if (photoAlbumNavigator != null)
            {
                return this.GetPreviousPhotoAlbum(photoAlbumNavigator);
            }
            else
            {
                PhotoNavigator photoNavigator = child as PhotoNavigator;
                if (photoNavigator != null)
                {
                    return this.GetPreviousPhoto(photoNavigator);
                }
            }

            return null;
        }

        /// <summary>
        /// Populates the child navigator collections and stores guids for quick lookup.
        /// </summary>
        protected virtual void CreateChildNavigators()
        {
            this.guidStore = new NavigationGuidStore();
            this.photoAlbums = new List<PhotoAlbumNavigator>();
            this.photos = new List<PhotoNavigator>();
            PhotoGallery photoGallery = Content as PhotoGallery;

            // Insert a pseudo-navigator for the home section
            HomePhotoAlbum homeAlbum = new HomePhotoAlbum("HOMEALBUM");
            homeAlbum.Title = "Gallery Home";
            HomePhotoAlbumNavigator homePhotoAlbumNavigator = new HomePhotoAlbumNavigator(homeAlbum, this);
            if (!this.guidStore.ContainsKey(homeAlbum.Guid))
            {
                this.guidStore.Add(homeAlbum.Guid, homePhotoAlbumNavigator);
                this.photoAlbums.Add(homePhotoAlbumNavigator);
            }
            else
            {
                // Guid duplicated, same photo album appears twice. Log error
                ServiceProvider.Logger.Error(Strings.NavigatorDuplicateGuid, homeAlbum.Guid);
            }

            // PhotoGallery navigator creates navigators for main photo albums and photos.
            foreach (PhotoAlbum photoAlbum in photoGallery.PhotoAlbums)
            {
                PhotoAlbumNavigator photoAlbumNavigator = new PhotoAlbumNavigator(photoAlbum, this);

                if (!this.guidStore.ContainsKey(photoAlbum.Guid))
                {
                    this.guidStore.Add(photoAlbum.Guid, photoAlbumNavigator);
                    this.photoAlbums.Add(photoAlbumNavigator);
                }
                else
                {
                    // Guid duplicated, same photo album appears twice. Log error
                    ServiceProvider.Logger.Error(Strings.NavigatorDuplicateGuid, photoAlbum.Guid);
                }
            }

            // Calling CreateChildNavigators resets any prevously cached values for first, last child, etc.
            this.firstPhotoAlbum = null;
            this.lastPhotoAlbum = null;
        }

        /// <summary>
        /// Gets next photo navigator from the given navigator.
        /// </summary>
        /// <param name="photoNavigator">The photo navigator to use.</param>
        /// <returns>The following photo.</returns>
        protected virtual PhotoNavigator GetNextPhoto(PhotoNavigator photoNavigator)
        {
            PhotoNavigator nextPhotoNavigator = null;
            if (photoNavigator != null)
            {
                IList<PhotoNavigator> photoNavigators = this.GetPhotos();
                int photoIndex = photoNavigators.IndexOf(photoNavigator);
                if (photoIndex >= 0 && photoIndex < photoNavigators.Count - 1)
                {
                    nextPhotoNavigator = photoNavigators[photoIndex + 1];
                }
            }

            return nextPhotoNavigator;
        }

        /// <summary>
        /// Gets previous photo navigator from the given navigator.
        /// </summary>
        /// <param name="photoNavigator">The photo navigator to use.</param>
        /// <returns>The preceeding photo.</returns>
        protected virtual PhotoNavigator GetPreviousPhoto(PhotoNavigator photoNavigator)
        {
            PhotoNavigator previousPhotoNavigator = null;
            if (photoNavigator != null)
            {
                IList<PhotoNavigator> photoNavigators = this.GetPhotos();
                int photoIndex = photoNavigators.IndexOf(photoNavigator);
                if (photoIndex > 0)
                {
                    previousPhotoNavigator = photoNavigators[photoIndex - 1];
                }
            }

            return previousPhotoNavigator;
        }

        /// <summary>
        /// Gets the next photo album navigator from the given navigator.
        /// </summary>
        /// <param name="photoAlbumNavigator">The phot album navigator to use.</param>
        /// <returns>The following photo album.</returns>
        protected virtual PhotoAlbumNavigator GetNextPhotoAlbum(PhotoAlbumNavigator photoAlbumNavigator)
        {
            PhotoAlbumNavigator nextPhotoAlbumNavigator = null;
            if (photoAlbumNavigator != null)
            {
                IList<PhotoAlbumNavigator> photoAlbumNavigators = this.GetPhotoAlbums();
                int photoAlbumIndex = photoAlbumNavigators.IndexOf(photoAlbumNavigator);
                if (photoAlbumIndex >= 0 && photoAlbumIndex < photoAlbumNavigators.Count - 1)
                {
                    nextPhotoAlbumNavigator = photoAlbumNavigators[photoAlbumIndex + 1];
                }
            }

            return nextPhotoAlbumNavigator;
        }

        /// <summary>
        /// Gets the previous photo album navigator from the given navigator.
        /// </summary>
        /// <param name="photoAlbumNavigator">The photo album navigator to use.</param>'
        /// <returns>The preceeding photo album.</returns>
        protected virtual PhotoAlbumNavigator GetPreviousPhotoAlbum(PhotoAlbumNavigator photoAlbumNavigator)
        {
            PhotoAlbumNavigator previousPhotoAlbumNavigator = null;
            if (photoAlbumNavigator != null)
            {
                IList<PhotoAlbumNavigator> photoAlbumNavigators = this.GetPhotoAlbums();
                int photoAlbumIndex = photoAlbumNavigators.IndexOf(photoAlbumNavigator);
                if (photoAlbumIndex > 0)
                {
                    previousPhotoAlbumNavigator = photoAlbumNavigators[photoAlbumIndex - 1];
                }
            }

            return previousPhotoAlbumNavigator;
        } 
        #endregion
    }

    /// <summary>
    /// PhotoAlbumNavigator wraps a PhotoAlbum object and exposes Navigator collections for its Photos collection.
    /// </summary>
    public class PhotoAlbumNavigator : NavigatableObjectNavigator
    {
        #region Fields
        /// <summary>
        /// The collection of photos in the album.
        /// </summary>
        private List<PhotoNavigator> photos;

        /// <summary>
        /// The first photo in the album.
        /// </summary>
        private PhotoNavigator firstPhoto;

        /// <summary>
        /// The last photo in the album.
        /// </summary>
        private PhotoNavigator lastPhoto;

        /// <summary>
        /// The guid store for the album.
        /// </summary>
        private NavigationGuidStore guidStore;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor for PhotoAlbumNavigator.
        /// </summary>
        /// <param name="photoAlbum">PhotoAlbum that is this navigator's content.</param>
        /// <param name="parent">Parent navigator for this navigator, if the parent exists.</param>
        public PhotoAlbumNavigator(PhotoAlbum photoAlbum, Navigator parent)
            : base(photoAlbum, parent)
        {
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets the navigation guid store, which can be used to look up child navigators by item guid.
        /// </summary>
        protected NavigationGuidStore GuidStore
        {
            get
            {
                if (this.guidStore == null)
                {
                    this.CreateChildNavigators();
                }

                return this.guidStore;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns the navigator for the first Photo of the PhotoAlbum.
        /// </summary>
        /// <returns>The navigator for the first Photo of the PhotoAlbum.</returns>
        public virtual PhotoNavigator GetFirstPhoto()
        {
            if (this.firstPhoto == null)
            {
                IList<PhotoNavigator> photoNavigators = this.GetPhotos();
                if (photoNavigators.Count > 0)
                {
                    this.firstPhoto = photoNavigators[0];
                }
            }

            return this.firstPhoto;
        }

        /// <summary>
        /// Returns the navigator for the last Photo of the PhotoAlbum.
        /// </summary>
        /// <returns>The navigator for the last Photo of the PhotoAlbum.</returns>
        public virtual PhotoNavigator GetLastPhoto()
        {
            if (this.lastPhoto == null)
            {
                IList<PhotoNavigator> photoNavigators = this.GetPhotos();
                if (photoNavigators.Count > 0)
                {
                    this.lastPhoto = photoNavigators[photoNavigators.Count - 1];
                }
            }

            return this.lastPhoto;
        }

        /// <summary>
        /// Returns collection of navigators for all photos of the photo album.
        /// </summary>
        /// <returns>Collection of navigators for all photos of the photo album.</returns>
        public virtual ReadOnlyCollection<PhotoNavigator> GetPhotos()
        {
            if (this.photos == null)
            {
                this.CreateChildNavigators();
            }

            return new ReadOnlyCollection<PhotoNavigator>(this.photos);
        }

        /// <summary>
        /// Extracts the navigator for the object indicated by the path.
        /// </summary>
        /// <param name="parentPath">The path to the object.</param>
        /// <returns>The navigator for the provided object path.</returns>
        public override Navigator GetChildNavigatorFromPath(string parentPath)
        {
            Navigator childNavigator = null;
            if (!String.IsNullOrEmpty(parentPath))
            {
                // Extract the first guid in the path and match it to a child navigator. If there is no child separator,
                // treat the entire path as the child's guid
                string remainder = String.Empty;
                string childGuid = ExtractFirstChildPath(parentPath, true, out remainder);
                if (!String.IsNullOrEmpty(childGuid))
                {
                    this.GuidStore.TryGetValue(childGuid, out childNavigator);
                }

                if (childNavigator != null && !String.IsNullOrEmpty(remainder))
                {
                    // Look up the remainder of the path in the child navigator
                    childNavigator = childNavigator.GetChildNavigatorFromPath(remainder);
                }
            }

            return childNavigator;
        }

        /// <summary>
        /// Get next child navigator.
        /// </summary>
        /// <param name="child">The current child navigator.</param>
        /// <returns>The following child.</returns>
        public override Navigator GetNextChild(Navigator child)
        {
            PhotoNavigator photoNavigator = child as PhotoNavigator;
            if (photoNavigator != null)
            {
                return this.GetNextPhoto(photoNavigator);
            }

            return null;
        }

        /// <summary>
        /// Get previous child navigator.
        /// </summary>
        /// <param name="child">The current child navigator.</param>
        /// <returns>The preceeding child.</returns>
        public override Navigator GetPreviousChild(Navigator child)
        {
            PhotoNavigator photoNavigator = child as PhotoNavigator;
            if (photoNavigator != null)
            {
                return this.GetPreviousPhoto(photoNavigator);
            }

            return null;
        }

        /// <summary>
        /// Gets next photo navigator from the given navigator.
        /// </summary>
        /// <param name="photoNavigator">The current photo navigator.</param>
        /// <returns>Photo navigator for the next photo.</returns>
        protected virtual PhotoNavigator GetNextPhoto(PhotoNavigator photoNavigator)
        {
            PhotoNavigator nextPhotoNavigator = null;
            IList<PhotoNavigator> photoNavigators = this.GetPhotos();
            if (photoNavigator != null)
            {
                int photoIndex = photoNavigators.IndexOf(photoNavigator);
                if (photoIndex >= 0 && photoIndex < photoNavigators.Count - 1)
                {
                    nextPhotoNavigator = photoNavigators[photoIndex + 1];
                }
            }

            return nextPhotoNavigator;
        }

        /// <summary>
        /// Gets previous photo navigator from the given navigator.
        /// </summary>
        /// <param name="photoNavigator">The current photo navigator.</param>
        /// <returns>Photo navigator for the previous photo.</returns>
        protected virtual PhotoNavigator GetPreviousPhoto(PhotoNavigator photoNavigator)
        {
            PhotoNavigator previousPhotoNavigator = null;
            IList<PhotoNavigator> photoNavigators = this.GetPhotos();
            if (photoNavigator != null)
            {
                int photoIndex = photoNavigators.IndexOf(photoNavigator);
                if (photoIndex > 0)
                {
                    previousPhotoNavigator = photoNavigators[photoIndex - 1];
                }
            }

            return previousPhotoNavigator;
        }

        /// <summary>
        /// Populates the child navigator collections and stores guids for quick lookup.
        /// </summary>
        protected virtual void CreateChildNavigators()
        {
            this.guidStore = new NavigationGuidStore();
            this.photos = new List<PhotoNavigator>();
            PhotoAlbum photoAlbum = Content as PhotoAlbum;

            foreach (Photo photo in photoAlbum.Photos)
            {
                PhotoNavigator photoNavigator = new PhotoNavigator(photo, this);

                if (!this.guidStore.ContainsKey(photo.Guid))
                {
                    this.guidStore.Add(photo.Guid, photoNavigator);
                    this.photos.Add(photoNavigator);
                }
                else
                {
                    // Guid duplicated, same photo appears twice. Log error
                    ServiceProvider.Logger.Error(Strings.NavigatorDuplicateGuid, photo.Guid);
                }
            }

            // When child navigators are created, it counts as a reset of any cached values of first, last child etc.
            this.firstPhoto = null;
            this.lastPhoto = null;
        } 
        #endregion
    }

    /// <summary>
    /// HomePhotoAlbumNavigator is a dummy navigator used to indicate when a photo gallery's home page should be displayed.
    /// </summary>
    public class HomePhotoAlbumNavigator : PhotoAlbumNavigator
    {
        /// <summary>
        /// Constructor for HomePhotoAlbumNavigator.
        /// </summary>
        /// <param name="photoAlbum">Normally the navigator's content, in this case, unused.</param>
        /// <param name="parent">Parent navigator for this navigator, if the parent exists.</param>
        public HomePhotoAlbumNavigator(PhotoAlbum photoAlbum, Navigator parent)
            : base(photoAlbum, parent)
        {
        }
    }

    /// <summary>
    /// PhotoNavigator wraps a Photo object.
    /// </summary>
    public class PhotoNavigator : NavigatableObjectNavigator
    {
        #region Constructor
        /// <summary>
        /// Constructor for PhotoNavigator objects.
        /// </summary>
        /// <param name="photo">Photo that is navigator's content.</param>
        /// <param name="parent">Parent navigator which gives navigation context for Photo, if it exists.</param>
        public PhotoNavigator(Photo photo, Navigator parent)
            : base(photo, parent)
        {
        } 
        #endregion
    }

    /// <summary>
    /// PhotoSlideShowNavigator wraps a PhotoSlideShow object.
    /// </summary>
    public class PhotoSlideShowNavigator : NavigatableObjectNavigator
    {
        #region Fields

        /// <summary>
        /// Full path stored in navigation journal
        /// </summary>
        private string path;

        #endregion

        #region Constructor
        /// <summary>
        /// Constructor for PhotoSlideShowNavigator objects.
        /// </summary>
        /// <param name="photoSlideShow">photoSlideShow that is navigator's content.</param>
        public PhotoSlideShowNavigator(PhotoSlideShow photoSlideShow)
            : base(photoSlideShow, null)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the guid that identifies slide show in journal navigation
        /// </summary>
        public static string SlideShowGuid
        {
            get { return "[slideshow]"; }
        }

        /// <summary>
        /// Gets the path for slide show navigator stored in navigation journal.
        /// </summary>
        public override string Path
        {
            get
            {
                if (this.path == null)
                {
                    PhotoSlideShow pss = this.Content as PhotoSlideShow;
                    PhotoAlbum album = pss.Album.Content as PhotoAlbum;
                    this.path = SlideShowGuid + this.ChildSeparator +
                           ServiceProvider.ViewManager.PhotoGallery.Guid + this.ChildSeparator + 
                           album.Guid;
                }

                return this.path;
            }
        }

        #endregion
    }

    /// <summary>
    /// SearchNavigator wraps a Search photo album. It can also refresh the photo album it wraps by resetting its Content, since
    /// only one Search photo album can be active at any given time in ScePhoto applications.
    /// </summary>
    public class SearchNavigator : PhotoAlbumNavigator
    {
        #region Fields
        /// <summary>
        /// The query text for this SearchPhotoAlbum.
        /// </summary>
        private string searchText;
        
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor for SearchNavigator objects.
        /// </summary>
        /// <param name="searchPhotoAlbum">SearchPhotoAlbum that is navigator's content.</param>
        public SearchNavigator(SearchPhotoAlbum searchPhotoAlbum)
            : base(searchPhotoAlbum, null)
        {
            if (searchPhotoAlbum == null)
            {
                throw new ArgumentNullException("searchPhotoAlbum");
            }

            this.SearchText = searchPhotoAlbum.SearchText;
        }
        
        #endregion

        #region Properties
        /// <summary>
        /// Gets the SearchText for this navigator's SearchPhotoAlbum - can be used to regenerate the SearchPhotoAlbum from the navigator
        /// when navigating back/forward to it through the journal.
        /// </summary>
        public string SearchText
        {
            get { return this.searchText; }
            protected set { this.searchText = value; }
        } 
        #endregion

        #region Methods
        /// <summary>
        /// Refreshes the Navigator's SearchPhotoAlbum with a new SearchPhotoAlbum, after a new Search. Since only SearchPhotoAlbum is active
        /// is active at a time only one Navigator for search photo album is needed and can be refreshed on new searches.
        /// </summary>
        /// <param name="newSearchPhotoAlbum">New SearchPhotoAlbum that refreshes navigator content.</param>
        public virtual void RefreshSearchPhotoAlbum(SearchPhotoAlbum newSearchPhotoAlbum)
        {
            if (newSearchPhotoAlbum != null)
            {
                Content = newSearchPhotoAlbum;
                this.SearchText = newSearchPhotoAlbum.SearchText;
                CreateChildNavigators();
            }
            else
            {
                throw new ArgumentNullException("newSearchPhotoAlbum");
            }
        } 
        #endregion
    }
}
