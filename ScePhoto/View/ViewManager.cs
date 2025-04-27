//-----------------------------------------------------------------------
// <copyright file="ViewManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     The main class to which UI Pages and Controls are bound.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.View
{
    using System;
    using System.IO;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Navigation;
    using System.Windows.Annotations.Storage;
    using ScePhoto.Data;
    using System.Collections;
    using ScePhoto.Feed;

    /// <summary>
    /// This represents the state of the sync. It can either be in progress, not in progress, or failed.
    /// </summary>
    public enum SyncState
    {
        /// <summary>
        /// Awaiting first-ever sync (on application startup).
        /// </summary>
        PendingFirstSync,

        /// <summary>
        /// No sync currently in progress.
        /// </summary>
        NoSyncInProgress,

        /// <summary>
        /// Sync is currently taking place.
        /// </summary>
        SyncInProgress,

        /// <summary>
        /// Sync has failed to complete successfully.
        /// </summary>
        SyncFailed
    }

    /// <summary>
    /// Indicates the navigation mode, whether next, previous, etc. Superset of NavigationMode exposed in platform.
    /// </summary>
    public enum ScePhotoNavigationMode
    {
        /// <summary>
        /// Navigation to the next item in the application's logical item order.
        /// </summary>
        Next,

        /// <summary>
        /// Navigation to the previous item in the application's logical item order.
        /// </summary>
        Previous,

        /// <summary>
        /// Navigation to the previous item on the journal's back stack.
        /// </summary>
        Back,

        /// <summary>
        /// Navigation to the next item on the journal's back stack.
        /// </summary>
        Forward,

        /// <summary>
        /// Refresh of current item.
        /// </summary>
        Refresh,

        /// <summary>
        /// Navigation without context, i.e. to an arbitrary object or Navigator.
        /// </summary>
        Normal
    }

    /// <summary>
    /// ViewManager provides a view on all data contained in a ScePhoto application, whether it is fetched as part of the RSS feed
    /// or custom data objects created in other ways, e.g. ReadingList and Search. ViewManager exposes properties revealing the
    /// state of the data, e.g. the data currently displayed, the photo gallery currently selected, and implements INotifyPropertyChanged
    /// to give notification when state changes. It also provides APIs to manipulate the data, such as commands to navigate from
    /// one object to another, to start or stop sync, etc. Application UI is typically bound to ViewManager and relies on it to provide
    /// notifications about state change so that this can be reflected in UI.
    /// </summary>
    public class ViewManager : INotifyPropertyChanged
    {
        #region Private Fields

        /// <summary>
        /// The current navigator.
        /// </summary>
        private Navigator currentNavigator;

        /// <summary>
        /// Command set for navigation.
        /// </summary>
        private NavigationCommands navigationCommands;

        /// <summary>
        /// The master navigator.
        /// </summary>
        private MasterNavigator masterNavigator;

        /// <summary>
        /// The current photo gallery.
        /// </summary>
        private PhotoGallery photoGallery;

        /// <summary>
        /// Path for journal navigation.
        /// </summary>
        private string journaledPath = String.Empty;

        /// <summary>
        /// How to navigate through the journal.
        /// </summary>
        private ScePhotoNavigationMode journalNavigationMode;

        /// <summary>
        /// Indicates whether a navigation by guid is pending.
        /// </summary>
        private string guidNavigationPending;

        /// <summary>
        /// The origin of a specific navigation.
        /// </summary>
        private NavigationOrigin navigationOrigin;

        /// <summary>
        /// The current visual.
        /// </summary>
        private Visual currentVisual;

        /// <summary>
        /// The current dialog.
        /// </summary>
        private UserControl dialog;

        /// <summary>
        /// The active photo album.
        /// </summary>
        private PhotoAlbum activePhotoAlbum;

        /// <summary>
        /// The active photo.
        /// </summary>
        private Photo activePhoto;

        /// <summary>
        /// The command set for syncing.
        /// </summary>
        private SyncCommands syncCommands;

        /// <summary>
        /// Indicates whether photo album navigation is nested.
        /// </summary>
        private bool nestPhotoAlbumNavigation;

        /// <summary>
        /// Indicates whether photo navigation is nested.
        /// </summary>
        private bool nestPhotoNavigation;

        /// <summary>
        /// The current sync state.
        /// </summary>
        private SyncState syncState;

        /// <summary>
        /// The last time the feed was updated.
        /// </summary>
        private DateTime lastUpdated;

        /// <summary>
        /// The last sync error message.
        /// </summary>
        private string syncErrorMessage;

        /// <summary>
        /// The sync progress, in percent.
        /// </summary>
        private double syncProgress;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor initializes all key services provided by the ViewManager for navigation and data management.
        /// </summary>
        public ViewManager()
        {
            this.InitializeDataServices();
            this.InitializeNavigationServices();
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Fired when navigation is taking place.
        /// </summary>
        public event EventHandler<ViewManagerNavigatedEventArgs> Navigated;

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// PropertyChanged event notifies listeners if a property value on this element has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Enums
        /// <summary>
        /// Origin of last navigation.
        /// </summary>
        private enum NavigationOrigin
        {
            /// <summary>
            /// NavigationCommand was executed.
            /// </summary>
            Command,

            /// <summary>
            /// Current navigator reinstated, e.g. after opening from system tray.
            /// </summary>
            Refresh,

            /// <summary>
            /// Startup w/ package from command line.
            /// </summary>
            External,

            /// <summary>
            /// Initial state.
            /// </summary>
            None,

            /// <summary>
            /// The journal.
            /// </summary>
            Journal
        } 
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the photo galleries available for selection.
        /// </summary>
        public PhotoGalleryCollection PhotoGalleries
        {
            get { return ServiceProvider.DataManager.PhotoGalleries; }
        }

        /// <summary>
        /// Gets the currently active modal dialog.
        /// </summary>
        public UserControl Dialog
        {
            get { return this.dialog; }
        }

        /// <summary>
        /// Gets or sets the current PhotoGallery exposed on DataManager. This is the latest publisher feed state with photo albums and photos that the
        /// application should display.
        /// </summary>
        public PhotoGallery PhotoGallery
        {
            get 
            { 
                return this.photoGallery; 
            }

            set
            {
                if (this.photoGallery != value)
                {
                    if (this.photoGallery != null)
                    {
                        this.photoGallery.PhotoAlbums.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.OnPhotoGalleryAlbumsCollectionChanged);
                    }

                    this.photoGallery = value;
                    this.NotifyPropertyChanged("PhotoGallery");
                    if (this.photoGallery != null)
                    {
                        this.photoGallery.PhotoAlbums.CollectionChanged += new NotifyCollectionChangedEventHandler(this.OnPhotoGalleryAlbumsCollectionChanged);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the commands controlling navigation.
        /// </summary>
        public NavigationCommands NavigationCommands
        {
            get { return this.navigationCommands; }
            set { this.navigationCommands = value; }
        }

        /// <summary>
        /// Gets or sets the commands controlling sync.
        /// </summary>
        public SyncCommands SyncCommands
        {
            get { return this.syncCommands; }
            set { this.syncCommands = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether navigation modes for photo albums are nested.
        /// </summary>
        public bool NestPhotoAlbumNavigation
        {
            get { return this.nestPhotoAlbumNavigation; }
            set { this.nestPhotoAlbumNavigation = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the navigation modes for photos are nested.
        /// </summary>
        public bool NestPhotoNavigation
        {
            get { return this.nestPhotoNavigation; }
            set { this.nestPhotoNavigation = value; }
        }

        /// <summary>
        /// Gets the active photo album.
        /// </summary>
        /// <remarks>
        /// If a photo album is the current object that has been navigated to, this property reflects that photo album
        /// If a photo is the current navigation content, ActivePhotoAlbum is the photo album to which that photo belongs.
        /// </remarks>
        public PhotoAlbum ActivePhotoAlbum
        {
            get 
            { 
                return this.activePhotoAlbum; 
            }

            protected set
            {
                if (this.activePhotoAlbum != value)
                {
                    this.activePhotoAlbum = value;
                    this.NotifyPropertyChanged("ActivePhotoAlbum");
                }
            }
        }

        /// <summary>Gets the active photo.</summary>
        /// <remarks>
        /// If a photo is the last object navigated to, this property reflects that photo. If the last object navigated to was a photo album,
        /// this is null.
        /// </remarks>
        public Photo ActivePhoto
        {
            get
            { 
                return this.activePhoto; 
            }

            protected set
            {
                this.activePhoto = value;
                this.NotifyPropertyChanged("ActivePhoto");
            }
        }

        /// <summary>
        /// Gets or sets the current navigator.
        /// </summary>
        public Navigator CurrentNavigator
        {
            get 
            { 
                return this.currentNavigator; 
            }

            set
            {
                this.currentNavigator = value;
                this.NotifyPropertyChanged("CurrentNavigator");
            }
        }

        /// <summary>
        /// Gets or sets the current visual.
        /// </summary>
        /// <remarks>
        /// In case controls need to manipulate focus, etc. within data templates, ViewManager gives access to the visual currently displayed in
        /// transition element. This is the data template root for any content in main content container.
        /// </remarks>
        public Visual CurrentVisual
        {
            get 
            { 
                return this.currentVisual; 
            }

            set
            {
                this.currentVisual = value;
                this.NotifyPropertyChanged("CurrentVisual");
            }
        }

        /// <summary>
        /// Gets the master navigator handles navigation across multiple feeds and for non-feed photo albums such as Search, ReadingList, etc.
        /// </summary>
        public MasterNavigator MasterNavigator
        {
            get { return this.masterNavigator; }
            protected set { this.masterNavigator = value; }
        }

        /// <summary>
        /// Gets or sets the current sync state - whether first sync is pending, sync in progress, sync failed, or sync completed.
        /// </summary>
        public SyncState SyncState
        {
            get 
            {
                return this.syncState; 
            }

            set
            {
                this.syncState = value;
                this.NotifyPropertyChanged("SyncState");
            }
        }

        /// <summary>
        /// Gets or sets the last updated date (date of last sync).
        /// </summary>
        public DateTime LastUpdated
        {
            get 
            {
                return this.lastUpdated; 
            }

            set
            {
                this.lastUpdated = value;
                this.NotifyPropertyChanged("LastUpdated");
            }
        }

        /// <summary>
        /// Gets or sets the error message to be displayed if sync failed.
        /// </summary>
        public string SyncErrorMessage
        {
            get 
            {
                return this.syncErrorMessage; 
            }

            set
            {
                this.syncErrorMessage = value;
                this.NotifyPropertyChanged("SyncErrorMessage");
            }
        }

        /// <summary>
        /// Gets or sets the measure of sync progress in terms of percentage of data requests completed.
        /// </summary>
        public double SyncProgress
        {
            get 
            {
                return this.syncProgress; 
            }

            set
            {
                this.syncProgress = value;
                this.NotifyPropertyChanged("SyncProgress");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Displays dialogPage and blocks input to the main application until EndDialog is called.
        /// </summary>
        /// <param name="sourceDialog">The dialog to show.</param>
        public virtual void ShowDialog(UserControl sourceDialog)
        {
            if (sourceDialog == null)
            {
                throw new ArgumentNullException("sourceDialog");
            }

            if (this.dialog != null)
            {
                throw new InvalidOperationException(Strings.ViewManagerInvalidOperationDialogVisible);
            }

            this.dialog = sourceDialog;

            this.dialog.Loaded += Dialog_OnLoaded; // Focus the dialog once it is loaded
            this.dialog.KeyDown += Dialog_OnKeyDown; // Dismiss the dialog if escape is pressed
            this.NotifyPropertyChanged("Dialog");
        }

        /// <summary>
        /// Closes dialogPage if it is the current dialogPage.
        /// </summary>
        /// <param name="sourceDialog">The dialog to close.</param>
        public virtual void EndDialog(UserControl sourceDialog)
        {
            if (sourceDialog != null)
            {
                if (object.ReferenceEquals(sourceDialog, this.dialog))
                {
                    this.dialog.Loaded -= Dialog_OnLoaded;
                    this.dialog.KeyDown -= Dialog_OnKeyDown;
                    this.dialog = null;
                    this.NotifyPropertyChanged("Dialog");
                }
            }
        }

        /// <summary>
        /// Navigate to object on a new navigation. Content state information for this navigation should be entered in the journal
        /// When navigating by command, the command should specify what mode of navigation it initiates - next, previous, etc.
        /// </summary>
        /// <param name="navigator">The navigator to navigate.</param>
        /// <param name="mode">The ScePhotoNavigationMode to use when navigating.</param>
        public virtual void NavigateByCommand(Navigator navigator, ScePhotoNavigationMode mode)
        {
            // Navigation by ViewManager's navigation commands is always new
            if (navigator != null)
            {
                this.navigationOrigin = NavigationOrigin.Command;
                this.NavigateNew(navigator, mode);
            }
        }

        /// <summary>
        /// Navigation may also take place through the journal when the application is hosted in a NavigationWindow, as is normal.
        /// On navigation through journal, the ViewContentState object gives the path of the navigation target.
        /// </summary>
        /// <remarks>
        /// When navigation takes place through the journal, the navigation mode reported by the navigaiton service is the platform's
        /// NavigationMode. It must be converted into ScePhoto's custom navigation mode type here.
        /// </remarks>
        /// <param name="path">The journal path to use.</param>
        /// <param name="mode">The journal navigation mode (NavigationMode.Back or .Forward).</param>
        public virtual void NavigateByJournal(string path, NavigationMode mode)
        {
            // Journal navigation must be back or forward
            if (mode == NavigationMode.Back || mode == NavigationMode.Forward)
            {
                ScePhotoNavigationMode readerNavigationMode = (mode == NavigationMode.Back) ? ScePhotoNavigationMode.Back : ScePhotoNavigationMode.Forward;
                if (!string.IsNullOrEmpty(path))
                {
                    this.navigationOrigin = NavigationOrigin.Journal;
                    Navigator currentNavigator = MasterNavigator.GetChildNavigatorFromPath(path);
                    if (currentNavigator != null)
                    {
                        // Current navigator found, raise Navigating event
                        object content = this.GetNavigatorContent(currentNavigator);
                        ViewManagerNavigatedEventArgs args = new ViewManagerNavigatedEventArgs(content, readerNavigationMode, new ViewContentState(currentNavigator), this.CurrentNavigator, currentNavigator);
                        this.CurrentNavigator = currentNavigator;
                        this.OnNavigated(args);
                    }
                    else
                    {
                        if (SyncState == SyncState.PendingFirstSync || SyncState == SyncState.SyncInProgress)
                        {
                            // Navigator wasn't found, but sync is in progress and paths may not be resolved yet. 
                            // Store journaled information for lookup later
                            this.journaledPath = path;
                            this.journalNavigationMode = readerNavigationMode;
                        }
                        else
                        {
                            // Object is gone. Return error
                            MissingItemError error = new MissingItemError(path);
                            ViewManagerNavigatedEventArgs args = new ViewManagerNavigatedEventArgs(error, readerNavigationMode, null, this.CurrentNavigator, null);
                            this.OnNavigated(args);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Current navigator is reinstated.
        /// </summary>
        /// <remarks>
        /// Navigate by refresh always has Refresh navigation mode.
        /// </remarks>
        /// <param name="navigator">The navigator to use.</param>
        public virtual void NavigateByRefresh(Navigator navigator)
        {
            if (navigator != null)
            {
                this.navigationOrigin = NavigationOrigin.Refresh;
                this.NavigateNew(navigator, ScePhotoNavigationMode.Refresh);
            }
        }

        /// <summary>
        /// Selects an photo gallery from the available photo galleries that matches the given guid
        /// If no match is found, the current selected photo gallery does not change and the method returns false.
        /// </summary>
        /// <param name="photoGalleryGuid">Guid of photo gallery to select as current photo gallery.</param>
        /// <returns>True when the photo gallery is found.</returns>
        public virtual bool SelectPhotoGalleryByGuid(string photoGalleryGuid)
        {
            bool match = false;
            PhotoGallery currentPhotoGallery = null;
            if (!String.IsNullOrEmpty(photoGalleryGuid))
            {
                foreach (PhotoGallery gallery in this.PhotoGalleries)
                {
                    if (photoGalleryGuid == gallery.Guid)
                    {
                        currentPhotoGallery = gallery;
                        match = true;
                        break;
                    }
                }
            }

            PhotoGallery = currentPhotoGallery;
            return match;
        }

        /// <summary>
        /// Selects the given photo gallery, makes it current, and navigates to the first photo album in that photo gallery.
        /// </summary>
        /// <param name="destinationPhotoGallery">PhotoGallery of photo gallery to switch to.</param>
        /// <param name="navigateToLastAlbum">True if navigation should take place to last album in  gallery on the switch. If false, navigates to gallery home
        /// </param>
        public virtual void SwitchToPhotoGallery(PhotoGallery destinationPhotoGallery, bool navigateToLastAlbum)
        {
            PhotoGallery = destinationPhotoGallery;

            // If navigateToLastAlbum parameter is true, navigate to last album, else navigate to first photo album
            if (navigateToLastAlbum)
            {
                if (NavigationCommands.NavigateToLastPhotoAlbumCommand.CanExecute(null))
                {
                    NavigationCommands.NavigateToLastPhotoAlbumCommand.Execute(null);
                }
            }
            else
            {
                if (NavigationCommands.NavigateToFirstPhotoAlbumCommand.CanExecute(null))
                {
                    NavigationCommands.NavigateToFirstPhotoAlbumCommand.Execute(null);
                }
            }
        }

        /// <summary>
        /// Request a SearchPhotoAlbum object which contains the search results.
        /// </summary>
        /// <param name="searchText">Text of search query.</param>
        /// <returns>A SearchPhotoAlbum containing the search results.</returns>
        public virtual SearchPhotoAlbum GenerateSearchPhotoAlbum(string searchText)
        {
            SearchPhotoAlbum searchPhotoAlbum = null;

            // Parameter validation
            if (String.IsNullOrEmpty(searchText))
            {
                return new SearchPhotoAlbum(searchText, new List<Photo>(), new List<PhotoNavigator>());
            }

            // Use only the first 1024 characters
            if (searchText.Length > 1024)
            {
                searchText = searchText.Substring(0, 1024);
            }

            if (searchText.StartsWith("tag:", StringComparison.OrdinalIgnoreCase) || searchText.StartsWith("explore:", StringComparison.OrdinalIgnoreCase))
            {
                // Search for a specific tag
                IList<Photo> searchResultPhotos = new List<Photo>();
                IList<PhotoNavigator> searchResultNavigators = new List<PhotoNavigator>();
                string[] parts = searchText.Split(':');
                this.SearchPhotoGalleriesForTag(parts[1], searchResultPhotos, searchResultNavigators);
                searchPhotoAlbum = new SearchPhotoAlbum(searchText, searchResultPhotos, searchResultNavigators);
            }
            else
            {
                string[] keywords = searchText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (keywords.Length == 0)
                {
                    searchPhotoAlbum = new SearchPhotoAlbum(searchText, new List<Photo>(), new List<PhotoNavigator>());
                }
                else
                {
                    // Search all photo galleries, and sort results
                    IList<Photo> searchResultPhotos = new List<Photo>();
                    IList<PhotoNavigator> searchResultNavigators = new List<PhotoNavigator>();
                    this.SearchPhotoGalleriesForKeywords(keywords, searchResultPhotos, searchResultNavigators);
                    searchPhotoAlbum = new SearchPhotoAlbum(searchText, searchResultPhotos, searchResultNavigators);
                }
            }

            return searchPhotoAlbum;
        }

        /// <summary>
        /// Processes command line args from ServiceProvider.
        /// </summary>
        /// <param name="commandLineArgs">Command line arguments passed as string collection.</param>
        public virtual void ProcessCommandLineArgs(IList<string> commandLineArgs)
        {
            if (commandLineArgs != null && commandLineArgs.Count > 0)
            {
                int argIndex = 0;
                while (argIndex < commandLineArgs.Count)
                {
                    string commandSwitch = commandLineArgs[argIndex];
                    if (String.Compare(commandSwitch.Substring(1), Strings.CommandLineArgsGuid, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (argIndex + 1 < commandLineArgs.Count)
                        {
                            string commandSwitchData = commandLineArgs[argIndex + 1];
                            if (!string.IsNullOrEmpty(commandSwitchData))
                            {
                                this.navigationOrigin = NavigationOrigin.External;
                                if (SyncState == SyncState.PendingFirstSync || SyncState == SyncState.SyncInProgress)
                                {
                                    this.guidNavigationPending = commandSwitchData;
                                }
                                else
                                {
                                    this.NavigateToGuid(commandSwitchData);
                                }

                                commandLineArgs = null;
                                break;
                            }
                        }
                    }

                    argIndex++;
                }
            }

            if (commandLineArgs != null)
            {
                ServiceProvider.Logger.Information("Count: {0}", commandLineArgs.Count);
                for (int i = 0; i < commandLineArgs.Count; i++)
                {
                    ServiceProvider.Logger.Information("Arg: {0}", commandLineArgs[i]);
                }
            }
        }

        /// <summary>
        /// Searches all photo galleries for photos with a certain tag, and adds photos that match to the results list.
        /// </summary>
        /// <param name="tag">String containing the tag text.</param>
        /// <param name="results">List of Photos which should be populated with search results.</param>
        /// <param name="resultNavigators">List of PhotoNavigators for the Photos found.</param>
        /// <remarks>
        /// If a Photo appears in multiple photo galleries, the corresponding Photo reflects the first photo gallery it was found in.
        /// </remarks>
        public virtual void SearchPhotoGalleriesForTag(string tag, IList<Photo> results, IList<PhotoNavigator> resultNavigators)
        {
            this.SearchPhotoGalleriesForTag(ServiceProvider.DataManager.TagStore.GetIdForTag(tag, false), results, resultNavigators);
        }

        /// <summary>
        /// Searches all photo galleries for photos with a certain tag, and adds photos that match to the results list.
        /// </summary>
        /// <param name="tagId">Short integer id of the tag to find.</param>
        /// <param name="results">List of Photos which should be populated with search results.</param>
        /// <param name="resultNavigators">List of PhotoNavigators for the Photos found.</param>
        /// <remarks>
        /// If a Photo appears in multiple photo galleries, the corresponding Photo reflects the first photo gallery it was found in.
        /// </remarks>
        public virtual void SearchPhotoGalleriesForTag(short tagId, IList<Photo> results, IList<PhotoNavigator> resultNavigators)
        {
            Dictionary<string, object> resultGuids = new Dictionary<string, object>();

            foreach (Navigator navigator in this.MasterNavigator.GetAllAlbumNavigators())
            {
                PhotoAlbumNavigator photoAlbumNavigator = navigator as PhotoAlbumNavigator;
                
                if (navigator != null)
                {
                    foreach (PhotoNavigator photoNavigator in photoAlbumNavigator.GetPhotos())
                    {
                        Photo photo = photoNavigator.Content as Photo;

                        if (photo != null)
                        {
                            if (photo.PhotoTagIds.Contains(tagId) && !resultGuids.ContainsKey(photo.Guid))
                            {
                                resultGuids.Add(photo.Guid, null);
                                results.Add(photo);
                                resultNavigators.Add(photoNavigator);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Raise property changed event.
        /// </summary>
        /// <param name="propertyName">Name of the property which has changed.</param>
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// On either remot update or load from cache, set sync state.
        /// </summary>
        protected virtual void OnUpdateStarted()
        {
            SyncState = SyncState.SyncInProgress;
        }

        /// <summary>
        /// Virtual handler for photo galleries updated event.
        /// </summary>
        protected virtual void OnPhotoGalleriesUpdated()
        {
            this.DoPhotoGalleriesUpdatedWork();
        }

        /// <summary>
        /// Virtual handler for update progress changed event, sets sync progress percentage.
        /// </summary>
        /// <param name="e">EventArgs describing the ProgressChanged event.</param>
        protected virtual void OnUpdateProgressChanged(ProgressChangedEventArgs e)
        {
            this.SyncProgress = e.ProgressPercentage;
        }

        /// <summary>
        /// When either local or remote update completes, set error message if any and status.
        /// </summary>
        /// <param name="e">EventArgs describing the AsyncCompleted event.</param>
        protected virtual void OnUpdateCompleted(AsyncCompletedEventArgs e)
        {
            if (e.Error == null || e.Cancelled)
            {
                this.SyncState = SyncState.NoSyncInProgress;
            }
            else
            {
                this.SyncState = SyncState.SyncFailed;
                this.SyncErrorMessage = e.Error.Message;
            }

            this.LastUpdated = ServiceProvider.DataManager.LastUpdateDate;
        }

        /// <summary>
        /// Gets content that should be displayed for a given navigator. In most cases, this is just the Navigator.Content object, except for 
        /// PhotoNavigators where is a photo-document wrapper class.
        /// </summary>
        /// <param name="navigator">Navigator whose content must be retrieved.</param>
        /// <returns>The object that forms the content of the Navigator.</returns>
        protected virtual object GetNavigatorContent(Navigator navigator)
        {
            object content = null;
            if (navigator != null)
            {
                PhotoNavigator photoNavigator = navigator as PhotoNavigator;
                PhotoAlbumNavigator albumNavigator = navigator as PhotoAlbumNavigator;
                PhotoSlideShowNavigator slideShowNavigator = navigator as PhotoSlideShowNavigator;
                if (photoNavigator != null)
                {
                    Photo photo = photoNavigator.Content as Photo;
                    content = photo;

                    // If photo navigator's parent is a photo album, set active photo album to this value
                    PhotoAlbumNavigator parent = photoNavigator.GetParent() as PhotoAlbumNavigator;
                    if (parent != null)
                    {
                        PhotoGalleryNavigator galleryNavigator = parent.GetParent() as PhotoGalleryNavigator;
                        if (galleryNavigator != null)
                        {
                            PhotoGallery newGallery = galleryNavigator.Content as PhotoGallery;
                            if (this.PhotoGallery != newGallery && newGallery != null)
                            {
                                this.PhotoGallery = newGallery;
                            }
                        }

                        this.ActivePhotoAlbum = parent.Content as PhotoAlbum;
                    }
                    else
                    {
                        this.ActivePhotoAlbum = null;
                    }

                    this.ActivePhoto = photo;
                }
                else if (albumNavigator != null)
                {
                    PhotoAlbum album = albumNavigator.Content as PhotoAlbum;
                    content = album;
                    this.ActivePhotoAlbum = album;
                    this.ActivePhoto = null;
                }
                else if (slideShowNavigator != null)
                {
                    PhotoSlideShow slideShow = slideShowNavigator.Content as PhotoSlideShow;
                    content = slideShow;
                    this.ActivePhotoAlbum = slideShow.Album.Content as PhotoAlbum;
                    this.ActivePhoto = null;
                }
                else
                {
                    // Return navigator's content 
                    content = navigator.Content;
                }
            }

            return content;
        }

        /// <summary>
        /// Perform new navigation.
        /// </summary>
        /// <param name="navigator">Navigator which is the target of new navigation.</param>
        /// <param name="mode">Navigation mode, whether back, forward, etc.</param>
        protected virtual void NavigateNew(Navigator navigator, ScePhotoNavigationMode mode)
        {
            if (navigator != null)
            {
                ViewManagerNavigatedEventArgs args = null;
                object content = this.GetNavigatorContent(navigator);
                if (Object.ReferenceEquals(navigator, this.CurrentNavigator))
                {
                    // Navigating to the same content. Args should show Refresh mode since this should not be separate journal entry.
                    // Ignore navigation mode passed in this case
                    args = new ViewManagerNavigatedEventArgs(content, ScePhotoNavigationMode.Refresh, new ViewContentState(navigator), this.CurrentNavigator, navigator);
                }
                else
                {
                    // Raise event indicating new navigation
                    args = new ViewManagerNavigatedEventArgs(content, mode, new ViewContentState(navigator), this.CurrentNavigator, navigator);
                }

                this.CurrentNavigator = navigator;
                this.OnNavigated(args);
            }
        }

        /// <summary>
        /// After a sync, current navigation state may not be valid. Refresh the state to check that the current navigator content still exists.
        /// </summary>
        protected virtual void RefreshNavigationState()
        {
            ViewManagerNavigatedEventArgs args = null;
            Navigator currentNavigator = null;
            ScePhotoNavigationMode mode = ScePhotoNavigationMode.Refresh;
            string path = String.Empty;
            if (PhotoGallery != null && !this.SelectPhotoGalleryByGuid(PhotoGallery.Guid))
            {
                // Cannot find current content. Navigate to error page
                NoDataAvailableError error = new NoDataAvailableError(PhotoGallery);

                // Pass null for new navigator to event args
                args = new ViewManagerNavigatedEventArgs(error, mode, null, null, null);
            }
            else
            {
                if (this.CurrentNavigator != null)
                {
                    path = this.CurrentNavigator.Path;
                }
                else if (!String.IsNullOrEmpty(this.journaledPath))
                {
                    path = this.journaledPath;
                    mode = this.journalNavigationMode;
                    this.journaledPath = String.Empty;
                }

                if (!String.IsNullOrEmpty(path))
                {
                    currentNavigator = this.MasterNavigator.GetChildNavigatorFromPath(path);
                }

                if (currentNavigator == null)
                {
                    // Cannot find current content. Navigate to error page
                    MissingItemError error = new MissingItemError(path);

                    // Pass null for new navigator to event args
                    args = new ViewManagerNavigatedEventArgs(error, mode, null, this.CurrentNavigator, null);
                }
                else
                {
                    this.CurrentNavigator = currentNavigator;
                    object content = this.GetNavigatorContent(currentNavigator);
                    args = new ViewManagerNavigatedEventArgs(content, mode, new ViewContentState(this.CurrentNavigator), this.CurrentNavigator, currentNavigator);
                }
            }

            this.OnNavigated(args);
        }

        /// <summary>
        /// Raise Navigated event with event args indicating content and type of navigation.
        /// </summary>
        /// <param name="e">ViewManagerNavigatedEventArgs providing information about the navigation.</param>
        protected virtual void OnNavigated(ViewManagerNavigatedEventArgs e)
        {
            this.LogNavigationInfo(e);

            if (this.Navigated != null)
            {
                this.Navigated(this, e);
            }
        }

        /// <summary>
        /// Log information about navigation path.
        /// </summary>
        /// <param name="e">EventArgs describing the ViewManager's Navigated event.</param>
        protected virtual void LogNavigationInfo(ViewManagerNavigatedEventArgs e)
        {
            if (e.Content is MissingItemError)
            {
                // Log error info
                MissingItemError error = e.Content as MissingItemError;
                ServiceProvider.Logger.Information(Strings.ViewManagerMissingItemError, error.Message);
            }
            else
            {
                string path = (e.NewNavigator != null) ? e.NewNavigator.Path : string.Empty;
                ServiceProvider.Logger.Information(Strings.ViewManagerNavigated, path);
            }
        }

        /// <summary>
        /// Searches all photo galleries for keywords, and adds photos that match to the results list.
        /// </summary>
        /// <param name="keywords">String array containing search keywords.</param>
        /// <param name="results">List of Photos which should be populated with search results.</param>
        /// <param name="resultNavigators">List of PhotoNavigators for the Photos found.</param>
        /// <remarks>
        /// If a Photo appears in multiple photo galleries, the corresponding Photo reflects the first photo gallery it was found in.
        /// </remarks>
        protected virtual void SearchPhotoGalleriesForKeywords(string[] keywords, IList<Photo> results, IList<PhotoNavigator> resultNavigators)
        {
            Dictionary<string, object> resultGuids = new Dictionary<string, object>();

            foreach (Navigator navigator in this.MasterNavigator.GetAllAlbumNavigators())
            {
                PhotoAlbumNavigator photoAlbumNavigator = navigator as PhotoAlbumNavigator;

                if (navigator != null)
                {
                    foreach (PhotoNavigator photoNavigator in photoAlbumNavigator.GetPhotos())
                    {
                        Photo photo = photoNavigator.Content as Photo;

                        if (photo != null)
                        {
                            for (int i = 0; i < keywords.Length; i++)
                            {
                                bool existInTitle = photo.Title.IndexOf(keywords[i], StringComparison.OrdinalIgnoreCase) >= 0;
                                bool existInDescription = photo.Title.IndexOf(keywords[i], StringComparison.OrdinalIgnoreCase) >= 0;
                                bool existInTags = photo.PhotoTagIds.Contains(ServiceProvider.DataManager.TagStore.GetIdForTag(keywords[i], false));

                                if ((existInTitle || existInDescription || existInTags) && !resultGuids.ContainsKey(photo.Guid))
                                {
                                    resultGuids.Add(photo.Guid, null);
                                    results.Add(photo);
                                    resultNavigators.Add(photoNavigator);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Do actual work for photo galleries updated event.
        /// </summary>
        protected virtual void DoPhotoGalleriesUpdatedWork()
        {
            this.NotifyPropertyChanged("PhotoGalleries");

            if (this.PhotoGalleries.Count > 0)
            {
                // If there was no previous navigation origin, then the user has not initiated any navigation. Navigate to default home.
                if (this.navigationOrigin == NavigationOrigin.None)
                {
                    // Navigate to the default home page, users can override to decide what they want to do here
                    this.NavigateToHomeDefault();
                }
                else
                {
                    if ((this.PhotoGallery == null || this.CurrentNavigator == null))
                    {
                        if (this.navigationOrigin == NavigationOrigin.Journal)
                        {
                            this.RefreshNavigationState();
                        }
                        else if (this.navigationOrigin == NavigationOrigin.External)
                        {
                            // Only other possibility on startup when DFC is null. Set PhotoGallery, but don't navigate
                            this.PhotoGallery = this.PhotoGalleries[0];

                            if (!string.IsNullOrEmpty(this.guidNavigationPending))
                            {
                                this.NavigateToGuid(this.guidNavigationPending);
                                this.guidNavigationPending = null;
                            }
                        }
                    }
                    else
                    {
                        if (this.navigationOrigin != NavigationOrigin.External)
                        {
                            // For any but external navigation, refresh
                            this.RefreshNavigationState();
                        }
                        else
                        {
                            // For external navigation, refresh PhotoGallery and feed navigators
                            if (this.PhotoGallery != null)
                            {
                                this.SelectPhotoGalleryByGuid(PhotoGallery.Guid);
                            }
                        }
                    }
                }
            }
            else
            {
                this.PhotoGallery = null;
            }
        }

        /// <summary>
        /// Navigates to the default home page when there is no pre determined navigation state.
        /// By default this is the first photo album of the first photo gallery. if this doesn't exist, we return
        /// a NoDataAvailable error.
        /// </summary>
        /// <remarks>
        /// Navigate to home default leaves navigation origin set to None because there is no user-initiated navigation.
        /// </remarks>
        protected void NavigateToHomeDefault()
        {
            if (this.PhotoGalleries != null && this.PhotoGalleries.Count > 0 && this.PhotoGalleries[0] != null && this.PhotoGalleries[0].PhotoAlbums.Count > 0)
            {
                this.PhotoGallery = this.PhotoGalleries[0];
                PhotoAlbumNavigator defaultHomePhotoAlbum = this.MasterNavigator.GetFirstPhotoAlbumNavigator();
                this.navigationOrigin = NavigationOrigin.None;
                this.NavigateNew(defaultHomePhotoAlbum, ScePhotoNavigationMode.Normal);
            }
            else
            {
                // No data in first feed. Navigate to NoDataAvailable error, passing empty photo gallery if possible
                NoDataAvailableError error;
                if (this.PhotoGalleries != null && this.PhotoGalleries.Count > 0)
                {
                    error = new NoDataAvailableError(this.PhotoGalleries[0]);
                }
                else
                {
                    error = new NoDataAvailableError(null);
                }

                ViewManagerNavigatedEventArgs args = new ViewManagerNavigatedEventArgs(error, ScePhotoNavigationMode.Normal, null, null, null);
                this.OnNavigated(args);
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Handler for a dialogs loaded event. Focuses the dialog once loaded.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Information about the event.</param>
        private static void Dialog_OnLoaded(object sender, RoutedEventArgs e)
        {
            UserControl dialog = sender as UserControl;
            if (dialog != null)
            {
                dialog.Loaded -= Dialog_OnLoaded;
                dialog.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        /// <summary>
        /// Default handler for a dialogs KeyDown event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">EventArgs describing the key event.</param>
        private static void Dialog_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Escape:
                    {
                        UserControl dialog = sender as UserControl;
                        if (dialog != null)
                        {
                            ServiceProvider.ViewManager.EndDialog(dialog);
                        }

                        break;
                    }

                default:
                    {
                        break;
                    }
            }
        }

        /// <summary>
        /// Initialize Data services - event listeners for DataManager events that set sync state, sync commands, etc.
        /// </summary>
        private void InitializeDataServices()
        {
            // Initially, sync has never occurred and first sync is pending
            this.syncState = SyncState.PendingFirstSync;

            // Listen for a sync to finish in the DataManager and update the SyncState accordingly
            this.syncCommands = new SyncCommands(this);
            ServiceProvider.DataManager.UpdateCompleted += new EventHandler<AsyncCompletedEventArgs>(this.DataManager_UpdateCompleted);
            ServiceProvider.DataManager.LoadCachedDataCompleted += new EventHandler<AsyncCompletedEventArgs>(this.DataManager_LoadCachedDataCompleted);
            ServiceProvider.DataManager.FeedsUpdated += new EventHandler<EventArgs>(this.DataManager_PhotoGalleriesUpdated);
            ServiceProvider.DataManager.UpdateStarted += new EventHandler<EventArgs>(this.DataManager_UpdateStarted);
            ServiceProvider.DataManager.LoadCachedDataStarted += new EventHandler<EventArgs>(this.DataManager_LoadCachedDataStarted);
            ServiceProvider.DataManager.UpdateProgressChanged += new EventHandler<ProgressChangedEventArgs>(this.DataManager_UpdateProgressChanged);
        }

        /// <summary>
        /// Initialize master navigator and navigation commands, navigation settings and navigation items.
        /// </summary>
        private void InitializeNavigationServices()
        {
            this.navigationCommands = new NavigationCommands(this);
            this.masterNavigator = new MasterNavigator(this);

            // Navigation settings for how photo albums and photos are accessed w/ deep pad navigation
            this.nestPhotoAlbumNavigation = true;
            this.nestPhotoNavigation = true;

            // Set navigation origin to none
            this.navigationOrigin = NavigationOrigin.None;
        }

        /// <summary>
        /// Handler for DataManager's PhotoGalleriesUpdatedEvent.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">EventArgs describing the event.</param>
        private void DataManager_PhotoGalleriesUpdated(object sender, EventArgs e)
        {
            this.OnPhotoGalleriesUpdated();
        }

        /// <summary>
        /// Handler for DataManager's UpdateCompleted event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">EventArgs describing the AsyncCompleted event.</param>
        private void DataManager_UpdateCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.OnUpdateCompleted(e);
        }

        /// <summary>
        /// Load from cached data started - from UI's point of view, this is the same as remote update completed.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">EventArgs describing the AsyncCompleted event.</param>
        private void DataManager_LoadCachedDataCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.OnUpdateCompleted(e);
        }

        /// <summary>
        /// Handler for DataManager's UpdateStarted event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">EventArgs describing the event.</param>
        private void DataManager_UpdateStarted(object sender, EventArgs e)
        {
            this.OnUpdateStarted();
        }

        /// <summary>
        /// Load from cached data started - set sync state to sync in progress.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Information about the event.</param>
        private void DataManager_LoadCachedDataStarted(object sender, EventArgs e)
        {
            this.OnUpdateStarted();
        }

        /// <summary>
        /// Handler for DataManager's UpdateProgressChanged event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Information about the progress changed event.</param>
        private void DataManager_UpdateProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.OnUpdateProgressChanged(e);
        }

        /// <summary>
        /// When a guid is passed in the command line, try to retrieve the photo and navigate to it.
        /// </summary>
        /// <param name="guid">Guid of object that is navigation target.</param>
        private void NavigateToGuid(string guid)
        {
            guid = Uri.EscapeDataString(guid);

            Navigator navigator = this.MasterNavigator.GetChildNavigatorFromGuid(guid);

            // Navigator navigator = this.MasterNavigator.GetChildNavigatorFromPath(path);
            this.NavigateNew(navigator, ScePhotoNavigationMode.Normal);
        }

        /// <summary>
        /// Notifies that the PhotoGallery has changed when its collection of albums changes so that the MasterNavigator can regenerate
        /// the navigators collection.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Arguments describing the event.</param>
        private void OnPhotoGalleryAlbumsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.NotifyPropertyChanged("PhotoGallery");
        }

        #endregion

        /// <summary>
        /// Custom content state for view navigators. Stores the navigator's path so it can be serialized in the journal and looked up on journal navigation.
        /// </summary>
        [Serializable]
        private class ViewContentState : CustomContentState
        {
            #region Fields
            /// <summary>
            /// The content name.
            /// </summary>
            private string name;

            /// <summary>
            /// The content path.
            /// </summary>
            private string path; 
            #endregion

            #region Constructor
            /// <summary>
            /// Contructor - creates ViewContentState on a navigation, given a Navigator that forms the target of that navigation. The constructor
            /// initializes the path and the name for the navigator, which appear in the navigation journal.
            /// </summary>
            /// <param name="navigator">The <see cref="Navigator"/> for which thie content state is created. ViewContentState
            /// is a CustomContentState, based on navigators, which uses the <see cref="Navigator.Path"/> property to save to the journal.</param>
            public ViewContentState(Navigator navigator)
            {
                if (navigator != null)
                {
                    this.path = navigator.Path;
                    if (navigator is NavigatableObjectNavigator)
                    {
                        this.name = ((NavigatableObject)navigator.Content).Title;
                    }
                    else
                    {
                        this.name = this.path;
                    }
                }
            } 
            #endregion

            #region Properties
            /// <summary>
            /// Gets the entry name that appears in the navigation journal for this navigator's content.
            /// </summary>
            public override string JournalEntryName
            {
                get
                {
                    return this.name;
                }
            } 
            #endregion

            #region Methods
            /// <summary>
            /// Called on journal navigation to this entry.
            /// </summary>
            /// <param name="navigationService">The NavigationService owned by the navigator responsible for the content to which this CustomContentState is being applied.</param>
            /// <param name="mode">A NavigationMode that specifies how the content to which the CustomContentState is being applied was navigated to.</param>
            public override void Replay(NavigationService navigationService, NavigationMode mode)
            {
                ServiceProvider.ViewManager.NavigateByJournal(this.path, mode);
            } 
            #endregion
        }
    }

    /// <summary>
    /// EventArgs providing information about a ViewManager navigation event.
    /// </summary>
    public class ViewManagerNavigatedEventArgs : EventArgs
    {
        #region Fields
        /// <summary>
        /// Navigation Mode (Back/Forward through Journal), etc.
        /// </summary>
        private ScePhotoNavigationMode navigationMode;

        /// <summary>
        /// Navigation target content.
        /// </summary>
        private object content;

        /// <summary>
        /// Content state saved to Journal.
        /// </summary>
        private CustomContentState contentStateToSave;

        /// <summary>
        /// Navigator for old content.
        /// </summary>
        private Navigator oldNavigator;

        /// <summary>
        /// Navigator for new content.
        /// </summary>
        private Navigator newNavigator; 
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor for ViewManagerNavigatedEventArgs.
        /// </summary>
        /// <param name="content">Content to which navigation has taken place.</param>
        /// <param name="navigationMode">Navigation mode.</param>
        /// <param name="contentStateToSave">CustomContentState to be persisted to the navigation journal for this navigation.</param>
        /// <param name="oldNavigator">Navigator for previously navigated content before navigation took place.</param>
        /// <param name="newNavigator">Navigator for current content after navigation.</param>
        public ViewManagerNavigatedEventArgs(object content, ScePhotoNavigationMode navigationMode, CustomContentState contentStateToSave, Navigator oldNavigator, Navigator newNavigator)
        {
            this.content = content;
            this.navigationMode = navigationMode;
            this.contentStateToSave = contentStateToSave;
            this.oldNavigator = oldNavigator;
            this.newNavigator = newNavigator;
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets the actual content target of this navigation.
        /// </summary>
        public object Content
        {
            get { return this.content; }
        }

        /// <summary>
        /// Gets the navigation mode - Back or Forward (through journal), New or Refresh. Only New warrants a new journal entry.
        /// </summary>
        public ScePhotoNavigationMode NavigationMode
        {
            get { return this.navigationMode; }
        }

        /// <summary>
        /// Gets the content state to be saved to the journal on new navigations.
        /// </summary>
        public CustomContentState ContentStateToSave
        {
            get { return this.contentStateToSave; }
        }

        /// <summary>
        /// Gets the navigator for old content.
        /// </summary>
        public Navigator OldNavigator
        {
            get { return this.oldNavigator; }
        }

        /// <summary>
        /// Gets the navigator for new content.
        /// </summary>
        public Navigator NewNavigator
        {
            get { return this.newNavigator; }
        } 
        #endregion
    }
}