using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MSS
{
    public class Server
    {
        int serviceTime;
        protected ThreadPool threadPool;
        public Server(int serviceIntensity, ThreadPool threadPool) {
            serviceTime = Program.TIME_TICK / serviceIntensity;
            this.threadPool = threadPool; 
        }
        public Server(int serviceIntensity, int? pool_size) : this(serviceIntensity, new ThreadPool(pool_size)) { }
        
        public virtual bool HandleRequest(Client sender, Client.ClientEventArgs e)
        {
            Worker worker = threadPool.GetWorker();
            if (worker == null)
            {
                return false;
            }
            worker.Start(ProcessRequest);
            worker.Free();
            return true;
        }

        public void ProcessRequest(object? parameters)
        {
            Thread.Sleep(serviceTime);
        }

    }
}
