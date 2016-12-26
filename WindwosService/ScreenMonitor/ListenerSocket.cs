using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace ScreenMonitor
{
    class ListenerSocket
    {
        public TcpListener Listener { get; set; }
        public List<TcpClient> Clients { get; set; }
        
    }
}
