//-----------------------------------------------------------------------
// <copyright file="ScePhotoException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Generic exception for problems encountered in the ScePhoto project.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Generic exception for problems encountered in the ScePhoto project.
    /// </summary>
    [Serializable]
    public class ScePhotoException : Exception
    {
        /// <summary>
        /// ScePhotoException constructor.
        /// </summary>
        public ScePhotoException()
        {
        }

        /// <summary>
        /// ScePhotoException constructor; contains a description of the exception.
        /// </summary>
        /// <param name="message">A string describing the exception.</param>
        public ScePhotoException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// ScePhotoException constructor; contains a description of the error and the original exception.
        /// </summary>
        /// <param name="message">A string describing the exception.</param>
        /// <param name="innerException">The original exception thrown.</param>
        public ScePhotoException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// ScePhotoException constructor; contains information about the serialization exception.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information about the source or destination.</param>
        protected ScePhotoException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
