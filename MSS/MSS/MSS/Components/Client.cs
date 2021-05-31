using System;
using System.Collections.Generic;
using System.Text;

namespace MSS
{
    public class Client
    {
        public Client(ClientEventHandler handler)
        {
            OnTask += handler;
        }

        public class ClientEventArgs : EventArgs
        {
            Dictionary<string, object> taskKwargs;
            public ClientEventArgs(Dictionary<string, object> kwargs) { taskKwargs = kwargs; }
        }

        public delegate bool ClientEventHandler(Client sender, ClientEventArgs e);
        event ClientEventHandler OnTask;

        public bool SendTask(Dictionary<string, object> kwargs = null)
        {
            if (kwargs == null)
            {
                kwargs = new Dictionary<string, object>();
            }
            bool ret_code = OnTask.Invoke(this, new ClientEventArgs(kwargs));
            return ret_code;
        }
    }
}
