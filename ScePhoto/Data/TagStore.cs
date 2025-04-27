//-----------------------------------------------------------------------
// <copyright file="TagStore.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Stores a collection of tag strings by assigning them a short integer ID reference.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Data
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    
    /// <summary>
    /// Stores a collection of tag strings by assigning them a short integer ID reference.
    /// </summary>
    public class TagStore
    {
        /// <summary>
        /// The key to use when inserting a new tag.  0 indicates tag not found.
        /// </summary>
        private short currentTagId = 1;

        /// <summary>
        /// The dictionary containing all of the tag/id pairs.
        /// </summary>
        private Dictionary<short, string> tagDictionary = new Dictionary<short, string>();

        /// <summary>
        /// Gets a short integer id for the provided tag.
        /// </summary>
        /// <param name="tag">The tag to retrieve the id for.</param>
        /// <param name="addIfNotPresent">A value indicating whether the tag should be added to the store if it's not currently present.</param>
        /// <returns>The short integer id for the provided tag.</returns>
        public short GetIdForTag(string tag, bool addIfNotPresent)
        {
            short tagId = 0;

            if (tag != null)
            {
                foreach (short key in this.tagDictionary.Keys)
                {
                    if (this.tagDictionary[key] == tag)
                    {
                        tagId = key;
                        break;
                    }
                }

                if (tagId == 0 && addIfNotPresent)
                {
                    tagId = this.AddTag(tag);
                }
            }

            return tagId;
        }

        /// <summary>
        /// Gets the tag corresponding to the short integer tag id.
        /// </summary>
        /// <param name="tagId">The id of the tag to retrieve.</param>
        /// <returns>The corresponding tag string.</returns>
        public string GetTagForId(short tagId)
        {
            string tag;
            if (tagId != 0)
            {
                this.tagDictionary.TryGetValue(tagId, out tag);
            }
            else
            {
                tag = "unknown tag";
            }

            return tag;
        }

        /// <summary>
        /// Adds a tag to the TagStore.
        /// </summary>
        /// <param name="tag">The tag to add to the TagStore.</param>
        /// <returns>The Id of the added tag.</returns>
        private short AddTag(string tag)
        {
            this.tagDictionary.Add(this.currentTagId, tag);
            this.currentTagId++;
            return Convert.ToInt16(this.currentTagId - 1);
        }
    }
}
