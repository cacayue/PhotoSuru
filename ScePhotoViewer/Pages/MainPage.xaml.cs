//-----------------------------------------------------------------------
// <copyright file="MainPage.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Code-behind file for the ScePhoto main window.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using System;
    using System.Windows;
    using System.Windows.Input;
    using ScePhoto;
    using ScePhoto.View;

    /// <summary>
    /// The ScePhoto view mode; regular or full-screen with options.
    /// </summary>
    public enum ViewingMode
    {
        /// <summary>
        /// Full-screen viewing with the navigation UI.
        /// </summary>
        FullScreenNavigationUI,

        /// <summary>
        /// Full-screen viewing without the navigation UI.
        /// </summary>
        FullScreenNoNavigationUI,

        /// <summary>
        /// Normal viewing without the navigation UI.
        /// </summary>
        NormalScreenNoNavigationUI,

        /// <summary>
        /// Normal viewing with the navigation UI.
        /// </summary>
        NormalScreenNavigationUI
    }

    /// <summary>
    /// The ScePhoto Window.
    /// </summary>
    public partial class MainWindow
    {
        #region Fields
        /// <summary>
        /// DependencyProperty backing store for NavigationUIVisibility.
        /// </summary>
        public static readonly DependencyProperty NavigationUIVisibilityProperty =
            DependencyProperty.Register("NavigationUIVisibility", typeof(Visibility), typeof(MainWindow), new UIPropertyMetadata(Visibility.Visible));

        /// <summary>
        /// DependencyProperty backing store for FullScreenMode.
        /// </summary>
        public static readonly DependencyProperty FullScreenModeProperty =
            DependencyProperty.Register("FullScreenMode", typeof(bool), typeof(MainWindow), new UIPropertyMetadata(false));

        /// <summary>
        /// The RoutedCommand to switch the screen viewing mode.
        /// </summary>
        private static RoutedCommand switchFullScreenModeCommand = new RoutedCommand("SwitchFullScreenMode", typeof(MainWindow));

        /// <summary>
        /// Custom command to allow overrides of navigation keys if journal navigation on input is disabled but application
        /// authors still want to enable journal navigation on certain keys. This is presently only applied to browse back
        /// command on backspace, but similar logic could be added for forward journal navigation keys.
        /// </summary>
        private RoutedCommand backNavigationKeyOverrideCommand = new RoutedCommand();

        /// <summary>
        /// Saved window style if window style was changed to None for full screen mode.
        /// </summary>
        private WindowStyle windowStyle = WindowStyle.None;

        /// <summary>
        /// Saved state for resize mode.
        /// </summary>
        private ResizeMode resizeMode = ResizeMode.NoResize;

        /// <summary>
        /// Viewing mode for the next view.
        /// </summary>
        private ViewingMode viewingMode = ViewingMode.NormalScreenNavigationUI; 
        #endregion

        #region Constructor
        /// <summary>
        /// The MainWindow constructor.
        /// </summary>
        public MainWindow()
        {
#if DEBUG
            this.InputBindings.Add(new KeyBinding(DebugCommands.GarbageCollectCommand, new KeyGesture(Key.G, ModifierKeys.Control)));
#endif
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(System.Windows.Input.NavigationCommands.BrowseBack, new ExecutedRoutedEventHandler(OnNavigationCommandExecuted), new CanExecuteRoutedEventHandler(OnNavigationCommandCanExecute)));
            CommandBindings.Add(new CommandBinding(System.Windows.Input.NavigationCommands.BrowseForward, new ExecutedRoutedEventHandler(OnNavigationCommandExecuted), new CanExecuteRoutedEventHandler(OnNavigationCommandCanExecute)));
            CommandBindings.Add(new CommandBinding(System.Windows.Input.ComponentCommands.ScrollPageUp, new ExecutedRoutedEventHandler(OnNavigationCommandExecuted), new CanExecuteRoutedEventHandler(OnNavigationCommandCanExecute)));
            CommandBindings.Add(new CommandBinding(System.Windows.Input.ComponentCommands.ScrollPageDown, new ExecutedRoutedEventHandler(OnNavigationCommandExecuted), new CanExecuteRoutedEventHandler(OnNavigationCommandCanExecute)));
            CommandBindings.Add(new CommandBinding(System.Windows.Input.MediaCommands.TogglePlayPause, new ExecutedRoutedEventHandler(OnPlayCommandExecuted), new CanExecuteRoutedEventHandler(OnPlayCommandCanExecute)));
            CommandBindings.Add(new CommandBinding(System.Windows.Input.MediaCommands.Play, new ExecutedRoutedEventHandler(OnPlayCommandExecuted), new CanExecuteRoutedEventHandler(OnPlayCommandCanExecute)));
            this.CommandBindings.Add(new CommandBinding(switchFullScreenModeCommand, new ExecutedRoutedEventHandler(this.OnSwitchFullScreenCommand)));

            // If journal navigation on input is disabled, all navigation keys, such as Backspace, etc. will be consumed as next/previous navigation.
            // Application authors may want to preserve some keys for journal navigation even though by and large input gestures are disabled for
            // this feature. In the handler for BrowseBack/Forward commands it is not possible to tell whether the command came from a 
            // key gesture or not, or which key caused it. To override specific keys, MainWindow exposes a navigationKeyOverrideCommand member,
            // and binds certain keys to it. Presently this is done for the Backspace key.
            if (!SampleScePhotoSettings.EnableJournalNavigationOnInput)
            {
                CommandBindings.Add(new CommandBinding(this.backNavigationKeyOverrideCommand, new ExecutedRoutedEventHandler(OnJournalBackNavigationKeyOverride)));
                InputBindings.Add(new InputBinding(this.backNavigationKeyOverrideCommand, new KeyGesture(Key.Back)));
            }
        } 
        #endregion

        #region Properties

        /// <summary>
        /// Gets the command to switch the screen viewing mode.
        /// </summary>
        public static RoutedCommand SwitchFullScreenModeCommand
        {
            get { return MainWindow.switchFullScreenModeCommand; }
        }

        /// <summary>
        /// Gets the visibility of the Navigation UI.
        /// </summary>
        public Visibility NavigationUIVisibility
        {
            get { return (Visibility)GetValue(NavigationUIVisibilityProperty); }
            protected set { SetValue(NavigationUIVisibilityProperty, value); }
        }

        /// <summary>
        /// Gets a value indicating whether full screen mode is active for use by the UI toggle button.
        /// </summary>
        public bool FullScreenMode
        {
            get { return (bool)GetValue(FullScreenModeProperty); }
            protected set { SetValue(FullScreenModeProperty, value); }
        }

        /// <summary>
        /// Gets a value indicating whether navigation UI is visible, helper that ORs two viewing mode values.
        /// </summary>
        private bool NavigationUIVisible
        {
            get { return (this.viewingMode == ViewingMode.FullScreenNavigationUI || this.viewingMode == ViewingMode.NormalScreenNavigationUI); }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Override for MouseWheel event. In MainPage this handler does no custom operations, it invokes the MouseWheel handler for
        /// the application.
        /// </summary>
        /// <param name="e">EventArgs describing the event.</param>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            // Custom logic for MainPage's MouseWheel should be put before calling global handler

            // Next, call global handler
            if (!e.Handled)
            {
                ApplicationInputHandler.OnMouseWheel(e);
            }

            // Finally, call base if not handled
            if (!e.Handled)
            {
                base.OnMouseWheel(e);
            }
        }

        /// <summary>
        /// OnInitialized override loads saved bounds if saving is enabled.
        /// </summary>
        /// <param name="e">EventArgs describing the event.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // Restore bounds from settings if enabled
            if (SampleScePhotoSettings.SaveMainWindowBounds && SampleScePhotoSettings.MainWindowBounds != Rect.Empty)
            {
                this.Left = SampleScePhotoSettings.MainWindowBounds.Left;
                this.Top = SampleScePhotoSettings.MainWindowBounds.Top;
                this.Width = SampleScePhotoSettings.MainWindowBounds.Width;
                this.Height = SampleScePhotoSettings.MainWindowBounds.Height;
                this.WindowState = SampleScePhotoSettings.MainWindowState;
            }
        }

        /// <summary>
        /// OnClosing override stores window bounds to settings, if enabled.
        /// </summary>
        /// <param name="e">EventArgs describing the event.</param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            if (SampleScePhotoSettings.SaveMainWindowBounds)
            {
                SampleScePhotoSettings.MainWindowBounds = this.RestoreBounds;
                SampleScePhotoSettings.MainWindowState = this.WindowState;
            }
            else
            {
                SampleScePhotoSettings.MainWindowBounds = Rect.Empty;
                SampleScePhotoSettings.MainWindowState = WindowState.Normal;
            }
        }

        /// <summary>
        /// Override for KeyDown event. Application authors can put app-specific global key handling here.
        /// </summary>
        /// <param name="e">EventArgs describing the event.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Custom key handling for main page
            if (!e.Handled)
            {
                if (e.KeyboardDevice.Modifiers == ModifierKeys.None)
                {
                    switch (e.Key)
                    {
                        case Key.Oem2:
                            this.OnOem2KeyPress(e);
                            break;
                        case Key.F9:
                            this.OnF9KeyPress(e);
                            break;
                        case Key.Escape:
                            this.OnEscapeKeyPress(e);
                            break;
                        case Key.F11:
                            this.OnF11KeyPress(e);
                            break;
                        case Key.F12:
                            this.OnF12KeyPress(e);
                            break;
                        case Key.System:
                            {
                                if (e.SystemKey == Key.F10)
                                {
                                    this.OnF10KeyPress(e);
                                }

                                break;
                            }

                        default:
                            break;
                    }
                }
            }

            // Next, call application-wide input handler.
            if (!e.Handled)
            {
                ApplicationInputHandler.OnKeyDown(e);
            }

            // Finally, call base if not handled.
            if (!e.Handled)
            {
                base.OnKeyDown(e);
            }
        } 
        #endregion

        #region Private Methods
        /// <summary>
        /// Command handler for BrowseBack CanExecute.
        /// </summary>
        /// <param name="sender">Source of the command.</param>
        /// <param name="e">EventArgs describing the event.</param>
        private static void OnNavigationCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            MainWindow mainWindow = sender as MainWindow;
            if (mainWindow != null && !e.Handled)
            {
                if (e.Command == System.Windows.Input.NavigationCommands.BrowseBack)
                {
                    mainWindow.OnBrowseBack(e);
                }
                else if (e.Command == System.Windows.Input.NavigationCommands.BrowseForward)
                {
                    mainWindow.OnBrowseForward(e);
                }
                else if (e.Command == System.Windows.Input.ComponentCommands.ScrollPageUp)
                {
                    mainWindow.OnScrollPageUp(e);
                }
                else if (e.Command == System.Windows.Input.ComponentCommands.ScrollPageDown)
                {
                    mainWindow.OnScrollPageDown(e);
                }
            }
        }

        /// <summary>
        /// Command handler for CanExecute for RoutedUI Commands interpreted as causing navigation - browse back, browse forward, scroll page up, scroll page down.
        /// </summary>
        /// <param name="sender">Source of the command.</param>
        /// <param name="e">EventArgs describing the event.</param>
        private static void OnNavigationCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            MainWindow mainWindow = sender as MainWindow;
            if (mainWindow != null && !e.Handled)
            {
                if (e.Command == System.Windows.Input.NavigationCommands.BrowseBack)
                {
                    mainWindow.OnBrowseBackCanExecute(e);
                }
                else if (e.Command == System.Windows.Input.NavigationCommands.BrowseForward)
                {
                    mainWindow.OnBrowseForwardCanExecute(e);
                }
                else if (e.Command == System.Windows.Input.ComponentCommands.ScrollPageUp)
                {
                    mainWindow.OnScrollPageUpCanExecute(e);
                }
                else if (e.Command == System.Windows.Input.ComponentCommands.ScrollPageDown)
                {
                    mainWindow.OnScrollPageDownCanExecute(e);
                }
            }
        }

        /// <summary>
        /// Can execute handler for play command
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnPlayCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                if (!(ServiceProvider.ViewManager.CurrentNavigator is PhotoSlideShowNavigator))
                {
                    e.CanExecute = true;
                }

                e.Handled = true;
            }
        }

        /// <summary>
        /// Executed event handler for play command
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnPlayCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                if (!(ServiceProvider.ViewManager.CurrentNavigator is PhotoSlideShowNavigator))
                {
                    if (ServiceProvider.ViewManager.NavigationCommands.NavigateToPhotoSlideShowCommand.CanExecute(null))
                    {
                        ServiceProvider.ViewManager.NavigationCommands.NavigateToPhotoSlideShowCommand.Execute(null);
                    }
                }

                e.Handled = true;
            }
        }

        /// <summary>
        /// Command handler for back navigation key override.
        /// </summary>
        /// <param name="sender">Source of the command.</param>
        /// <param name="e">EventArgs describing the event.</param>
        private static void OnJournalBackNavigationKeyOverride(object sender, ExecutedRoutedEventArgs e)
        {
            MainWindow mainWindow = sender as MainWindow;
            if (mainWindow != null && !e.Handled)
            {
                mainWindow.OnJournalBackNavigationKeyOverride(e);
            }
        }

        /// <summary>
        /// Get the next viewing mode for the given value.
        /// </summary>
        /// <param name="viewingMode">Current viewing mode.</param>
        /// <returns>Next viewing mode.</returns>
        private static ViewingMode GetNextViewingMode(ViewingMode viewingMode)
        {
            ViewingMode nextViewingMode = ViewingMode.NormalScreenNoNavigationUI;
            switch (viewingMode)
            {
                case ViewingMode.FullScreenNavigationUI:
                    nextViewingMode = ViewingMode.FullScreenNoNavigationUI;
                    break;
                case ViewingMode.FullScreenNoNavigationUI:
                    nextViewingMode = ViewingMode.NormalScreenNoNavigationUI;
                    break;
                case ViewingMode.NormalScreenNoNavigationUI:
                    nextViewingMode = ViewingMode.NormalScreenNavigationUI;
                    break;
                case ViewingMode.NormalScreenNavigationUI:
                    nextViewingMode = ViewingMode.FullScreenNavigationUI;
                    break;
                default:
                    break;
            }

            return nextViewingMode;
        }

        /// <summary>
        /// On Oem2 press, move focus to Search text box. If focus move was successful, set handled to true to prevent further handling.
        /// </summary>
        /// <param name="e">EventArgs describing the event.</param>
        private void OnOem2KeyPress(KeyEventArgs e)
        {
            if (this.MoveFocusToSearch())
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// On F12 key press, switch navigation UI visibility.
        /// </summary>
        /// <param name="e">EventArgs describing the event.</param>
        private void OnF12KeyPress(KeyEventArgs e)
        {
            // Update viewing mode based on navigation UI visibility
            this.UpdateViewingModeNavUI();
            e.Handled = true;
        }

        /// <summary>
        /// On F11 key press, switch full screen mode.
        /// </summary>
        /// <param name="e">EventArgs describing the event.</param>
        private void OnF11KeyPress(KeyEventArgs e)
        {
            // Update viewing mode based on full screen setting
            this.UpdateViewingModeFullScreen();
            e.Handled = true;
        }

        /// <summary>
        /// On escape, go back to normal screen and navigation ui.
        /// </summary>
        /// <param name="e">Event args describing the event.</param>
        private void OnEscapeKeyPress(KeyEventArgs e)
        {
            // Restore viewing mode
            this.RestoreViewingMode();
            ContentPane.Focus();
            e.Handled = true;
        }

        /// <summary>
        /// Cycle viewing mode on F9 key press.
        /// </summary>
        /// <param name="e">EventArgs describing the event.</param>
        private void OnF9KeyPress(KeyEventArgs e)
        {
            this.CycleViewingMode();
            e.Handled = true;
        }

        /// <summary>
        /// Opens the breadcrumb bar's album menu on F10 key press.
        /// </summary>
        /// <param name="e">Areguments describing the key event.</param>
        private void OnF10KeyPress(KeyEventArgs e)
        {
            this.BreadcrumbBar.OpenAlbumMenu();
            e.Handled = true;
        }

        /// <summary>
        /// Turn off full screen, make navigation UI visibile.
        /// </summary>
        private void RestoreViewingMode()
        {
            this.SwitchFullScreenMode(false);
            this.SwitchNavigationUIVisibility(true);
            this.viewingMode = ViewingMode.NormalScreenNavigationUI;
        }

        /// <summary>
        /// Cycles the viewing mode to the next value.
        /// </summary>
        private void CycleViewingMode()
        {
            ViewingMode nextViewingMode = GetNextViewingMode(this.viewingMode);
            switch (nextViewingMode)
            {
                case ViewingMode.FullScreenNavigationUI:
                    this.SwitchFullScreenMode(true);
                    this.SwitchNavigationUIVisibility(true);
                    break;
                case ViewingMode.FullScreenNoNavigationUI:
                    this.SwitchFullScreenMode(true);
                    this.SwitchNavigationUIVisibility(false);
                    break;
                case ViewingMode.NormalScreenNoNavigationUI:
                    this.SwitchFullScreenMode(false);
                    this.SwitchNavigationUIVisibility(false);
                    break;
                case ViewingMode.NormalScreenNavigationUI:
                    this.SwitchFullScreenMode(false);
                    this.SwitchNavigationUIVisibility(true);
                    break;
                default:
                    break;
            }

            this.viewingMode = nextViewingMode;
        }

        /// <summary>
        /// Switches viewing mode based on navigation UI visiblity, and toggles the visibilty of navigation UI.
        /// </summary>
        private void UpdateViewingModeNavUI()
        {
            switch (this.viewingMode)
            {
                case ViewingMode.FullScreenNavigationUI:
                    this.SwitchNavigationUIVisibility(false);
                    this.viewingMode = ViewingMode.FullScreenNoNavigationUI;
                    break;
                case ViewingMode.NormalScreenNavigationUI:
                    this.SwitchNavigationUIVisibility(false);
                    this.viewingMode = ViewingMode.NormalScreenNoNavigationUI;
                    break;
                case ViewingMode.FullScreenNoNavigationUI:
                    this.SwitchNavigationUIVisibility(true);
                    this.viewingMode = ViewingMode.FullScreenNavigationUI;
                    break;
                case ViewingMode.NormalScreenNoNavigationUI:
                    this.SwitchNavigationUIVisibility(true);
                    this.viewingMode = ViewingMode.NormalScreenNavigationUI;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Switches viewing mode based on full screen setting, and toggles the visibilty of navigation UI.
        /// </summary>
        private void UpdateViewingModeFullScreen()
        {
            switch (this.viewingMode)
            {
                case ViewingMode.FullScreenNavigationUI:
                    this.SwitchFullScreenMode(false);
                    this.viewingMode = ViewingMode.NormalScreenNavigationUI;
                    break;
                case ViewingMode.NormalScreenNavigationUI:
                    this.SwitchFullScreenMode(true);
                    this.viewingMode = ViewingMode.FullScreenNavigationUI;
                    break;
                case ViewingMode.FullScreenNoNavigationUI:
                    this.SwitchFullScreenMode(false);
                    this.viewingMode = ViewingMode.NormalScreenNoNavigationUI;
                    break;
                case ViewingMode.NormalScreenNoNavigationUI:
                    this.SwitchFullScreenMode(true);
                    this.viewingMode = ViewingMode.FullScreenNoNavigationUI;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Moves the current focus to the SearchControl for this page.
        /// </summary>
        /// <returns>True if focus move succeeded.</returns>
        private bool MoveFocusToSearch()
        {
            // Check that we're in a mode that allows search visibility, and that the search control does not already have focus in the search area
            if (this.SearchControl != null && this.NavigationUIVisible && !this.SearchControl.IsSearchAreaFocused)
            {
                return this.SearchControl.MoveFocusToSearch();
            }

            return false;
        }

        /// <summary>
        /// Switches full screen mode on or off.
        /// </summary>
        /// <param name="fullScreen">If true, full screen mode is on, otherwise, it's turned off.</param>
        private void SwitchFullScreenMode(bool fullScreen)
        {
            // If viewing mode is already in a full screen state, not changes are necessary
            if (fullScreen && !this.FullScreenMode)
            {
                // Window.ResizeMode must be set before other window properties or else the window
                // will be positioned slightly off screen when maximized. 
                this.resizeMode = Application.Current.MainWindow.ResizeMode;
                Application.Current.MainWindow.ResizeMode = ResizeMode.NoResize;

                this.windowStyle = Application.Current.MainWindow.WindowStyle;
                Application.Current.MainWindow.WindowStyle = WindowStyle.None;
                Application.Current.MainWindow.Topmost = true;
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
                this.FullScreenMode = true;
            }
            else if (!fullScreen && this.FullScreenMode)
            {
                Application.Current.MainWindow.Topmost = false;
                Application.Current.MainWindow.WindowState = WindowState.Normal;
                Application.Current.MainWindow.WindowStyle = this.windowStyle;
                Application.Current.MainWindow.ResizeMode = this.resizeMode;
                this.FullScreenMode = false;
            }
        }

        /// <summary>
        /// Switches navigation UI visibility on or off.
        /// </summary>
        /// <param name="visible">If true, navigation UI is visible, if false, visibility is collapsed.</param>
        private void SwitchNavigationUIVisibility(bool visible)
        {
            if (visible && !this.NavigationUIVisible)
            {
                this.NavigationUIVisibility = Visibility.Visible;
            }
            else if (!visible && this.NavigationUIVisible)
            {
                this.NavigationUIVisibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Switches the viewing mode between full screen and windowed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Arguments describing the routed event.</param>
        private void OnSwitchFullScreenCommand(object sender, ExecutedRoutedEventArgs e)
        {
            this.UpdateViewingModeFullScreen();
            ContentPane.Focus();
        }

        /// <summary>
        /// Non-static handler for BrowseForward command.
        /// </summary>
        /// <param name="e">EventArgs describing the event.</param>
        private void OnBrowseForward(ExecutedRoutedEventArgs e)
        {
            if (SampleScePhotoSettings.EnableJournalNavigationOnInput)
            {
                if (this.CanGoForward)
                {
                    this.GoForward();
                }
            }
            else
            {
                // Check original source, if journal navigation on input is disabled, only navigate through journal if source is a navigation button
                // on main page
                if (e.OriginalSource == this.BrowseForwardButton)
                {
                    if (this.CanGoForward)
                    {
                        this.GoForward();
                    }
                }
                else
                {
                    if (ServiceProvider.ViewManager.NavigationCommands.NextPhotoCommand.CanExecute(null))
                    {
                        ServiceProvider.ViewManager.NavigationCommands.NextPhotoCommand.Execute(null);
                    }
                }
            }
        }

        /// <summary>
        /// Handler for scroll page up command
        /// </summary>
        /// <param name="e">Event args describing the executed event</param>
        private void OnScrollPageUp(ExecutedRoutedEventArgs e)
        {
            // On scroll commands, there is no need to check for journal navigation since these commands do not initiate journal navigation in any case

            // Scroll page commands directionally specify the way the content moves on a scrolling surface. So a scroll up is actually a
            // photo album down and vice vera  
            // There is no up/down concept when looking at an individual photo, so this command will do nothing when on a photo
            // If we are at a HomePhotoAlbumNavigator then we are at the top and don't want to do anything
            if (ServiceProvider.ViewManager.CurrentNavigator is PhotoAlbumNavigator)
            {
                // On a album, try previous album command
                ServiceProvider.ViewManager.NavigationCommands.PreviousPhotoAlbumCommand.Execute(null);
            }
        }

        /// <summary>
        /// Handler for scroll page down command
        /// </summary>
        /// <param name="e">Event args describing the executed event</param>
        private void OnScrollPageDown(ExecutedRoutedEventArgs e)
        {
            // On scroll commands, there is no need to check for journal navigation since these commands do not initiate journal navigation in any case

            // Scroll page commands directionally specify the way the content moves on a scrolling surface. So a scroll up is actually a
            // photo album down and vice vera  
            // There is no up/down concept when looking at an individual photo, so this command will do nothing when on a photo           
            if (ServiceProvider.ViewManager.CurrentNavigator is PhotoAlbumNavigator || ServiceProvider.ViewManager.CurrentNavigator is HomePhotoAlbumNavigator)
            {
                // On a album or album home, try Next album command
                ServiceProvider.ViewManager.NavigationCommands.NextPhotoAlbumCommand.Execute(null);
            }
        }

        /// <summary>
        /// Command handler for BrowseBack.
        /// </summary>
        /// <param name="e">EventArgs describing the event.</param>
        private void OnBrowseBack(ExecutedRoutedEventArgs e)
        {
            if (SampleScePhotoSettings.EnableJournalNavigationOnInput)
            {
                if (this.CanGoBack)
                {
                    this.GoBack();
                }
            }
            else
            {
                // Check original source, if journal navigation on input is disabled, only navigate through journal if source is a navigation button
                // on main page
                if (e.OriginalSource == this.BrowseBackButton)
                {
                    if (this.CanGoBack)
                    {
                        this.GoBack();
                    }
                }
                else
                {
                    if (ServiceProvider.ViewManager.NavigationCommands.PreviousPhotoCommand.CanExecute(null))
                    {
                        ServiceProvider.ViewManager.NavigationCommands.PreviousPhotoCommand.Execute(null);
                    }
                }
            }
        }

        /// <summary>
        /// Command handler for BrowseForward CanExecute.
        /// </summary>
        /// <param name="e">EventArgs describing the event.</param>
        private void OnBrowseForwardCanExecute(CanExecuteRoutedEventArgs e)
        {
            if (SampleScePhotoSettings.EnableJournalNavigationOnInput)
            {
                e.CanExecute = this.CanGoForward;
            }
            else
            {
                // Check original source, if journal navigation on input is disabled, only navigate through journal if source is a navigation button
                // on main page
                if (e.OriginalSource == this.BrowseForwardButton)
                {
                    e.CanExecute = this.CanGoForward;
                }
                else
                {
                    e.CanExecute = ServiceProvider.ViewManager.NavigationCommands.NextPhotoCommand.CanExecute(null);
                }
            }
        }

        /// <summary>
        /// Command handler for BrowseBack CanExecute.
        /// </summary>
        /// <param name="e">EventArgs describing the event.</param>
        private void OnBrowseBackCanExecute(CanExecuteRoutedEventArgs e)
        {
            if (SampleScePhotoSettings.EnableJournalNavigationOnInput)
            {
                e.CanExecute = this.CanGoBack;
            }
            else
            {
                // Check original source, if journal navigation on input is disabled, only navigate through journal if source is a navigation button
                // on main page
                if (e.OriginalSource == this.BrowseBackButton)
                {
                    e.CanExecute = this.CanGoBack;
                }
                else
                {
                    e.CanExecute = ServiceProvider.ViewManager.NavigationCommands.PreviousPhotoCommand.CanExecute(null);
                }
            }
        }

        /// <summary>
        /// Event handler for scroll page up executed event
        /// </summary>
        /// <param name="e">Event args describing the event></param>
        private void OnScrollPageUpCanExecute(CanExecuteRoutedEventArgs e)
        {
            // On scroll commands, there is no need to check for journal navigation since these commands do not initiate journal navigation in any case

            // Scroll page commands directionally specify the way the content moves on a scrolling surface. So a scroll up is actually a
            // photo album down and vice vera  
            // There is no up/down concept when looking at an individual photo, so this command will do nothing when on a photo
            // If we are at a HomePhotoAlbumNavigator then we are at the top and don't want to do anything
            if (ServiceProvider.ViewManager.CurrentNavigator is PhotoAlbumNavigator)
            {
                // On an album, try previous album command
                e.CanExecute = ServiceProvider.ViewManager.NavigationCommands.PreviousPhotoAlbumCommand.CanExecute(null);
            }
        }

        /// <summary>
        /// Event handler for scroll page down executed event
        /// </summary>
        /// <param name="e">Event args describing the event></param>
        private void OnScrollPageDownCanExecute(CanExecuteRoutedEventArgs e)
        {
            // On scroll commands, there is no need to check for journal navigation since these commands do not initiate journal navigation in any case

            // Scroll page commands directionally specify the way the content moves on a scrolling surface. So a scroll up is actually a
            // photo album down and vice vera  
            // There is no up/down concept when looking at an individual photo, so this command will do nothing when on a photo            
            if (ServiceProvider.ViewManager.CurrentNavigator is PhotoAlbumNavigator || ServiceProvider.ViewManager.CurrentNavigator is HomePhotoAlbumNavigator)
            {
                // On an album or album home, try previous album command
                e.CanExecute = ServiceProvider.ViewManager.NavigationCommands.NextPhotoAlbumCommand.CanExecute(null);
            }
        }

        /// <summary>
        /// Handler for back navigation key override.
        /// </summary>
        /// <param name="e">Event Args describing the event.</param>
        private void OnJournalBackNavigationKeyOverride(ExecutedRoutedEventArgs e)
        {
            // On back navigation override, go back regardless of whether journal navigation on input is enabled or not
            if (this.CanGoBack)
            {
                this.GoBack();
            }
        } 
        #endregion
    }
}
