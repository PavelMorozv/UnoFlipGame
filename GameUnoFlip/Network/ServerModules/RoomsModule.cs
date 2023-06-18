﻿using System.Collections.Generic;
using System;
using System.Linq;

namespace Network.ServerModules
{
    public class RoomsModule : IModule
    {
        private NetworkModule networkModule;
        private GamesModule gamesModule;
        private List<Room> rooms;
        readonly object lockRoom = new object();

        public string Name { get; private set; }

        public RoomsModule(string name)
        {
            Name = name;
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
                rooms = new List<Room>()
                {
                    new Room("TestRoom1"),
                    new Room("TestRoom2")
                };
            }

            Console.WriteLine($"[{Name}] Инициализация завершена");
        }

        private void NetworkModule_onClientDisconnected(Client client)
        {

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
                            client.Send(new Packet().
                                Add(Property.Type, PacketType.Response)
                                .Add(Property.TargetModule, Name)
                                .Add(Property.Method, packet.Get<string>(Property.Method))
                                .Add(Property.Data, true));

                            Console.WriteLine($"[{Name}] Клиент {client.ConnectedID} создал комнату: {room.Name}");
                            break;
                        }

                    case "join":
                        {
                            if (rooms.Any((x) => x.Id == packet.Get<int>(Property.Data)))
                            {
                                room = rooms.Where((x) => x.Id == packet.Get<int>(Property.Data)).First();
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
                                        room.GameId = gamesModule.GetNewGameID(room.Id, room.Clients);
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
                                room = rooms.First((x) => x.Id == i);
                                room.Clients.Remove(client);
                                client.Send(new Packet()
                                    .Add(Property.Type, PacketType.Response)
                                    .Add(Property.Method, packet.Get<string>(Property.Method))
                                    .Add(Property.TargetModule, Name)
                                    .Add(Property.Data, true));

                                if (room.Owner == client)
                                {
                                    if (room.Clients.Count > 0)
                                        room.Owner = room.Clients.FirstOrDefault();
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
                            //List<string> listRooms = new List<string>();

                            //foreach (var r in rooms)
                            //{
                            //    listRooms.Add($"[{r.Id}][{r.Name}][{r.Clients.Count}]");
                            //}

                            //rooms.Select(r => r.ToString()).ToArray()

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
                                room = rooms.First((x) => x.Id == i);
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
                        client.Send(new Packet()
                                        .Add(Property.Type, PacketType.Response)
                                        .Add(Property.TargetModule, Name)
                                        .Add(Property.Method, packet.Get<string>(Property.Method))
                                        .Add(Property.Error, $"Error: В модуле {Name} нет имеет метода {packet.Get<string>(Property.Method)}!"));

                        Console.WriteLine($"[{Name}] Клиент {client.ConnectedID} пытается выполнить несуществующую команду");
                        break;
                }
            }

        }

        public Client GetClientsById(int roomId, int playerId)
        {
            //findClient = rooms.SelectMany((r) => r.Clients).ToList().First((c) => c.ConnectedID == id);
            return rooms.First(r => r.Id == roomId).Clients.First(c => c.ConnectedID == playerId);
        }

        public void Shutdown()
        {

        }

        public void Update()
        {
            lock (lockRoom)
            {
                rooms.RemoveAll(r => r.GameId != null && gamesModule.GetGame((int)r.GameId).GetState().Status == GameCore.Enums.GameStatus.EndGame);
            }
        }

        private void PrintRooms()
        {
            Console.WriteLine($"- Rooms Module --------------------------");
            foreach (var r in rooms)
            {
                Console.WriteLine($"[{r.Id}][{r.Name}][{r.Clients.Count}]");
            }
            Console.WriteLine($"=========================================");
        }
    }

    [Serializable]
    public class Room
    {
        private static int RoomsCount = 0;
        public int Id { get; private set; } = -1;
        public string? Name { get; private set; }
        public Client? Owner { get; set; }
        public List<Client>? Clients { get; private set; }
        public int? GameId { get; set; }


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
            //return Regex.Unescape(JsonSerializer.Serialize(this));
        }
    }
}