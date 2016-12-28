using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Compression;

namespace ScreenMonitor
{
    class ScreenImageService
    {
        #region 获取系统截图
        static ScreenShotPackage cache;
        static DateTime imgTime = new DateTime(1, 1, 1);
        static int flushTime = 30;
        static int count = 0;
        public static ScreenShotPackage getScrreenShot()
        {
            if (cache == null || DateTime.Now > imgTime.AddMilliseconds(flushTime))
            {
                count++;
                using (Bitmap screenShot = new Bitmap(Screen.PrimaryScreen.Bounds.Size.Width, Screen.PrimaryScreen.Bounds.Size.Height))
                {
                    using (Graphics graphics = Graphics.FromImage(screenShot))
                    {
                        graphics.CopyFromScreen(new Point(0, 0), new Point(0, 0), Screen.PrimaryScreen.Bounds.Size);
                        imgTime = DateTime.Now;
                        return cache = new ScreenShotPackage(screenShot, 16, 9, 1280);

                    }
                }
            }
            return cache;
        }
        #endregion
    }
}
