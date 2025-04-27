//-----------------------------------------------------------------------
// <copyright file="NullableBoolToVisibilityConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Converts a Nullable bool value to a Visibility indicating whether or not the element can be seen.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using System;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Class to convert a Nullable bool to a Visibility value indicating whether or not the element can be seen.
    /// </summary>
    public class NullableBoolToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Converts a Nullable bool value to a Visibility indicating whether or not the element can be seen.
        /// </summary>
        /// <param name="value">The original Visibility.</param>
        /// <param name="targetType">The target type of the conversion.</param>
        /// <param name="parameter">The converter parameter.</param>
        /// <param name="culture">The application culture.</param>
        /// <returns>A Visibility indicating whether or not the element can be seen.</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool? v = value as bool?;

            if (v == null || v == false)
            {
                return Visibility.Collapsed;
            }
            
            return Visibility.Visible;
        }

        /// <summary>
        /// Converts back by returning the boolean value.
        /// </summary>
        /// <param name="value">The visibility value.</param>
        /// <param name="targetType">The target type of the conversion.</param>
        /// <param name="parameter">The converter parameter.</param>
        /// <param name="culture">The application culture.</param>
        /// <returns>The provided value.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        #endregion
    }
}
