using System;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;

namespace Network
{
    public class Client : IDisposable
    {
        private static int ClientCounts = 0;
        private TcpClient? _client;
        private readonly string _serverAddress = string.Empty;
        private readonly int _serverPort = 0;
        private bool _connected;
        private bool _disposed;
        private readonly object _ClientLock = new object();

        public Action<int> onAssignedConnectionId;

        public int ConnectedID { get; private set; } = -1;

        public bool Connected
        {
            get
            {
                return _connected;
            }
        }

        public Client(string serverAddress, int serverPort)
        {
            _serverAddress = serverAddress;
            _serverPort = serverPort;
            Connect();
        }

        public Client(TcpClient tcpClient)
        {
            ConnectedID = ++ClientCounts;

            _connected = true;
            _client = tcpClient;
            Send(new Packet().Add(Property.Type, PacketType.Response)
                             .Add(Property.Method, "ConnectionIdAssigned")
                             .Add(Property.Data, ConnectedID));
        }

        public EndPoint? RemoteEndPoint()
        {
            lock (_ClientLock)
            {
                return _client?.Client.RemoteEndPoint;
            }
        }

        public void Connect()
        {
            try
            {
                _client = new TcpClient(_serverAddress, _serverPort);
                _connected = true;
            }
            catch (Exception)
            {
                _connected = false;
            }
        }

        public void Disconnect()
        {
            try
            {
                lock (_ClientLock)
                {
                    if (_client != null)
                    {
                        Packet packet = new Packet().Add(Property.Type, PacketType.Disconnect);
                        Send(packet);
                        _client.GetStream().Flush();
                        _client.Close();
                        _client.Dispose();
                        _client = null;
                    }
                    _connected = false;
                }
            }
            catch (Exception)
            {
                // Обработка исключения отключения
            }
        }

        /// <summary>
        /// Отправляетпереданный пакет в выходной поток
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public bool Send(Packet packet)
        {
            try
            {
                // Преобразование пакета в байтовый массив
                byte[] packetData = packet.Serialize();

                // Отправка пакета с использованием метода SendMessage
                NetworkUtils.SendMessage(_client.Client, packetData);

                return true;
            }
            catch (Exception ex)
            {
                // Обработка ошибок при отправке пакета
                //Console.WriteLine($"Error writing packet: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Читает сообщение из входного потока
        /// </summary>
        /// <returns>Возвращает данные в виде пакета</returns>
        public Packet Read()
        {
            try
            {
                // Чтение пакета с использованием метода ReceiveMessage
                byte[] packetData = NetworkUtils.ReceiveMessage(_client.Client);

                // Преобразование байтового массива в пакет
                Packet packet = Packet.Deserialize(packetData);

                if (packet.Get<string>(Property.Method) == "ConnectionIdAssigned")
                {
                    ConnectedID = packet.Get<int>(Property.Data);
                    onAssignedConnectionId?.Invoke(ConnectedID);
                }

                return packet;
            }
            catch (Exception ex)
            {
                // Обработка ошибок при чтении пакета
                //Console.WriteLine($"Error reading packet: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> SendAsync(Packet packet)
        {
            try
            {
                byte[] packetData = packet.Serialize();
                await _client.GetStream().WriteAsync(packetData, 0, packetData.Length);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing packet: {ex.Message}");
                return false;
            }
        }

        public async Task<Packet> ReadAsync()
        {
            try
            {
                byte[] packetData = new byte[4];
                await _client.GetStream().ReadAsync(packetData, 0, 4);
                int packetLength = BitConverter.ToInt32(packetData, 0);

                byte[] data = new byte[packetLength];
                await _client.GetStream().ReadAsync(data, 0, packetLength);

                Packet packet = Packet.Deserialize(data);
                return packet;
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error reading packet: {ex.Message}");
                return null;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Disconnect();
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        public bool Available()
        {
            lock (_ClientLock)
            {
                if (!_connected) return false;

                try
                {
                    if (_client.Available < 4) return false;
                }
                catch (Exception)
                {
                    _connected = false;
                    return false;
                }

                return true;
            }
        }

        ~Client()
        {
            Dispose();
        }
    }
}
