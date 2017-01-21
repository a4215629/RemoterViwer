using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ScreenMonitor
{
   public partial class ScreenMonitorService
    {
        static int port = 3929;
        static TcpListener listener;
        static ConcurrentDictionary<int, MyClient> myClinets = new ConcurrentDictionary<int, MyClient>();
        public static bool IsRunning { get; private set; } = false;
        static Task mainTask;

        public static void Start()
        {
            if (IsRunning == true)
                return;
            IsRunning = true;
            mainTask = new Task(Listen);
            mainTask.Start();
            Task.WaitAll(mainTask);

        }
        public static void Stop()
        {
            if (IsRunning == true)
                return;
            IsRunning = false;
            listener.Stop();
            while (myClinets.Count != 0)
            {
                myClinets.First().Value.StopAsync();
            }
            Task.WaitAll(mainTask);
        }

        private static void Listen()
        {
            listener = new TcpListener(port);
            listener.Start(10);
            while (IsRunning)
            {
                try
                {
                    TcpClient tcpClient = listener.AcceptTcpClient();
                    if (tcpClient == null)
                        continue;
                    MyClient myClient = new MyClient(tcpClient);
                    myClinets.TryAdd(myClient.GetHashCode(), myClient);
                    ScreenShot.StartShot();
                    myClient.OnStoped += ((obj) =>
                    {
                        myClinets.TryRemove(obj.GetHashCode(),out obj);
                        if (myClinets.Count == 0)
                            ScreenShot.StopShot();
                    });
                    myClient.StartAsync();
                }
                catch (Exception e)
                {
                    IsRunning = false;
                }
            }

        }
    }
}
