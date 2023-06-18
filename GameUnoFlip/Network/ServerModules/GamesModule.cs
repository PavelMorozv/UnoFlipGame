using GameCore.Classes;
using GameCore.Structs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Network.ServerModules
{
    public class GamesModule : IModule
    {
        private LinkedList<Game> games;
        private LinkedList<Player> players;
        private NetworkModule networkModule;
        private RoomsModule roomsModule;

        private object roomsLock = new object();

        public string Name { get; set; }

        public GamesModule(string name)
        {
            Name = name;
        }

        public void Initialize()
        {
            Console.WriteLine($"[{Name}] Инициализация...");
            games = new LinkedList<Game>();
            players = new LinkedList<Player>();
            roomsModule = ModuleManager.GetModule<RoomsModule>();
            networkModule = ModuleManager.GetModule<NetworkModule>();
            networkModule.OnClientReciveMessage += NetworkModule_OnClientReciveMessage;
            Console.WriteLine($"[{Name}] Инициализация завершена");
        }

        private void NetworkModule_OnClientReciveMessage(Client client, Packet packet)
        {
            if (packet.Get<string>(Property.TargetModule) != Name) return;


            int pId = -1;
            if (players.Any((p) => { if (p.Id == client.ConnectedID) { pId = p.Id; return true; } else return false; }))
            {
                var p = players.First((p) => p.Id == client.ConnectedID);

                switch (packet.Get<string>(Property.Method))
                {
                    case "move":
                        {
                            var tempCard = packet.Get<Card>(Property.Data);
                            p = players.First((p) => p.Id == client.ConnectedID);

                            if (p.Game.Move(p.Id, tempCard))
                            {
                                Console.WriteLine("Клиент " + p.Id + " сделал ход " + tempCard.Id);
                            }

                            break;
                        }

                    case "giveCard":
                        {
                            GameState gameState = p.Game.GetState();

                            if (!p.IsGive)
                            {
                                p.Game.GetCard();
                                if (!Game.IsMovePosible(gameState.LastCardPlayed, p.Cards.Last(), gameState.Side))
                                {
                                    p.Game.ActionEndMove();
                                }
                            }
                            break;
                        }
                }

                foreach (var player in p.Game.GetPlayers())
                {
                    var pkg = new Packet()
                            .Add(Property.Type, PacketType.Request)
                            .Add(Property.TargetModule, Name)
                            .Add(Property.Method, "GameState")
                            .Add(Property.Data, player.Game.GetState());

                    if (players.Contains(player))
                        roomsModule.GetClientsById(player.Game.Id, player.Id).Send(pkg);
                }
            }

        }

        public void Shutdown()
        {

        }

        public void Update()
        {

        }

        public Game GetGame(int id)
        {
            return games.First(g => g.Id == id);
        }

        public int GetNewGameID(int id, List<Client> clients)
        {
            Game game = new Game(id);
            Player player;

            foreach (var client in clients)
            {
                player = new Player(client.ConnectedID, game);
                player.OnAddCard += Player_OnAddCard;

                player.OnAddRangeCard += Player_OnAddRangeCard;

                player.OnRemoveCard += Player_OnRemoveCard;

                player.OnChangeUno += Player_OnChangeUno;

                players.AddLast(player);
                game.AddPlayer(player);

                var pkg = new Packet()
                                .Add(Property.Type, PacketType.Request)
                                .Add(Property.TargetModule, Name)
                                .Add(Property.Method, "playerState")
                                .Add(Property.Data, player.GetState());

                clients.ElementAt(game.GetPlayerId(player)).Send(pkg);
            }

            games.AddLast(game);

            foreach (var p in game.GetPlayers())
            {
                var pkg = new Packet()
                        .Add(Property.Type, PacketType.Request)
                        .Add(Property.TargetModule, Name)
                        .Add(Property.Method, "GameInit")
                        .Add(Property.Data, p.Game.GetState());

                clients.First(c => c.ConnectedID == p.Id).Send(pkg);
            }

            game.Start();

            foreach (var p in game.GetPlayers())
            {
                var pkg = new Packet()
                        .Add(Property.Type, PacketType.Request)
                        .Add(Property.TargetModule, Name)
                        .Add(Property.Method, "GameState")
                        .Add(Property.Data, p.Game.GetState());

                clients.ElementAt(game.GetPlayerId(p)).Send(pkg);
            }

            return game.Id;
        }

        private void Player_OnAddCard(Player player, Card card)
        {
            var pkg = new Packet()
                                .Add(Property.Type, PacketType.Request)
                                .Add(Property.TargetModule, Name)
                                .Add(Property.Method, "AddCard")
                                .Add(Property.Data, card);

            roomsModule.GetClientsById(player.Game.Id, player.Id).Send(pkg);
        }

        private void Player_OnAddRangeCard(Player player, List<Card> cards)
        {
            var pkg = new Packet()
                                .Add(Property.Type, PacketType.Request)
                                .Add(Property.TargetModule, Name)
                                .Add(Property.Method, "AddCards")
                                .Add(Property.Data, cards);

            roomsModule.GetClientsById(player.Game.Id, player.Id).Send(pkg);
        }

        private void Player_OnRemoveCard(Player player, Card card)
        {
            var pkg = new Packet()
                                .Add(Property.Type, PacketType.Request)
                                .Add(Property.TargetModule, Name)
                                .Add(Property.Method, "RemoveCard")
                                .Add(Property.Data, card);

            roomsModule.GetClientsById(player.Game.Id, player.Id).Send(pkg);
        }

        private void Player_OnChangeUno(Player player)
        {
            var pkg = new Packet()
                            .Add(Property.Type, PacketType.Request)
                            .Add(Property.TargetModule, Name)
                            .Add(Property.Method, "ChangeUno")
                            .Add(Property.Data, player.IsUno);

            roomsModule.GetClientsById(player.Game.Id, player.Id).Send(pkg);
        }
    }

}
