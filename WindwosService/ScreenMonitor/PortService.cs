using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ScreenMonitor
{
    public class PortService
    {
        private bool isRunning = false;
        private int port;
        TcpListener listener;
        Thread ListenThread;
        List<MyClient> myClinets = new List<MyClient>();
        public bool IsRunning
        {
            get { return isRunning; }
        }
        public PortService(int Port)
        {
            this.port = Port;
            listener = new TcpListener(port);
        }

        public void Start() {
            isRunning = true;
            ListenThread = new Thread(new ThreadStart(Listen));
            ListenThread.Start();
        }
        public void Stop()
        {
            isRunning = false;
            listener.Stop();
            while(myClinets.Count!=0)
            {
                myClinets[0].Stop();
            }
            myClinets.Clear();
        }
        private void Listen()
        {
            listener.Start(10);
            while(isRunning)
            {
                try
                {
                    TcpClient tcpClient = listener.AcceptTcpClient();
                    if (tcpClient == null)
                        continue;
                    MyClient myClient = new MyClient(tcpClient);
                    myClinets.Add(myClient);
                    ScreenShot.StartShot();
                    myClient.OnStoped += ((obj) => {myClinets.Remove(obj); if (myClinets.Count == 0) ScreenShot.StopShot(); });
                    myClient.Start();
                }
                catch (Exception e)
                {
                    isRunning = false;
                }
            }

        }

    }
}
