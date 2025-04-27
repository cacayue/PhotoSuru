//-----------------------------------------------------------------------
// <copyright file="SingleTagIdToStringConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Converts a short integer tag ID to a string and back.
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
    /// Converts a short integer tag ID to a string and back.
    /// </summary>
    public class SingleTagIdToStringConverter : IValueConverter
    {
        /// <summary>
        /// Converts a short integer tag ID to a string.
        /// </summary>
        /// <param name="value">The short integer tag id to convert.</param>
        /// <param name="targetType">The conversion target type.</param>
        /// <param name="parameter">The conversion parameter.</param>
        /// <param name="culture">The conversion culture.</param>
        /// <returns>A tag string.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            short tagId = System.Convert.ToInt16(value, CultureInfo.InvariantCulture);
            return ServiceProvider.DataManager.TagStore.GetTagForId(tagId);
        }

        /// <summary>
        /// Converts a string to a short integer tag ID.
        /// </summary>
        /// <param name="value">The tag string to convert.</param>
        /// <param name="targetType">The conversion target type.</param>
        /// <param name="parameter">The conversion parameter.</param>
        /// <param name="culture">The conversion culture.</param>
        /// <returns>A short integer tag id.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string tag = value as string;
            return ServiceProvider.DataManager.TagStore.GetIdForTag(tag, false);
        }
    }
}
