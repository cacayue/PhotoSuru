//-----------------------------------------------------------------------
// <copyright file="PhotoSlideShowControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Control used to display a slideshow of photo's with transitions.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
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
    using TransitionEffects;
    using ScePhoto.View;

    /// <summary>
    /// Control used to display a slideshow of photo's with transitions.
    /// </summary>
    [TemplatePart(Name = "PART_PhotoHost", Type = typeof(Decorator))]
    public class PhotoSlideShowControl : Control
    {
        #region Fields

        /// <summary>
        /// Dependency property for <see cref="PhotoSlideShow"/> property.
        /// </summary>
        public static readonly DependencyProperty PhotoSlideShowProperty =
            DependencyProperty.Register(
                "PhotoSlideShow",
                typeof(PhotoSlideShow),
                typeof(PhotoSlideShowControl),
                new UIPropertyMetadata(null, OnPhotoSlideSlideShowChanged));

        /// <summary>
        /// DependencyPropertyKey for <see cref="Paused"/> property.
        /// </summary>
        private static readonly DependencyPropertyKey PausedPropertyKey =
                DependencyProperty.RegisterReadOnly(
                        "Paused",
                        typeof(bool),
                        typeof(PhotoSlideShowControl),
                        new FrameworkPropertyMetadata(false));

        /// <summary>
        /// DependencyProperty for <see cref="Paused"/> property.
        /// </summary>
        public static readonly DependencyProperty PausedProperty =
                PausedPropertyKey.DependencyProperty;

        /// <summary>
        /// Transition effects used by this control.
        /// </summary>
        private static TransitionEffect[][] transitionEffects = new TransitionEffect[][]
        {
            new TransitionEffect[]
            {
                new ShrinkTransitionEffect(),
                new BlindsTransitionEffect(),
                new CloudRevealTransitionEffect(),
                new RandomCircleRevealTransitionEffect(),
                new FadeTransitionEffect(),
            },
            new TransitionEffect[]
            {
                new WaveTransitionEffect(),
                new RadialWiggleTransitionEffect(),
            },
            new TransitionEffect[]
            {  
                new BloodTransitionEffect(),
                new CircleStretchTransitionEffect(),
            },   
            new TransitionEffect[]
            {  
                new DissolveTransitionEffect(),
                new DropFadeTransitionEffect(),   
            },
            new TransitionEffect[]
            {
                new RotateCrumbleTransitionEffect(),
                new WaterTransitionEffect(),
                new CrumbleTransitionEffect(),
            },
            new TransitionEffect[]
            {
                new RadialBlurTransitionEffect(),
                new CircularBlurTransitionEffect(),
            },
            new TransitionEffect[]
            {
                new PixelateTransitionEffect(),
                new PixelateInTransitionEffect(),
                new PixelateOutTransitionEffect(),
            },
            new TransitionEffect[]
            {   
                new SwirlGridTransitionEffect(Math.PI * 4), 
                new SwirlGridTransitionEffect(Math.PI * 16),
                new SmoothSwirlGridTransitionEffect(Math.PI * 4),
                new SmoothSwirlGridTransitionEffect(Math.PI * 16),
                new SmoothSwirlGridTransitionEffect(-Math.PI * 8),
                new SmoothSwirlGridTransitionEffect(-Math.PI * 6),
            },
            new TransitionEffect[]
            {
                new MostBrightTransitionEffect(),
                new LeastBrightTransitionEffect(),
                new SaturateTransitionEffect(),
            },
            new TransitionEffect[]
            {
                new BandedSwirlTransitionEffect(Math.PI / 5.0, 50.0),
                new BandedSwirlTransitionEffect(Math.PI, 10.0),
                new BandedSwirlTransitionEffect(-Math.PI, 10.0),
            },
            new TransitionEffect[]
            {
                new CircleRevealTransitionEffect(0.0),
                new CircleRevealTransitionEffect(0.1),
                new CircleRevealTransitionEffect(0.5),
            },
            new TransitionEffect[]
            {
                new LineRevealTransitionEffect(new Point(-0.2, -0.2), new Vector(1, 0), new Vector(1.4, 0), 0.2),
                new LineRevealTransitionEffect(new Point(1.2, -0.2), new Vector(-1, 0), new Vector(-1.4, 0), 0.2),
                new LineRevealTransitionEffect(new Point(-.2, -0.2), new Vector(0, 1), new Vector(0, 1.4), 0.2),
                new LineRevealTransitionEffect(new Point(-0.2, 1.2), new Vector(0, -1), new Vector(0, -1.4), 0.2),
                new LineRevealTransitionEffect(new Point(-0.2, -0.2), new Vector(1, 1), new Vector(1.4, 1.4), 0.2),
                new LineRevealTransitionEffect(new Point(1.2, 1.2), new Vector(-1, -1), new Vector(-1.4, -1.4), 0.2),
                new LineRevealTransitionEffect(new Point(1.2, -0.2), new Vector(-1, 1), new Vector(-1.4, 1.4), 0.2),
                new LineRevealTransitionEffect(new Point(-0.2, 1.2), new Vector(1, -1), new Vector(1.4, -1.4), 0.2),
            },
            new TransitionEffect[]
            {
                new RippleTransitionEffect(10),
                new RippleTransitionEffect(25),
                new RippleTransitionEffect(50),
                new RippleTransitionEffect(100),
                new RippleTransitionEffect(200),
            },
            new TransitionEffect[]
            {
                new SlideInTransitionEffect(new Vector(1, 0)),
                new SlideInTransitionEffect(new Vector(0, 1)),
                new SlideInTransitionEffect(new Vector(-1, 0)),
                new SlideInTransitionEffect(new Vector(0, -1)),
            },
            new TransitionEffect[]
            {
                new SwirlTransitionEffect(Math.PI * 4),
                new SwirlTransitionEffect(-Math.PI * 4),
                new SwirlTransitionEffect(Math.PI * 4),
                new SwirlTransitionEffect(-Math.PI * 4),
            },
        };

        /// <summary>
        /// Control hosting the current slide show image.
        /// </summary>
        private SimplePhotoViewerControl currentChild;

        /// <summary>
        /// Control that temporarily hosts the old slide show image upon transition to the next image.
        /// </summary>
        private SimplePhotoViewerControl oldChild;

        /// <summary>
        /// Decorator that hosts photo controls.
        /// </summary>
        private Decorator photoHost;

        /// <summary>
        /// Timer to control interval between transitions.
        /// </summary>
        private DispatcherTimer timer;

        /// <summary>
        /// PRNG used to select the next transition to be applied.
        /// </summary>
        private Random rand = new Random();

        #endregion

        #region Constructor

        /// <summary>
        /// PhotoSlideShowControl constructor
        /// </summary>
        public PhotoSlideShowControl()
        {
            this.currentChild = new SimplePhotoViewerControl();
            this.oldChild = new SimplePhotoViewerControl();

            this.timer = new DispatcherTimer(TimeSpan.FromSeconds(8), DispatcherPriority.Input, this.OnTimerTick, Dispatcher);
            this.timer.Stop();

            this.Loaded += this.OnLoaded;
            this.Unloaded += this.OnUnloaded;

            this.InputBindings.Add(new InputBinding(MediaCommands.Stop, new KeyGesture(Key.Escape)));
            this.InputBindings.Add(new InputBinding(MediaCommands.NextTrack, new KeyGesture(Key.Right)));
            this.InputBindings.Add(new InputBinding(MediaCommands.PreviousTrack, new KeyGesture(Key.Left)));
            this.CommandBindings.Add(new CommandBinding(System.Windows.Input.MediaCommands.TogglePlayPause, new ExecutedRoutedEventHandler(OnPlayPauseCommandExecuted), new CanExecuteRoutedEventHandler(OnPlayPauseCommandCanExecute)));
            this.CommandBindings.Add(new CommandBinding(System.Windows.Input.MediaCommands.Pause, new ExecutedRoutedEventHandler(OnPauseCommandExecuted), new CanExecuteRoutedEventHandler(OnPauseCommandCanExecute)));
            this.CommandBindings.Add(new CommandBinding(System.Windows.Input.MediaCommands.Play, new ExecutedRoutedEventHandler(OnResumeCommandExecuted), new CanExecuteRoutedEventHandler(OnResumeCommandCanExecute)));
            this.CommandBindings.Add(new CommandBinding(System.Windows.Input.MediaCommands.NextTrack, new ExecutedRoutedEventHandler(OnNextSlideCommandExecuted), new CanExecuteRoutedEventHandler(OnNextSlideCommandCanExecute)));
            this.CommandBindings.Add(new CommandBinding(System.Windows.Input.MediaCommands.PreviousTrack, new ExecutedRoutedEventHandler(OnPreviousSlideCommandExecuted), new CanExecuteRoutedEventHandler(OnPreviousSlideCommandCanExecute)));
            this.CommandBindings.Add(new CommandBinding(System.Windows.Input.MediaCommands.Stop, new ExecutedRoutedEventHandler(OnStopCommandExecuted), new CanExecuteRoutedEventHandler(OnStopCommandCanExecute)));
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets PhotoSlideShow object displayed in this control.
        /// </summary>
        public PhotoSlideShow PhotoSlideShow
        {
            get { return (PhotoSlideShow)GetValue(PhotoSlideShowProperty); }
            set { SetValue(PhotoSlideShowProperty, value); }
        }

        /// <summary>
        /// Gets a value indicating whether slide show is in paused mode or not.
        /// </summary>
        public bool Paused
        {
            get { return (bool)GetValue(PausedProperty); }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// OnApplyTemplate override
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.photoHost = this.Template.FindName("PART_PhotoHost", this) as Decorator;
            this.photoHost.Child = this.currentChild;

            if (this.photoHost != null && this.PhotoSlideShow != null)
            {
                this.StartTimer();
            }
        }

        #endregion

        #region Protected Methods
        /// <summary>
        /// Loaded override, attaches listener for DataManager's GetTextDocumentCompleted event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event args describing the event.</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Focus();
        }

        /// <summary>
        /// Loaded override, detaches listener for DataManager's GetTextDocumentCompleted event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event args describing the event.</param>
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.currentChild.Effect = null;
            this.timer.Stop();
            this.SetValue(PausedPropertyKey, false);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Dependency property changed handler for SlideShowProperty.
        /// </summary>
        /// <param name="d">Dependency object for which DP change has occurred.</param>
        /// <param name="e">EventArgs describing property change.</param>
        private static void OnPhotoSlideSlideShowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PhotoSlideShowControl c = d as PhotoSlideShowControl;
            PhotoSlideShow pss = e.NewValue as PhotoSlideShow;
            if (pss == null)
            {
                c.currentChild.Photo = null;
                c.oldChild.Photo = null;
                c.timer.Stop();
                c.SetValue(PausedPropertyKey, false);
            }
            else
            {
                c.currentChild.Photo = pss.CurrentPhoto.Content as Photo;
                c.oldChild.Photo = pss.NextPhoto.Content as Photo;

                if (c.photoHost != null)
                {
                    c.StartTimer();
                }
            }
        }

        /// <summary>
        /// Can execute handler for TogglePlayPause command
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnPlayPauseCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                PhotoSlideShowControl slideShow = sender as PhotoSlideShowControl;
                if (slideShow.Paused)
                {
                    OnResumeCommandCanExecute(sender, e);
                }
                else
                {
                    OnPauseCommandCanExecute(sender, e);
                }

                e.Handled = true;
            }
        }

        /// <summary>
        /// Executed event handler for TogglePlayPause command
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnPlayPauseCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                PhotoSlideShowControl slideShow = sender as PhotoSlideShowControl;
                if (slideShow.Paused)
                {
                    OnResumeCommandExecuted(sender, e);
                }
                else
                {
                    OnPauseCommandExecuted(sender, e);
                }

                e.Handled = true;
            }
        }

        /// <summary>
        /// Can execute handler for Pause command
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnPauseCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                PhotoSlideShowControl slideShow = sender as PhotoSlideShowControl;
                if (slideShow != null)
                {
                    e.CanExecute = !slideShow.Paused;
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Executed event handler for pause command
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnPauseCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                PhotoSlideShowControl slideShow = sender as PhotoSlideShowControl;
                if (slideShow != null)
                {
                    slideShow.StopTimer();
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Can execute handler for resume command
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnResumeCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                PhotoSlideShowControl slideShow = sender as PhotoSlideShowControl;
                if (slideShow != null)
                {
                    e.CanExecute = slideShow.Paused;
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Executed event handler for resume command
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnResumeCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                PhotoSlideShowControl slideShow = sender as PhotoSlideShowControl;
                if (slideShow != null)
                {
                    slideShow.StartTimer();
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Can execute handler for next slide command
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnNextSlideCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                PhotoSlideShowControl slideShow = sender as PhotoSlideShowControl;
                if (slideShow != null)
                {
                    // Since slide show wraps around, this can always execute
                    e.CanExecute = true;
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Executed event handler for next slide command
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnNextSlideCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                PhotoSlideShowControl slideShow = sender as PhotoSlideShowControl;
                if (slideShow != null)
                {
                    // Stop the timer, change the photo, move to the next photo and restart timer
                    slideShow.MoveNext();
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Can execute handler for previous slide command
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnPreviousSlideCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                PhotoSlideShowControl slideShow = sender as PhotoSlideShowControl;
                if (slideShow != null)
                {
                    // Since slide show wraps around, this can always execute
                    e.CanExecute = true;
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Executed event handler for previous slide command
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnPreviousSlideCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                PhotoSlideShowControl slideShow = sender as PhotoSlideShowControl;
                if (slideShow != null)
                {
                    // Stop the timer, change the photo, move to the next photo and restart timer
                    slideShow.MovePrevious();
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Can execute handler for stop command
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnStopCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                PhotoSlideShowControl slideShow = sender as PhotoSlideShowControl;
                if (slideShow != null)
                {
                    // Slide show can always stop and navigate to current photo
                    e.CanExecute = true;
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Executed event handler for stop command
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private static void OnStopCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                PhotoSlideShowControl slideShow = sender as PhotoSlideShowControl;
                if (slideShow != null)
                {
                    // Stop the timer, change the photo, move to the next photo and restart timer
                    slideShow.NavigateToPhoto();
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Swaps control displaying current photo with the control for the next photo, enabling transition.
        /// </summary>
        private void SwapChildren()
        {
            SimplePhotoViewerControl temp = this.currentChild;
            this.currentChild = this.oldChild;
            this.oldChild = temp;
            this.currentChild.Width = double.NaN;
            this.currentChild.Height = double.NaN;
            if (this.photoHost != null)
            {
                this.photoHost.Child = this.currentChild;
            }

            this.oldChild.Effect = null;
        }

        /// <summary>
        /// Starts timer and resets Paused property
        /// </summary>
        private void StartTimer()
        {
            this.timer.Start();
            this.SetValue(PausedPropertyKey, false);
        }

        /// <summary>
        /// Stops timer and sets Paused property
        /// </summary>
        private void StopTimer()
        {
            this.timer.Stop();
            this.SetValue(PausedPropertyKey, true);
        }

        /// <summary>
        /// Applies a random transition effect between current and next slide show images
        /// </summary>
        private void ApplyTransitionEffect()
        {
            TransitionEffect[] effectGroup = transitionEffects[this.rand.Next(transitionEffects.Length)];
            TransitionEffect effect = effectGroup[this.rand.Next(effectGroup.Length)];
            RandomizedTransitionEffect randEffect = effect as RandomizedTransitionEffect;
            if (randEffect != null)
            {
                randEffect.RandomSeed = this.rand.NextDouble();
            }

            DoubleAnimation da = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromSeconds(2.0)), FillBehavior.HoldEnd);
            da.AccelerationRatio = 0.5;
            da.DecelerationRatio = 0.5;
            da.Completed += new EventHandler(this.TransitionCompleted);
            effect.BeginAnimation(TransitionEffect.ProgressProperty, da);

            VisualBrush vb = new VisualBrush(this.oldChild);
            vb.Viewbox = new Rect(0, 0, this.oldChild.ActualWidth, this.oldChild.ActualHeight);
            vb.ViewboxUnits = BrushMappingMode.Absolute;
            this.oldChild.Width = this.oldChild.ActualWidth;
            this.oldChild.Height = this.oldChild.ActualHeight;
            this.oldChild.Measure(new Size(this.oldChild.ActualWidth, this.oldChild.ActualHeight));
            this.oldChild.Arrange(new Rect(0, 0, this.oldChild.ActualWidth, this.oldChild.ActualHeight));
            
            effect.OldImage = vb;
            this.currentChild.Effect = effect;
        }

        /// <summary>
        /// Advances to next photo. This action stops the timer and puts the slide show in paused mode, slide changes now only take place
        /// through user-initiated action.
        /// </summary>
        private void MoveNext()
        {
            if (!this.Paused)
            {
                this.StopTimer();
            }

            if (this.PhotoSlideShow != null)
            {
                this.PhotoSlideShow.MoveNext();
            }

            this.ChangePhoto(false);    
        }

        /// <summary>
        /// Goes back to previous photo. This action stops the timer and puts the slide show in paused mode, slide changes now only take place
        /// through user-initiated action.
        /// </summary>
        private void MovePrevious()
        {
            if (!this.Paused)
            {
                this.StopTimer();
            }

            if (this.PhotoSlideShow != null)
            {
                this.PhotoSlideShow.MovePrevious();
            }

            this.ChangePhoto(false);
        }

        /// <summary>
        /// Stops slide show and navigates to the currently displayed photo.
        /// </summary>
        private void NavigateToPhoto()
        {
            this.timer.Stop();
            this.SetValue(PausedPropertyKey, false);
            PhotoNavigator photo = this.PhotoSlideShow.CurrentPhoto;
            if (ServiceProvider.ViewManager.NavigationCommands.NavigateToPhotoCommand.CanExecute(photo))
            {
                ServiceProvider.ViewManager.NavigationCommands.NavigateToPhotoCommand.Execute(photo);
            }
        }

        /// <summary>
        /// Handler for timer tick - initiates transition to next photo.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event args describing the event.</param>
        private void OnTimerTick(object sender, EventArgs e)
        {
            this.ChangePhoto(true);
            if (this.PhotoSlideShow != null)
            {
                this.PhotoSlideShow.MoveNext();
            }
        }

        /// <summary>
        /// If applyTransitionEffect is true, initiates transition animation to next photo. If false, assumes that next photo has been
        /// selected by manually advancing the slide show, and just displays the current photo.
        /// </summary>
        /// <param name="applyTransitionEffect">If true, transition animation and effects are initiated.</param>
        private void ChangePhoto(bool applyTransitionEffect)
        {
            if (this.PhotoSlideShow != null && !this.oldChild.ImageDownloadInProgress)
            {
                if (applyTransitionEffect)
                {
                    this.SwapChildren();
                    this.ApplyTransitionEffect();
                }
                else
                {    
                    // Apply the current slide show content. 
                    // Load the old child with the next photo so it will advance to the next photo if the user resumes play.
                    this.currentChild.Photo = PhotoSlideShow.CurrentPhoto.Content as Photo;
                    this.oldChild.Photo = PhotoSlideShow.NextPhoto.Content as Photo;
                }
            }
        }

        /// <summary>
        /// Handler for slide transition completed.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event args describing the event.</param>
        private void TransitionCompleted(object sender, EventArgs e)
        {
            this.currentChild.Effect = null;
            if (this.PhotoSlideShow != null)
            {
                this.oldChild.Photo = PhotoSlideShow.NextPhoto.Content as Photo;
            }
        }

        #endregion
    }
}
