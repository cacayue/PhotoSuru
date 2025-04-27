//-----------------------------------------------------------------------
// <copyright file="PhotoExplorerBaseNode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     The base class from which other Photo Explorer Nodes derive.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Controls
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// The base type for all nodes displayed by the photo explorer.
    /// </summary>
    public class PhotoExplorerBaseNode
    {
        #region Fields
        /// <summary>
        /// Nodes the are related in some way to the current node.
        /// </summary>
        private ObservableCollection<PhotoExplorerBaseNode> relatedNodes = new ObservableCollection<PhotoExplorerBaseNode>();

        /// <summary>
        /// The name of the current node.
        /// </summary>
        private string name; 
        #endregion

        #region Properties
        /// <summary>
        /// PhotoExplorerBaseNode constructor; initializes the node name.
        /// </summary>
        /// <param name="name">The name to display for the node.</param>
        public PhotoExplorerBaseNode(string name)
        {
            this.name = name;
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets the collection of nodes that are related in some way to the current node.
        /// </summary>
        public virtual ObservableCollection<PhotoExplorerBaseNode> RelatedNodes
        {
            get { return this.relatedNodes; }
        }

        /// <summary>
        /// Gets the name of the current node.
        /// </summary>
        public virtual string Name
        {
            get { return this.name; }
        } 
        #endregion
    }
}
