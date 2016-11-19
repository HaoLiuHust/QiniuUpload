/****************************** Module Header ******************************\ 
* Module Name:  Win32.cs
* Project:      CSWPFClipboardViewer
* Copyright (c) Microsoft Corporation.
* 
* The CSWPFClipboardViewer project provides the sample on how to monitor
* Windows clipboard changes in a WPF application.

 This source is subject to the Microsoft Public License.
 See http://www.microsoft.com/en-us/openness/resources/licenses.aspx#MPL
 All other rights reserved.

 THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
 EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
 WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
\***************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace QiniuUpload
{
    /// <summary>
    /// This static class holds the Win32 function declarations and constants needed by
    /// this sample application.
    /// </summary>
    internal static class Win32
    {
        internal const int WM_CLIPBOARDUPDATE = 0x031D;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr AddClipboardFormatListener(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr RemoveClipboardFormatListener(IntPtr hWnd);
    }
}
