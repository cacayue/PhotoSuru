//-----------------------------------------------------------------------
// <copyright file="DualImage.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Control used to display one of two possible images.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>
    /// Control used to display one of two possible images.
    /// </summary>
    /// <remarks>
    /// Image.Source is set by OnDisplayAlternateImageChanged, which could cause issues with databinding or animating
    /// the Image.Source property.
    /// </remarks>
    public class DualImage : Image
    {
        /// <summary>
        /// Dependency Property backing store for AlternateSource.
        /// </summary>
        public static readonly DependencyProperty AlternateSourceProperty =
            DependencyProperty.Register("AlternateSource", typeof(ImageSource), typeof(DualImage), new UIPropertyMetadata(null));

        /// <summary>
        /// Dependency Property backing store for DisplayAlternateImage.
        /// </summary>
        public static readonly DependencyProperty DisplayAlternateImageProperty =
            DependencyProperty.Register("DisplayAlternateImage", typeof(bool), typeof(DualImage), new UIPropertyMetadata(false, new PropertyChangedCallback(OnDisplayAlternateImageChanged)));

        /// <summary>
        /// The primary ImageSource for the image.
        /// </summary>
        private ImageSource primarySource;

        /// <summary>
        /// Gets or sets the alternate ImageSource for the image.
        /// </summary>
        public ImageSource AlternateSource
        {
            get { return (ImageSource)GetValue(AlternateSourceProperty); }
            set { SetValue(AlternateSourceProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to display the alternate image.
        /// </summary>
        public bool DisplayAlternateImage
        {
            get { return (bool)GetValue(DisplayAlternateImageProperty); }
            set { SetValue(DisplayAlternateImageProperty, value); }
        }

        /// <summary>
        /// Handler for DisplayAlternateImage changes.
        /// </summary>
        /// <param name="element">The element that changed.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnDisplayAlternateImageChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            DualImage dualImage = (DualImage)element;
            if ((bool)e.NewValue)
            {
                dualImage.primarySource = dualImage.Source;
                dualImage.Source = dualImage.AlternateSource;
            }
            else
            {
                dualImage.Source = dualImage.primarySource;
            }
        }
    }
}
