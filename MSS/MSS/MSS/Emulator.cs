using System;
using System.Collections.Generic;
using System.Text;
using MSS.Statistic;
using System.Diagnostics;
using System.Threading;

namespace MSS
{
    public class MSSEmulator
    {
        public ServerStatistic server;
        Client client;
        int requestsTimeout;
        int requestsBucketSize;
        int consoleRefreshRate = 100;
        Stopwatch simulationStartTimeStamp;
        const int MIN_POSSIBLE_TIMEOUT = 50;
        const int DELAY_EPSILON = 10;
        public MSSEmulator(int requestsIntensity, int serviceIntensity, int numberOfChanels)
        {
            server = new ServerStatistic(serviceIntensity, numberOfChanels);
            client = new Client(server.HandleRequest);
            requestsTimeout = Math.Max(Program.TIME_TICK / requestsIntensity, MIN_POSSIBLE_TIMEOUT);
            requestsBucketSize = requestsIntensity / (Program.TIME_TICK / requestsTimeout);
            simulationStartTimeStamp = new Stopwatch();
        }
        public void Start(int simulationDuration)
        {
            simulationStartTimeStamp.Reset();
            simulationStartTimeStamp.Start();

            CancellationTokenSource token = new CancellationTokenSource();
            Thread drawThread = new Thread(() => Draw(token.Token));
            drawThread.Start();
            Stopwatch bucketTime = new Stopwatch();
            while (true)
            {
                if (simulationStartTimeStamp.ElapsedMilliseconds > simulationDuration)
                {
                    break;
                }
                for (int i=0; i<requestsBucketSize; i++)
                {
                    client.SendTask();
                }
                int timeout = Math.Max(requestsTimeout - DELAY_EPSILON, 0);
                if (timeout != 0)
                {
                    Thread.Sleep(timeout);
                }
            }
            simulationStartTimeStamp.Stop();
            token.Cancel();
            Draw(token.Token);
        }
        public string VerboseStatistic(string prefix = "")
        {
            return server.VerboseStatistic(prefix);
        }
        public void Draw(CancellationToken token)
        {
            Console.Clear();
            while (true)
            {
                Console.SetCursorPosition(0, 0);

                Console.Write(VerboseStatistic());
                Console.Write($"Elapsed: {simulationStartTimeStamp.ElapsedMilliseconds / 1000f}s");
                Console.Write(new string(' ', 10) + "\n");
                Thread.Sleep(consoleRefreshRate);
                if (token.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }
}
