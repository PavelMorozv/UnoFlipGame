using ServerLib;
using System.Net.WebSockets;

namespace ConsoleServer
{
    public class Program
    {
        static void Main(string[] args)
        {
            var server = new Server();
            server.Start();            
        }
    }
}