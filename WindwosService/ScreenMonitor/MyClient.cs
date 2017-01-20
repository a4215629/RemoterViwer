using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace ScreenMonitor
{
    class MyClient
    {
        bool isConnect=true;
        
        public TcpClient Client;
        NetworkStream ns;
        DataPackageProducer producer = null;
        DataPackageConsumer consumer = null;
        BlockingCollection<ScreenShotPackage> queue = null;
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
            queue = new BlockingCollection<ScreenShotPackage>(2);
            producer = new DataPackageProducer(queue);
            consumer = new DataPackageConsumer(queue,ns);
            ThreadStart stR = new ThreadStart(ReadCommand);
            receiverThread = new Thread(stR);
        }
        /// <summary>
        /// 启动交互程序
        /// </summary>
        public async void StartAsync()
        {
            producer.Start();
            receiverThread.Start();
            await consumer.StartAsync();
            Stop();
        }

        public void Stop()
        {
            consumer.Stop();
            producer.Stop();
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
    }
}
