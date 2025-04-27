//-----------------------------------------------------------------------
// <copyright file="PhotoExplorerPhotoNode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Photo Explorer node containing a photo.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Controls
{
    using System;
    using System.Collections.ObjectModel;
    using ScePhoto.View;
    using ScePhoto.Data;

    /// <summary>
    /// A Photo Explorer node containing a photo.
    /// </summary>
    public class PhotoExplorerPhotoNode : PhotoExplorerBaseNode
    {
        #region Fields
        /// <summary>
        /// The photo object this node references.
        /// </summary>
        private PhotoNavigator photoNavigator;

        /// <summary>
        /// The photo navigator containing this object's original location.
        /// </summary>
        private Photo photo;
        #endregion

        #region Constructor
        /// <summary>
        /// PhotoExplorerPhotoNode constructor; initializes this node's photo.
        /// </summary>
        /// <param name="photoNavigator">The photo navigator to the photo this node represents.</param>
        public PhotoExplorerPhotoNode(PhotoNavigator photoNavigator)
            : base(String.Empty)
        {
            this.photoNavigator = photoNavigator;
            this.photo = photoNavigator.Content as Photo;
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets the photo object that this node references.
        /// </summary>
        public PhotoNavigator PhotoNavigator
        {
            get { return this.photoNavigator; }
        }

        /// <summary>
        /// Gets the photo object that this node references.
        /// </summary>
        public Photo Photo
        {
            get { return this.photo; }
        }

        /// <summary>
        /// Gets the related tag nodes for this photo.
        /// </summary>
        public override ObservableCollection<PhotoExplorerBaseNode> RelatedNodes
        {
            get
            {
                if (base.RelatedNodes.Count == 0)
                {
                    foreach (short tagId in this.Photo.PhotoTagIds)
                    {
                        base.RelatedNodes.Add(new PhotoExplorerTagNode(tagId));
                    }
                }

                return base.RelatedNodes;
            }
        } 
        #endregion
    }
}
