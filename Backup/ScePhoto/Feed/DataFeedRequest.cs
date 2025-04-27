//-----------------------------------------------------------------------
// <copyright file="DataFeedRequest.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Represents an asynchronous request for data.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Feed
{
    using System;
    using Microsoft.SubscriptionCenter.Sync;

    /// <summary>
    /// Represents an asynchronous request for data.
    /// </summary>
    public class DataFeedRequest : AsyncDataRequest
    {
        /// <summary>
        /// The type of content associated with a resource.
        /// </summary>
        private ResourceContentType contentType;

        /// <summary>
        /// Flag indicating whether or not requesting local only data.
        /// </summary>
        private bool localData;

        /// <summary>
        /// Initializes the DataFeedRequest instance.
        /// </summary>
        /// <param name="contentType">The type of content associated with a resource.</param>
        /// <param name="localData">Whether or not requesting local only data.</param>
        /// <param name="uri">The URI of the resource to retrieve.</param>
        /// <param name="userState">The user-supplied state object.</param>
        public DataFeedRequest(ResourceContentType contentType, bool localData, Uri uri, object userState)
            : base(uri, userState)
        {
            this.contentType = contentType;
            this.localData = localData;
        }

        /// <summary>
        /// Gets the type of content associated with a resource.
        /// </summary>
        public ResourceContentType ContentType
        {
            get { return this.contentType; }
        }

        /// <summary>
        /// Gets a value indicating whether or not requesting local only data.
        /// </summary>
        public bool LocalData
        {
            get { return this.localData; }
        }
    }
}
