//-----------------------------------------------------------------------
// <copyright file="Main.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Main class for applications using a splash screen (running in release mode).
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Threading;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Windows.Navigation;
    using ScePhoto;

    /// <summary>
    /// Main class for applications using a splash screen (running in release mode).
    /// </summary>
    public static class ScePhotoMain
    {
        /// <summary>
        /// The application splash screen.
        /// </summary>
        private static SplashScreen splash;

        /// <summary>
        /// Application Entry Point.
        /// </summary>
        /// <remarks>
        /// ScePhotoViewer contains an explicit Main method instead of one implicitly defined in the
        /// application code to bring up a splash screen while the application starts.
        /// </remarks>
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public static void Main()
        {
            ScePhoto.ScePhotoSettings.Initialize(new SampleScePhotoSettings());

            // ScePhotoViewer is a single instance application; the splash screen and application object are only
            // created for the first instance. Single instancing is controlled by the SingleInstance class which manages
            // a mutex for the application
            if (SingleInstance.InitializeAsFirstInstance())
            {
                // Open splash screen
                splash = new SplashScreen();
                splash.Open();

                // Start main application
                StartApplication();
            }
        }

        /// <summary>
        /// Creates and runs a new instance of the ScePhotoViewerApplication. Called when single instance code has
        /// established that there is no other instance of the application currently running.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void StartApplication()
        {
            ScePhotoViewerApplication application = new ScePhotoViewerApplication();

            // Listen for application's load completed and exit events, to detect when splash screen can be closed
            application.LoadCompleted += OnApplicationLoadCompleted;
            application.Exit += OnApplicationExit;

            application.InitializeComponent();
            application.Run();

            // Allow single instance code to perform cleanup operations
            SingleInstance.Cleanup();
        }

        /// <summary>
        /// EventHandler for application's LoadCompleted event. This is used to access the application's MainWindow object
        /// and listen for it's ContentRendered event. When main window content is rendered, splash screeen can be closed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments describing the event.</param>
        private static void OnApplicationLoadCompleted(object sender, NavigationEventArgs e)
        {
            // Check event args for navigator, which is the application's MainWindow
            NavigationWindow window = e.Navigator as NavigationWindow;
            if (window != null)
            {
                window.ContentRendered += OnMainWindowContentRendered;
            }
        }

        /// <summary>
        /// Event handler for application's exit event. Splash screen needs to be closed here in case application exits before
        /// main window content has rendered.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments describing the event.</param>
        private static void OnApplicationExit(object sender, ExitEventArgs e)
        {
            CloseSplashScreen();
        }

        /// <summary>
        /// EventHandler for application's main window content rendered event. When window content is rendered, splash screen can be
        /// closed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments describing the event.</param>
        private static void OnMainWindowContentRendered(object sender, EventArgs e)
        {
            CloseSplashScreen();

            if (!Properties.Settings.Default.SkipIntroWindow)
            {
                ServiceProvider.ViewManager.ShowDialog(new IntroWindow());
            }
        }

        /// <summary>
        /// Closes splash screen and frees its resources.
        /// </summary>
        private static void CloseSplashScreen()
        {
            if (splash != null)
            {
                splash.Close();
                splash = null;
            }
        }
    }
}
