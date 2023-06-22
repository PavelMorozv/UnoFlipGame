using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Network
{
    public static class NetworkUtils
    {
        public static void SendMessage(TcpClient client, byte[] message)
        {
            int packetSize = message.Length;
            byte[] packetSizeBytes = BitConverter.GetBytes(packetSize);
            NetworkStream stream = client.GetStream();
            stream.Write(packetSizeBytes, 0, packetSizeBytes.Length);
            stream.Write(message, 0, message.Length);
        }

        public static byte[] ReceiveMessage(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] packetSizeBytes = new byte[sizeof(int)];
            stream.Read(packetSizeBytes, 0, packetSizeBytes.Length);
            int packetSize = BitConverter.ToInt32(packetSizeBytes, 0);
            byte[] message = new byte[packetSize];
            int bytesRead = 0;
            int totalBytesRead = 0;
            while (totalBytesRead < packetSize)
            {
                bytesRead = stream.Read(message, totalBytesRead, packetSize - totalBytesRead);
                totalBytesRead += bytesRead;
            }
            return message;
        }

        public static async Task SendMessageAsync(TcpClient client, byte[] message)
        {
            int packetSize = message.Length;
            byte[] packetSizeBytes = BitConverter.GetBytes(packetSize);
            NetworkStream stream = client.GetStream();
            await stream.WriteAsync(packetSizeBytes, 0, packetSizeBytes.Length);
            await stream.WriteAsync(message, 0, message.Length);
        }

        public static async Task<byte[]> ReceiveMessageAsync(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] packetSizeBytes = new byte[sizeof(int)];
            await stream.ReadAsync(packetSizeBytes, 0, packetSizeBytes.Length);
            int packetSize = BitConverter.ToInt32(packetSizeBytes, 0);
            byte[] message = new byte[packetSize];
            int bytesRead = 0;
            int totalBytesRead = 0;
            while (totalBytesRead < packetSize)
            {
                bytesRead = await stream.ReadAsync(message, totalBytesRead, packetSize - totalBytesRead);
                totalBytesRead += bytesRead;
            }
            return message;
        }
    }
}
