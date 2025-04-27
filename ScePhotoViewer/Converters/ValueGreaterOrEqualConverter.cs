//-----------------------------------------------------------------------
// <copyright file="ValueGreaterOrEqualConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Returns true when the value passed is greater than the parameter; useful when triggering on a value that's at least (parameter).
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Data;
    using System.Globalization;

    /// <summary>
    /// Returns true when the value passed is greater than the parameter.
    /// </summary>
    public class ValueGreaterOrEqualConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Determines whether the provided value is large than the parameter value (as doubles).
        /// </summary>
        /// <param name="value">The value to compare.</param>
        /// <param name="targetType">The target type for the conversion.</param>
        /// <param name="parameter">The value to compare against.</param>
        /// <param name="culture">The conversion culture.</param>
        /// <returns>True if the value is greater or equal to the parameter.</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double sourceValue = System.Convert.ToDouble(value, culture);
            double parameterValue = System.Convert.ToDouble(parameter, culture);

            if (sourceValue >= parameterValue)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Converts from a boolean back to the source value.  Not used.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target type of the conversion.</param>
        /// <param name="parameter">The conversion parameter.</param>
        /// <param name="culture">The conversion culture.</param>
        /// <returns>Throws a NotImplementedException().</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
