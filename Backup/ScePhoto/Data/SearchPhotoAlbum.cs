//-----------------------------------------------------------------------
// <copyright file="SearchPhotoAlbum.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Created photo album used for the current search results.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Data
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using ScePhoto.View;
    using Microsoft.SubscriptionCenter.Sync;

    /// <summary>
    /// This photo album is created only upon a search request.
    /// </summary>
    public class SearchPhotoAlbum : PhotoAlbum
    {
        #region Private Fields
        /// <summary>
        /// The Photo collection for the search.
        /// </summary>
        private IList<Photo> searchResults;

        /// <summary>
        /// The collection of photo navigators for the search.
        /// </summary>
        private IList<PhotoNavigator> searchNavigators;

        /// <summary>
        /// The query text for the search.
        /// </summary>
        private string searchText;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor initializes SearchPhotoAlbum with relevant search information.
        /// </summary>
        /// <param name="searchText">
        /// Search text used as the search query.
        /// </param>
        /// <param name="searchResults">
        /// Collection of Photos that have been found in response to the search query.
        /// </param>
        /// <param name="searchNavigators">
        /// Collection of PhotoNavigators for the photos found in the search.
        /// </param>
        public SearchPhotoAlbum(string searchText, IList<Photo> searchResults, IList<PhotoNavigator> searchNavigators)
            : base(Strings.SearchName, Strings.SearchDescription, DateTime.Now, new CsxRevision(DateTime.Now).ChangeDate, new GuidStore(), new PhotoCollection())
        {
            this.Guid = "[search]";

            if (searchText == null)
            {
                throw new ArgumentNullException("searchText");
            }

            if (searchResults == null)
            {
                throw new ArgumentNullException("searchResults");
            }

            this.searchText = searchText;
            this.searchResults = searchResults;
            this.searchNavigators = searchNavigators;

            IList<Photo> searchPhotos = new List<Photo>();
            for (int i = 0, count = searchResults.Count; i < count; i++)
            {
                searchPhotos.Add(searchResults[i]);
            }

            this.Photos.Merge(searchPhotos);
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets the Photo collection for the search.
        /// </summary>
        public IList<Photo> SearchResults
        {
            get { return this.searchResults; }
        }

        /// <summary>
        /// Gets the collection of photo navigators for the search.
        /// </summary>
        public IList<PhotoNavigator> PhotoNavigators
        {
            get { return this.searchNavigators; }
        }

        /// <summary>
        /// Gets the query text for the search request.
        /// </summary>
        public string SearchText
        {
            get { return this.searchText; }
        }
        #endregion
    }
}
