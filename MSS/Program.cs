using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MSS.Statistic;

namespace MSS
{
    public class Program
    {
        public const int TIME_TICK = 200;
        static void Main(string[] args)
        {
            int requestsIntensity = 16;
            int serviseIntensity = 2;
            int nchanels = 6;
            int simulationDuration = 12000;

            ParseArgs(args, ref requestsIntensity, ref serviseIntensity, ref nchanels, ref simulationDuration);

            MSSEmulator mss = new MSSEmulator(requestsIntensity, serviseIntensity, nchanels);
            mss.Start(simulationDuration);
        }
        static void ParseArgs(string[] args, ref int requestsIntensity, ref int serviseIntensity, ref int nchanels, ref int simulationDuration)
        {
            if (args.Length > 0)
            {
                requestsIntensity = int.Parse(args[0]);
            }
            if (args.Length > 1)
            {
                serviseIntensity = int.Parse(args[1]);
            }
            if (args.Length > 2)
            {
                nchanels = int.Parse(args[2]);
            }
            if (args.Length > 3)
            {
                simulationDuration = int.Parse(args[3]);
            }
        }
    }
}
