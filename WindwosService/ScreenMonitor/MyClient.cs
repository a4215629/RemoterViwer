﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace ScreenMonitor
{
    class MyClient
    {
        bool isConnect=true;
        
        public TcpClient Client;
        NetworkStream ns;
        public ScreenShotPackage Cache { get;private set; }
        Thread sendThread = null;
        Thread receiverThread = null;
        public MyClient(TcpClient client)
        {
            this.Client = client;
            Init();
        }
        public event Action<MyClient> OnStoped;

        void Init()
        {
            ns = Client.GetStream();
            ThreadStart stR = new ThreadStart(ReadCommand);
            ThreadStart stS = new ThreadStart(SendDate);
            sendThread = new Thread(stS);
            receiverThread = new Thread(stR);
        }
        /// <summary>
        /// 启动交互程序
        /// </summary>
        public void Start()
        {
            sendThread.Start();
            receiverThread.Start();
        }

        public void Stop()
        {
            isConnect = false;
            ns.Close();
            Client.Client.Close();
            if (OnStoped != null)
                OnStoped(this);
        }

        void ReadCommand()
        {
            while (isConnect)
            {
                try
                {
                    byte[] buffer = new byte[256];
                    int readLength = 0;
                    if (ns.CanRead)
                    {
                        readLength += ns.Read(buffer, readLength, buffer.Length);
                    }
                    
                    if (readLength == buffer.Length)
                        DataOpretor.sys_Operate(buffer);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    if(isConnect)
                    {
                        Stop();
                    }
                    return;

                }
                Thread.Sleep(100);

            }
        }
        void SendDate()
        {
            while (isConnect)
            {
                try
                {
                    Bitmap screenShot = ScreenImageService.getScrreenShot();
                    if (Cache!= null && Cache.Bitmap.Equals(screenShot))
                    {
                        Thread.Sleep(50);
                        continue;
                    }
                    ScreenShotPackage spakage = new ScreenShotPackage((Bitmap)screenShot.Clone(),1280);

                    byte[] data = null;
                    if (Cache != null )
                    {
                        //Stopwatch watch = new Stopwatch();watch.Start();
                        spakage.InitializeSplitting(16, 9);
                        spakage.CompressByBasePackage(Cache);
                        //watch.Stop(); Console.WriteLine("合成数据: " + watch.ElapsedMilliseconds); watch.Reset(); watch.Start();
                    }

                    data = spakage.GetBytes();
                    //watch.Stop(); Console.WriteLine("合成数据: " + watch.ElapsedMilliseconds); watch.Reset(); watch.Start();
                    writeToNet(data, data.Length, ns);
                    //watch.Stop(); Console.WriteLine("发送数据: " + watch.ElapsedMilliseconds);
                    this.Cache = spakage;


                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    if (isConnect)
                    {
                        Stop();
                    }
                    return;
                }
                Thread.Sleep(10);
            }
        }




        static void writeToNet(byte[] buffer, int count, NetworkStream ns)
        {
            int writeCount = 10240;
            int indext = 0;
            //lock (ns)
            //{
            while (indext < count)
            {
                int thisWriteLength = (count - indext) > writeCount ? writeCount : count - indext;
                ns.Write(buffer, indext, thisWriteLength);
                indext += thisWriteLength;
            }
            ns.Flush();

            //}

        }
    }
}
