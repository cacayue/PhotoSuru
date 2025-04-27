//-----------------------------------------------------------------------
// <copyright file="PhotoDisplayControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Control used to display and animate a photo.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Data;
    using EffectControls;
    using EffectLibrary;

    /// <summary>
    /// Control used to display and animate a photo.
    /// </summary>
    [TemplatePart(Name = "PART_PhotoDisplayAngleTransform", Type = typeof(RotateTransform)),
     TemplatePart(Name = "PART_PhotoZoomFactorTransform", Type = typeof(ScaleTransform)),
     TemplatePart(Name = "PART_PhotoTransitionScaleTransform", Type = typeof(ScaleTransform)),
     TemplatePart(Name = "PART_PhotoViewbox", Type = typeof(Viewbox)),
     TemplatePart(Name = "PART_PhotoImage", Type = typeof(Image)),
     TemplatePart(Name = "PART_MagnifyEffectHost", Type = typeof(FrameworkElement)),
     TemplatePart(Name = "PART_SwirlEffectHost", Type = typeof(FrameworkElement)),
     TemplatePart(Name = "PART_BandedSwirlEffectHost", Type = typeof(FrameworkElement)),
     TemplatePart(Name = "PART_RippleEffectHost", Type = typeof(FrameworkElement)),
     TemplatePart(Name = "PART_MonochromeEffectHost", Type = typeof(FrameworkElement)),
     TemplatePart(Name = "PART_InvertColorEffectHost", Type = typeof(FrameworkElement))]
    public class PhotoDisplayControl : Control
    {
        #region Fields
        /// <summary>
        /// Dependency Property backing store for ContainerWidth.
        /// </summary>
        public static readonly DependencyProperty ContainerWidthProperty =
            DependencyProperty.Register("ContainerWidth", typeof(double), typeof(PhotoDisplayControl), new UIPropertyMetadata(0.0));

        /// <summary>
        /// Dependency Property backing store for ContainerHeight.
        /// </summary>
        public static readonly DependencyProperty ContainerHeightProperty =
            DependencyProperty.Register("ContainerHeight", typeof(double), typeof(PhotoDisplayControl), new UIPropertyMetadata(0.0));

        /// <summary>
        /// Dependency Property backing store for ViewWidth.
        /// </summary>
        public static readonly DependencyProperty ViewWidthProperty =
            DependencyProperty.Register("ViewWidth", typeof(double), typeof(PhotoDisplayControl), new UIPropertyMetadata(0.0));

        /// <summary>
        /// Dependency Property backing store for ViewHeight.
        /// </summary>
        public static readonly DependencyProperty ViewHeightProperty =
            DependencyProperty.Register("ViewHeight", typeof(double), typeof(PhotoDisplayControl), new UIPropertyMetadata(0.0));

        /// <summary>
        /// Dependency Property backing store for IsTransitioning.
        /// </summary>
        public static readonly DependencyProperty IsTransitioningProperty =
            DependencyProperty.Register("IsTransitioning", typeof(bool), typeof(PhotoDisplayControl), new UIPropertyMetadata(false));

        /// <summary>
        /// Dependency Property backing store for ImageSource.
        /// </summary>
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(PhotoDisplayControl), new UIPropertyMetadata(null));

        /// <summary>
        /// Dependency Property backing store for PhotoDisplayAngle.
        /// </summary>
        public static readonly DependencyProperty PhotoDisplayAngleProperty =
            DependencyProperty.Register("PhotoDisplayAngle", typeof(double), typeof(PhotoDisplayControl), new UIPropertyMetadata(0.0));

        /// <summary>
        /// Dependency Property backing store for PhotoZoomFactor.
        /// </summary>
        public static readonly DependencyProperty PhotoZoomFactorProperty =
            DependencyProperty.Register("PhotoZoomFactor", typeof(double), typeof(PhotoDisplayControl), new UIPropertyMetadata(1.0));

        /// <summary>
        /// DependencyProperty backing store for FittingPhotoToWindow.
        /// </summary>
        public static readonly DependencyProperty FittingPhotoToWindowProperty =
            DependencyProperty.Register("FittingPhotoToWindow", typeof(bool), typeof(PhotoDisplayControl), new UIPropertyMetadata(true, new PropertyChangedCallback(OnFittingPhotoToWindowChanged)));

        /// <summary>
        /// DependencyProperty backing store for RotatePhotoAnimation.
        /// </summary>
        public static readonly DependencyProperty RotatePhotoAnimationProperty =
            DependencyProperty.Register("RotatePhotoAnimation", typeof(DoubleAnimation), typeof(PhotoDisplayControl), new UIPropertyMetadata(null));

        /// <summary>
        /// DependencyProperty backing store for ZoomPhotoAnimation.
        /// </summary>
        public static readonly DependencyProperty ZoomPhotoAnimationProperty =
            DependencyProperty.Register("ZoomPhotoAnimation", typeof(DoubleAnimation), typeof(PhotoDisplayControl), new UIPropertyMetadata(null));

        /// <summary>
        /// DependencyProperty backing store for DisplayPhotoAnimation.
        /// </summary>
        public static readonly DependencyProperty DisplayPhotoAnimationProperty =
            DependencyProperty.Register("DisplayPhotoAnimation", typeof(DoubleAnimation), typeof(PhotoDisplayControl), new UIPropertyMetadata(null));

        /// <summary>
        /// RoutedCommand to rotate the displayed photo counterclockwise.
        /// </summary>
        public static readonly RoutedCommand RotatePhotoClockwiseCommand = new RoutedCommand("RotatePhotoClockwise", typeof(PhotoDisplayControl));

        /// <summary>
        /// RoutedCommand to rotate the displayed photo clockwise.
        /// </summary>
        public static readonly RoutedCommand RotatePhotoCounterClockwiseCommand = new RoutedCommand("RotatePhotoCounterClockwise", typeof(PhotoDisplayControl));

        /// <summary>
        /// RoutedCommand to zoom the displayed photo in.
        /// </summary>
        public static readonly RoutedCommand ZoomPhotoInCommand = new RoutedCommand("ZoomPhotoIn", typeof(PhotoDisplayControl));

        /// <summary>
        /// RoutedCommand to zoom the displayed photo out.
        /// </summary>
        public static readonly RoutedCommand ZoomPhotoOutCommand = new RoutedCommand("ZooomPhotoOut", typeof(PhotoDisplayControl));

        /// <summary>
        /// RoutedCommand to fit the displayed photo to the window size.
        /// </summary>
        public static readonly RoutedCommand FitPhotoToWindowCommand = new RoutedCommand("FitPhotoToWindow", typeof(PhotoDisplayControl));

        /// <summary>
        /// Whether the current animation results in a fit-to-window upon completion.
        /// </summary>
        private bool switchFittingMode;

        /// <summary>
        /// The Viewbox displaying the photo.
        /// </summary>
        private Viewbox photoViewbox;

        /// <summary>
        /// The Image containing the photo.
        /// </summary>
        private Image photoImage;

        /// <summary>
        /// The FrameworkElement that hosts the magnify effect for the photo.
        /// </summary>
        private FrameworkElement magnifyEffectHost;

        /// <summary>
        /// The FrameworkElement that hosts the swirl effect for the photo.
        /// </summary>
        private FrameworkElement swirlEffectHost;

        /// <summary>
        /// The FrameworkElement that hosts the banded swirl effect for the photo.
        /// </summary>
        private FrameworkElement bandedSwirlEffectHost;

        /// <summary>
        /// The FrameworkElement that hosts the ripple effect for the photo.
        /// </summary>
        private FrameworkElement rippleEffectHost;

        /// <summary>
        /// The FrameworkElement that hosts the monochrome effect for the photo.
        /// </summary>
        private FrameworkElement monochromeEffectHost;

        /// <summary>
        /// The FrameworkElement that hosts the invert color effect for the photo.
        /// </summary>
        private FrameworkElement invertColorEffectHost;

        /// <summary>
        /// Animation on the ZoomBlurEffect used when a photo's zoomPhotoAnimation is active.
        /// </summary>
        private DoubleAnimation zoomPhotoBlurEffectAnimation;

        /// <summary>
        /// The Effect applied to the photo when it is being zoomed.
        /// </summary>
        private ZoomBlurEffect zoomPhotoBlurEffect;

        /// <summary>
        /// Transform used to rotate the displayed photo.
        /// </summary>
        private RotateTransform photoAngleTransform;

        /// <summary>
        /// Transform used to zoom the displayed photo.
        /// </summary>
        private ScaleTransform photoZoomTransform;

        /// <summary>
        /// Transform used to scale the photo when transitioning.
        /// </summary>
        private ScaleTransform photoTransitionScaleTransform;

        /// <summary>
        /// Animation used when rotating a photo.
        /// </summary>
        private DoubleAnimation rotatePhotoAnimation;

        /// <summary>
        /// Animation used when zooming a photo.
        /// </summary>
        private DoubleAnimation zoomPhotoAnimation;

        /// <summary>
        /// Animation used when initially displaying a photo.
        /// </summary>
        private DoubleAnimation displayPhotoAnimation;

        /// <summary>
        /// The total size of all borders around the photo (to be excluded when calculating the fit-to-window zoom factor).
        /// </summary>
        private double photoBorderWidth = 65;

        /// <summary>
        /// The factor to scale by when zooming.
        /// </summary>
        private double baseZoomFactor = 0.20;

        /// <summary>
        /// The zoom factor for the current that causes a change in size by baseZoomFactor percent.
        /// </summary>
        private double scaledZoomFactor;
        #endregion

        #region Constructor
        /// <summary>
        /// photoDisplayControl constructor; registers its command with the CommandManager.
        /// </summary>
        public PhotoDisplayControl()
        {
            this.CommandBindings.Add(new CommandBinding(RotatePhotoClockwiseCommand, new ExecutedRoutedEventHandler(OnRotatePhotoClockwiseCommand)));
            this.CommandBindings.Add(new CommandBinding(RotatePhotoCounterClockwiseCommand, new ExecutedRoutedEventHandler(OnRotatePhotoCounterClockwiseCommand)));
            this.CommandBindings.Add(new CommandBinding(ZoomPhotoInCommand, new ExecutedRoutedEventHandler(OnZoomPhotoInCommand)));
            this.CommandBindings.Add(new CommandBinding(ZoomPhotoOutCommand, new ExecutedRoutedEventHandler(OnZoomPhotoOutCommand), new CanExecuteRoutedEventHandler(OnZoomPhotoOutCanExecute)));
            this.CommandBindings.Add(new CommandBinding(FitPhotoToWindowCommand, new ExecutedRoutedEventHandler(OnFitPhotoToWindow)));
        }
        #endregion

        #region Enums
        /// <summary>
        /// How we're fitting the photo to the window size.
        /// </summary>
        protected enum FitToWindowType
        {
            /// <summary>
            /// Zoom the photo in (initial display).
            /// </summary>
            InitialFit,

            /// <summary>
            /// Animate the change in size.
            /// </summary>
            AnimatedFit,

            /// <summary>
            /// Snap the change in size.
            /// </summary>
            ImmediateFit
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the Image containing the photo.
        /// </summary>
        public Image PhotoImage
        {
            get { return this.photoImage; }
        }

        /// <summary>
        /// Gets the FrameworkElement that hosts the magnify effect for the photo.
        /// </summary>
        public FrameworkElement MagnifyEffectHost
        {
            get { return this.magnifyEffectHost; }
        }

        /// <summary>
        /// Gets the FrameworkElement that hosts the swirl effect for the photo.
        /// </summary>
        public FrameworkElement SwirlEffectHost
        {
            get { return this.swirlEffectHost; }
        }

        /// <summary>
        /// Gets the FrameworkElement that hosts the banded swirl effect for the photo.
        /// </summary>
        public FrameworkElement BandedSwirlEffectHost
        {
            get { return this.bandedSwirlEffectHost; }
        }

        /// <summary>
        /// Gets the FrameworkElement that hosts the ripple effect for the photo.
        /// </summary>
        public FrameworkElement RippleEffectHost
        {
            get { return this.rippleEffectHost; }
        }

        /// <summary>
        /// Gets the FrameworkElement that hosts the monochrome effect for the photo.
        /// </summary>
        public FrameworkElement MonochromeEffectHost
        {
            get { return this.monochromeEffectHost; }
        }

        /// <summary>
        /// Gets the FrameworkElement that hosts the invert color effect for the photo.
        /// </summary>
        public FrameworkElement InvertColorEffectHost
        {
            get { return this.invertColorEffectHost; }
        }

        /// <summary>
        /// Gets or sets the actual width of the container object.
        /// </summary>
        public double ContainerWidth
        {
            get { return (double)GetValue(ContainerWidthProperty); }
            set { SetValue(ContainerWidthProperty, value); }
        }

        /// <summary>
        /// Gets or sets the actual height of the container object.
        /// </summary>
        public double ContainerHeight
        {
            get { return (double)GetValue(ContainerHeightProperty); }
            set { SetValue(ContainerHeightProperty, value); }
        }

        /// <summary>
        /// Gets the actual width of the contained Viewbox.
        /// </summary>
        public double ViewWidth
        {
            get { return (double)GetValue(ViewWidthProperty); }
            protected set { SetValue(ViewWidthProperty, value); }
        }

        /// <summary>
        /// Gets the actual height of the contained Viewbox.
        /// </summary>
        public double ViewHeight
        {
            get { return (double)GetValue(ViewHeightProperty); }
            protected set { SetValue(ViewHeightProperty, value); }
        }

        /// <summary>
        /// Gets a value indicating whether or not the image is currently being animated.
        /// </summary>
        public bool IsTransitioning
        {
            get { return (bool)GetValue(IsTransitioningProperty); }
            protected set { SetValue(IsTransitioningProperty, value); }
        }

        /// <summary>
        /// Gets or sets the actual image content to display.
        /// </summary>
        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        /// <summary>
        /// Gets the angle by which to rotate the photo when displaying.
        /// </summary>
        public double PhotoDisplayAngle
        {
            get { return (double)GetValue(PhotoDisplayAngleProperty); }
            protected set { SetValue(PhotoDisplayAngleProperty, value); }
        }

        /// <summary>
        /// Gets a value by which to scale the displayed photo.
        /// </summary>
        public double PhotoZoomFactor
        {
            get { return (double)GetValue(PhotoZoomFactorProperty); }
            protected set { SetValue(PhotoZoomFactorProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether we are fitting the photo to the window size or resizing it independently.
        /// </summary>
        public bool FittingPhotoToWindow
        {
            get { return (bool)GetValue(FittingPhotoToWindowProperty); }
            set { SetValue(FittingPhotoToWindowProperty, value); }
        }

        /// <summary>
        /// Gets or sets animation used when rotating a photo.
        /// </summary>
        public DoubleAnimation RotatePhotoAnimation
        {
            get { return (DoubleAnimation)GetValue(RotatePhotoAnimationProperty); }
            set { SetValue(RotatePhotoAnimationProperty, value); }
        }

        /// <summary>
        /// Gets or sets animation used when zooming a photo.
        /// </summary>
        public DoubleAnimation ZoomPhotoAnimation
        {
            get { return (DoubleAnimation)GetValue(ZoomPhotoAnimationProperty); }
            set { SetValue(ZoomPhotoAnimationProperty, value); }
        }

        /// <summary>
        /// Gets or sets animation used when initially displaying a photo.
        /// </summary>
        public DoubleAnimation DisplayPhotoAnimation
        {
            get { return (DoubleAnimation)GetValue(DisplayPhotoAnimationProperty); }
            set { SetValue(DisplayPhotoAnimationProperty, value); }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Sets up the rotation animations once the control template has been applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Get private modifiable copies of animations since they will be frozen
            if (this.RotatePhotoAnimation != null)
            {
                this.rotatePhotoAnimation = this.RotatePhotoAnimation.Clone();
            }
            else
            {
                this.rotatePhotoAnimation = new DoubleAnimation();
                this.rotatePhotoAnimation.Duration = new TimeSpan(0);
            }

            if (this.ZoomPhotoAnimation != null)
            {
                this.zoomPhotoAnimation = this.ZoomPhotoAnimation.Clone();
            }
            else
            {
                this.zoomPhotoAnimation = new DoubleAnimation();
                this.zoomPhotoAnimation.Duration = new TimeSpan(0);
            }

            if (this.DisplayPhotoAnimation != null)
            {
                this.displayPhotoAnimation = this.DisplayPhotoAnimation.Clone();
            }
            else
            {
                this.displayPhotoAnimation = new DoubleAnimation();
                this.displayPhotoAnimation.Duration = new TimeSpan(0);
            }

            this.zoomPhotoBlurEffectAnimation = new DoubleAnimation();
            this.zoomPhotoBlurEffectAnimation.AccelerationRatio = 0.4;
            this.zoomPhotoBlurEffectAnimation.DecelerationRatio = 0.2;

            this.photoAngleTransform = this.Template.FindName("PART_PhotoDisplayAngleTransform", this) as RotateTransform;
            this.photoZoomTransform = this.Template.FindName("PART_PhotoZoomFactorTransform", this) as ScaleTransform;
            this.photoTransitionScaleTransform = this.Template.FindName("PART_PhotoTransitionScaleTransform", this) as ScaleTransform;
            this.photoViewbox = this.Template.FindName("PART_PhotoViewbox", this) as Viewbox;
            this.photoImage = this.Template.FindName("PART_PhotoImage", this) as Image;

            this.magnifyEffectHost = this.Template.FindName("PART_MagnifyEffectHost", this) as FrameworkElement;
            this.swirlEffectHost = this.Template.FindName("PART_SwirlEffectHost", this) as FrameworkElement;
            this.bandedSwirlEffectHost = this.Template.FindName("PART_BandedSwirlEffectHost", this) as FrameworkElement;
            this.rippleEffectHost = this.Template.FindName("PART_RippleEffectHost", this) as FrameworkElement;
            this.monochromeEffectHost = this.Template.FindName("PART_MonochromeEffectHost", this) as FrameworkElement;
            this.invertColorEffectHost = this.Template.FindName("PART_InvertColorEffectHost", this) as FrameworkElement;

            // Set up bindings for the ViewWidth and ViewHeight properties
            Binding binding = new Binding();
            binding.Source = this.photoViewbox;
            binding.Path = new PropertyPath("ActualWidth");
            binding.Mode = BindingMode.OneWay;
            this.SetBinding(PhotoDisplayControl.ViewWidthProperty, binding);

            binding = new Binding();
            binding.Source = this.photoViewbox;
            binding.Path = new PropertyPath("ActualHeight");
            binding.Mode = BindingMode.OneWay;
            this.SetBinding(PhotoDisplayControl.ViewHeightProperty, binding);

            // Set up an event handler on the transitions
            this.displayPhotoAnimation.Completed += new EventHandler(this.OnDisplayPhotoElementCompleted);
            this.rotatePhotoAnimation.Completed += new EventHandler(this.OnRotatePhotoAnimationCompleted);
        }

        /// <summary>
        /// Animate the photo back to defaults.
        /// </summary>
        public void ResetPosition()
        {
            // Animate the photo back to defaults (simply setting the value doesn't work).
            if (this.photoAngleTransform != null)
            {
                Duration rotatePhotoDuration = this.rotatePhotoAnimation.Duration;
                this.rotatePhotoAnimation.Duration = new Duration(new TimeSpan(0));
                this.rotatePhotoAnimation.From = this.PhotoDisplayAngle % 360;
                this.rotatePhotoAnimation.To = 0.0;
                this.photoAngleTransform.BeginAnimation(RotateTransform.AngleProperty, this.rotatePhotoAnimation);
                this.rotatePhotoAnimation.Duration = rotatePhotoDuration;
            }

            this.PhotoDisplayAngle = 0.0;
        }

        /// <summary>
        /// Does initial animated fit after image source has been set.
        /// </summary>
        public void DoInitialFit()
        {
            this.FitPhotoToWindow(FitToWindowType.InitialFit);
            this.scaledZoomFactor = this.PhotoZoomFactor * this.baseZoomFactor;
        }

        #endregion

        #region Protected Methods
        /// <summary>
        /// Rotates the PhotoDisplayAngle by 90 degrees clockwise.
        /// </summary>
        protected void RotatePhotoDisplayClockwise()
        {
            if (this.rotatePhotoAnimation != null)
            {
                this.rotatePhotoAnimation.From = this.PhotoDisplayAngle;
                this.rotatePhotoAnimation.To = this.PhotoDisplayAngle + 90;
                this.PhotoDisplayAngle = this.PhotoDisplayAngle + 90;

                if (this.photoAngleTransform != null)
                {
                    this.IsTransitioning = true;
                    this.photoAngleTransform.BeginAnimation(RotateTransform.AngleProperty, this.rotatePhotoAnimation);
                }
            }
        }

        /// <summary>
        /// Rotates the PhotoDisplayAngle by 90 degrees counter-clockwise.  That is, by -90 degrees.
        /// </summary>
        protected void RotatePhotoDisplayCounterClockwise()
        {
            if (this.rotatePhotoAnimation != null)
            {
                this.rotatePhotoAnimation.From = this.PhotoDisplayAngle;
                this.rotatePhotoAnimation.To = this.PhotoDisplayAngle - 90;
                this.PhotoDisplayAngle = this.PhotoDisplayAngle - 90;

                if (this.photoAngleTransform != null)
                {
                    this.IsTransitioning = true;
                    this.photoAngleTransform.BeginAnimation(RotateTransform.AngleProperty, this.rotatePhotoAnimation);
                }
            }
        }

        /// <summary>
        /// Fits the currently displayed photo to the window size.
        /// </summary>
        /// <param name="initialFit">Whether this is the initial fit-to-window pass, or as a response to a user click.</param>
        protected void FitPhotoToWindow(FitToWindowType initialFit)
        {
            if (this.ImageSource != null)
            {
                double oldPhotoZoomFactor = this.PhotoZoomFactor;

                if (this.zoomPhotoAnimation != null)
                {
                    this.CalculateStandardZoom();

                    if (initialFit == FitToWindowType.InitialFit)
                    {
                        // Reset the scaling value
                        Duration zoomPhotoDuration = this.zoomPhotoAnimation.Duration;
                        this.zoomPhotoAnimation.Duration = new TimeSpan(0);
                        this.zoomPhotoAnimation.From = 1;
                        this.zoomPhotoAnimation.To = this.PhotoZoomFactor;
                        if (this.photoZoomTransform != null)
                        {
                            this.IsTransitioning = true;
                            this.photoZoomTransform.BeginAnimation(ScaleTransform.ScaleXProperty, this.zoomPhotoAnimation);
                            this.photoZoomTransform.BeginAnimation(ScaleTransform.ScaleYProperty, this.zoomPhotoAnimation);
                        }

                        this.zoomPhotoAnimation.Duration = zoomPhotoDuration;

                        // And animate the photo in...
                        this.displayPhotoAnimation.From = 0.95;
                        this.displayPhotoAnimation.To = 1.0;
                        if (this.photoTransitionScaleTransform != null)
                        {
                            this.IsTransitioning = true;
                            this.photoTransitionScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, this.displayPhotoAnimation);
                            this.photoTransitionScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, this.displayPhotoAnimation);
                        }
                    }
                    else if (initialFit == FitToWindowType.AnimatedFit)
                    {
                        this.zoomPhotoAnimation.From = oldPhotoZoomFactor;
                        this.zoomPhotoAnimation.To = this.PhotoZoomFactor;
                        if (this.photoZoomTransform != null)
                        {
                            this.BeginZoomPhotoBlurEffectAnimationOnPhotoImage();
                            this.IsTransitioning = true;
                            this.photoZoomTransform.BeginAnimation(ScaleTransform.ScaleXProperty, this.zoomPhotoAnimation);
                            this.zoomPhotoAnimation.Completed += new EventHandler(this.OnZoomPhotoAnimationCompleted);
                            this.photoZoomTransform.BeginAnimation(ScaleTransform.ScaleYProperty, this.zoomPhotoAnimation);
                        }
                    }
                    else
                    {
                        Duration zoomPhotoDuration = this.zoomPhotoAnimation.Duration;
                        this.zoomPhotoAnimation.From = oldPhotoZoomFactor;
                        this.zoomPhotoAnimation.To = this.PhotoZoomFactor;
                        this.zoomPhotoAnimation.Duration = new Duration(new TimeSpan(0));
                        if (this.photoZoomTransform != null)
                        {
                            this.BeginZoomPhotoBlurEffectAnimationOnPhotoImage();
                            this.IsTransitioning = true;
                            this.photoZoomTransform.BeginAnimation(ScaleTransform.ScaleXProperty, this.zoomPhotoAnimation);
                            this.photoZoomTransform.BeginAnimation(ScaleTransform.ScaleYProperty, this.zoomPhotoAnimation);
                        }

                        this.zoomPhotoAnimation.Duration = zoomPhotoDuration;
                    }
                }
            }
        }

        /// <summary>
        /// Zooms the currently displayed photo in. 
        /// </summary>
        protected void ZoomPhotoIn()
        {
            if (this.zoomPhotoAnimation != null)
            {
                if (this.FittingPhotoToWindow)
                {
                    this.FittingPhotoToWindow = false;
                    this.CalculateStandardZoom();
                }

                this.zoomPhotoAnimation.From = this.PhotoZoomFactor;
                this.zoomPhotoAnimation.To = this.PhotoZoomFactor + this.scaledZoomFactor;
                this.PhotoZoomFactor += this.scaledZoomFactor;

                if (this.photoZoomTransform != null)
                {
                    this.IsTransitioning = true;
                    this.photoZoomTransform.BeginAnimation(ScaleTransform.ScaleXProperty, this.zoomPhotoAnimation);
                    this.zoomPhotoAnimation.Completed += new EventHandler(this.OnZoomPhotoAnimationCompleted);
                    this.photoZoomTransform.BeginAnimation(ScaleTransform.ScaleYProperty, this.zoomPhotoAnimation);
                }
            }
        }

        /// <summary>
        /// Zooms the currently displayed photo out.
        /// </summary>
        protected void ZoomPhotoOut()
        {
            if (this.zoomPhotoAnimation != null && this.CanZoomPhotoOut())
            {
                if (this.FittingPhotoToWindow)
                {
                    this.FittingPhotoToWindow = false;
                    this.CalculateStandardZoom();
                }

                this.zoomPhotoAnimation.From = this.PhotoZoomFactor;
                this.zoomPhotoAnimation.To = this.PhotoZoomFactor - this.scaledZoomFactor;
                this.PhotoZoomFactor -= this.scaledZoomFactor;

                if (this.photoZoomTransform != null)
                {
                    this.IsTransitioning = true;
                    this.photoZoomTransform.BeginAnimation(ScaleTransform.ScaleXProperty, this.zoomPhotoAnimation);
                    this.zoomPhotoAnimation.Completed += new EventHandler(this.OnZoomPhotoAnimationCompleted);
                    this.photoZoomTransform.BeginAnimation(ScaleTransform.ScaleYProperty, this.zoomPhotoAnimation);
                }
            }
        }

        /// <summary>
        /// Determines whether the currently displayed photo can be zoomed out any further.
        /// </summary>
        /// <returns>A value indicating whether the currently displayed photo can be zoomed out any further.</returns>
        protected bool CanZoomPhotoOut()
        {
            if ((this.PhotoZoomFactor - this.scaledZoomFactor) > 0.2)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets and beings and element's ZoomPhotoBlurEffectAnimation if the client
        /// has the right hardware capabilities to run the Effect.
        /// </summary>
        protected void BeginZoomPhotoBlurEffectAnimationOnPhotoImage()
        {
            if (ScePhotoViewerApplication.IsShaderEffectSupported)
            {
                if (!(this.photoImage.Effect is ZoomBlurEffect))
                {
                    this.zoomPhotoBlurEffect = new ZoomBlurEffect();
                    this.zoomPhotoBlurEffect.CenterX = 0.5;
                    this.zoomPhotoBlurEffect.CenterY = 0.5;
                    this.photoImage.Effect = this.zoomPhotoBlurEffect;
                }

                // The amount of blur is relative to the zoomed factor of the photo
                double blurAmount = .2 * this.PhotoZoomFactor;

                this.zoomPhotoBlurEffectAnimation.Duration = this.zoomPhotoAnimation.Duration + TimeSpan.FromMilliseconds(100);
                this.zoomPhotoBlurEffectAnimation.From = blurAmount;
                this.zoomPhotoBlurEffectAnimation.To = 0;

                this.photoImage.Effect.BeginAnimation(ZoomBlurEffect.BlurAmountProperty, this.zoomPhotoBlurEffectAnimation, HandoffBehavior.SnapshotAndReplace);
            }
        }

        /// <summary>
        /// Handler to change the control layout when FittingPhotoToWindow changes so that
        /// fit to window does indeed cause the photo to fit to window.
        /// </summary>
        /// <param name="newValue">The new FittingPhotoToWindow value.</param>
        protected void OnFittingPhotoToWindowChanged(bool newValue)
        {
            if (this.photoViewbox != null)
            {
                if (newValue)
                {
                    this.FitPhotoToWindow(FitToWindowType.AnimatedFit);
                    this.switchFittingMode = true;

                    // Viewbox setting is changed at the end of the zoom animation.
                }
                else
                {
                    this.photoViewbox.Stretch = Stretch.None;
                }
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Command to rotate the displayed photo clockwise.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnRotatePhotoClockwiseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            PhotoDisplayControl photoDisplay = sender as PhotoDisplayControl;
            if (photoDisplay != null)
            {
                photoDisplay.RotatePhotoDisplayClockwise();
            }
        }

        /// <summary>
        /// Command to rotate the displayed photo counterclockwise.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnRotatePhotoCounterClockwiseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            PhotoDisplayControl photoDisplay = sender as PhotoDisplayControl;
            if (photoDisplay != null)
            {
                photoDisplay.RotatePhotoDisplayCounterClockwise();
            }
        }

        /// <summary>
        /// Command to zoom the displayed photo in.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnZoomPhotoInCommand(object sender, ExecutedRoutedEventArgs e)
        {
            PhotoDisplayControl photoDisplay = sender as PhotoDisplayControl;
            if (photoDisplay != null)
            {
                photoDisplay.ZoomPhotoIn();
            }
        }

        /// <summary>
        /// Command to zoom the displayed photo out.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnZoomPhotoOutCommand(object sender, ExecutedRoutedEventArgs e)
        {
            PhotoDisplayControl photoDisplay = sender as PhotoDisplayControl;
            if (photoDisplay != null)
            {
                photoDisplay.ZoomPhotoOut();
            }
        }

        /// <summary>
        /// Determines whether the photo can be zoomed out any further.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnZoomPhotoOutCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            PhotoDisplayControl photoDisplay = sender as PhotoDisplayControl;
            if (photoDisplay != null)
            {
                e.CanExecute = photoDisplay.CanZoomPhotoOut();
            }
        }

        /// <summary>
        /// Command to fit the displayed photo to the window size.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnFitPhotoToWindow(object sender, ExecutedRoutedEventArgs e)
        {
            PhotoDisplayControl photoDisplay = sender as PhotoDisplayControl;
            if (photoDisplay != null)
            {
                photoDisplay.FittingPhotoToWindow = !photoDisplay.FittingPhotoToWindow;
            }
        }

        /// <summary>
        /// Handler for FittingPhotoToWindow changes.
        /// </summary>
        /// <param name="element">The element that changed.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnFittingPhotoToWindowChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            ((PhotoDisplayControl)element).OnFittingPhotoToWindowChanged((bool)e.NewValue);
        }

        /// <summary>
        /// Once the zoom animation has completed, reset the scrollviewer/viewbox/description.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private void OnZoomPhotoAnimationCompleted(object sender, EventArgs e)
        {
            this.zoomPhotoAnimation.Completed -= new EventHandler(this.OnZoomPhotoAnimationCompleted);
            this.IsTransitioning = false;

            if (this.switchFittingMode && this.photoViewbox != null)
            {
                this.photoViewbox.Stretch = Stretch.Uniform;
                this.switchFittingMode = false;
            }
        }

        /// <summary>
        /// Once the rotate animation has completed, unhide the scrollbars.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private void OnRotatePhotoAnimationCompleted(object sender, EventArgs e)
        {
            this.IsTransitioning = false;
        }

        /// <summary>
        /// Once the display animation has completed, show the description (if appropriate) and fit to window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event details.</param>
        private void OnDisplayPhotoElementCompleted(object sender, EventArgs e)
        {
            this.IsTransitioning = false;
            this.FittingPhotoToWindow = true;
        }

        /// <summary>
        /// Calculates the zoom factor for a fit-to-window zoom.
        /// </summary>
        private void CalculateStandardZoom()
        {
            double widthZoom;
            double heightZoom;

            if (this.ImageSource != null)
            {
                // Determine if the photo is right-way-up or on it's side so that we calculate the zoom correctly.
                if (this.PhotoDisplayAngle % 180 == 0)
                {
                    widthZoom = (this.ContainerWidth - this.photoBorderWidth) / this.ImageSource.Width;
                    heightZoom = (this.ContainerHeight - this.photoBorderWidth) / this.ImageSource.Height;
                }
                else
                {
                    widthZoom = (this.ContainerWidth - this.photoBorderWidth) / this.ImageSource.Height;
                    heightZoom = (this.ContainerHeight - this.photoBorderWidth) / this.ImageSource.Width;
                }

                this.PhotoZoomFactor = Math.Min(widthZoom, heightZoom);
            }
        }
        #endregion
    }
}
