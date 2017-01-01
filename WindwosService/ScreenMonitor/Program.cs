using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.ServiceProcess;
using System.Diagnostics;
using System.Windows.Forms;

namespace ScreenMonitor
{
    class Program  
    {
      
        static void Main()
        {
            ScreenService screenService = new ScreenService();
            screenService.Start();
        }

    }
}
