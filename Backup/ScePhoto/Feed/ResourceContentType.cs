//-----------------------------------------------------------------------
// <copyright file="ResourceContentType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     The ResourceContentType enumeration defines a type of content 
//     associated with a resource.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Feed
{
    using System;

    /// <summary>
    /// Defines a type of content associated with a resource.
    /// </summary>
    public enum ResourceContentType
    {
        /// <summary>
        /// Xml document.
        /// </summary>
        Xml,

        /// <summary>
        /// Image content.
        /// </summary>
        Image,

        /// <summary>
        /// Text content.
        /// </summary>
        Text
    }
}
