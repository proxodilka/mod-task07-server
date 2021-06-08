using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace rolll
{
    class Emulator
    {
        Server server;
        Client client;
        int clients_delay;
        int clients_intensity;

        int n_accepted = 0, n_declined = 0;

        public Emulator(int clients_intensity, int service_intensity, int n_workers)
        {
            server = new Server(n_workers, service_intensity);
            client = new Client(server);
            this.clients_intensity = clients_intensity;
            clients_delay = 900 / clients_intensity;
        }

        public void Start(int n_clients)
        {
            for (int i=0; i< n_clients; i++)
            {
                bool res = client.Task();
                if (res)
                {
                    n_accepted++;
                }
                else
                {
                    n_declined++;
                }
                Thread.Sleep(clients_delay);
            }
            server.Stop();
            Console.WriteLine($"Total clients: {n_clients} | Served: {n_accepted} | Declined: {n_declined}");
            Console.WriteLine($"Decline probability: {(double)n_declined / n_clients}");
            Console.WriteLine($"Relative throughput: {1 - (double)n_declined / n_clients}");
            Console.WriteLine($"Absolute throughput: {clients_intensity * (1 - (double)n_declined / n_clients)}");
            Console.WriteLine($"Idle probability: {server.IdleProbability}");
            Console.WriteLine($"Mean busy threads: {server.MeanBusyThreads}");
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Emulator emulator = new Emulator(10, 2, 3);
            emulator.Start(100);
        }
    }
}
