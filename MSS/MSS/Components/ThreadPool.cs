using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MSS
{
    public class Worker
    {
        public delegate void WorkerEventHandler(Worker sender, EventArgs e);
        public event WorkerEventHandler OnFinish;

        Thread thread;
        bool allocated;
        bool ReadyToWork 
        { 
            get 
            {
                return thread == null || thread.ThreadState == ThreadState.Stopped; 
            } 
        }
        public bool IsFree { get { return !allocated && ReadyToWork; } }
        public Worker() { }
        public virtual void Start(ParameterizedThreadStart work)
        {
            if (!ReadyToWork)
            {
                throw new Exception();
            }
            thread = new Thread(
                (object? parameters) =>
                {
                    work(parameters);
                    OnFinish.Invoke(this, new EventArgs());
                }
            );
            thread.Start();
        }
        public void Join()
        {
            thread.Join();
        }
        public void Allocate()
        {
            if (allocated)
            {
                throw new Exception();
            }
            allocated = true;
        }
        public void Free()
        {
            allocated = false;
        }
    }
    public class ThreadPool
    {
        protected List<Worker> workers;
        public int PoolSize { get { return workers.Count; } }
        public ThreadPool(int? pool_size = null)
        {
            int _pool_size = (pool_size != null) ? (int)pool_size : Environment.ProcessorCount;
            workers = new List<Worker>(_pool_size);
            InitializePool();
        }
        public virtual void InitializePool()
        {
            for (int i=0; i<workers.Capacity; i++)
            {
                workers.Add(new Worker());
            }
        }
        public Worker GetWorker()
        {
            lock (workers)
            {
                foreach (Worker worker in workers)
                {
                    if (worker.IsFree)
                    {
                        worker.Allocate();
                        return worker;
                    }
                }
            }
            return null;
        }
    }
}
