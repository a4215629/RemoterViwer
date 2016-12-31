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
    public class ScreenShot
    {
        #region 获取系统截图
        static Bitmap cache;
        static DateTime imgTime = new DateTime(1, 1, 1);
        const int flushTime = 30;
        static int count = 0;
        static object lockObj = new object();
        public Bitmap bitmap { get;private set; }
        private int hashCode = 0;
        public static ScreenShot CurenntScreenShort
        {
            get
            {
                lock (lockObj)
                {
                    
                    if (cache == null || DateTime.Now > imgTime.AddMilliseconds(flushTime))
                    {
                        if (count++ % 30 == 0)
                            Console.WriteLine(DateTime.Now+" Screenshot: "+ count);
                        try
                        {
                            CreateScreenShort();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.StackTrace);
                        }

                    }
                    return new ScreenShot((Bitmap)cache.Clone(),cache.GetHashCode());
                }
            }
        }

        private static Bitmap CreateScreenShort()
        {
            Bitmap screenShot = new Bitmap(Screen.PrimaryScreen.Bounds.Size.Width, Screen.PrimaryScreen.Bounds.Size.Height);
            using (Graphics graphics = Graphics.FromImage(screenShot))
            {
                graphics.CopyFromScreen(new Point(0, 0), new Point(0, 0), Screen.PrimaryScreen.Bounds.Size);
                imgTime = DateTime.Now;
                return cache = screenShot;
            }
        }
        
        private ScreenShot(Bitmap bitmap, int hashCode)
        {
            this.bitmap = bitmap;
            this.hashCode = hashCode;
        }

        public override bool Equals(object obj)
        {
            return GetHashCode() == obj.GetHashCode();
        }
        public override int GetHashCode()
        {
            return hashCode == 0 ? base.GetHashCode() : hashCode;
        }
        public static int Width { get { return cache != null ? cache.Width : CreateScreenShort().Width; } }
        public static int Height { get { return cache != null ? cache.Height : CreateScreenShort().Height; } }
        #endregion
    }
}
