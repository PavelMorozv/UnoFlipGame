using System;
using System.Net.Sockets;

namespace Network
{
    public static class NetworkUtils
    {
        // Метод для отправки сообщения с указанием размера пакета
        public static void SendMessage(Socket socket, byte[] message)
        {
            // Получение размера пакета
            int packetSize = message.Length;
            byte[] packetSizeBytes = BitConverter.GetBytes(packetSize);

            // Отправка размера пакета
            socket.Send(packetSizeBytes);

            // Отправка самого пакета
            socket.Send(message);
        }

        // Метод для приема сообщения с указанием размера пакета
        public static byte[] ReceiveMessage(Socket socket)
        {
            // Получение размера пакета
            byte[] packetSizeBytes = new byte[sizeof(int)];
            socket.Receive(packetSizeBytes);
            int packetSize = BitConverter.ToInt32(packetSizeBytes, 0);

            // Получение самого пакета
            byte[] message = new byte[packetSize];
            int bytesRead = 0;
            int totalBytesRead = 0;
            while (totalBytesRead < packetSize)
            {
                bytesRead = socket.Receive(message, totalBytesRead, packetSize - totalBytesRead, SocketFlags.None);
                totalBytesRead += bytesRead;
            }

            return message;
        }
    }
}
