using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace ScreenMonitor
{
    class DataConsumer
    {
        BlockingCollection<byte[]> queue = null;
        CancellationTokenSource tokenSource = null;
        NetworkStream ns = null;
        Task task = null;

        public DataConsumer(BlockingCollection<byte[]> queue, NetworkStream ns)
        {
            if (queue == null || ns == null)
                throw new NullReferenceException();
            this.queue = queue;
            this.ns = ns;
        }

        public async Task StartAsync()
        {
            tokenSource = new CancellationTokenSource();
            await Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        if (tokenSource.IsCancellationRequested)
                            return;
                        byte[] networkData = null;
                        if (queue.TryTake(out networkData))
                        {
                            writeToNet(networkData, networkData.Length, ns);
                        }
                        else
                            Thread.Sleep(10);
                    }
                    catch (Exception e)
                    {
                        return;
                    }
                }
            }, tokenSource.Token);

        }

        public async Task StopAsync()
        {
            tokenSource.Cancel();
            if (task != null)
                await task;
        }

        static void writeToNet(byte[] buffer, int count, NetworkStream ns)
        {
            int writeCount = 10240;
            int indext = 0;
            while (indext < count)
            {
                int thisWriteLength = (count - indext) > writeCount ? writeCount : count - indext;
                ns.Write(buffer, indext, thisWriteLength);
                indext += thisWriteLength;
                ns.Flush();
            }
        }
    }

}
