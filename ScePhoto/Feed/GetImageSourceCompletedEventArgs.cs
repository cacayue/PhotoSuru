//-----------------------------------------------------------------------
// <copyright file="GetImageSourceCompletedEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Event arguments accompanying delegate for GetImageSource event.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Feed
{
    using System;
    using System.ComponentModel;
    using System.Windows.Media;

    /// <summary>
    /// Event arguments accompanying delegate for GetImageSource event.
    /// </summary>
    public class GetImageSourceCompletedEventArgs : AsyncCompletedEventArgs
    {
        /// <summary>
        /// The ImageSource representing data from the Internet resource.
        /// </summary>
        private ImageSource imageSource;

        /// <summary>
        /// Initializes a new instance of arguments for successful completion.
        /// </summary>
        /// <param name="imageSource">The ImageSource representing data from the Internet resource.</param>
        /// <param name="userState">The user-supplied state object.</param>
        public GetImageSourceCompletedEventArgs(ImageSource imageSource, object userState)
            : base(null, false, userState)
        {
            this.imageSource = imageSource;
        }

        /// <summary>
        /// Initializes a new instance of arguments for an error or a cancellation.
        /// </summary>
        /// <param name="error">Any error that occurred during the asynchronous operation.</param>
        /// <param name="cancelled">A value indicating whether the asynchronous operation was canceled.</param>
        /// <param name="userState">The user-supplied state object.</param>
        public GetImageSourceCompletedEventArgs(Exception error, bool cancelled, object userState)
            : base(error, cancelled, userState)
        {
        }

        /// <summary>
        /// Gets the ImageSource representing data from the Internet resource.
        /// </summary>
        public ImageSource ImageSource
        {
            get
            {
                RaiseExceptionIfNecessary();
                return this.imageSource;
            }
        }
    }
}
