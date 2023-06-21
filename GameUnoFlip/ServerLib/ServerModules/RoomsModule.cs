using Network;

namespace ServerLib.ServerModules
{
    public class RoomsModule : IModule
    {
        public string Name { get; private set; }

        private NetworkModule networkModule;
        private GamesModule gamesModule;
        private List<Room> rooms;

        readonly object lockRoom = new object();


        public RoomsModule(string name)
        {
            Name = name;
        }

        private void NetworkModule_onClientReciveMessage(Client client, Packet packet)
        {
            if (packet.Get<string>(Property.TargetModule) != Name) return;
            Room room;
            lock (lockRoom)
            {
                switch (packet.Get<string>(Property.Method))
                {
                    case "create":
                        {
                            room = new Room(packet.Get<string>(Property.Data), client);
                            room.Clients.Add(client);
                            rooms.Add(room);
                            client.Send(new Packet()
                                .Add(Property.Type, PacketType.Response)
                                .Add(Property.TargetModule, Name)
                                .Add(Property.Method, packet.Get<string>(Property.Method))
                                .Add(Property.Data, true));

                            Console.WriteLine($"[{Name}] Клиент {client.ConnectedID} создал комнату: {room.Id}:{room.Name}");
                            break;
                        }

                    case "join":
                        {
                            if (rooms.Any((x) => x.Id == packet.Get<int>(Property.Data)))
                            {
                                room = rooms.Where((x) => x.Id == packet.Get<int>(Property.Data)).FirstOrDefault();
                                if (room.Clients.Count != 2)
                                {
                                    room.Clients.Add(client);
                                    client.Send(new Packet()
                                        .Add(Property.Type, PacketType.Response)
                                        .Add(Property.TargetModule, Name)
                                        .Add(Property.Method, packet.Get<string>(Property.Method))
                                        .Add(Property.Data, true));

                                    Console.WriteLine($"[{Name}] Клиент {client.ConnectedID} присоеденился к комнате: {room.Name}");
                                    if (room.Clients.Count == 2)
                                    {
                                        room.GameId = gamesModule.GetIdNewGame(room.Id, room.Clients);
                                    }
                                    break;
                                }
                            }

                            client.Send(new Packet()
                                .Add(Property.Type, PacketType.Response)
                                .Add(Property.TargetModule, Name)
                                .Add(Property.Method, packet.Get<string>(Property.Method))
                                .Add(Property.Data, false));
                            break;
                        }

                    case "leave":
                        {
                            int i = -1;
                            if (rooms.Any((x) => { if (x.Clients.Contains(client)) { i = x.Id; return true; } else { return false; }; }))
                            {
                                room = rooms.FirstOrDefault((x) => x.Id == i);
                                room.Clients.Remove(client);
                                client.Send(new Packet()
                                    .Add(Property.Type, PacketType.Response)
                                    .Add(Property.Method, packet.Get<string>(Property.Method))
                                    .Add(Property.TargetModule, Name)
                                    .Add(Property.Data, true));

                                if (room.Owner == client)
                                {
                                    if (room.Clients.Count > 0) room.Owner = room.Clients.FirstOrDefault();
                                }

                                Console.WriteLine($"[{Name}] Клиент {client.ConnectedID} Вышел из комнаты: {room.Name}");
                            }
                            else
                            {
                                client.Send(new Packet()
                                    .Add(Property.Type, PacketType.Response)
                                    .Add(Property.TargetModule, Name)
                                    .Add(Property.Method, packet.Get<string>(Property.Method))
                                    .Add(Property.Data, false));
                            }

                            break;
                        }

                    case "getList":
                        {
                            client.Send(new Packet()
                                .Add(Property.Type, PacketType.Response)
                                .Add(Property.Method, packet.Get<string>(Property.Method))
                                .Add(Property.TargetModule, Name)
                                .Add(Property.Data, rooms.Select(r => r.ToString()).ToArray()));

                            Console.WriteLine($"[{Name}] Клиент {client.ConnectedID} запросил список комнат");
                            break;
                        }

                    case "message":
                        {
                            int i = -1;
                            if (rooms.Any((x) => { if (x.Clients.Contains(client)) { i = x.Id; return true; } else { return false; }; }))
                            {
                                room = rooms.FirstOrDefault((x) => x.Id == i);
                                var newPacket = new Packet()
                                    .Add(Property.Type, PacketType.Response)
                                    .Add(Property.TargetModule, Name)
                                    .Add(Property.Method, packet.Get<string>(Property.Method))
                                    .Add(Property.Data, packet.Get<string>(Property.Data));

                                foreach (var c in room.Clients)
                                {
                                    c.Send(newPacket);
                                }

                                Console.WriteLine($"[{Name}] Клиент {client.ConnectedID} отправил сообщение в комнате: {room.Name}");
                            }
                            else
                            {
                                client.Send(new Packet()
                                    .Add(Property.Type, PacketType.Response)
                                    .Add(Property.TargetModule, Name)
                                    .Add(Property.Method, packet.Get<string>(Property.Method))
                                    .Add(Property.Error, $"Error: На сервере нет конат в которых вы состоите!"));
                            }

                            break;
                        }

                    default:
                        client.Send(new Packet().Add(Property.Type, PacketType.Response)
                            .Add(Property.TargetModule, Name)                                    
                            .Add(Property.Method, packet.Get<string>(Property.Method))
                            .Add(Property.Error, $"Error: В модуле {Name} нет имеет метода {packet.Get<string>(Property.Method)}!"));

                        Console.WriteLine($"[{Name}] Клиент {client.ConnectedID} пытается выполнить несуществующую команду");
                        break;
                }
            }
        }
        private void NetworkModule_onClientDisconnected(Client client)
        {

        }


        public void Initialize()
        {
            Console.WriteLine($"[{Name}] Инициализация...");
            networkModule = ModuleManager.GetModule<NetworkModule>();
            networkModule.OnClientReciveMessage += NetworkModule_onClientReciveMessage;
            networkModule.OnClientDisconnected += NetworkModule_onClientDisconnected;

            gamesModule = ModuleManager.GetModule<GamesModule>();
            lock (lockRoom)
            {
                rooms = new List<Room>();
            }

            Console.WriteLine($"[{Name}] Инициализация завершена");
        }
        public void Update()
        {
            lock (lockRoom)
            {
                var room = rooms.FirstOrDefault(r => r.GameId != null && gamesModule.GetGame((int)r.GameId).GetState().Status == GameCore.Enums.GameStatus.EndGame);
                if (room?.Id > -1)
                {
                    gamesModule.DeleteGame(room.Id);
                    rooms.Remove(room);
                }
            }
        }
        public void Shutdown()
        {

        }


        public Client GetClientsById(int roomId, int playerId)
        {
            return rooms.FirstOrDefault(r => r.Id == roomId).Clients.FirstOrDefault(c => c.ConnectedID == playerId);
        }

    }

    [Serializable]
    public enum RoomState
    {
        Waiting
    }

    [Serializable]
    public class Room
    {
        private static int RoomsCount = 1;
        public int Id { get; private set; } = -1;
        public string? Name { get; private set; }
        public Client? Owner { get; set; }
        public List<Client>? Clients { get; private set; }
        public int? GameId { get; set; }

        public Room() { }
        public Room(string name, Client owner = null)
        {
            Id = RoomsCount++;
            Name = name;
            Owner = owner;
            Clients = new List<Client>();
        }

        public override string ToString()
        {
            return $"{Id}|{Name}|{Clients?.Count}";
        }
    }
}