//-----------------------------------------------------------------------
// <copyright file="SplashScreen.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Native and GDI methods for displaying the application splash screen
//      before the .NET/WPF libraries load.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhotoViewer
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    /// <summary>
    /// Native and GDI methods for displaying the application splash screen before the .NET/WPF libraries load.
    /// </summary>
    internal class SplashScreen
    {
        #region Fields

        /// <summary>
        /// Default window callback.
        /// </summary>
        private static readonly NativeMethods.WindowProc splashWndProc = new NativeMethods.WindowProc(NativeMethods.DefWindowProc); // Keep the reference alive

        /// <summary>
        /// Class name for splash window.
        /// </summary>
        private const string SplashScreenClassName = "WpfSplashScreen";

        /// <summary>
        /// Splash image resource ID.
        /// </summary>
        private const int SplashScreenResourceId = 101;

        /// <summary>
        /// Splash image resource type.
        /// </summary>
        private const string SplashScreenResourceType = "PNG";

        /// <summary>
        /// Splash window handle.
        /// </summary>
        private IntPtr splashScreenHwnd;

        /// <summary>
        /// GDI+ token.
        /// </summary>
        private IntPtr gdiPlusToken;

        /// <summary>
        /// GDI+ startup input.
        /// </summary>
        private NativeMethods.StartupInput gdiPlusStartupInput;

        /// <summary>
        /// Buffer for resource data.
        /// </summary>
        private IntPtr hBuffer;

        #endregion

        #region Public Methods

        /// <summary>
        /// Opens the splash screen and displays it on screen.
        /// </summary>
        public void Open()
        {
            IntPtr hInstance = Marshal.GetHINSTANCE(typeof(SplashScreen).Module);

            NativeMethods.BITMAP bitmapInfo = new NativeMethods.BITMAP();
            IntPtr splashScreenBitmap = this.GetHBitmapFromResource(hInstance, SplashScreenResourceType, SplashScreenResourceId);

            NativeMethods.GetBitmapInformation(splashScreenBitmap, Marshal.SizeOf(typeof(NativeMethods.BITMAP)), ref bitmapInfo);
            int top, left;
            this.CreateWindow(hInstance, bitmapInfo.bmWidth, bitmapInfo.bmHeight, out left, out top);
            this.SelectBitmap(splashScreenBitmap, bitmapInfo.bmWidth, bitmapInfo.bmHeight, left, top);
        }

        /// <summary>
        /// Closes the splash screen and hides it from view.
        /// </summary>
        public void Close()
        {
            NativeMethods.DestroyWindow(this.splashScreenHwnd);
            this.splashScreenHwnd = IntPtr.Zero;
            NativeMethods.GlobalUnlock(this.hBuffer);
            NativeMethods.GlobalFree(this.hBuffer);
            NativeMethods.GdiplusShutdown(this.gdiPlusToken);
        }

        /// <summary>
        /// Sets the splash screen's parent window.
        /// </summary>
        /// <param name="hNewParent">IntPtr to the new parent window.</param>
        public void SetParent(IntPtr hNewParent)
        {
            NativeMethods.SetParent(this.splashScreenHwnd, hNewParent);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Displays a bitmap at the given coordinates.
        /// </summary>
        /// <param name="hBitmap">Bitmap handle.</param>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        /// <param name="left">x coordinate.</param>
        /// <param name="top">y coordinate.</param>
        private void SelectBitmap(IntPtr hBitmap, int width, int height, int left, int top)
        {
            IntPtr screenDc = NativeMethods.GetDC(IntPtr.Zero);
            IntPtr memDc = NativeMethods.CreateCompatibleDC(screenDc);
            IntPtr hOldBitmap = IntPtr.Zero;

            try
            {
                hOldBitmap = NativeMethods.SelectObject(memDc, hBitmap);

                NativeMethods.Size newSize = new NativeMethods.Size(width, height);
                NativeMethods.Point sourceLocation = new NativeMethods.Point(0, 0);
                NativeMethods.Point newLocation = new NativeMethods.Point(left, top);
                NativeMethods.BLENDFUNCTION blend = new NativeMethods.BLENDFUNCTION();
                blend.BlendOp = NativeMethods.AC_SRC_OVER;
                blend.BlendFlags = 0;
                blend.SourceconstantAlpha = 255;
                blend.AlphaFormat = NativeMethods.AC_SRC_ALPHA;

                NativeMethods.UpdateLayeredWindow(this.splashScreenHwnd, screenDc, ref newLocation, ref newSize, memDc, ref sourceLocation, 0, ref blend, NativeMethods.ULW_ALPHA);
            }
            finally
            {
                NativeMethods.ReleaseDC(IntPtr.Zero, screenDc);
                if (hBitmap != IntPtr.Zero)
                {
                    NativeMethods.SelectObject(memDc, hOldBitmap);
                    NativeMethods.DeleteObject(hBitmap);
                }

                NativeMethods.DeleteDC(memDc);
            }
        }

        /// <summary>
        /// Creates and displays the window for the splash screen.
        /// </summary>
        /// <param name="hInstance">Application HINSTANCE.</param>
        /// <param name="width">Window width.</param>
        /// <param name="height">Window height.</param>
        /// <param name="left">Screen x coordinate.</param>
        /// <param name="top">Screen y coordinate.</param>
        private void CreateWindow(IntPtr hInstance, int width, int height, out int left, out int top)
        {
            left = top = 0;

            // Prepare the window class
            NativeMethods.WNDCLASSEX splashScreenWindowClass = new NativeMethods.WNDCLASSEX();
            splashScreenWindowClass.cbSize = Marshal.SizeOf(typeof(NativeMethods.WNDCLASSEX));
            splashScreenWindowClass.style = NativeMethods.CS_HREDRAW | NativeMethods.CS_VREDRAW;
            splashScreenWindowClass.lpfnWndProc = splashWndProc;
            splashScreenWindowClass.cbClsExtra = 0;
            splashScreenWindowClass.cbWndExtra = 0;
            splashScreenWindowClass.hInstance = hInstance;
            splashScreenWindowClass.hCursor = NativeMethods.LoadCursor(IntPtr.Zero, NativeMethods.IDC_ARROW);
            splashScreenWindowClass.lpszClassName = SplashScreenClassName;
            splashScreenWindowClass.lpszMenuName = String.Empty;

            // Register the window class
            if (NativeMethods.RegisterClassEx(ref splashScreenWindowClass) != 0)
            {
                // Calculate the window position
                int screenWidth = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN);
                int screenHeight = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN);
                int x = (screenWidth - width) / 2;
                int y = (screenHeight - height) / 2;

                // Create and display the window
                this.splashScreenHwnd = NativeMethods.CreateWindowEx(
                    NativeMethods.WS_EX_PALETTEWINDOW | NativeMethods.WS_EX_LAYERED, // | NativeMethods.WS_EX_TOPMOST,
                    SplashScreenClassName,
                    String.Empty,
                    NativeMethods.WS_POPUP | NativeMethods.WS_VISIBLE,
                    x, 
                    y, 
                    width, 
                    height,
                    IntPtr.Zero, 
                    IntPtr.Zero, 
                    hInstance, 
                    IntPtr.Zero);

                left = x;
                top = y;
            }
        }

        /// <summary>
        /// Gets a bitmap from resource.
        /// </summary>
        /// <param name="hInstance">Application hInstance.</param>
        /// <param name="resourceType">Resource type.</param>
        /// <param name="resourceId">Resource ID.</param>
        /// <returns>Bitmap handle.</returns>
        private IntPtr GetHBitmapFromResource(IntPtr hInstance, string resourceType, int resourceId)
        {
            // Initialize GDIPLUS
            this.gdiPlusStartupInput.GdiplusVersion = 1;
            this.gdiPlusStartupInput.DebugEventCallback = IntPtr.Zero;
            this.gdiPlusStartupInput.SuppressBackgroundThread = false;
            this.gdiPlusStartupInput.SuppressExternalCodecs = false;
            NativeMethods.StartupOutput output;
            NativeMethods.GdiplusStartup(out this.gdiPlusToken, ref this.gdiPlusStartupInput, out output);

            IntPtr hBitmap = IntPtr.Zero;

            IntPtr hResource = NativeMethods.FindResource(hInstance, (IntPtr)resourceId, resourceType);
            uint size = NativeMethods.SizeofResource(hInstance, hResource);
            IntPtr pResourceData = NativeMethods.LoadResource(hInstance, hResource);
            pResourceData = NativeMethods.LockResource(pResourceData);

            this.hBuffer = NativeMethods.GlobalAlloc(NativeMethods.GMEM_MOVEABLE, (UIntPtr)size);
            if (this.hBuffer != IntPtr.Zero)
            {
                IntPtr pBuffer = NativeMethods.GlobalLock(this.hBuffer);
                NativeMethods.CopyMemory(pBuffer, pResourceData, size);

                IStream pIStream;
                if (NativeMethods.CreateStreamOnHGlobal(this.hBuffer, false, out pIStream) == 0)
                {
                    IntPtr pBmp;
                    NativeMethods.GdipCreateBitmapFromStream(pIStream, out pBmp);
                    NativeMethods.GdipCreateHBITMAPFromBitmap(pBmp, out hBitmap, 0);
                }
            }

            return hBitmap;
        } 

        #endregion
    }
}
