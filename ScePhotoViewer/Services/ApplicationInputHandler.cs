//-----------------------------------------------------------------------
// <copyright file="ApplicationInputHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Global input handler for the application.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using System.Windows.Input;
    using ScePhoto;
    using ScePhoto.View;
    using ScePhoto.Controls;
    using ScePhoto.Data;
    using System.Windows;
    using System;

    /// <summary>
    /// Global input handler for the application. Global input handler receives the OnKeyDown event from the main page.
    /// If no control or other UI has handled the event on the route, global application key handling can take over.
    /// It can then take make "application-wide" decisions about what action take based on
    /// application state that individual controls may not have, or if users do not want to customize controls to handle key input.
    /// </summary>
    public static class ApplicationInputHandler
    {
        /// <summary>
        /// Enum describing navigation actions that can be taken by the application.
        /// </summary>
        private enum ApplicationNavigationAction
        {
            /// <summary>
            /// Navigate out of a PhotoAlbumControl or other photo album display.
            /// </summary>
            NavigateFromPhotoAlbum,

            /// <summary>
            /// Navigate out of the SearchPhotoAlbum.
            /// </summary>
            NavigateFromSearchAlbum,

            /// <summary>
            /// Navigate out of a photo display.
            /// </summary>
            NavigateFromPhoto,

            /// <summary>
            /// No navigation action taken.
            /// </summary>
            None
        }

        /// <summary>
        /// Application-level navigation handler for key down - checks if key event impacts navigation and takes
        /// necessary action.
        /// </summary>
        /// <param name="e">EventArgs describing the event.</param>
        public static void OnKeyDown(KeyEventArgs e)
        {
            if (!e.Handled)
            {
                if (e.KeyboardDevice.Modifiers == ModifierKeys.None && !(e.OriginalSource is System.Windows.Controls.TextBox))
                {
                    // Shortcut keys without modifiers generally initiate navigation, handled here
                    switch (e.Key)
                    {
                        case Key.Left:
                        case Key.J:
                            NavigateLeft(e);
                            break;
                        case Key.Right:
                        case Key.K:
                            NavigateRight(e);
                            break;
                        case Key.Up:
                        case Key.I:
                        case Key.PageUp:
                            NavigateUp(e);
                            break;
                        case Key.Down:
                        case Key.PageDown:
                            NavigateDown(e);
                            break;
                        case Key.Space:
                            NavigateSpace(e);
                            break;
                        case Key.Home:
                            // On Home, navigate to first photo album in the gallery.
                            NavigateHome(e);
                            break;
                        case Key.End:
                            NavigateEnd(e);
                            break;
                        case Key.Enter:
                            NavigateToParentPhotoAlbumOnPhoto(e);
                            break;
                        case Key.M:
                            // Without Control modifier, M navigates down
                            NavigateDown(e);
                            break;
                        case Key.B:
                            // On B, try to open a Photo in the browser depending on current focus state
                            OpenPhotoInBrowser();
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Application-wide handler for MouseWheel event.
        /// </summary>
        /// <param name="e">EventArgs describing the event.</param>
        public static void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                if (ServiceProvider.ViewManager.CurrentNavigator is PhotoAlbumNavigator)
                {
                    NavigatePhotoAlbumOnMouseWheel(e);
                }
                else if (ServiceProvider.ViewManager.CurrentNavigator is PhotoNavigator)
                {
                    NavigatePhotosOnMouseWheel(e);
                }
            }
        }

        /// <summary>
        /// Gets the top level ancestor of the specified element that has a parent.
        /// </summary>
        /// <param name="element">The element to start searching at.</param>
        /// <param name="limit">The highest parent element to search up to, or null.</param>
        /// <param name="type">The type of the element to search for.</param>
        /// <returns>The top level containing element.</returns>
        public static DependencyObject GetTopLevelContainingElement(DependencyObject element, DependencyObject limit, Type type)
        {
            if (element != null && type != null)
            {
                DependencyObject containingElement = element;
                do
                {
                    DependencyObject parent = LogicalTreeHelper.GetParent(containingElement);
                    if ((type.IsInstanceOfType(containingElement) || containingElement.GetType().IsSubclassOf(type)) && (parent == limit || limit == null))
                    {
                        return containingElement;
                    }

                    containingElement = parent;
                }
                while (containingElement != null && Object.ReferenceEquals(containingElement, limit) == false);
            }

            return null;
        }

        /// <summary>
        /// Navigates to next/previous photo album on mouse wheel input.
        /// </summary>
        /// <param name="e">EventArgs describing mouse wheel event.</param>
        private static void NavigatePhotoAlbumOnMouseWheel(MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                if (DoubleUtilities.LessThan(e.Delta, 0))
                {
                    GoToNextPhotoAlbum();
                }
                else if (DoubleUtilities.GreaterThan(e.Delta, 0))
                {
                    GoToPreviousPhotoAlbum();
                }

                e.Handled = true;
            }
        }

        /// <summary>
        /// Navigates to the next/previous photo on mouse wheel input.
        /// </summary>
        /// <param name="e">EventArgs describing mouse wheel event.</param>
        private static void NavigatePhotosOnMouseWheel(MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                if (DoubleUtilities.LessThan(e.Delta, 0))
                {
                    GoToNextPhoto();
                }
                else if (DoubleUtilities.GreaterThan(e.Delta, 0))
                {
                    GoToPreviousPhoto();
                }

                e.Handled = true;
            }
        }

        /// <summary>
        /// Navigate to end (last section) on key event.
        /// </summary>
        /// <param name="e">EventArgs describing the event.</param>
        private static void NavigateEnd(KeyEventArgs e)
        {
            if (ServiceProvider.ViewManager.NavigationCommands.NavigateToLastPhotoAlbumCommand.CanExecute(null))
            {
                ServiceProvider.ViewManager.NavigationCommands.NavigateToLastPhotoAlbumCommand.Execute(null);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Navigate to home (first section) on key event.
        /// </summary>
        /// <param name="e">EventArgs describing the event.</param>
        private static void NavigateHome(KeyEventArgs e)
        {
            if (ServiceProvider.ViewManager.NavigationCommands.NavigateToFirstPhotoAlbumCommand.CanExecute(null))
            {
                ServiceProvider.ViewManager.NavigationCommands.NavigateToFirstPhotoAlbumCommand.Execute(null);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Navigate to a Photo's parent photo album on key input.
        /// </summary>
        /// <param name="e">EventArgs describing the event.</param>
        private static void NavigateToParentPhotoAlbumOnPhoto(KeyEventArgs e)
        {
            // Check if photo is currently displayed
            PhotoNavigator photoNavigator = ServiceProvider.ViewManager.CurrentNavigator as PhotoNavigator;
            if (photoNavigator != null)
            {
                if (photoNavigator.GetParent() == null)
                {
                    // Photo has no parent, navigation will not succeed. Navigate to home photo album instead
                    if (ServiceProvider.ViewManager.NavigationCommands.NavigateToFirstPhotoAlbumCommand.CanExecute(null))
                    {
                        ServiceProvider.ViewManager.NavigationCommands.NavigateToFirstPhotoAlbumCommand.Execute(null);
                    }
                }
                else if (ServiceProvider.ViewManager.NavigationCommands.NavigateToParentPhotoAlbumCommand.CanExecute(null))
                {
                    ServiceProvider.ViewManager.NavigationCommands.NavigateToParentPhotoAlbumCommand.Execute(null);
                }

                e.Handled = true;
            }
        }

        /// <summary>
        /// Navigates to previous (left) item or directional focus object if possible on key input.
        /// </summary>
        /// <param name="e">EventArgs describing key input.</param>
        private static void NavigateLeft(KeyEventArgs e)
        {
            ApplicationNavigationAction action = GetApplicationNavigationActionForKeyEvent(e);
            if (action != ApplicationNavigationAction.None)
            {
                switch (action)
                {
                    case ApplicationNavigationAction.NavigateFromPhotoAlbum:
                    case ApplicationNavigationAction.NavigateFromPhoto:
                        GoToPreviousPhoto();
                        e.Handled = true;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Navigates to next (right) item or directional focus object if possible on key input.
        /// </summary>
        /// <param name="e">EventArgs describing key input.</param>
        private static void NavigateRight(KeyEventArgs e)
        {
            ApplicationNavigationAction action = GetApplicationNavigationActionForKeyEvent(e);
            if (action != ApplicationNavigationAction.None)
            {
                switch (action)
                {
                    case ApplicationNavigationAction.NavigateFromPhotoAlbum:
                    case ApplicationNavigationAction.NavigateFromPhoto:
                        GoToNextPhoto();
                        e.Handled = true;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Navigates to previous (up) item or directional focus object if possible on key input.
        /// </summary>
        /// <param name="e">EventArgs describing key input.</param>
        private static void NavigateUp(KeyEventArgs e)
        {
            ApplicationNavigationAction action = GetApplicationNavigationActionForKeyEvent(e);
            if (action != ApplicationNavigationAction.None)
            {
                switch (action)
                {
                    case ApplicationNavigationAction.NavigateFromPhotoAlbum:
                        GoToPreviousPhotoAlbum();
                        e.Handled = true;
                        break;
                    case ApplicationNavigationAction.NavigateFromPhoto:
                        
                        // Disable parent navigation on Up key from photo, up/down will navigate pages in the photo's flow doc description if shown
                        // Enter key goes to parent album
                        // GoToParentPhotoAlbum();
                        e.Handled = true;
                        break;
                    case ApplicationNavigationAction.NavigateFromSearchAlbum:
                        GoToFirstPhotoAlbum();
                        e.Handled = true;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Navigates to next (down) item or directional focus object if possible on key input.
        /// </summary>
        /// <param name="e">EventArgs describing key input.</param>
        private static void NavigateDown(KeyEventArgs e)
        {
            ApplicationNavigationAction action = GetApplicationNavigationActionForKeyEvent(e);
            switch (action)
            {
                case ApplicationNavigationAction.NavigateFromPhotoAlbum:
                    GoToNextPhotoAlbum();
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Navigates to the next item in the current set; either the next photo album when viewing photo albums,
        /// or the next photo when viewing photos.
        /// </summary>
        /// <param name="e">Arguments describing the event.</param>
        private static void NavigateSpace(KeyEventArgs e)
        {
            ApplicationNavigationAction action = GetApplicationNavigationActionForKeyEvent(e);
            switch (action)
            {
                case ApplicationNavigationAction.NavigateFromPhotoAlbum:
                    GoToNextPhotoAlbum();
                    e.Handled = true;
                    break;
                case ApplicationNavigationAction.NavigateFromPhoto:
                    GoToNextPhoto();
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Gets the appropriate navigation action for key input.
        /// </summary>
        /// <param name="e">Event arguments describing the key press.</param>
        /// <returns>Navigation action to perform for the key input.</returns>
        private static ApplicationNavigationAction GetApplicationNavigationActionForKeyEvent(KeyEventArgs e)
        {
            ApplicationNavigationAction action = ApplicationNavigationAction.None;
            switch (e.Key)
            {
                case Key.Left:
                case Key.Right:
                case Key.Up:
                case Key.Down:
                case Key.I:
                case Key.J:
                case Key.K:
                case Key.M:
                case Key.Space:
                case Key.PageDown:
                case Key.PageUp:
                    if (ServiceProvider.ViewManager.CurrentNavigator is PhotoAlbumNavigator && (e.OriginalSource is PhotoAlbumControl || e.OriginalSource is MainWindow || e.OriginalSource is PhotoGalleryAlbumSelector || e.OriginalSource is GalleryHomeControl || e.OriginalSource is MainContentContainer))
                    {
                        if (ServiceProvider.ViewManager.CurrentNavigator is SearchNavigator)
                        {
                            if (e.Key == Key.Left || e.Key == Key.Right)
                            {
                                action = ApplicationNavigationAction.NavigateFromPhotoAlbum;
                            }
                            else if (e.Key == Key.Up)
                            {
                                action = ApplicationNavigationAction.NavigateFromSearchAlbum;
                            }
                        }
                        else
                        {
                            action = ApplicationNavigationAction.NavigateFromPhotoAlbum;
                        }
                    }
                    else if (ServiceProvider.ViewManager.CurrentNavigator is PhotoNavigator)
                    {
                        UIElement element = e.OriginalSource as UIElement;
                        PhotoViewerControl photoViewer = GetTopLevelContainingElement(element, null, typeof(PhotoViewerControl)) as PhotoViewerControl;
                        if (e.OriginalSource is MainContentContainer || (element != null && photoViewer != null))
                        {
                            action = ApplicationNavigationAction.NavigateFromPhoto;
                        }
                    }

                    break;
                default:
                    break;
            }

            return action;
        }

        /// <summary>
        /// Logic to navigate to the previous photo album on receiving key input.
        /// </summary>
        private static void GoToPreviousPhotoAlbum()
        {
            if (ServiceProvider.ViewManager.NavigationCommands.PreviousPhotoAlbumCommand.CanExecute(null))
            {
                ServiceProvider.ViewManager.NavigationCommands.PreviousPhotoAlbumCommand.Execute(null);
            }
        }

        /// <summary>
        /// Logic to navigate to the next photo album on receiving key input.
        /// </summary>
        private static void GoToNextPhotoAlbum()
        {
            if (ServiceProvider.ViewManager.NavigationCommands.NextPhotoAlbumCommand.CanExecute(null))
            {
                ServiceProvider.ViewManager.NavigationCommands.NextPhotoAlbumCommand.Execute(null);
            }
        }

        /// <summary>
        /// Logic to navigate to the parent photo album on receiving key input.
        /// </summary>
        /*private static void GoToParentPhotoAlbum()
        {
            if (ServiceProvider.ViewManager.NavigationCommands.NavigateToParentPhotoAlbumCommand.CanExecute(null))
            {
                ServiceProvider.ViewManager.NavigationCommands.NavigateToParentPhotoAlbumCommand.Execute(null);
            }
        }*/

        /// <summary>
        /// Logic to navigate to the first photo album in the gallery on receiving key input.
        /// </summary>
        private static void GoToFirstPhotoAlbum()
        {
            if (ServiceProvider.ViewManager.NavigationCommands.NavigateToFirstPhotoAlbumCommand.CanExecute(null))
            {
                ServiceProvider.ViewManager.NavigationCommands.NavigateToFirstPhotoAlbumCommand.Execute(null);
            }
        }

        /// <summary>
        /// Logic to navigate to the previous photo on receiving key input.
        /// </summary>
        private static void GoToPreviousPhoto()
        {
            if (ServiceProvider.ViewManager.NavigationCommands.PreviousPhotoCommand.CanExecute(null))
            {
                ServiceProvider.ViewManager.NavigationCommands.PreviousPhotoCommand.Execute(null);
            }
        }

        /// <summary>
        /// Logic to navigate to the next photo on receiving key input.
        /// </summary>
        private static void GoToNextPhoto()
        {
            if (ServiceProvider.ViewManager.NavigationCommands.NextPhotoCommand.CanExecute(null))
            {
                ServiceProvider.ViewManager.NavigationCommands.NextPhotoCommand.Execute(null);
            }
        }

        /// <summary>
        /// Open Photo in Browser on B key press
        /// </summary>
        private static void OpenPhotoInBrowser()
        {
            // Check if photo is currently displayed
            Photo photo = ServiceProvider.ViewManager.ActivePhoto;
            if (photo != null && photo.WebLink != null)
            {
                System.Diagnostics.Process.Start(photo.WebLink.ToString());
            }
        }
    }
}
