using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace App
{
    internal static class Program
    {
        private const int Port = 19000;
        private static readonly IPAddress Localhost = IPAddress.Parse("127.0.0.1");

        private static void Main()
        {
            var server = default(TcpListener);

            try
            {
                server = new TcpListener(Localhost, Port);
                server.Start();

                while (true)
                {
                    Console.WriteLine("Ожидание подключений...");
                    var client = server.AcceptTcpClient();
                    Console.WriteLine("Подключен клиент. Выполнение запроса...");

                    WorkIt(client);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
            finally
            {
                server?.Stop();
            }
        }

        private static void WorkIt(TcpClient client)
        {
            var stream = client.GetStream();

            for (var i = 0D; i < 50; i += 0.05)
            {
                var generate = Generate(i);
                var data = new[] {generate, generate};

                var commandBytes = BitConverter.GetBytes(2);
                var modelTimeBytes = BitConverter.GetBytes(0);
                var dataBytes = GetBytes(data);
                var dataLengthBytes = BitConverter.GetBytes(data.Length);

                var message = commandBytes
                    .Concat(modelTimeBytes)
                    .Concat(dataLengthBytes)
                    .Concat(dataBytes)
                    .ToArray();

                stream.Write(message, 0, message.Length);
                stream.Flush();
                // Console.WriteLine(DateTime.UtcNow + " Send " + i);
                // Thread.Sleep(10);
            }

            stream.Close();
            client.Close();
        }

        private static double Generate(double t) =>
            2 * Math.Sin(2 * t + 4) + 3 * Math.Sin(5 * t - 2);

        private static byte[] GetBytes(double[] values)
        {
            var result = new byte[values.Length * sizeof(double)];
            Buffer.BlockCopy(values, 0, result, 0, result.Length);
            return result;
        }
    }
}
