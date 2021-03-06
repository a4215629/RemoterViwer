﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Compression;
using ScreenMonitor.Tools;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenMonitor
{
    public class ScreenShot
    {
        #region 获取系统截图
        static Bitmap cache;
        static DateTime imgTime = new DateTime(1, 1, 1);
        static CancellationTokenSource tokenSource = null;
        static Task mainTask = null;
        const int flushTime = 38;
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
                    if (cache == null)
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
        private static Bitmap CreateScreenShortGdi()
        {
            return cache = APIWrapper.GetScreenShotGdi();
        }

        private ScreenShot(Bitmap bitmap, int hashCode)
        {
            this.bitmap = bitmap;
            this.hashCode = hashCode;
        }
        public static void StartShot() {
            if (tokenSource?.IsCancellationRequested == false)
                return;
            tokenSource = new CancellationTokenSource();
            mainTask = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (tokenSource.IsCancellationRequested)
                        return;
                    if (DateTime.Now > imgTime.AddMilliseconds(flushTime))
                    {
                        try
                        {
                            CreateScreenShort();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.StackTrace);
                        }
                        imgTime = DateTime.Now;
                    }
                    else
                        Thread.Sleep(5);
                }
            }, tokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        public static void StopShot()
        {
            if (tokenSource == null)
                return;
            tokenSource.Cancel();
            Task.WaitAll(mainTask);
        }
        public override bool Equals(object obj)
        {
            return GetHashCode() == obj.GetHashCode();
        }
        public override int GetHashCode()
        {
            return hashCode == 0 ? base.GetHashCode() : hashCode;
        }
        public static int Width { get
            {
                lock (lockObj)
                {
                    return cache != null ? cache.Width : CreateScreenShort().Width;
                }

            } }
        public static int Height {
            get
            {
                lock (lockObj)
                {
                    return cache != null ? cache.Height : CreateScreenShort().Height;
                }
            }
        }
        #endregion
    }
}
