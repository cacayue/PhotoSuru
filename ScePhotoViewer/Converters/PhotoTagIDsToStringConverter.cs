//-----------------------------------------------------------------------
// <copyright file="PhotoTagIDsToStringConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Converts an ObservableCollection of tag IDs to a space-delimited string and back.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Text;
    using System.Windows.Data;
    using ScePhoto;

    /// <summary>
    /// Converts an ObservableCollection of tag IDs to a space-delimited string and back.
    /// </summary>
    public class PhotoTagIDsToStringConverter : IValueConverter
    {
        /// <summary>
        /// Converts an ObservableCollection of tag IDs to a space-delimited string.
        /// </summary>
        /// <param name="value">The ObservableCommection of tag IDs to convert.</param>
        /// <param name="targetType">The conversion target type.</param>
        /// <param name="parameter">The conversion parameter.</param>
        /// <param name="culture">The conversion culture.</param>
        /// <returns>A space-delimited string of tags.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ObservableCollection<short> photoTags = value as ObservableCollection<short>;
            string photoTagsString = String.Empty;
            if (photoTags != null)
            {
                StringBuilder s = new StringBuilder();
                foreach (short tagId in photoTags)
                {
                    s.Append(ServiceProvider.DataManager.TagStore.GetTagForId(tagId));
                    s.Append(" ");
                }

                photoTagsString = s.ToString().TrimEnd(' ');
            }

            return photoTagsString;
        }

        /// <summary>
        /// Converts a space-delimited string to an ObservableCollection of tag IDs.
        /// </summary>
        /// <param name="value">The space-delimited string of tags to convert.</param>
        /// <param name="targetType">The conversion target type.</param>
        /// <param name="parameter">The conversion parameter.</param>
        /// <param name="culture">The conversion culture.</param>
        /// <returns>An ObservableCommection of tag IDs.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string tags = value as string;
            ObservableCollection<short> photoTags = new ObservableCollection<short>();

            if (tags != null)
            {    
                foreach (string tag in tags.Split(' '))
                {
                    photoTags.Add(ServiceProvider.DataManager.TagStore.GetIdForTag(tag, true));
                }   
            }
            
            return photoTags;
        }
    }
}
