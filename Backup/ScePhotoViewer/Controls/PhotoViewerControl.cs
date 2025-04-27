//-----------------------------------------------------------------------
// <copyright file="PhotoViewerControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Control used to display a full photo.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Threading;
    using EffectControls;
    using EffectLibrary;
    using ScePhoto;
    using ScePhoto.Controls;
    using ScePhoto.Data;
    using ScePhoto.Feed;
    using Microsoft.SubscriptionCenter.Sync;

    /// <summary>
    /// Control used to display a full photo.
    /// </summary>
    [TemplatePart(Name = "PART_PhotoDisplay", Type = typeof(PhotoDisplayControl)),
     TemplatePart(Name = "PART_PhotoDescriptionViewer", Type = typeof(FlowDocumentPageViewer)),
     TemplatePart(Name = "PART_FilmStrip", Type = typeof(FilmStripControl))]
    public class PhotoViewerControl : PhotoBaseControl
    {
        #region Fields

        /// <summary>
        /// Dependency Property backing store for PhotoDescriptionVisibility.
        /// </summary>
        public static readonly DependencyProperty PhotoDescriptionVisibilityProperty =
            DependencyProperty.Register("PhotoDescriptionVisibility", typeof(Visibility), typeof(PhotoViewerControl), new UIPropertyMetadata(Visibility.Collapsed));

        /// <summary>
        /// Dependency Property backing store for PhotoFlowDescriptionVisibility.
        /// </summary>
        public static readonly DependencyProperty PhotoFlowDescriptionVisibilityProperty =
            DependencyProperty.Register("PhotoFlowDescriptionVisibility", typeof(Visibility), typeof(PhotoViewerControl), new UIPropertyMetadata(Visibility.Collapsed, new PropertyChangedCallback(OnFlowDescriptionVisibilityChanged)));

        /// <summary>
        /// RoutedCommand to toggle the display of the photo's description.
        /// </summary>
        private static RoutedCommand displayPhotoDescriptionCommand = new RoutedCommand("DisplayPhotoDescription", typeof(PhotoViewerControl));

        /// <summary>
        /// RoutedCommand to toggle the display of the photo's flow description.
        /// </summary>
        private static RoutedCommand displayPhotoFlowDescriptionCommand = new RoutedCommand("DisplayPhotoFlowDescription", typeof(PhotoViewerControl));
       
        /// <summary>
        /// RoutedCommand to apply an effect.
        /// </summary>
        private static RoutedCommand toggleEffectCommand = new RoutedCommand("ToggleEffect", typeof(PhotoViewerControl));

        /// <summary>
        /// RoutedCommand to explore a tag with the tag explorer.
        /// </summary>
        private static RoutedCommand exploreTagCommand = new RoutedCommand("ExploreTag", typeof(PhotoViewerControl));

        /// <summary>
        /// The PhotoDisplayControl which will display the photo image.
        /// </summary>
        private PhotoDisplayControl photoDisplay;

        /// <summary>
        /// The FlowDocumentPageViewer displaying the photo description.
        /// </summary>
        private FlowDocumentPageViewer photoDescriptionViewer;

        /// <summary>
        /// The FilmStripControl that displays the thumbnails.
        /// </summary>
        private FilmStripControl filmStripControl;

        /// <summary>
        /// Whether descriptions were on or off in the previous photo.
        /// </summary>
        private Visibility previousDescriptionVisibility = Visibility.Collapsed;

        /// <summary>
        /// Whether descriptions were on or off in the previous photo.
        /// </summary>
        private Visibility previousFlowDescriptionVisibility = Visibility.Collapsed;

        /// <summary>
        /// Indicates whether image description download is in progress.
        /// </summary>
        private bool photoDescriptionDownloadInProgress;

        /// <summary>
        /// A reference to the delegate that updates the current Effect on the element targetElementForEffectPreviewMouseMove.
        /// </summary>
        private MouseEventHandler photoEffectPreviewMouseMoveImplementation;

        /// <summary>
        /// A reference to the element the has the photoEffectPreviewMouseMoveImplementation event attached to its PreviewMouseMove.
        /// </summary>
        private FrameworkElement targetElementForEffectPreviewMouseMove;
        #endregion

        #region Constructor
        /// <summary>
        /// PhotoViewerControl constructor; registers its command with the CommandManager.
        /// </summary>
        public PhotoViewerControl()
        {
            // Set the key commands for the photo viewer control.
            this.CommandBindings.Add(new CommandBinding(toggleEffectCommand, new ExecutedRoutedEventHandler(OnToggleEffectCommand)));
            this.CommandBindings.Add(new CommandBinding(displayPhotoDescriptionCommand, new ExecutedRoutedEventHandler(OnDisplayPhotoDescriptionCommand)));
            this.CommandBindings.Add(new CommandBinding(displayPhotoFlowDescriptionCommand, new ExecutedRoutedEventHandler(OnDisplayPhotoFlowDescriptionCommand)));            
            this.CommandBindings.Add(new CommandBinding(exploreTagCommand, new ExecutedRoutedEventHandler(OnExploreTagCommand)));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Print, new ExecutedRoutedEventHandler(OnPrintCommand)));
            this.KeyDown += new KeyEventHandler(OnKeyDown);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the RoutedCommand to toggle and Effect.
        /// </summary>
        public static RoutedCommand ToggleEffectCommand
        {
            get { return PhotoViewerControl.toggleEffectCommand; }
        }

        /// <summary>
        /// Gets the RoutedCommand to toggle the display of the photo's description.
        /// </summary>
        public static RoutedCommand DisplayPhotoDescriptionCommand
        {
            get { return PhotoViewerControl.displayPhotoDescriptionCommand; }
        }

        /// <summary>
        /// Gets the RoutedCommand to toggle the display of the photo's flow description.
        /// </summary>
        public static RoutedCommand DisplayPhotoFlowDescriptionCommand
        {
            get { return PhotoViewerControl.displayPhotoFlowDescriptionCommand; }
        }       

        /// <summary>
        /// Gets the RoutedCommand to explore a tag with the photo explorer.
        /// </summary>
        public static RoutedCommand ExploreTagCommand
        {
            get { return PhotoViewerControl.exploreTagCommand; }
        }

        /// <summary>
        /// Gets a value indicating whether the photo description is displayed or hidden/collapsed.
        /// </summary>
        public Visibility PhotoDescriptionVisibility
        {
            get { return (Visibility)GetValue(PhotoDescriptionVisibilityProperty); }
            protected set { SetValue(PhotoDescriptionVisibilityProperty, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the photo flow description is displayed or hidden/collapsed.
        /// </summary>
        public Visibility PhotoFlowDescriptionVisibility
        {
            get { return (Visibility)GetValue(PhotoFlowDescriptionVisibilityProperty); }
            protected set { SetValue(PhotoFlowDescriptionVisibilityProperty, value); }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Sets up the rotation animations once the control template has been applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.previousDescriptionVisibility = SampleScePhotoSettings.PhotoViewerShowsDescription ? Visibility.Visible : Visibility.Collapsed;
            this.previousFlowDescriptionVisibility = SampleScePhotoSettings.PhotoViewerShowsFlowDescription ? Visibility.Visible : Visibility.Collapsed;

            this.photoDisplay = this.Template.FindName("PART_PhotoDisplay", this) as PhotoDisplayControl;
            this.photoDescriptionViewer = this.Template.FindName("PART_PhotoDescriptionViewer", this) as FlowDocumentPageViewer;
            this.filmStripControl = this.Template.FindName("PART_FilmStrip", this) as FilmStripControl;

            // Focus the control so that we can grab keyboard events.
            this.Focus();
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Toggles the visibility of the photo's description.
        /// </summary>
        protected void TogglePhotoDescriptionVisibility()
        {
            if (this.PhotoDescriptionVisibility == Visibility.Collapsed)
            {
                this.previousDescriptionVisibility = Visibility.Visible;
                this.PhotoDescriptionVisibility = Visibility.Visible;
                SampleScePhotoSettings.PhotoViewerShowsDescription = true;
            }
            else
            {
                this.previousDescriptionVisibility = Visibility.Collapsed;
                this.PhotoDescriptionVisibility = Visibility.Collapsed;
                SampleScePhotoSettings.PhotoViewerShowsDescription = false;
            }
        }

        /// <summary>
        /// Toggles the visibility of the photo's flow description.
        /// Also disable the animated shader Effects on the filmstrip
        /// because the animations are not smooth when a large FlowDocument loads. 
        /// </summary>
        protected void TogglePhotoFlowDescriptionVisibility()
        {
            if (this.PhotoFlowDescriptionVisibility == Visibility.Collapsed)
            {
                this.filmStripControl.IsEffectsEnabled = false;
                this.previousFlowDescriptionVisibility = Visibility.Visible;
                this.PhotoFlowDescriptionVisibility = Visibility.Visible;
                SampleScePhotoSettings.PhotoViewerShowsFlowDescription = true;
            }
            else
            {
                this.filmStripControl.IsEffectsEnabled = true;
                this.previousFlowDescriptionVisibility = Visibility.Collapsed;
                this.PhotoFlowDescriptionVisibility = Visibility.Collapsed;
                SampleScePhotoSettings.PhotoViewerShowsFlowDescription = false;
            }
        }

        /// <summary>
        /// Allows the photo to be zoomed in and out using the mouse wheel.
        /// </summary>
        /// <param name="e">Arguments describing the event.</param>
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Delta > 0)
                {
                    PhotoDisplayControl.ZoomPhotoInCommand.Execute(null, this.photoDisplay);
                }
                else if (e.Delta < 0)
                {
                    PhotoDisplayControl.ZoomPhotoOutCommand.Execute(null, this.photoDisplay);
                }
            }
        }

        /// <summary>
        /// Allows the photo to be fit to the current window size using the mouse wheel.
        /// </summary>
        /// <param name="e">Arguments describing the event.</param>
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.MiddleButton == MouseButtonState.Pressed)
                {
                    this.photoDisplay.FittingPhotoToWindow = true;
                }
            }
        }

        /// <summary>
        /// Loaded override, attaches listener for DataManager's GetTextDocumentCompleted event.
        /// </summary>
        protected override void OnLoaded()
        {
            ServiceProvider.DataManager.GetTextDocumentCompleted += this.OnGetPhotoDescriptionTextCompleted;
            base.OnLoaded();
        }

        /// <summary>
        /// Loaded override, detaches listener for DataManager's GetTextDocumentCompleted event.
        /// </summary>
        protected override void OnUnloaded()
        {
            ServiceProvider.DataManager.CancelAsync(this);
            ServiceProvider.DataManager.GetTextDocumentCompleted -= this.OnGetPhotoDescriptionTextCompleted;

            this.RemoveEffects();

            base.OnUnloaded();
        }

        /// <summary>
        /// Updates the content of the control to contain the image at Photo.ImageUri.
        /// </summary>
        protected override void OnUpdateContent()
        {
            Photo photo = Photo;
            if (photo != null && photo.ImageUri != null)
            {
                ImageDownloadInProgress = true;
                ServiceProvider.DataManager.GetImageSourceAsync(photo.ImageUri, this);
            }
            else
            {
                ImageSource = null;
            }
        }

        /// <summary>
        /// Invalidates the content of the control and starts an asynchronous content update.
        /// </summary>
        protected override void InvalidateContent()
        {
            base.InvalidateContent();
            if (this.photoDescriptionDownloadInProgress)
            {
                ServiceProvider.DataManager.CancelAsync(this);
            }
        }

        /// <summary>
        /// Resets the photo display angle then calls the base function to update the displayed photo.
        /// </summary>
        /// <param name="e">Argument containing the details of the event.</param>
        protected override void OnGetImageSourceCompleted(ScePhoto.Feed.GetImageSourceCompletedEventArgs e)
        {
            // Focus the main control on image changed
            this.Focus();

            // Animate the photo back to defaults (simply setting the value doesn't work).
            this.photoDisplay.ResetPosition();
            this.PhotoDescriptionVisibility = Visibility.Collapsed;

            base.OnGetImageSourceCompleted(e);
            this.photoDisplay.DoInitialFit();

            this.PhotoDescriptionVisibility = this.previousDescriptionVisibility;
            this.PhotoFlowDescriptionVisibility = this.previousFlowDescriptionVisibility;

            if (this.previousFlowDescriptionVisibility == Visibility.Visible)
            {
                // If flow description is visible, fetch the full description asynchronously
                Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new DispatcherOperationCallback(this.GetFullPhotoDescription), null);
            }
        }

        /// <summary>
        /// Handler for changes in flow description visibility.
        /// </summary>
        /// <param name="visibility">New visibility of photo flow description.</param>
        protected void OnFlowDescriptionVisibilityChanged(Visibility visibility)
        {
            if (visibility == Visibility.Visible)
            {
                // If flow description is visible, fetch the full description asynchronously
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(this.GetFullPhotoDescription), null);
            }
        }

        /// <summary>
        /// Command to apply an the Effect on photoImage. This applies each effect to the target element
        /// and also listens to the target element's mouse position to update the center.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnToggleEffectCommand(object sender, ExecutedRoutedEventArgs e)
        {
            PhotoViewerControl photoViewer = sender as PhotoViewerControl;

            if (photoViewer != null)
            {
                ToggleButton toggleEffectButton = (ToggleButton)photoViewer.Template.FindName("PART_ToggleEffect", photoViewer);
                UIElement panel = (UIElement)photoViewer.Template.FindName("PART_PhotoToolbar", photoViewer);

                if (toggleEffectButton.IsChecked == true)
                {
                    photoViewer.ApplyEffects();
                    panel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    panel.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Command to toggle the display of the photo's description.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnDisplayPhotoDescriptionCommand(object sender, ExecutedRoutedEventArgs e)
        {
            PhotoViewerControl photoViewer = sender as PhotoViewerControl;
            if (photoViewer != null)
            {
                photoViewer.TogglePhotoDescriptionVisibility();
            }
        }

        /// <summary>
        /// Command to toggle the display of the photo's flow description.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnDisplayPhotoFlowDescriptionCommand(object sender, ExecutedRoutedEventArgs e)
        {
            PhotoViewerControl photoViewer = sender as PhotoViewerControl;
            if (photoViewer != null)
            {
                photoViewer.TogglePhotoFlowDescriptionVisibility();
            }
        }       

        /// <summary>
        /// Command to use the photo explorer to explore a tag.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnExploreTagCommand(object sender, ExecutedRoutedEventArgs e)
        {
            PhotoViewerControl photoViewer = sender as PhotoViewerControl;
            if (photoViewer != null)
            {
                ServiceProvider.ViewManager.NavigationCommands.SearchCommand.Execute("explore:" + e.Parameter.ToString());
            }
        }

        /// <summary>
        /// Command to print the current photo.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnPrintCommand(object sender, ExecutedRoutedEventArgs e)
        {
            PhotoViewerControl photoViewer = sender as PhotoViewerControl;
            if (photoViewer != null)
            {
                try
                {
                    string photoLocalPath = string.Empty;
                    LocalDataFeedSource dataFeedSource = ServiceProvider.DataFeedSource as LocalDataFeedSource;
                    if (dataFeedSource != null)
                    {
                        if (dataFeedSource.SyncItemCache.IsCached(photoViewer.Photo.ImageUri))
                        {
                            photoLocalPath = dataFeedSource.SyncItemCache.CreateTempFileFromCache(photoViewer.Photo.ImageUri);
                        }
                    }

                    if (!string.IsNullOrEmpty(photoLocalPath))
                    {
                        photoLocalPath = System.IO.Path.GetFullPath(photoLocalPath);
                        object photoFile = photoLocalPath;
                        WIA.CommonDialog dialog = new WIA.CommonDialogClass();
                        dialog.ShowPhotoPrintingWizard(ref photoFile);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// Handler for key press events that target the photo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                PhotoViewerControl photoViewerControl = sender as PhotoViewerControl;
                if (photoViewerControl != null)
                {
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                    {
                        switch (e.Key)
                        {
                            case Key.OemPeriod:
                                PhotoDisplayControl.RotatePhotoClockwiseCommand.Execute(null, photoViewerControl.photoDisplay);
                                e.Handled = true;
                                break;
                            case Key.OemComma:
                                PhotoDisplayControl.RotatePhotoCounterClockwiseCommand.Execute(null, photoViewerControl.photoDisplay);
                                e.Handled = true;
                                break;
                            case Key.OemPlus:
                                PhotoDisplayControl.ZoomPhotoInCommand.Execute(null, photoViewerControl.photoDisplay);
                                e.Handled = true;
                                break;
                            case Key.OemMinus:
                                PhotoDisplayControl.ZoomPhotoOutCommand.Execute(null, photoViewerControl.photoDisplay);
                                e.Handled = true;
                                break;
                            case Key.D0:
                                PhotoDisplayControl.FitPhotoToWindowCommand.Execute(null, photoViewerControl.photoDisplay);
                                e.Handled = true;
                                break;
                            case Key.D:
                                photoViewerControl.TogglePhotoDescriptionVisibility();
                                e.Handled = true;
                                break;
                            case Key.R:
                                photoViewerControl.TogglePhotoFlowDescriptionVisibility();
                                break;
                            default:
                                break;
                        }
                    }
                    else if (e.KeyboardDevice.Modifiers == ModifierKeys.None)
                    {
                        switch (e.Key)
                        {
                            case Key.Escape:
                                photoViewerControl.Focus();
                                e.Handled = true;
                                break;
                            case Key.Up:
                            case Key.PageUp:
                                photoViewerControl.HandleKeyUp();
                                e.Handled = true;
                                break;
                            case Key.Down:
                            case Key.PageDown:
                                photoViewerControl.HandleKeyDown();
                                e.Handled = true;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handler for FlowDescriptionVisibility changes.
        /// </summary>
        /// <param name="element">The element that changed.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnFlowDescriptionVisibilityChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            ((PhotoViewerControl)element).OnFlowDescriptionVisibilityChanged((Visibility)e.NewValue);
        }

        /// <summary>
        /// Dispatcher callback to get full photo description
        /// </summary>
        /// <param name="arg">Argument to the callback</param>
        /// <returns>Photo description as a string</returns>
        private object GetFullPhotoDescription(object arg)
        {
            Photo photo = this.Photo;
            if (photo != null)
            {
                if (photo.DescriptionFileUri != null)
                {
                    // Photo has a separate description file, fetch it
                    this.photoDescriptionDownloadInProgress = true;
                    ServiceProvider.DataManager.GetTextDocumentAsync(photo.DescriptionFileUri, this);
                }
                else
                {
                    this.ApplyPhotoDescriptionText(photo.Description, false);
                }
            }

            return null;
        }

        /// <summary>
        /// Handle Down key - if flow description is visible in viewer, go to next page in page viewer.
        /// </summary>
        private void HandleKeyDown()
        {
            if (this.PhotoFlowDescriptionVisibility == Visibility.Visible)
            {
                if (this.photoDescriptionViewer != null && this.photoDescriptionViewer.CanGoToNextPage)
                {
                    this.photoDescriptionViewer.NextPage();
                }
            }
        }

        /// <summary>
        /// Handle Up key - if flow description is visible in viewer, go to previous page in page viewer.
        /// </summary>
        private void HandleKeyUp()
        {
            if (this.PhotoFlowDescriptionVisibility == Visibility.Visible)
            {
                if (this.photoDescriptionViewer != null && this.photoDescriptionViewer.CanGoToPreviousPage)
                {
                    this.photoDescriptionViewer.PreviousPage();
                }
            }
        }

        /// <summary>
        /// If photo description is stored in another file, fetches it asynchronously and updates full description.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Arguments describing the event.</param>
        private void OnGetPhotoDescriptionTextCompleted(object sender, GetTextDocumentCompletedEventArgs e)
        {
            this.photoDescriptionDownloadInProgress = false;
            string description = String.Empty;
            if (e.Error == null && !e.Cancelled && this.Photo != null)
            {
                if (Uri.Compare(this.Photo.DescriptionFileUri, e.Link, UriComponents.HttpRequestUrl, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    description = e.DocumentText;
                }
            }

            this.ApplyPhotoDescriptionText(description, true);
        }

        /// <summary>
        /// Prepares photo viewer for photo description by setting appropriate properties for drop cap, etc
        /// </summary>
        /// <param name="text">Text to apply for full photo description</param>
        /// <param name="applyDropCap">True if the description and should be styled w/ drop cap</param>
        private void ApplyPhotoDescriptionText(string text, bool applyDropCap)
        {
            if (this.photoDescriptionViewer != null)
            {
                // Prepare flow document with description text
                FlowDocument document = new FlowDocument();
                document.IsHyphenationEnabled = true;
                document.IsOptimalParagraphEnabled = true;
                this.photoDescriptionViewer.Document = document;

                Paragraph emptyParagraph = new Paragraph();
                emptyParagraph.LineHeight = 0.1;
                emptyParagraph.FontSize = 0.1;
                document.Blocks.Add(emptyParagraph);

                Paragraph descriptionParagraph = new Paragraph();
                HtmlToCleanStringConverter converter = new HtmlToCleanStringConverter();
                string cleanText = converter.Convert(text, typeof(string), null, CultureInfo.CurrentCulture) as string;
                descriptionParagraph.Style = Application.Current.FindResource("DropCapParaStyle") as Style;

                if (!applyDropCap)
                {
                    // No drop cap, display text as single inline
                    Run run = new Run(cleanText);
                    descriptionParagraph.Inlines.Add(run);
                }
                else
                {
                    if (cleanText.Length > 0)
                    {
                        // Extract the first character for drop cap
                        Run dropCapRun = new Run(cleanText.Substring(0, 1));
                        dropCapRun.Style = Application.Current.FindResource("DropCapTextStyle") as Style;
                        Paragraph dropCapPara = new Paragraph(dropCapRun);
                        dropCapPara.Margin = new Thickness(0);
                        dropCapPara.Padding = new Thickness(0);
                        Figure dropCapFigure = new Figure(dropCapPara);
                        dropCapFigure.Style = Application.Current.FindResource("DropCapFigureStyle") as Style;
                        descriptionParagraph.Inlines.Add(dropCapFigure);

                        if (cleanText.Length > 1)
                        {
                            // Put the remaining text in a run
                            Run remainingText = new Run(cleanText.Substring(1, cleanText.Length - 1));
                            descriptionParagraph.Inlines.Add(remainingText);
                        }
                    }
                }

                document.Blocks.Add(descriptionParagraph);
            }
        }

        /// <summary>
        /// Enables effects within this control and on the MainWindow.
        /// </summary>
        private void ApplyEffects()
        {
            this.targetElementForEffectPreviewMouseMove = Application.Current.MainWindow;

            SmoothMagnifyControl smc = (SmoothMagnifyControl)this.Template.FindName("PART_SmoothMagnifyControl", this);
            smc.SetTarget(this.photoDisplay.MagnifyEffectHost);

            InvertColorControl icc = (InvertColorControl)this.Template.FindName("PART_InvertColorControl", this);
            icc.SetTarget(this.photoDisplay.InvertColorEffectHost);

            MonochromeControl mcc = (MonochromeControl)this.Template.FindName("PART_MonochromeControl", this);
            mcc.SetTarget(this.photoDisplay.MonochromeEffectHost);

            RippleShaderControl rsc = (RippleShaderControl)this.Template.FindName("PART_RippleShaderControl", this);
            rsc.SetTarget(this.photoDisplay.RippleEffectHost);

            SwirlControl sc = (SwirlControl)this.Template.FindName("PART_SwirlControl", this);
            sc.SetTarget(this.photoDisplay.SwirlEffectHost);

            BandedSwirlControl bsc = (BandedSwirlControl)this.Template.FindName("PART_BandedSwirlControl", this);
            bsc.SetTarget(this.photoDisplay.BandedSwirlEffectHost);

            if (this.photoEffectPreviewMouseMoveImplementation != null)
            {
                this.targetElementForEffectPreviewMouseMove.PreviewMouseMove -= this.photoEffectPreviewMouseMoveImplementation;
                this.photoEffectPreviewMouseMoveImplementation = null;
            }

            // Set position Center point for the effects that need it to the current position of the mouse
            this.photoEffectPreviewMouseMoveImplementation = delegate(object o, MouseEventArgs mouseargs)
            {
                Point mousePosition;

                if (this.photoDisplay != null && this.photoDisplay.PhotoImage != null)
                {
                    mousePosition = Mouse.GetPosition(this.photoDisplay.PhotoImage);
                    mousePosition.X /= this.photoDisplay.PhotoImage.ActualWidth;
                    mousePosition.Y /= this.photoDisplay.PhotoImage.ActualHeight;
                    bsc.SetMousePosition(mousePosition);
                    rsc.SetMousePosition(mousePosition);
                    smc.SetMousePosition(mousePosition);
                }

                mouseargs.Handled = false;
            };

            this.targetElementForEffectPreviewMouseMove.PreviewMouseMove += this.photoEffectPreviewMouseMoveImplementation;
        }

        /// <summary>
        /// Removes all effects applied to this control and the MainWindow.
        /// </summary>
        private void RemoveEffects()
        {
            if (this.photoEffectPreviewMouseMoveImplementation != null && this.targetElementForEffectPreviewMouseMove != null)
            {
                this.targetElementForEffectPreviewMouseMove.PreviewMouseMove -= this.photoEffectPreviewMouseMoveImplementation;
                this.targetElementForEffectPreviewMouseMove = null;
                this.photoEffectPreviewMouseMoveImplementation = null;
            }

            if (this.photoDisplay.MagnifyEffectHost != null)
            {
                this.photoDisplay.MagnifyEffectHost.Effect = null;
            }

            if (this.photoDisplay.InvertColorEffectHost != null)
            {
                this.photoDisplay.InvertColorEffectHost.Effect = null;
            }

            if (this.photoDisplay.MonochromeEffectHost != null)
            {
                this.photoDisplay.MonochromeEffectHost.Effect = null;
            }

            if (this.photoDisplay.RippleEffectHost != null)
            {
                this.photoDisplay.RippleEffectHost.Effect = null;
            }

            if (this.photoDisplay.SwirlEffectHost != null)
            {
                this.photoDisplay.SwirlEffectHost.Effect = null;
            }

            if (this.photoDisplay.BandedSwirlEffectHost != null)
            {
                this.photoDisplay.BandedSwirlEffectHost.Effect = null;
            }
        }
        #endregion
    }
}
