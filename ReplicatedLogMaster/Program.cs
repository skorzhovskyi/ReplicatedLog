using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace ReplicatedLogMaster
{

    class Program
    {
        static void Main(string[] args)
        {
            string? _numOfSlaves = Environment.GetEnvironmentVariable("SECONDARIES_NUM");
            string? _host = Environment.GetEnvironmentVariable("MASTER_HOST");
            string? _port = Environment.GetEnvironmentVariable("MASTER_PORT");
            string? _broadCastingTimeOut = Environment.GetEnvironmentVariable("BROADCASTING_TIME_OUT");
            string? _retryTimeout = Environment.GetEnvironmentVariable("RETRY_TIME_OUT");
            string? _pingDelay = Environment.GetEnvironmentVariable("PING_DELAY");
            string? _retryDelay = Environment.GetEnvironmentVariable("RETRY_DELAY");
            string? _quorum = Environment.GetEnvironmentVariable("QUORUM");

            string host = _host == null ? "localhost" : _host;
            int port = _port == null ? 2100 : int.Parse(_port);
            int numOfSlaves = _numOfSlaves == null ? 2 : int.Parse(_numOfSlaves);
            int broadCastingTimeOut = _broadCastingTimeOut == null ? 20000 : int.Parse(_broadCastingTimeOut) * 1000;
            int retryTimeout = _retryTimeout == null ? 30000 : int.Parse(_retryTimeout) * 1000;
            int pingDelay = _pingDelay == null ? 5000 : int.Parse(_pingDelay) * 1000;
            int retryDelay = _retryDelay == null ? 5000 : int.Parse(_pingDelay) * 1000;
            int quorum = _quorum == null ? 1 : int.Parse(_quorum);

            Console.WriteLine("Host: " + host);
            Console.WriteLine("Port: " + port);

            List<Uri> secondaries = new List<Uri>();

            if (_host == null)
            {
                secondaries.Add(new Uri("http://localhost:2201"));
                secondaries.Add(new Uri("http://localhost:2202"));
            }
            else
            {
                for (int id = 1; id <= numOfSlaves; id++)
                {
                    string? slave_host = Environment.GetEnvironmentVariable("SECONDARY" + id + "_HOST");
                    string? slave_port = Environment.GetEnvironmentVariable("SECONDARY" + id + "_PORT");

                    if (slave_host == null || slave_port == null)
                        break;

                    Console.WriteLine("Slave host: " + slave_host);
                    Console.WriteLine("Slave port: " + slave_port);

                    secondaries.Add(new Uri("http://" + slave_host + ":" + slave_port));
                }
            }

            new Server(host, port, retryTimeout, secondaries, broadCastingTimeOut, retryDelay, pingDelay, quorum);
        }
    }
}
