//-----------------------------------------------------------------------
// <copyright file="GuidStore.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Dictionary of objects from the data feed indexed by guid to allow quick guid-based lookup and avoid guid duplication.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Data
{
    using System;
    using System.Collections.Generic;
    using Microsoft.SubscriptionCenter.Sync;

    /// <summary>
    /// Dictionary of objects from the data feed indexed by guid to allow quick guid-based lookup and avoid guid duplication.
    /// </summary>
    public class GuidStore : Dictionary<string, Item>
    {
        /// <summary>
        /// Initializes the GuidStore instance.
        /// </summary>
        public GuidStore()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        /// Merges data from another GuidStore into this GuidStore.
        /// </summary>
        /// <param name="oldGuidStore">The GuidStore containing old data.</param>
        public void Merge(GuidStore oldGuidStore)
        {
            if (oldGuidStore == null)
            {
                throw new ArgumentNullException("oldGuidStore");
            }

            foreach (Item item in this.Values)
            {
                if (oldGuidStore.ContainsKey(item.Guid))
                {
                    Item oldItem = oldGuidStore[item.Guid];
                    if (CsxRevision.Compare(item.Revision, oldItem.Revision) == 0)
                    {
                        item.Merge(oldItem);
                    }
                }
            }
        }

        /// <summary>
        /// Resets IsNew flag on all items in the GuidStore.
        /// </summary>
        public void ResetIsNew()
        {
            foreach (Item item in this.Values)
            {
                item.IsNew = false;
            }
        }
    }
}
