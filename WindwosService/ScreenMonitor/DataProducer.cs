using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;

namespace ScreenMonitor
{
    class DataProducer
    {
        BlockingCollection<byte[]> queue = null;
        CancellationTokenSource tokenSource = null;
        Task mainTask = null;

        public DataProducer(BlockingCollection<byte[]> queue)
        {
            if (queue == null)
                throw new NullReferenceException();
            this.queue = queue;
        }
        public void Start()
        {
            if (tokenSource?.IsCancellationRequested == false)
                return;
            tokenSource = new CancellationTokenSource();
            mainTask = Task.Factory.StartNew(() =>
            {
                ScreenShotPackage cache = null;
                while (true)
                {
                    if (tokenSource.IsCancellationRequested)
                        return;
                    var package = CollectData(cache);
                    if (package == null)
                    {
                        Thread.Sleep(5);
                        continue;
                    }
                    if(queue.TryAdd(package.GetBytes()))
                        cache = package;
                    else
                        Thread.Sleep(10);
                }
            }, tokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        }

        public void Stop()
        {
            if (tokenSource == null)
                return;
            tokenSource.Cancel();
            Task.WaitAll(mainTask);
        }

        private ScreenShotPackage CollectData(ScreenShotPackage cache)
        {
            //Stopwatch tolwatch = new Stopwatch(); tolwatch.Start();
            //Stopwatch watch = new Stopwatch(); watch.Start();
            var screenShot = ScreenShot.CurenntScreenShort;
            if (cache != null && cache.ScreenShot.Equals(screenShot))
            {
                return null;
            }
            //watch.Stop(); Console.WriteLine("获取截图: " + watch.ElapsedMilliseconds); watch.Reset(); watch.Start();
            ScreenShotPackage spakage = new ScreenShotPackage(screenShot, 1280);
            //watch.Stop(); Console.WriteLine("压缩截图: " + watch.ElapsedMilliseconds); watch.Reset(); watch.Start();
            if (cache != null)
            {
                spakage.InitializeSplitting(14, 7);
                //watch.Stop(); Console.WriteLine("分块截图: " + watch.ElapsedMilliseconds); watch.Reset(); watch.Start();
                spakage.CompressByBasePackage(cache);
                //watch.Stop(); Console.WriteLine("压缩数据: " + watch.ElapsedMilliseconds); watch.Reset(); watch.Start();
            }
            return spakage;
        }
    }
}
