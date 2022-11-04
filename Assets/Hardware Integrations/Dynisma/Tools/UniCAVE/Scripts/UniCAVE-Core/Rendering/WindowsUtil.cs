using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;

namespace UniCAVE
{
    /// <summary>
    /// Used to resize, rename, and relocate the unity window
    /// </summary>
    public class WindowsUtils
    {
#if UNITY_STANDALONE_WIN

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

        [DllImport("user32.dll")]
        private static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        private static int _setWidth = -1, _setHeight = -1;
        private static int _desiredWidth = -1, _desiredHeight = -1;
        private static int _desiredX = -1, _desiredY = -1;
        private static string _desiredName;
        /// <summary>
        /// Set the unity window's position, title, and size
        /// </summary>
        /// <param name="text">new window title</param>
        /// <param name="posX">new window position x</param>
        /// <param name="posY">new window position y</param>
        /// <param name="width">new window width</param>
        /// <param name="height">new window height</param>
        /// <param name="setWidth">unique parameter to identify this window; do not use the same value for multiple instances of unity</param>
        /// <param name="setHeight">unique parameter to identify this window; do not use the same value for multiple instances of unity</param>
        public static void SetMyWindowInfo(string text, int posX, int posY, int width, int height, int setWidth, int setHeight)
        {
            _setWidth = setWidth;
            _setHeight = setHeight;
            _desiredWidth = width;
            _desiredHeight = height;
            _desiredName = text;
            _desiredX = posX;
            _desiredY = posY;
            _resolutions = loadAllWindowSizes();
            Screen.SetResolution(setWidth, setHeight, false);
        }

        /// <summary>
        /// Map from ID -> Dimensions, of all windows existing
        /// We must search this map for the dimensions assigned earlier, to find the ID associated with those dimensions
        /// This ID is the ID of the unity window
        /// (If other windows have the same dimensions, this will not work. Fortunately this situation is rare)
        /// </summary>
        private static Dictionary<long, Vector2Int> _resolutions = null;

        /// <summary>
        /// Use Windows calls to generate map of ID -> Dimensions
        /// </summary>
        /// <returns></returns>
        private static Dictionary<long, Vector2Int> loadAllWindowSizes()
        {
            Dictionary<long, Vector2Int> res = new Dictionary<long, Vector2Int>();

            EnumWindows(delegate (IntPtr wnd, IntPtr param)
            {
                RECT r = new RECT();
                GetWindowRect(wnd, out r);

                int wndWidth = r.Right - r.Left;
                int wndHeight = r.Bottom - r.Top;

                res.Add(wnd.ToInt64(), new Vector2Int(wndWidth, wndHeight));

                return true;
            }, IntPtr.Zero);

            return res;
        }

        /// <summary>
        /// Returns true if there are no pending operations, or all operations have completed
        /// Otherwise returns false
        /// </summary>
        /// <returns>Whether all operations have completed (or there are none)</returns>
        public static bool CompletedOperation()
        {
            //it seems that Windows doe not immediately set the window properties so
            //we try over and over until it does
#if UNITY_STANDALONE_WIN
            if(_resolutions != null)
            {
                Dictionary<long, Vector2Int> newResolutions = loadAllWindowSizes();
                foreach(var kvp in newResolutions)
                {
                    if(kvp.Value.x == _setWidth && kvp.Value.y == _setHeight)
                    {
                        SetWindowText(new IntPtr(kvp.Key), _desiredName);
                        SetWindowPos(new IntPtr(kvp.Key), 0, _desiredX, _desiredY, _desiredWidth, _desiredHeight, _desiredWidth * _desiredHeight == 0 ? 1 : 0);
                        _resolutions = null;
                        return true;
                    }
                }
            }
            else
            {
                return true;
            }
            return false;
#else
        return true; // if not on windows, no operations to complete
#endif
        }

#endif
    }
}