using System.Net.Sockets;
using System.Net;
using Network;
using ServerLib.GameContent;
using Microsoft.EntityFrameworkCore;
using GameCore.Classes;

namespace ServerLib.ServerModules
{
    public class NetworkModule : IModule
    {

        private AppDBContext dbContext;

        /// <summary>
        /// Слушатель подключений
        /// </summary>
        private TcpListener _listener;

        /// <summary>
        /// Список подключенных клиентов
        /// </summary>
        private List<Client> _clients = new List<Client>();

        /// <summary>
        /// Флаг определяющий работу сервера
        /// </summary>
        private bool isRun = false;

        /// <summary>
        /// отдельный поток слушателя, для приема подключений и сообщений, а также отключения клиентов
        /// </summary>
        private Task? serverTask;

        /// <summary>
        /// Происходит когда от клиента приходит сообщение
        /// </summary>
        public event Action<Client, Packet>? OnClientReciveMessage;

        /// <summary>
        /// Происходит когда потеряно соединение с клиентом
        /// </summary>
        public event Action<Client>? OnClientDisconnected;

        /// <summary>
        /// Происходит когда с клиентом установленно соединение
        /// </summary>
        public event Action<Client>? OnClientConnected;

        /// <summary>
        /// Инициализирует адрес сервера 0.0.0.0:9999
        /// </summary>
        public NetworkModule()
        {
            _listener = new TcpListener(IPAddress.Any, 9999);
        }

        /// <summary>
        /// Инициалиирует сервер по адресу 0.0.0.0 на переданном порту
        /// </summary>
        /// <param name="port">Порт</param>
        public NetworkModule(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        /// <summary>
        /// Инициалиирует сервер по переданным IP и порту
        /// </summary>
        /// <param name="ip">IP-адресс в виде "127.0.0.1"</param>
        /// <param name="port">порт</param>
        public NetworkModule(string ip, int port)
        {
            _listener = new TcpListener(IPAddress.Parse(ip), port);
        }

        public void Initialize()
        {
            dbContext = ModuleManager.GetModule<DBModule>().AppDBContext;

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

        /// <summary>
        /// Подключает ожидающих клиентов
        /// </summary>
        private void AcceptClients()
        {
            while (_listener.Pending())
            {
                var client = new Client(_listener.AcceptTcpClient());
                Console.WriteLine("[NetworkModule] Клиент {0} подключен его идентификатор {1}", client.RemoteEndPoint(), client.ConnectedID);
                if (dbContext.Users.FirstOrDefault(u => u.Id == client.ConnectedID) == null)
                {
                    dbContext.Users.Add(new t_User() { Login = "Client " + client.ConnectedID });
                    dbContext.SaveChanges();
                }

                _clients.Add(client);
                OnClientConnected?.Invoke(client);
            }
        }

        /// <summary>
        /// Прослушивает сообщения от клиентов
        /// </summary>
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

                    OnClientReciveMessage?.Invoke(client, pkg);
                }
            }
        }

        /// <summary>
        /// Проверяет клиентов на соединение с сервером
        /// </summary>
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
