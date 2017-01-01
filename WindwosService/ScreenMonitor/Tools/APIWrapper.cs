using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ScreenMonitor.Tools
{
    class APIWrapper
    {
        /// <summary>
        /// 鼠标移动
        /// </summary>
        /// <param name="x">水平坐标</param>
        /// <param name="y">垂直坐标</param>
        public static void Mouse_Move(int x, int y)
        {

            int pointXInScreen = (int)(x / (float)Screen.PrimaryScreen.Bounds.Size.Width * 65536);
            int pointYInScreen = (int)(y / (float)Screen.PrimaryScreen.Bounds.Size.Height * 65536);
            WindowsAPI.mouse_event(Mouse_Flags.MOUSEEVENTF_ABSOLUTE | Mouse_Flags.MOUSEEVENTF_MOVE, pointXInScreen, pointYInScreen, 0, 0);
        }

        /// <summary>
        /// 鼠标左击
        /// </summary>
        /// <param name="x">水平坐标</param>
        /// <param name="y">垂直坐标</param>
        public static void Mouse_LeftClick(int x, int y)
        {
            int pointXInScreen = (int)(x / (float)Screen.PrimaryScreen.Bounds.Size.Width * 65536);
            int pointYInScreen = (int)(y / (float)Screen.PrimaryScreen.Bounds.Size.Height * 65536);
            WindowsAPI.mouse_event(Mouse_Flags.MOUSEEVENTF_ABSOLUTE | Mouse_Flags.MOUSEEVENTF_MOVE, pointXInScreen, pointYInScreen, 0, 0);
            WindowsAPI.mouse_event(Mouse_Flags.MOUSEEVENTF_LEFTDOWN | Mouse_Flags.MOUSEEVENTF_LEFTUP, pointXInScreen, pointYInScreen, 0, 0);
        }

        /// <summary>
        /// 鼠标右击
        /// </summary>
        /// <param name="x">水平坐标</param>
        /// <param name="y">垂直坐标</param>
        public static void Mouse_RightClick(int x, int y)
        {
            int pointXInScreen = (int)(x / (float)Screen.PrimaryScreen.Bounds.Size.Width * 65536);
            int pointYInScreen = (int)(y / (float)Screen.PrimaryScreen.Bounds.Size.Height * 65536);
            WindowsAPI.mouse_event(Mouse_Flags.MOUSEEVENTF_ABSOLUTE | Mouse_Flags.MOUSEEVENTF_MOVE, pointXInScreen, pointYInScreen, 0, 0);
            WindowsAPI.mouse_event(Mouse_Flags.MOUSEEVENTF_RIGHTDOWN | Mouse_Flags.MOUSEEVENTF_RIGHTUP, pointXInScreen, pointYInScreen, 0, 0);
        }

        public static Bitmap GetScreenShotGdi()
        {
            Bitmap destImage = new Bitmap(Screen.AllScreens[0].Bounds.Width, Screen.AllScreens[0].Bounds.Height);
            Graphics G_dest = Graphics.FromImage(destImage);
            Graphics G_source = Graphics.FromHwnd(IntPtr.Zero);
            //得到屏幕的DC
            IntPtr srcDc = G_source.GetHdc();
            //得到Bitmap的DC
            IntPtr desDc = G_dest.GetHdc();
            //调用彼API函数，完成屏幕捕捉
            WindowsAPI.BitBlt(desDc, 0, 0, destImage.Width, destImage.Height, srcDc, 0, 0, 0x00CC0020);
            //开释掉屏幕的DC
            G_dest.ReleaseHdc();
            G_source.ReleaseHdc();
            return destImage;
        }

    }
    class Mouse_Flags
    {
        public const int MOUSEEVENTF_MOVE = 0x0001; //     移动鼠标 
        public const int MOUSEEVENTF_LEFTDOWN = 0x0002; //模拟鼠标左键按下 
        public const int MOUSEEVENTF_LEFTUP = 0x0004;// 模拟鼠标左键抬起 
        public const int MOUSEEVENTF_RIGHTDOWN = 0x0008; //模拟鼠标右键按下 
        public const int MOUSEEVENTF_RIGHTUP = 0x0010; //模拟鼠标右键抬起 
        public const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;// 模拟鼠标中键按下 
        public const int MOUSEEVENTF_MIDDLEUP = 0x0040; //模拟鼠标中键抬起 
        public const int MOUSEEVENTF_ABSOLUTE = 0x8000; //标示是否采用绝对坐标 
    }
}

