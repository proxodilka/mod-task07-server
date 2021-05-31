using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace MSS.Statistic
{
    public class StatisticHelper
    {
        public static int Factorial(int f)
        {
            if (f == 0)
                return 1;
            else
                return f * Factorial(f - 1);
        }
    }

    public class ServerStatistic : Server
    {
        int nrecieved;
        int naccepted;

        public int NumberOfRecieved { get { return nrecieved; } }
        public int NumberOfProcessed { get { return naccepted; } }
        public int NumberOfDeclined { get { return nrecieved - naccepted; } }

        public ServerStatistic(int serviceIntensity, ThreadPoolStatistic threadPool) : base(serviceIntensity, threadPool) { }
        public ServerStatistic(int serviceIntensity, int? pool_size) : base(serviceIntensity, new ThreadPoolStatistic(pool_size)) { }
        public override bool HandleRequest(Client sender, Client.ClientEventArgs e)
        {
            nrecieved += 1;
            bool is_accepted = base.HandleRequest(sender, e);
            if (is_accepted)
            {
                naccepted += 1;
            }
            return is_accepted;
        }

        public string VerboseStatistic(string prefix = "")
        {
            string result = "";
            ThreadPoolStatistic threadPool = (this.threadPool as ThreadPoolStatistic);

            result += prefix + $"Number of requests: {NumberOfRecieved} | Number of processed: {NumberOfProcessed} | Number of declined: {NumberOfDeclined}\n";
            result += prefix + $"Per thread statistic:\n";
            result += threadPool.VerboseStatistic(prefix + "\t");

            double plainProbability = threadPool.PlainProbability;
            double declineProbability = (double)NumberOfDeclined / NumberOfRecieved;
            double relativethroughput = 1 - declineProbability;
            double requestsIntensity = (double)NumberOfRecieved / (threadPool.TotalTime / Program.TIME_TICK);
            double absoluteThroughput = requestsIntensity * relativethroughput;
            double meanBusyThreads = threadPool.MeanBusyThreads;

            result += prefix + $"Plain probability:\t{Math.Round(plainProbability, 2)}                        \n";
            result += prefix + $"Decline probability:\t{Math.Round(declineProbability, 2)}                    \n";
            result += prefix + $"Relative throughput:\t{Math.Round(relativethroughput, 2)}                    \n";
            result += prefix + $"Absolute throughput:\t{Math.Round(absoluteThroughput, 2)}                    \n";
            result += prefix + $"Mean busy threads:\t{Math.Round(meanBusyThreads, 2)}                         \n";
            return result;
        }
    }

    public class WorkerStatistic : Worker
    {
        int nrecieved;
        int nprocessed;
        long workTime;
        Stopwatch timer;
        Stopwatch totalTime;

        public WorkerStatistic()
        {
            OnFinish += WorkerStatistic_OnFinish;
            timer = new Stopwatch();
            totalTime = new Stopwatch();
            totalTime.Start();
        }
        void WorkerStatistic_OnFinish(Worker sender, EventArgs e)
        {
            timer.Stop();
            workTime += timer.ElapsedMilliseconds;
            nprocessed += 1;
        }
        public int NumberOfRecieved { get { return nrecieved; } }
        public int NumberOfProcessed { get { return nprocessed; } }
        public long WorkTime { get { return workTime; } }
        public long TotalTime { get { return totalTime.ElapsedMilliseconds; } }
        public long PlainTime { get { return totalTime.ElapsedMilliseconds - WorkTime; } }
        public double PlainProbability { get { return (double)PlainTime / totalTime.ElapsedMilliseconds; } }
        public override void Start(ParameterizedThreadStart work)
        {
            nrecieved += 1;
            timer.Restart();
            base.Start(work);
        }
        public string VerboseStatistic(string prefix="")
        {
            return prefix + $"Number of recieved: {NumberOfRecieved} | Number of processed: {NumberOfProcessed} | Plain probability: {Math.Round(PlainProbability, 2)}";
        }
    }

    public class ThreadPoolStatistic : ThreadPool
    {
        public ThreadPoolStatistic(int? pool_size = null) : base(pool_size) { }

        public override void InitializePool()
        {
            for (int i=0; i<workers.Capacity; i++)
            {
                workers.Add(new WorkerStatistic());
            }
        }

        public double PlainProbability 
        { 
            get 
            {
                double plainProbability = 0;
                foreach (WorkerStatistic worker in workers)
                {
                    plainProbability += worker.PlainProbability;
                }
                return plainProbability / workers.Count;
            } 
        }

        public double MeanBusyThreads
        {
            get
            {
                long totalWorkTime = 0;
                foreach (WorkerStatistic worker in workers)
                {
                    totalWorkTime += worker.WorkTime;
                }
                return totalWorkTime / (double)(workers[0] as WorkerStatistic).TotalTime;
            }
        }
        public long TotalTime { get { return (workers[0] as WorkerStatistic).TotalTime; } }

        public string VerboseStatistic(string prefix="")
        {
            string result = "";
            for (int i=0; i<workers.Count; i++)
            {
                result += prefix + $"{i + 1}. {(workers[i] as WorkerStatistic).VerboseStatistic()}\n";
            }
            return result;
        }
    }
}
