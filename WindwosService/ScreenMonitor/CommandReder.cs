using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenMonitor
{
    class CommandReder
    {
        CancellationTokenSource tokenSource = null;
        Task mainTask = null;
        NetworkStream ns = null;
        public CommandReder(NetworkStream ns)
        {
            this.ns = ns;
        }

        public void Start()
        {
            if (tokenSource?.IsCancellationRequested == false)
                return;
            tokenSource = new CancellationTokenSource();
            mainTask = Task.Factory.StartNew(ReadCommand);
        }

        public void Stop()
        {
            if (tokenSource == null)
                return;
            tokenSource.Cancel();
            Task.WaitAll(mainTask);
        }

        void ReadCommand()
        {
            while (true)
            {
                if (tokenSource.IsCancellationRequested)
                    return;
                try
                {
                    byte[] buffer = new byte[256];
                    int readLength = 0;
                    if (ns.CanRead)
                    {
                        readLength += ns.Read(buffer, readLength, buffer.Length);
                    }
                    if (readLength == buffer.Length)
                        new Action(() => DataOpretor.sys_Operate(buffer)).BeginInvoke(null, null);
                        
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
                Thread.Sleep(50);
            }
        }
    }
}
