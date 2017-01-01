using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ScreenMonitor.Tools
{
    class WindowsAPI
    {
        [DllImport("user32.dll")]
        internal static extern bool SetCursorPos(int X, int Y);
        [DllImport("user32.dll")]
        internal static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        internal static extern IntPtr CreateDC(
          string lpszDriver, // 驱动名称 
          string lpszDevice, // 设备名称 
          string lpszOutput, // 无用，可以设定位"NULL" 
          IntPtr lpInitData // 任意的打印机数据 
        );
        [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
        internal static extern bool BitBlt(
        IntPtr hdcDest, //目标设备的句柄 
        int nXDest, // 目标对象的左上角的X坐标 
        int nYDest, // 目标对象的左上角的X坐标 
        int nWidth, // 目标对象的矩形的宽度 
        int nHeight, // 目标对象的矩形的长度 
        IntPtr hdcSrc, // 源设备的句柄 
        int nXSrc, // 源对象的左上角的X坐标 
        int nYSrc, // 源对象的左上角的X坐标 
        System.Int32 dwRop // 光栅的操作值 
        );

    }
}
