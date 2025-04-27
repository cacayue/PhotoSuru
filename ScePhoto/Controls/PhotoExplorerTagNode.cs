//-----------------------------------------------------------------------
// <copyright file="PhotoExplorerTagNode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Photo Explorer node containing a tag.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using ScePhoto.Data;
    using ScePhoto.View;

    /// <summary>
    /// A Photo Explorer node containing a tag.
    /// </summary>
    public class PhotoExplorerTagNode : PhotoExplorerBaseNode
    {
        #region Fields
        /// <summary>
        /// The Id of the tag this node represents.
        /// </summary>
        private short tagId; 
        #endregion

        #region Constructor
        /// <summary>
        /// PhotoExplorerTagNode constructor; initializes the tag by Id.
        /// </summary>
        /// <param name="tagId">The short integer identifier of the tag this node represents.</param>
        public PhotoExplorerTagNode(short tagId)
            : base(String.Empty)
        {
            this.tagId = tagId;
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name of the tag.
        /// </summary>
        public override string Name
        {
            get
            {
                return ScePhoto.ServiceProvider.DataManager.TagStore.GetTagForId(this.tagId);
            }
        }

        /// <summary>
        /// Gets the related photo nodes for this tag.
        /// </summary>
        public override ObservableCollection<PhotoExplorerBaseNode> RelatedNodes
        {
            get
            {
                if (base.RelatedNodes.Count == 0)
                {
                    IList<Photo> relatedPhotos = new List<Photo>();
                    IList<PhotoNavigator> relatedPhotoNavigators = new List<PhotoNavigator>();
                    ServiceProvider.ViewManager.SearchPhotoGalleriesForTag(this.tagId, relatedPhotos, relatedPhotoNavigators);

                    for (int i = 0; i < relatedPhotoNavigators.Count && i < PhotoExplorerControl.MaximumDisplayedPhotos; i++)
                    {
                        base.RelatedNodes.Add(new PhotoExplorerPhotoNode(relatedPhotoNavigators[i]));
                    }
                }

                return base.RelatedNodes;
            }
        } 
        #endregion

        #region Methods
        /// <summary>
        /// Creates a new PhotoExplorerTagNode given a tag's text.
        /// </summary>
        /// <param name="tag">The tag text.</param>
        /// <returns>A new PhotoExplorerTagNode.</returns>
        public static PhotoExplorerTagNode CreateTagNodeFromTag(string tag)
        {
            return new PhotoExplorerTagNode(ServiceProvider.DataManager.TagStore.GetIdForTag(tag, false));
        } 
        #endregion
    }
}
