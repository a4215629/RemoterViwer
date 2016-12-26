using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.ServiceProcess;

namespace ScreenMonitor
{
   public partial class ScreenService
    {
        static int port = 3929;
        static PortService service;

        public void Start()
        {
            service = new PortService(port);
            service.Start();
        }
        public void Stop()
        {
            service.Stop();
            //base.Stop();
        }
    }
}
