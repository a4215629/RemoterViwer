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
        public TcpClient Client;
        NetworkStream ns;
        DataPackageProducer producer = null;
        DataPackageConsumer consumer = null;
        CommandReder cmdReader = null;
        BlockingCollection<ScreenShotPackage> queue = null;
        bool stoping = false;

        public MyClient(TcpClient client)
        {
            this.Client = client;
            Init();
        }
        public event Action<MyClient> OnStoped;

        void Init()
        {
            ns = Client.GetStream();
            queue = new BlockingCollection<ScreenShotPackage>(1);
            producer = new DataPackageProducer(queue);
            consumer = new DataPackageConsumer(queue,ns);
            cmdReader = new CommandReder(ns);
        }
        /// <summary>
        /// 启动交互程序
        /// </summary>
        public async void StartAsync()
        {
            producer.Start();
            cmdReader.Start();
            await consumer.StartAsync();
            if(!stoping)
                StopAsync();
        }

        public async void StopAsync()
        {
            stoping = true;
            await consumer.StopAsync();
            producer.Stop();
            cmdReader.Stop();
            ns.Close();
            Client.Client.Close();
            if (OnStoped != null)
                OnStoped(this);
        }


    }
}
