//-----------------------------------------------------------------------
// <copyright file="PhotoFeedConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     The CsxRssConverter class provides methods for conversion NR RSS 
//     data streams to actual data objects.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Feed
{
    using System;
    using System.Xml;
    using System.IO;
    using System.Xml.XPath;
    using System.Text;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using Microsoft.SubscriptionCenter.Sync;
    using ScePhoto.Data;

    /// <summary>
    /// Provides methods for conversion CSX RSS data streams to actual PhotoViewer data objects.
    /// </summary>
    public class PhotoFeedConverter
    {
        #region Fields

        /// <summary>
        /// The URI for Content Sync Extensions (CSX) namespace definition.
        /// </summary>
        private static string csxNamespaceUri = "http://schemas.microsoft.com/rss/2007/contentsyncextensions";

        /// <summary>
        /// The URI for Simple Sharing Extensions (SSE) namespace definition.
        /// </summary>
        private static string sseNamespaceUri = "http://feedsync.org/2007/feedsync";

        /// <summary>
        /// The URI for Media RSS (MRSS) namespace definition.
        /// </summary>
        private static string mediaNamespaceUri = "http://search.yahoo.com/mrss";

        /// <summary>
        /// The Xml namespace manager used for loading NR RSS documents.
        /// </summary>
        private XmlNamespaceManager namespaceManager;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the URI for Content Sync Extensions (CSX) namespace definition.
        /// </summary>
        protected static string CsxNamespaceUri
        {
            get { return csxNamespaceUri; }
        }

        /// <summary>
        /// Gets the URI for Simple Sharing Extensions (SSE) namespace definition.
        /// </summary>
        protected static string SseNamespaceUri
        {
            get { return sseNamespaceUri; }
        }

        /// <summary>
        /// Gets the URI for Media RSS (MRSS) namespace definition.
        /// </summary>
        protected static string MediaNamespaceUri
        {
            get { return mediaNamespaceUri; }
        }

        /// <summary>
        /// Gets the prefix identifying the converter during message logging. 
        /// </summary>
        protected virtual string MessagePrefix
        {
            get { return "PhotoFeedConverter"; }
        }

        /// <summary>
        /// Gets the Xml namespace manager used for loading RSS documents.
        /// </summary>
        protected XmlNamespaceManager NamespaceManager
        {
            get
            {
                if (this.namespaceManager == null)
                {
                    this.namespaceManager = this.CreateNamespaceManager();
                }

                return this.namespaceManager;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Retrieves MasterFeedContent from specified Xml document.
        /// </summary>
        /// <param name="document">XPathDocument representing Xml document.</param>
        /// <param name="source">A string representing the source of the data.</param>
        /// <returns>The MasterFeedContent retrieved from specified Xml document.</returns>
        public virtual MasterFeedContent ConvertToMasterFeedContent(XPathDocument document, string source)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            MasterFeedContent masterFeedContent = null;
            XPathNavigator rootNavigator = document.CreateNavigator();
            XPathNavigator navigator = rootNavigator.SelectSingleNode("rss/channel");
            if (navigator != null)
            {
                // Get common channel data.
                string title, description, link;
                DateTime publishDate, changeDate;
                bool channelDataIsValid = this.GetChannelDataFromNavigator(navigator, source, out title, out description, out link, out publishDate, out changeDate);
                if (channelDataIsValid)
                {
                    // Get nested feeds
                    GuidStore guidStore = new GuidStore();
                    IList<FeedItem> nestedFeeds = new List<FeedItem>();
                    this.GetNestedFeedsFromNavigator(navigator, source, guidStore, nestedFeeds);
                    masterFeedContent = new MasterFeedContent(title, description, publishDate, changeDate, guidStore, nestedFeeds);
                }
            }
            else
            {
                ServiceProvider.Logger.Error(Strings.ConvertersMissingElement, this.MessagePrefix, GetNavigatorPath(navigator, source), "rss/channel");
            }

            return masterFeedContent;
        }

        /// <summary>
        /// Retrieves PhotoGallery from specified Xml document.
        /// </summary>
        /// <param name="document">XPathDocument representing Xml document.</param>
        /// <param name="source">A string representing the source of the data.</param>
        /// <returns>The PhotoGallery retrieved from specified Xml document.</returns>
        public virtual PhotoGallery ConvertToPhotoGallery(XPathDocument document, string source)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            PhotoGallery photoGallery = null;
            XPathNavigator rootNavigator = document.CreateNavigator();
            XPathNavigator navigator = rootNavigator.SelectSingleNode("rss/channel");
            if (navigator != null)
            {
                // Get common channel data.
                string title, description, link;
                DateTime publishDate, changeDate;
                bool channelDataIsValid = this.GetChannelDataFromNavigator(navigator, source, out title, out description, out link, out publishDate, out changeDate);
                if (channelDataIsValid)
                {
                    // Get nested feeds
                    GuidStore guidStore = new GuidStore();
                    IList<FeedItem> nestedFeeds = new List<FeedItem>();
                    this.GetNestedFeedsFromNavigator(navigator, source, guidStore, nestedFeeds);
                    photoGallery = new PhotoGallery(title, description, publishDate, changeDate, guidStore, nestedFeeds);
                }
            }
            else
            {
                ServiceProvider.Logger.Error(Strings.ConvertersMissingElement, this.MessagePrefix, GetNavigatorPath(navigator, source), "rss/channel");
            }

            return photoGallery;
        }

        /// <summary>
        /// Retrieves PhotoAlbum from specified Xml document.
        /// </summary>
        /// <param name="document">XPathDocument representing Xml document.</param>
        /// <param name="source">A string representing the source of the data.</param>
        /// <returns>The PhotoAlbum retrieved from specified Xml document.</returns>
        public virtual PhotoAlbum ConvertToPhotoAlbum(XPathDocument document, string source)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            PhotoAlbum photoAlbum = null;
            XPathNavigator rootNavigator = document.CreateNavigator();
            XPathNavigator navigator = rootNavigator.SelectSingleNode("rss/channel");
            if (navigator != null)
            {
                // Get common channel data.
                string title, description, link;
                DateTime publishDate, changeDate;
                bool channelDataIsValid = this.GetChannelDataFromNavigator(navigator, source, out title, out description, out link, out publishDate, out changeDate);
                if (channelDataIsValid)
                {
                    // Get photos
                    GuidStore guidStore = new GuidStore();
                    IList<Photo> photos = new List<Photo>();
                    XPathNodeIterator iterator = navigator.SelectChildren("item", string.Empty);
                    while (iterator.MoveNext())
                    {
                        // Retrive Photo object from xml node
                        Photo photo = this.GetPhotoFromNavigator(iterator.Current, source);
                        if (photo != null)
                        {
                            if (!guidStore.ContainsKey(photo.Guid))
                            {
                                guidStore.Add(photo.Guid, photo);
                                photos.Add(photo);
                            }
                            else
                            {
                                ServiceProvider.Logger.Error(Strings.ConvertersDuplicateGuid, this.MessagePrefix, GetNavigatorPath(iterator.Current, source), photo.Guid);
                            }
                        }
                    }

                    PhotoCollection photoCollection = new PhotoCollection(photos);
                    photoAlbum = new PhotoAlbum(title, description, publishDate, changeDate, guidStore, photoCollection);
                }
            }
            else
            {
                ServiceProvider.Logger.Error(Strings.ConvertersMissingElement, this.MessagePrefix, GetNavigatorPath(navigator, source), "rss/channel");
            }

            return photoAlbum;
        }

        /// <summary>
        /// Retrieves text content from specified xml node.
        /// </summary>
        /// <param name="navigator">Navigator representing xml node.</param>
        /// <returns>The text content retrieved from specified xml node.</returns>
        protected static string GetTextFromNavigator(XPathNavigator navigator)
        {
            return (navigator != null) ? navigator.Value : string.Empty;
        }

        /// <summary>
        /// Returns string representation of all ancestors of specified node, including the node.
        /// </summary>
        /// <param name="navigator">Navigator representing xml node.</param>
        /// <param name="source">A string representing the source of the data.</param>
        /// <returns>The string representation of all ancestors of specified node, including the node.</returns>
        protected static string GetNavigatorPath(XPathNavigator navigator, string source)
        {
            StringBuilder ancestors = new StringBuilder();
            if (navigator != null)
            {
                XPathNodeIterator iterator = navigator.SelectAncestors(XPathNodeType.Element, true);
                foreach (XPathNavigator node in iterator)
                {
                    ancestors.Insert(0, "/" + node.Name);
                }
            }

            string sourceInfo;
            IXmlLineInfo lineInfo = navigator as IXmlLineInfo;
            if (lineInfo != null)
            {
                sourceInfo = String.Format(CultureInfo.InvariantCulture, "[{0};LINE:{1};POS:{2}]", source, lineInfo.LineNumber.ToString(CultureInfo.InvariantCulture), lineInfo.LinePosition.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                sourceInfo = String.Format(CultureInfo.InvariantCulture, "[{0}]", source);
            }

            ancestors.Insert(0, sourceInfo);
            return ancestors.ToString();
        }

        /// <summary>
        /// Creates the Xml namespace manager used for loading RSS documents.
        /// </summary>
        /// <returns>The Xml namespace manager used for loading RSS documents.</returns>
        protected virtual XmlNamespaceManager CreateNamespaceManager()
        {
            NameTable nameTable = new NameTable();

            // csx namespace
            nameTable.Add("lastBuildDate");
            nameTable.Add("link");

            // sx namespace
            nameTable.Add("sync");
            nameTable.Add("history");

            // media namespace
            nameTable.Add("content");
            nameTable.Add("thumbnail");

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(nameTable);
            xmlNamespaceManager.AddNamespace("csx", csxNamespaceUri);
            xmlNamespaceManager.AddNamespace("sx", sseNamespaceUri);
            xmlNamespaceManager.AddNamespace("media", mediaNamespaceUri);
            return xmlNamespaceManager;
        }

        /// <summary>
        /// Retrieves common channel data from specified xml sub-node.
        /// </summary>
        /// <param name="navigator">Navigator representing xml sub-node.</param>
        /// <param name="source">A string representing the source of the data.</param>
        /// <param name="title">The name of the channel.</param>
        /// <param name="description">The description of the channel.</param>
        /// <param name="link">The URL to the website corresponding to the channel.</param>
        /// <param name="publishDate">The publication date for the content of the channel.</param>
        /// <param name="changeDate">The last time the content of the channel changed.</param>
        /// <returns>True if all required data has been successfully retrieved.</returns>
        protected virtual bool GetChannelDataFromNavigator(XPathNavigator navigator, string source, out string title, out string description, out string link, out DateTime publishDate, out DateTime changeDate)
        {
            bool dataIsValid = true;
            XPathNavigator childNavigator;

            // Title
            childNavigator = navigator.SelectSingleNode("title");
            title = GetTextFromNavigator(childNavigator);
            if (childNavigator == null)
            {
                ServiceProvider.Logger.Error(Strings.ConvertersMissingElement, this.MessagePrefix, GetNavigatorPath(navigator, source), "title");
                dataIsValid = false;
            }

            // Description
            childNavigator = navigator.SelectSingleNode("description");
            description = GetTextFromNavigator(childNavigator);
            if (childNavigator == null)
            {
                ServiceProvider.Logger.Error(Strings.ConvertersMissingElement, this.MessagePrefix, GetNavigatorPath(navigator, source), "description");
                dataIsValid = false;
            }

            // Link
            childNavigator = navigator.SelectSingleNode("link");
            link = GetTextFromNavigator(childNavigator);
            if (childNavigator == null)
            {
                ServiceProvider.Logger.Error(Strings.ConvertersMissingElement, this.MessagePrefix, GetNavigatorPath(navigator, source), "link");
                dataIsValid = false;
            }

            // PublishDate
            childNavigator = navigator.SelectSingleNode("pubDate");
            publishDate = this.GetDateFromNavigator(childNavigator, source);
            if (childNavigator == null)
            {
                ServiceProvider.Logger.Warning(Strings.ConvertersMissingElement, this.MessagePrefix, GetNavigatorPath(navigator, source), "pubDate");
            }

            // ChnageDate
            // If lastBuildDate is not specified, use pubDate as ChangeDate.
            childNavigator = navigator.SelectSingleNode("lastBuildDate");
            changeDate = this.GetDateFromNavigator(childNavigator, source);
            if (childNavigator == null)
            {
                ServiceProvider.Logger.Warning(Strings.ConvertersMissingElement, this.MessagePrefix, GetNavigatorPath(navigator, source), "lastBuildDate");
            }

            if (DateTime.Compare(changeDate, DateTime.MinValue) == 0)
            {
                changeDate = publishDate;
            }

            return dataIsValid;
        }

        /// <summary>
        /// Retrieves common item data from specified xml sub-node.
        /// </summary>
        /// <param name="navigator">Navigator representing xml sub-node.</param>
        /// <param name="source">A string representing the source of the data.</param>
        /// <param name="title">The name of the item.</param>
        /// <param name="guid">The string that uniquely identifies the item.</param>
        /// <param name="webLink">The Uri to the website corresponding to the item.</param>
        /// <param name="publishDate">The publication date for the content of the item.</param>
        /// <returns>True if all required data has been successfully retrieved.</returns>
        protected virtual bool GetItemDataFromNavigator(XPathNavigator navigator, string source, out string title, out string guid, out string webLink, out DateTime publishDate)
        {
            bool dataIsValid = true;
            XPathNavigator childNavigator;

            // Title
            childNavigator = navigator.SelectSingleNode("title");
            title = GetTextFromNavigator(childNavigator);
            if (childNavigator == null)
            {
                ServiceProvider.Logger.Error(Strings.ConvertersMissingElement, this.MessagePrefix, GetNavigatorPath(navigator, source), "title");
                dataIsValid = false;
            }

            // Guid
            childNavigator = navigator.SelectSingleNode("guid");
            guid = GetTextFromNavigator(childNavigator);
            if (childNavigator == null)
            {
                ServiceProvider.Logger.Error(Strings.ConvertersMissingElement, this.MessagePrefix, GetNavigatorPath(navigator, source), "guid");
                dataIsValid = false;
            }

            if (string.IsNullOrEmpty(guid) && childNavigator != null)
            {
                ServiceProvider.Logger.Error(Strings.ConvertersInvalidElementValue, this.MessagePrefix, GetNavigatorPath(childNavigator, source), guid);
                dataIsValid = false;
            }

            // Web Uri
            childNavigator = navigator.SelectSingleNode("link");
            webLink = GetTextFromNavigator(childNavigator);

            // PublishDate
            childNavigator = navigator.SelectSingleNode("pubDate");
            publishDate = this.GetDateFromNavigator(childNavigator, source);

            return dataIsValid;
        }

        /// <summary>
        /// Retrieves common link item data from specified xml sub-node.
        /// </summary>
        /// <param name="navigator">Navigator representing xml sub-node.</param>
        /// <param name="source">A string representing the source of the data.</param>
        /// <param name="title">The name of the item.</param>
        /// <param name="guid">The string that uniquely identifies the item.</param>
        /// <param name="webLink">The Uri to the website corresponding to the item.</param>
        /// <param name="linkUri">The URI representing the location of associated resource.</param>
        /// <param name="publishDate">The publication date for the content of the item.</param>
        /// <param name="revision">The revision information for the item.</param>
        /// <returns>True if all required data has been successfully retrieved.</returns>
        protected virtual bool GetLinkItemDataFromNavigator(XPathNavigator navigator, string source, out string title, out string guid, out string webLink, out Uri linkUri, out DateTime publishDate, out CsxRevision revision)
        {
            linkUri = null;
            revision = null;

                bool dataIsValid = this.GetItemDataFromNavigator(navigator, source, out title, out guid, out webLink, out publishDate);
                if (dataIsValid)
                {
                    XPathNavigator childNavigator;

                    // csx:Link
                    childNavigator = navigator.SelectSingleNode("csx:link", this.NamespaceManager);
                    if (childNavigator != null)
                    {
                        string link = GetTextFromNavigator(childNavigator);
                        if (!Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out linkUri))
                        {
                            ServiceProvider.Logger.Error(Strings.ConvertersInvalidElementValue, this.MessagePrefix, GetNavigatorPath(childNavigator, source), link);
                            dataIsValid = false;
                        }
                    }
                    else
                    {
                        ServiceProvider.Logger.Error(Strings.ConvertersMissingElement, this.MessagePrefix, GetNavigatorPath(navigator, source), "csx:link");
                        dataIsValid = false;
                    }

                    // Revision.
                    revision = this.GetRevisionFromNavigator(navigator, source, publishDate);
                    if (revision == null)
                    {
                        dataIsValid = false;
                    }
                }

            return dataIsValid;
        }

        /// <summary>
        /// Retrieves revision from specified xml node.
        /// </summary>
        /// <param name="navigator">Navigator representing xml node.</param>
        /// <param name="source">A string representing the source of the data.</param>
        /// <param name="publishDate">The publication date for the content of the item.</param>
        /// <returns>The revision retrieved from specified xml node.</returns>
        protected virtual CsxRevision GetRevisionFromNavigator(XPathNavigator navigator, string source, DateTime publishDate)
        {
            CsxRevision revision = null;
            bool deletedItem = false;

            // Always prefer data coming from SSE.
            XPathNavigator childNavigator = navigator.SelectSingleNode("sx:sync", this.NamespaceManager);
            if (childNavigator != null)
            {
                // Get sx:deleted attribute.
                string attributeValue = childNavigator.GetAttribute("deleted", string.Empty);
                if (!bool.TryParse(attributeValue, out deletedItem))
                {
                    if (!string.IsNullOrEmpty(attributeValue))
                    {
                        ServiceProvider.Logger.Warning(Strings.ConvertersInvalidAttributeValue, this.MessagePrefix, GetNavigatorPath(childNavigator, source), "deleted", attributeValue);
                    }
                }

                if (!deletedItem)
                {
                    int highestSequenceNumber = 0;
                    DateTime changeDate = DateTime.MinValue;

                    // Retrieve the collection of all sx:history elements in the sx:sync element and 
                    // search for the highest sequence number.
                    XPathNodeIterator iterator = childNavigator.SelectChildren("history", SseNamespaceUri);
                    while (iterator.MoveNext())
                    {
                        // Get sx:sequence attribute.
                        int currentSequenceNumber;
                        attributeValue = iterator.Current.GetAttribute("sequence", string.Empty);
                        if (!int.TryParse(attributeValue, out currentSequenceNumber))
                        {
                            ServiceProvider.Logger.Warning(Strings.ConvertersInvalidAttributeValue, this.MessagePrefix, GetNavigatorPath(iterator.Current, source), "sequence", attributeValue);
                        }
                        else if (currentSequenceNumber > highestSequenceNumber)
                        {
                            highestSequenceNumber = currentSequenceNumber;

                            // Retrieve the modification date from sx:history element.
                            attributeValue = iterator.Current.GetAttribute("when", string.Empty);
                            if (!DateTime.TryParse(attributeValue, out changeDate))
                            {
                                changeDate = DateTime.MinValue;
                                if (!string.IsNullOrEmpty(attributeValue))
                                {
                                    ServiceProvider.Logger.Warning(Strings.ConvertersInvalidAttributeValue, this.MessagePrefix, GetNavigatorPath(iterator.Current, source), "when", attributeValue);
                                }
                            }
                        }
                    }

                    // If the highest sequence number is positive, create revision object from it.
                    if (highestSequenceNumber > 0)
                    {
                        revision = new CsxRevision(highestSequenceNumber, changeDate);
                    }
                }
            }

            // If SSE data is not found or is invalid, fallback into csx:lastBuildDate value.
            if (!deletedItem && revision == null)
            {
                childNavigator = navigator.SelectSingleNode("csx:lastBuildDate", this.NamespaceManager);
                DateTime changeDate = this.GetDateFromNavigator(childNavigator, source);
                if (DateTime.Compare(changeDate, DateTime.MinValue) == 0)
                {
                    changeDate = publishDate;
                }

                revision = new CsxRevision(changeDate);
            }

            return revision;
        }

        /// <summary>
        /// Retrieves DateTime from specified xml sub-node.
        /// </summary>
        /// <param name="navigator">Navigator representing xml sub-node.</param>
        /// <param name="source">A string representing the source of the data.</param>
        /// <returns>The DateTime retrieved from specified xml sub-node.</returns>
        protected virtual DateTime GetDateFromNavigator(XPathNavigator navigator, string source)
        {
            DateTime date = DateTime.MinValue;
            string dateText = GetTextFromNavigator(navigator);
            if (!DateTime.TryParse(dateText, out date))
            {
                if (!string.IsNullOrEmpty(dateText))
                {
                    ServiceProvider.Logger.Warning(Strings.ConvertersInvalidElementValue, this.MessagePrefix, GetNavigatorPath(navigator, source), dateText);
                }
            }

            return date;
        }

        /// <summary>
        /// Retrieves Uri from specified xml node.
        /// </summary>
        /// <param name="navigator">Navigator representing xml node.</param>
        /// <param name="source">A string representing the source of the data.</param>
        /// <returns>The Uri retrieved from specified xml node.</returns>
        protected virtual Uri GetUriFromNavigator(XPathNavigator navigator, string source)
        {
            Uri uri = null;
            string uriText = GetTextFromNavigator(navigator);
            if (!Uri.TryCreate(uriText, UriKind.RelativeOrAbsolute, out uri))
            {
                if (navigator != null)
                {
                    ServiceProvider.Logger.Error(Strings.ConvertersInvalidElementValue, this.MessagePrefix, GetNavigatorPath(navigator, source), uriText);
                }
            }

            return uri;
        }

        /// <summary>
        /// Retrieves nested feeds from specified xml node.
        /// </summary>
        /// <param name="navigator">Navigator representing xml node.</param>
        /// <param name="source">A string representing the source of the data.</param>
        /// <param name="guidStore">The guid repository that provides mapping from a guid to associated with it object.</param>
        /// <param name="nestedFeeds">The collection of items representing nested feeds location and properties.</param>
        protected virtual void GetNestedFeedsFromNavigator(XPathNavigator navigator, string source, GuidStore guidStore, IList<FeedItem> nestedFeeds)
        {
            XPathNodeIterator iterator = navigator.SelectChildren("item", string.Empty);
            while (iterator.MoveNext())
            {
                // Get the item representing a feed location and properties.
                FeedItem nestedFeed = this.GetNestedFeedFromNavigator(iterator.Current, source);
                if (nestedFeed != null)
                {
                    if (!guidStore.ContainsKey(nestedFeed.Guid))
                    {
                        guidStore.Add(nestedFeed.Guid, nestedFeed);
                        nestedFeeds.Add(nestedFeed);
                    }
                    else
                    {
                        ServiceProvider.Logger.Error(Strings.ConvertersDuplicateGuid, this.MessagePrefix, GetNavigatorPath(iterator.Current, source), nestedFeed.Guid);
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves a single nested feed from from specified xml node.
        /// </summary>
        /// <param name="navigator">Navigator representing xml node.</param>
        /// <param name="source">A string representing the source of the data.</param>
        /// <returns>A single nested feed retrieved from specified xml node.</returns>
        protected virtual FeedItem GetNestedFeedFromNavigator(XPathNavigator navigator, string source)
        {
            FeedItem nestedFeed = null;

            string title, guid, webLink;
            Uri linkUri;
            DateTime publishDate;
            CsxRevision revision;
            bool dataIsValid = this.GetLinkItemDataFromNavigator(navigator, source, out title, out guid, out webLink, out linkUri, out publishDate, out revision);
            if (dataIsValid)
            {
                // If csx:link has nestedFeed=true, this is a nested feed
                XPathNavigator childNavigator = navigator.SelectSingleNode("csx:link", this.NamespaceManager);
                if (childNavigator != null)
                {
                    bool nestedFeedValue;
                    string nestedFeedString = childNavigator.GetAttribute("nestedFeed", String.Empty);
                    if (!bool.TryParse(nestedFeedString, out nestedFeedValue))
                    {
                        nestedFeedValue = false;
                    }

                    if (nestedFeedValue)
                    {
                        nestedFeed = new FeedItem(title, guid, webLink, publishDate, revision, linkUri);
                    }
                }
            }

            return nestedFeed;
        }

        /// <summary>
        /// Retrieves a Photo object from specified xml node.
        /// </summary>
        /// <param name="navigator">Navigator representing xml node.</param>
        /// <param name="source">A string representing the source of the data.</param>
        /// <returns>A photo object retrieved from specified xml node.</returns>
        protected virtual Photo GetPhotoFromNavigator(XPathNavigator navigator, string source)
        {
            Photo photo = null;
            XPathNavigator childNavigator;
            
            Uri baseUri;
            if (!Uri.TryCreate(ScePhotoSettings.DataFeedUri, source, out baseUri))
            {
                ServiceProvider.Logger.Error("Unable to create Uri with base {0} and source {1}", ScePhotoSettings.DataFeedUri, source);
                baseUri = null;
            }

            string title, guid, webLink;
            DateTime publishDate;
            bool dataIsValid = this.GetItemDataFromNavigator(navigator, source, out title, out guid, out webLink, out publishDate);
            if (dataIsValid)
            {
                // Revision.
                CsxRevision revision = this.GetRevisionFromNavigator(navigator, source, publishDate);
                if (revision == null)
                {
                    dataIsValid = false;
                }

                // Description
                childNavigator = navigator.SelectSingleNode("description");
                string description = GetTextFromNavigator(childNavigator);

                // Image Uri
                Uri imageUri = null;
                childNavigator = navigator.SelectSingleNode("media:content", this.NamespaceManager);
                if (childNavigator != null)
                {
                    string uriText = childNavigator.GetAttribute("url", String.Empty);
                    if (!Uri.TryCreate(uriText, UriKind.RelativeOrAbsolute, out imageUri))
                    {
                        ServiceProvider.Logger.Error(Strings.ConvertersInvalidAttributeValue, this.MessagePrefix, GetNavigatorPath(navigator, source), "url", uriText);
                        dataIsValid = false;
                    }
                    else if (!imageUri.IsAbsoluteUri && baseUri != null)
                    {
                        if (!Uri.TryCreate(baseUri, imageUri, out imageUri))
                        {
                            ServiceProvider.Logger.Error(Strings.ConvertersInvalidAttributeValue, this.MessagePrefix, GetNavigatorPath(navigator, source), "url", uriText);
                            dataIsValid = false;
                        }
                    }
                }

                // Thumbnail Uri
                Uri thumbnailUri = null;
                childNavigator = navigator.SelectSingleNode("media:thumbnail", this.NamespaceManager);
                if (childNavigator != null)
                {
                    string uriText = childNavigator.GetAttribute("url", String.Empty);
                    if (!Uri.TryCreate(uriText, UriKind.RelativeOrAbsolute, out thumbnailUri))
                    {
                        ServiceProvider.Logger.Error(Strings.ConvertersInvalidAttributeValue, this.MessagePrefix, GetNavigatorPath(navigator, source), "url", uriText);
                        dataIsValid = false;
                    }
                    else if (!thumbnailUri.IsAbsoluteUri && baseUri != null)
                    {
                        if (!Uri.TryCreate(baseUri, thumbnailUri, out thumbnailUri))
                        {
                            ServiceProvider.Logger.Error(Strings.ConvertersInvalidAttributeValue, this.MessagePrefix, GetNavigatorPath(navigator, source), "url", uriText);
                            dataIsValid = false;
                        }
                    }
                }

                // Photo Tags
                childNavigator = navigator.SelectSingleNode("media:category", this.NamespaceManager);
                string tags = GetTextFromNavigator(childNavigator);
                List<short> photoTags = new List<short>();
                foreach (string tag in tags.Split(' '))
                {
                    photoTags.Add(ServiceProvider.DataManager.TagStore.GetIdForTag(tag, true));
                }

                // Check for a separate description file in case of large description
                Uri descriptionFileUri = null;
                XPathNodeIterator iterator = navigator.SelectChildren("link", CsxNamespaceUri);
                if (iterator != null)
                {
                    while (iterator.MoveNext())
                    {
                        if (iterator.Current.HasAttributes)
                        {
                            bool hasDescriptionFile = false;
                            string hasDescriptionValue = iterator.Current.GetAttribute("descriptionFile", string.Empty);
                            if (bool.TryParse(hasDescriptionValue, out hasDescriptionFile))
                            {
                                if (hasDescriptionFile)
                                {
                                    string uriText = GetTextFromNavigator(iterator.Current);
                                    if (!Uri.TryCreate(uriText, UriKind.RelativeOrAbsolute, out descriptionFileUri))
                                    {
                                        ServiceProvider.Logger.Error(Strings.ConvertersInvalidElementValue, this.MessagePrefix, GetNavigatorPath(iterator.Current, source), uriText);
                                        dataIsValid = false;
                                    }
                                    else if (!descriptionFileUri.IsAbsoluteUri && baseUri != null)
                                    {
                                        if (!Uri.TryCreate(baseUri, descriptionFileUri, out descriptionFileUri))
                                        {
                                            ServiceProvider.Logger.Error(Strings.ConvertersInvalidElementValue, this.MessagePrefix, GetNavigatorPath(iterator.Current, source), uriText);
                                            dataIsValid = false;
                                        }
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }

                if (dataIsValid)
                {
                    photo = new Photo(guid, title, description, webLink, publishDate, revision, null, imageUri, thumbnailUri, descriptionFileUri, photoTags);
                }
            }

            return photo;
        }

        #endregion
    }
}
