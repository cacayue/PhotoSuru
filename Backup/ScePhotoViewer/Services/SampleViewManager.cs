//-----------------------------------------------------------------------
// <copyright file="SampleViewManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Sample application ViewManager.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using ScePhoto.View;

    /// <summary>
    /// The sample application's view manager; sits between the data model and the UI and provides properties and 
    /// other components for the UI to bind to without affecting the underlying data model, for example, it
    /// provides such properties as "Active" photos and phot albums.
    /// </summary>
    public class SampleViewManager : ViewManager
    {
        /// <summary>
        /// The sample command set.
        /// </summary>
        private SampleCommands sampleCommands;

        /// <summary>
        /// SampleViewManager Constructor.
        /// </summary>
        public SampleViewManager()
            : base()
        {
            this.InitializeSampleCommands();

            PropertyChanged += this.OnCurrentVisualChanged;
        }

        /// <summary>
        /// Gets the command set for the sample ViewManager.
        /// </summary>
        public SampleCommands SampleCommands
        {
            get { return this.sampleCommands; }
        }

        /// <summary>
        /// Initializes the sample command set.
        /// </summary>
        private void InitializeSampleCommands()
        {
            this.sampleCommands = new SampleCommands(this);
        }

        /// <summary>
        /// Event handler for property changed event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event Args describing property changed.</param>
        private void OnCurrentVisualChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "CurrentVisual")
            {
                return;
            }

            if (CurrentNavigator != null)
            {
                this.FocusCurrentVisual();
            }
        }

        /// <summary>
        /// On current visual changed, change focus as desired.
        /// </summary>
        private void FocusCurrentVisual()
        {
            System.Windows.UIElement element = CurrentVisual as System.Windows.UIElement;
            if (element == null)
            {                
                return;
            }

            bool focusElement;
            if (CurrentNavigator is SearchNavigator && CurrentVisual is SearchViewControl)
            {
                // Search view control implements custom focus, don't override it 
                focusElement = false;
            }
            else
            {
                // For anything else, focus current visual
                focusElement = true;
            }

            if (focusElement)
            {
                if (!element.Focus())
                {
                    element.MoveFocus(new System.Windows.Input.TraversalRequest(System.Windows.Input.FocusNavigationDirection.First));
                }
            }
        }
    }
}
