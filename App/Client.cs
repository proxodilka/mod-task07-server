using System;
using System.Collections.Generic;
using System.Text;

namespace rolll
{
    class Client
    {
        public delegate bool OnTaskEventHandler(Client sender);
        public event OnTaskEventHandler OnTask;
        public Client(Server server)
        {
            OnTask += server.Handler;
        }

        public bool Task()
        {
            return OnTask.Invoke(this);
        }
    }
}
