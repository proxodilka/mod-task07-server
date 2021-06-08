using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace rolll
{
    class Worker
    {
        Thread workThread;
        Stopwatch total_time, work_time;
        long total_work_time = 0;

        public Worker()
        {
            total_time = new Stopwatch();
            work_time = new Stopwatch();

            total_time.Start();
        }

        public bool IsFree()
        {
            return workThread == null || workThread.ThreadState == System.Threading.ThreadState.Stopped;
        }

        public double IdleProbability
        {
            get
            {
                long total = total_time.ElapsedMilliseconds;
                return (double)(total - total_work_time) / total;
            }
        }

        public long TotalTime
        {
            get { return total_time.ElapsedMilliseconds; }
        }

        public long WorkTime
        {
            get { return total_work_time; }
        }

        public void Start(int delay)
        {
            if (!IsFree())
            {
                throw new Exception("wtf?");
            }
            work_time.Restart();
            workThread = new Thread(() => {
                Thread.Sleep(delay);
                Console.WriteLine($"Another client served for {delay}ms");
                work_time.Stop();
                total_work_time += work_time.ElapsedMilliseconds;
            });
            workThread.Start();
        }

        public void Join()
        {
            if (!IsFree())
            {
                workThread.Join();
            }
        }
    }
    class Server
    {
        List<Worker> workers;
        object why_do_we_need_this_locker = new object();
        int service_time;

        public Server(int n_workers, int service_intensity)
        {
            workers = new List<Worker>(n_workers);
            service_time = 900 / service_intensity;
            for (int i = 0; i < n_workers; i++)
            {
                workers.Add(new Worker());
            }
        }

        public bool Handler(Client sender)
        {
            bool res = false;
            lock (why_do_we_need_this_locker)
            {
                foreach (var worker in workers)
                {
                    if (worker.IsFree())
                    {
                        worker.Start(service_time);
                        res = true;
                        break;
                    }
                }
            }
            return res;
        }

        public void Stop()
        {
            foreach (var worker in workers)
            {
                worker.Join();
            }
        }

        public double IdleProbability
        {
            get
            {
                double accum = 0;
                foreach (var worker in workers)
                {
                    accum += worker.IdleProbability;
                }
                return accum / workers.Count;
            }
        }

        public double MeanBusyThreads
        {
            get
            {
                long total_work_time = 0;
                foreach (var worker in workers)
                {
                    total_work_time += worker.WorkTime;
                }
                return total_work_time / (double)workers[0].TotalTime;
            }
        }
    }
}
