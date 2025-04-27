    //-----------------------------------------------------------------------
// <copyright file="FilmStripPanel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     The panel used to display items in the FilmStripControl;
//     it animates the items it displays into the center position.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    /// <summary>
    /// The panel used to display items in the FilmStripControl.
    /// </summary>
    /// <remarks>
    /// This derives from Panel and not VirtualizingPanel because MakeVisible() is not called on items that are virtualized, 
    /// which prevents us from centering the panel on those items.
    /// </remarks>
    public class FilmStripPanel : Panel, IScrollInfo
    {
        #region Fields
        /// <summary>
        /// DependencyProperty backing store for ItemWidth.
        /// </summary>
        public static readonly DependencyProperty ItemWidthProperty =
            DependencyProperty.Register("ItemWidth", typeof(double), typeof(FilmStripPanel), new UIPropertyMetadata(0.0));

        /// <summary>
        /// DependencyProperty backing store for ItemHeight.
        /// </summary>
        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register("ItemHeight", typeof(double), typeof(FilmStripPanel), new UIPropertyMetadata(0.0));

        /// <summary>
        /// The duration of a "standard" slide; that is, the amount of time the animation should take, regardless of the number of items moved.
        /// </summary>
        /// <remarks>To synchronized the background and the items, this should match the values set in FilmStripControl.</remarks>
        private static Duration standardSlideFilmStripDuration = new Duration(new TimeSpan(1700000));

        /// <summary>
        /// The time that should be added to the standard duration *per each item moved*; this prevents us warping from one side of the film 
        /// to the other if we move a large number of items.
        /// </summary>
        /// <remarks>To synchronized the background and the items, this should match the values set in FilmStripControl.</remarks>
        private static int perItemSlideFilmStripTime = 300000;

        /// <summary>
        /// The ScrollViewer displaying this panel.
        /// </summary>
        private ScrollViewer owner;

        /// <summary>
        /// A value indicating whether the content of this panel can scroll horizontally.
        /// </summary>
        private bool canHorizontallyScroll;

        /// <summary>
        /// A value indicating whether the content of this panel can scroll vertically.
        /// </summary>
        private bool canVerticallyScroll;

        /// <summary>
        /// The size of the entire RowScrollingPanel.
        /// </summary>
        private Size extent = new Size(0, 0);

        /// <summary>
        /// The size of the region currently in view.
        /// </summary>
        private Size viewport = new Size(0, 0);

        /// <summary>
        /// The viewport's absolute offset from the origin of its parent.
        /// </summary>
        private Point offset;

        /// <summary>
        /// The transform used to animate the panel back and forth.
        /// </summary>
        private TranslateTransform transform = new TranslateTransform();

        /// <summary>
        /// The animation used to animate position changes.
        /// </summary>
        private DoubleAnimation transformAnimation = new DoubleAnimation();
        #endregion

        #region Constructor
        /// <summary>
        /// FilmStripPanel constructor; sets up the panel transform and animation parameters.
        /// </summary>
        public FilmStripPanel()
        {
            this.RenderTransform = this.transform;
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value that specifies the width of all items that are contained within a RowScrollingPanel. This is a dependency property.
        /// </summary>
        public double ItemWidth
        {
            get { return (double)GetValue(ItemWidthProperty); }
            set { SetValue(ItemWidthProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value that specifies the height of all items that are contained within a RowScrollingPanel. This is a dependency property.
        /// </summary>
        public double ItemHeight
        {
            get { return (double)GetValue(ItemHeightProperty); }
            set { SetValue(ItemHeightProperty, value); }
        }

        /// <summary>
        /// Gets or sets the ScrollViewer displaying this panel.
        /// </summary>
        public ScrollViewer ScrollOwner
        {
            get { return this.owner; }
            set { this.owner = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the content of this panel can scroll horizontally.
        /// </summary>
        public bool CanHorizontallyScroll
        {
            get { return this.canHorizontallyScroll; }
            set { this.canHorizontallyScroll = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the content of this panel can scroll vertically.
        /// </summary>
        public bool CanVerticallyScroll
        {
            get { return this.canVerticallyScroll; }
            set { this.canVerticallyScroll = value; }
        }

        /// <summary>
        /// Gets the height of the entire RowScrollingPanel.
        /// </summary>
        public double ExtentHeight
        {
            get { return this.extent.Height; }
        }

        /// <summary>
        /// Gets the width of the entire RowScrollingPanel.
        /// </summary>
        public double ExtentWidth
        {
            get { return this.extent.Width; }
        }

        /// <summary>
        /// Gets the height of the region currently displayed.
        /// </summary>
        public double ViewportHeight
        {
            get { return this.viewport.Height; }
        }

        /// <summary>
        /// Gets the width of the region currently displayed.
        /// </summary>
        public double ViewportWidth
        {
            get { return this.viewport.Width; }
        }

        /// <summary>
        /// Gets the viewport's horizontal offset from the left of the panel.
        /// </summary>
        public double HorizontalOffset
        {
            get { return this.offset.X; }
        }

        /// <summary>
        /// Gets the viewport's vertical offset from the top of the panel.
        /// </summary>
        public double VerticalOffset
        {
            get { return this.offset.Y; }
        } 
        #endregion

        #region Public Methods
        /// <summary>
        /// Make a specific item of the panel visible.  In this case, the FilmStripControl calls BringIntoView()
        /// on the selected item, which then ends up here.  
        /// </summary>
        /// <param name="visual">The visual that needs to be made visible.</param>
        /// <param name="rectangle">The amount of the visual that should be made visible (not used -- the whole item is centered).</param>
        /// <returns>The area made visible (not used -- the whole item is made visible, so it is the same as the rectangle parameter).</returns>
        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            for (int i = 0; i < this.InternalChildren.Count; i++)
            {
                if ((Visual)this.InternalChildren[i] == visual)
                {
                    this.SetHorizontalOffset(i * this.ItemWidth);
                    break;
                }
            }

            return rectangle;
        }

        /// <summary>
        /// Animates the panel from its current position to the provided horizontal offset.
        /// We then set the horizontal offset so that the desiredOffsetX is in the center of the panel's
        /// ViewPortWidth.
        /// </summary>
        /// <param name="desiredOffsetX">The desired horizontal offset of the current item.</param>
        public void SetHorizontalOffset(double desiredOffsetX)
        {
            double centerRelativeOffset;
            int numberOfItemsDelta;

            // animate horizontally so that current item is positioned at the center of the viewport
            centerRelativeOffset = (this.ViewportWidth - this.ItemWidth) / 2;
            this.transformAnimation.From = centerRelativeOffset - this.offset.X;
            this.transformAnimation.To = centerRelativeOffset - desiredOffsetX;

            // this duration must match FilmStripControl.BringCurrentItemIntoView's Effect animation duration
            numberOfItemsDelta = (int)Math.Abs((int)(this.transformAnimation.From - this.transformAnimation.To) / (int)this.ItemWidth);
            this.transformAnimation.Duration = standardSlideFilmStripDuration + new Duration(new TimeSpan(numberOfItemsDelta * perItemSlideFilmStripTime));

            this.transformAnimation.AccelerationRatio = 0.4;
            this.transformAnimation.DecelerationRatio = 0.2;
            this.transform.BeginAnimation(TranslateTransform.XProperty, this.transformAnimation, HandoffBehavior.SnapshotAndReplace);

            // save the current horizontal position
            this.offset.X = desiredOffsetX;

            if (this.owner != null)
            {
                this.owner.InvalidateScrollInfo();
            }

            // Invalidate the measure so that new items come into view if required
            this.InvalidateMeasure();
        } 
        
        #region Unused IScrollInfo Members

        /// <summary>
        /// Shifts the panel one line down.  Not implemented -- this panel only shifts horizontally, and only through calls to MakeVisible().
        /// </summary>
        public void LineDown()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Shifts the panel one line left.  Not implemented -- this panel only shifts horizontally, and only through calls to MakeVisible().
        /// </summary>
        public void LineLeft()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Shifts the panel one line right.  Not implemented -- this panel only shifts horizontally, and only through calls to MakeVisible().
        /// </summary>
        public void LineRight()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Shifts the panel one line up.  Not implemented -- this panel only shifts horizontally, and only through calls to MakeVisible().
        /// </summary>
        public void LineUp()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Shifts the panel on mouse wheel input.  Not implemented -- this panel only shifts horizontally, and only through calls to MakeVisible().
        /// </summary>
        public void MouseWheelDown()
        {
           // Ignore mouse wheel commands, don't throw an error.
        }

        /// <summary>
        /// Shifts the panel on mouse wheel input.  Not implemented -- this panel only shifts horizontally, and only through calls to MakeVisible().
        /// </summary>
        public void MouseWheelLeft()
        {
            // Ignore mouse wheel commands, don't throw an error.
        }

        /// <summary>
        /// Shifts the panel on mouse wheel input.  Not implemented -- this panel only shifts horizontally, and only through calls to MakeVisible().
        /// </summary>
        public void MouseWheelRight()
        {
            // Ignore mouse wheel commands, don't throw an error.
        }

        /// <summary>
        /// Shifts the panel on mouse wheel input.  Not implemented -- this panel only shifts horizontally, and only through calls to MakeVisible().
        /// </summary>
        public void MouseWheelUp()
        {
            // Ignore mouse wheel commands, don't throw an error.
        }

        /// <summary>
        /// Shifts the panel one page down.  Not implemented -- this panel only shifts horizontally, and only through calls to MakeVisible().
        /// </summary>
        public void PageDown()
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Shifts the panel one page down.  Not implemented -- this panel only shifts horizontally, and only through calls to MakeVisible().
        /// </summary>
        public void PageLeft()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Shifts the panel one page right.  Not implemented -- this panel only shifts horizontally, and only through calls to MakeVisible().
        /// </summary>
        public void PageRight()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Shifts the panel one page up.  Not implemented -- this panel only shifts horizontally, and only through calls to MakeVisible().
        /// </summary>
        public void PageUp()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the panel's vertical offset.  Not implemented -- this panel only shifts horizontally, and only through calls to MakeVisible().
        /// </summary>
        /// <param name="offset">The desired vertical offset.</param>
        public void SetVerticalOffset(double offset)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region Protected Methods
        /// <summary>
        /// Overrides measure so that the scrolling position is updated.
        /// </summary>
        /// <param name="availableSize">The amount of space the panel has for layout.</param>
        /// <returns>The amount of space the panel wants for layout.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            this.UpdateScrollInfo(availableSize);

            foreach (UIElement child in this.InternalChildren)
            {
                child.Measure(new Size(this.ItemWidth, this.ItemHeight));
            }

            return availableSize;
        }

        /// <summary>
        /// Overrides this control's Arrange() method to display items in a wrapped grid.
        /// </summary>
        /// <param name="finalSize">The actual amount of space the control has for layout.</param>
        /// <returns>The size the control actually used for layout.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            this.UpdateScrollInfo(finalSize);

            for (int i = 0; i < this.Children.Count; i++)
            {
                this.ArrangeChild(i, this.Children[i]);
            }

            return finalSize;
        } 
        #endregion

        #region Private Methods
        /// <summary>
        /// Updates the scrolling position for the panel.
        /// </summary>
        /// <param name="availableSize">The amount of space the panel has for layout.</param>
        private void UpdateScrollInfo(Size availableSize)
        {
            if (double.IsPositiveInfinity(availableSize.Height) || double.IsPositiveInfinity(availableSize.Width))
            {
                throw new ArgumentException("Cannot create FilmStripPanel; ScrollViewer must set CanChildScroll to True and/or restrict the size of the film strip.");
            }

            this.UpdateExtent(availableSize, this.InternalChildren.Count);

            if (availableSize != this.viewport)
            {
                this.viewport = availableSize;
                if (this.owner != null)
                {
                    this.owner.InvalidateScrollInfo();
                }
            }
        }

        /// <summary>
        /// Updates the total size ('extent') of the panel, without scrolling.
        /// </summary>
        /// <param name="availableSize">The amount of space the panel has for layout.</param>
        /// <param name="itemsAvailable">The number of items it needs to display.</param>
        private void UpdateExtent(Size availableSize, int itemsAvailable)
        {
            Size measuredExtent = new Size(itemsAvailable * this.ItemWidth, availableSize.Width);

            if (measuredExtent != this.extent)
            {
                this.extent = measuredExtent;
                if (this.owner != null)
                {
                    this.owner.InvalidateScrollInfo();
                }
            }
        }

        /// <summary>
        /// Positions a specific control in the layout area.
        /// </summary>
        /// <param name="index">The child's index position.</param>
        /// <param name="child">The child UIElement.</param>
        private void ArrangeChild(int index, UIElement child)
        {
            child.Arrange(new Rect(index * this.ItemWidth, 0.0, this.ItemWidth, this.ItemHeight));
        } 
        #endregion
    }
}
