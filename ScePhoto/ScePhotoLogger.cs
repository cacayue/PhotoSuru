//-----------------------------------------------------------------------
// <copyright file="ScePhotoLogger.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Logging class for the ScePhoto application.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto
{
    using System;
    using System.Text;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;
    using System.IO;
    using System.Windows;
    using System.Windows.Threading;
    using Microsoft.SubscriptionCenter.Sync;

    /// <summary>
    /// Logging class for the ScePhoto application.
    /// </summary>
    public class ScePhotoLogger : Logger
    {
        /// <summary>
        /// Number of pending messages before a Flush is scheduled.
        /// </summary>
        private const int LogFlushLimit = 50;

        /// <summary>
        /// Format of time stamp in message log.
        /// </summary>
        private const string DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ssK";

        /// <summary>
        /// Thread synchronization object.
        /// </summary>
        private object syncRoot;

        /// <summary>
        /// Memory storage for log messages.
        /// </summary>
        private StringBuilder log;

        /// <summary>
        /// Number of log messages stored in the memory.
        /// </summary>
        private int messageCount;

        /// <summary>
        /// Initializes the ScePhotoLogger instance.
        /// </summary>
        public ScePhotoLogger()
        {
            this.syncRoot = new object();
            this.log = new StringBuilder();

            if (Application.Current != null)
            {
                Application.Current.Exit += new ExitEventHandler(this.Application_Exit);
                Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(this.Application_DispatcherUnhandledException);
            }
        }

        /// <summary>
        /// Writes an error message to the application log.
        /// </summary>
        /// <param name="message">The informative message to write.</param>
        public override void Error(string message)
        {
            Trace.TraceError(message);
            this.LogMessage(Strings.LoggerMessageTypeError, message);
        }

        /// <summary>
        /// Writes a warning message to the application log.
        /// </summary>
        /// <param name="message">The informative message to write.</param>
        public override void Warning(string message)
        {
            Trace.TraceWarning(message);
            this.LogMessage(Strings.LoggerMessageTypeWarning, message);
        }

        /// <summary>
        /// Writes an informational message to the application log.
        /// </summary>
        /// <param name="message">The informative message to write.</param>
        public override void Information(string message)
        {
            Trace.TraceInformation(message);
            this.LogMessage(Strings.LoggerMessageTypeInformation, message);
        }

        /// <summary>
        /// Checks for a condition and logs an error message to the application log, 
        /// if the condition is false.
        /// </summary>
        /// <param name="condition">True to prevent a message being logged; otherwise, false.</param>
        /// <param name="message">A message to log.</param>
        public virtual void Assert(bool condition, string message)
        {
            if (!condition)
            {
#if DEBUG
                Trace.Assert(condition, message);
#endif
                this.Error(message);
            }
        }

        /// <summary>
        /// Writes formatted message to the application log.
        /// </summary>
        /// <param name="type">The type of message to write.</param>
        /// <param name="message">The informative message to write.</param>
        private void LogMessage(string type, string message)
        {
            string dateTimeString = DateTime.Now.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
            string logMessage = String.Format(CultureInfo.InvariantCulture, Strings.LoggerMessageFormat, dateTimeString, type, message);

            lock (this.syncRoot)
            {
                this.log.AppendLine(logMessage);
                this.messageCount++;
                if (this.messageCount > LogFlushLimit)
                {
                    ThreadPool.QueueUserWorkItem(this.FlushAsyncCallback);
                }
            }
        }

        /// <summary>
        /// ThreadPool callback that flushes the logger.
        /// </summary>
        /// <param name="arg">Callback arguments.</param>
        private void FlushAsyncCallback(object arg)
        {
            this.Flush();
        }

        /// <summary>
        /// Clears the buffer used for the application log and causes any buffered data to be written 
        /// to the underlying log file.
        /// </summary>
        private void Flush()
        {
            lock (this.syncRoot)
            {
                try
                {
                    using (TextWriter writer = File.AppendText(ScePhotoSettings.LogFilePath))
                    {
                        writer.Write(this.log.ToString());
                        writer.Flush();
                        this.log.Remove(0, this.log.Length);
                        this.messageCount = 0;
                    }
                }
                catch (IOException)
                {
                }
            }
        }

        /// <summary>
        /// Handler for Application.DispatcherUnhandledException event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            lock (this.syncRoot)
            {
                this.LogMessage(Strings.LoggerMessageTypeFatalError, e.Exception.ToString());
                this.Flush();
            }
        }

        /// <summary>
        /// Handler for Application.Exit event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Application.Current.Exit -= new ExitEventHandler(this.Application_Exit);
            Application.Current.DispatcherUnhandledException -= new DispatcherUnhandledExceptionEventHandler(this.Application_DispatcherUnhandledException);

            lock (this.syncRoot)
            {
                this.Flush();
            }
        }
    }
}
