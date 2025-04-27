//-----------------------------------------------------------------------
// <copyright file="NavigatableObject.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Base class for Item and Channel so that the navigation can deal
//     with these objects in a common fashion.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Data
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Base class for Item and Channel so that the navigation can deal with these objects in a common fashion.
    /// </summary>
    public abstract class NavigatableObject
    {
        /// <summary>
        /// A string that uniquely identifies the object.
        /// </summary>
        private string guid;

        /// <summary>
        /// The name of the channel.
        /// </summary>
        private string title;

        /// <summary>
        /// Default constructor for NavigatableObject.
        /// </summary>
        protected NavigatableObject()
        {
            this.title = String.Empty;
            this.guid = String.Empty;
        }

        /// <summary>
        /// Constructor for Navigatable Object.
        /// </summary>
        /// <param name="title">The NavigatableObject's title.</param>
        /// <param name="guid">The NavigatableObject's guid.</param>
        protected NavigatableObject(string title, string guid)
        {
            this.title = title;
            this.guid = guid;
        }

        /// <summary>
        /// Gets or sets a string that uniquely identifies the object.
        /// </summary>
        public string Guid
        {
            get { return this.guid; }
            set { this.guid = value; }
        }

        /// <summary>
        /// Gets or sets the name of the channel.
        /// </summary>
        public string Title
        {
            get { return this.title; }
            set { this.title = value; }
        }
    }
}
