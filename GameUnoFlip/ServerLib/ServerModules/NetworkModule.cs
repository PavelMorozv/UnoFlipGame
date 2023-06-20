using System.Net.Sockets;
using System.Net;
using Network;
using GameCore.Classes;

namespace ServerLib.ServerModules
{
    public class NetworkModule : IModule
    {

        private List<Client> _clients = new List<Client>();
        private TcpListener _listener;
        private bool isRun = false;
        private Task? serverTask;
        private AuthorizationModule authModule;

        public event Action<Client, Packet>? OnClientReciveMessage;
        public event Action<Client>? OnClientDisconnected;
        public event Action<Client>? OnClientConnected;


        public NetworkModule()
        {
            _listener = new TcpListener(IPAddress.Any, 9999);
        }
        public NetworkModule(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }
        public NetworkModule(string ip, int port)
        {
            _listener = new TcpListener(IPAddress.Parse(ip), port);
        }


        public void Initialize()
        {
            authModule = ModuleManager.GetModule<AuthorizationModule>();

            List<Card> cards = DeckGenerator.GenerateRandomDeck();

            _clients = new List<Client>();

            Console.WriteLine($"[NetworkModule] Запуск инициализации");
            _listener.Start();
            isRun = true;

            serverTask = new Task(() =>
            {
                Console.WriteLine($"[NetworkModule listener] Задача слушателя запущена");
                while (isRun)
                {
                    AcceptClients();
                    ReadMessageFromClients();
                    CheckConnection();
                    Thread.Sleep(100);
                }
            });
            serverTask.Start();

            Console.WriteLine($"[NetworkModule] Инициализации завершена");
            Console.WriteLine("[NetworkModule] Сетевой модуль запущен по адресу: {0}", _listener.LocalEndpoint);
        }
        public void Update()
        {

        }
        public void Shutdown()
        {
            foreach (Client client in _clients)
            {
                client.Disconnect();
            }
            _clients.Clear();
        }

        private void AcceptClients()
        {
            while (_listener.Pending())
            {
                var client = new Client(_listener.AcceptTcpClient());
                
                _clients.Add(client);
                Console.WriteLine("[NetworkModule] Клиент {0} подключен", client.RemoteEndPoint());
                OnClientConnected?.Invoke(client);
            }
        }

        private void ReadMessageFromClients()
        {
            foreach (Client client in _clients)
            {
                if (client.Available())
                {
                    var pkg = client.Read();
                    if (pkg.Get<PacketType>(Property.Type) == PacketType.Test)
                    {
                        continue;
                    }
                    else if (pkg.Get<PacketType>(Property.Type) == PacketType.Connect) 
                    {
                        if (pkg.Get<string>(Property.Data) == null)
                        {
                            client.Disconnect();
                            continue;
                        }

                        client.ConnectedID = authModule.FastAuth(client, pkg.Get<string>(Property.Data));
                    }

                    OnClientReciveMessage?.Invoke(client, pkg);
                }
            }
        }

        private void CheckConnection()
        {
            List<Client> clientsIsLeft = new List<Client>();


            foreach (Client client in _clients)
            {
                if (!client.Connected) { clientsIsLeft.Add(client); OnClientDisconnected?.Invoke(client); break; }
                if (!client.Send(new Packet().Add(Property.Type, PacketType.Test))) clientsIsLeft.Add(client); OnClientDisconnected?.Invoke(client);
            }

            foreach (Client client in clientsIsLeft)
            {
                Console.WriteLine("[NetworkModule] Клиент {0} отключен", client.RemoteEndPoint());
                _clients.Remove(client);
                client.Dispose();
            }
        }
    }
}