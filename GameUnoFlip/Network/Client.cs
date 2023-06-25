using System;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;

namespace Network
{
    public class Client : IDisposable
    {
        private TcpClient? _client;
        private readonly string _serverAddress = string.Empty;
        private readonly int _serverPort = 0;
        private bool _connected;
        private bool _disposed;
        private readonly object _ClientReadLock = new object();
        private readonly object _ClientWriteLock = new object();



        public Action<int> onAssignedConnectionId;
        public int ConnectedID { get; set; } = -1;
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
            _connected = true;
            _client = tcpClient;
            Send(new Packet().Add(Property.Type, PacketType.Response)
                             .Add(Property.Method, "ConnectionIdAssigned")
                             .Add(Property.Data, ConnectedID));
        }


        public EndPoint? RemoteEndPoint()
        {
            return _client?.Client.RemoteEndPoint;
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
        public bool Available()
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
        public void Disconnect()
        {
            try
            {
                lock (_ClientWriteLock)
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
        ~Client()
        {
            Dispose();
        }


        public bool Send(Packet packet)
        {

            lock (_ClientWriteLock)
            {
                try
                {
                    byte[] packetData = packet.Serialize();
                    NetworkUtils.SendMessage(_client, packetData);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
        public Packet Read()
        {
            lock (_ClientReadLock)
            {
                try
                {
                    byte[] packetData = NetworkUtils.ReceiveMessage(_client);
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
                    return null;
                }
            }
        }
        public async Task<bool> SendAsync(Packet packet)
        {
            try
            {
                byte[] packetData = packet.Serialize();
                await NetworkUtils.SendMessageAsync(_client, packetData);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<Packet> ReadAsync()
        {
            try
            {
                byte[] packetData = await NetworkUtils.ReceiveMessageAsync(_client);
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
                return null;
            }
        }
    }
}
