using GameCore.Classes;
using GameCore.Structs;
using Microsoft.EntityFrameworkCore;
using Network;
using ServerLib.GameContent;

namespace ServerLib.ServerModules
{
    public class GamesModule : IModule
    {
        public string Name { get; set; }

        private LinkedList<Game>? games;
        private LinkedList<Player>? players;
        private NetworkModule? networkModule;
        private RoomsModule? roomsModule;
        private DBModule? DBM;
        private AppDBContext dbContext;

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
            DBM = ModuleManager.GetModule<DBModule>();
            Console.WriteLine($"[{Name}] Инициализация завершена");
        }
        public void Update()
        {

        }
        public void Shutdown()
        {

        }


        public int GetIdNewGame(int id, List<Client> clients)
        {
            var cards = DBM.AppDBContext.Cards.Include(c => c.Ligth).Include(c => c.Dark).Select(c => c.ToCard()).ToList();

            Game game = new Game(id, cards);

            foreach (var client in clients)
            {
                var player = new Player(client.ConnectedID, game);
                player.OnAddCard += Player_OnAddCard;
                player.OnAddRangeCard += Player_OnAddRangeCard;
                player.OnRemoveCard += Player_OnRemoveCard;
                player.OnChangeUno += Player_OnChangeUno;

                players.AddLast(player);
                game.AddPlayer(player);

                var pkg = new Packet().Add(Property.Type, PacketType.Request)
                    .Add(Property.TargetModule, Name)
                    .Add(Property.Method, "playerState")
                    .Add(Property.Data, player.GetState());

                clients.ElementAt(game.GetPlayerId(player)).Send(pkg);
            }

            games.AddLast(game);

            foreach (var p in game.GetPlayers())
            {
                var pkg = new Packet().Add(Property.Type, PacketType.Request)
                        .Add(Property.TargetModule, Name)
                        .Add(Property.Method, "GameInit")
                        .Add(Property.Data, p.Game.GetState());

                clients.FirstOrDefault(c => c.ConnectedID == p.Id).Send(pkg);
            }

            game.Start();

            foreach (var p in game.GetPlayers())
            {
                var pkg = new Packet().Add(Property.Type, PacketType.Request)
                    .Add(Property.TargetModule, Name)
                    .Add(Property.Method, "GameState")
                    .Add(Property.Data, p.Game.GetState());

                clients.ElementAt(game.GetPlayerId(p)).Send(pkg);
            }

            return game.Id;
        }
        public Game GetGame(int id)
        {
            return games.FirstOrDefault(g => g.Id == id);
        }
        public void DeleteGame(int gameid)
        {
            var game = games.FirstOrDefault(g => g.Id == gameid);
            foreach (var player in game.GetPlayers())
            {
                players.Remove(player);
            }
            games.Remove(game);
        }


        private async void NetworkModule_OnClientReciveMessage(Client client, Packet packet)
        {
            if (packet.Get<string>(Property.TargetModule) != Name) return;


            int pId = -1;
            if (players.Any((p) => { if (p.Id == client.ConnectedID) { pId = p.Id; return true; } else return false; }))
            {
                var p = players.FirstOrDefault((p) => p.Id == client.ConnectedID);

                switch (packet.Get<string>(Property.Method))
                {
                    case "move":
                        {
                            var tempCard = packet.Get<Card>(Property.Data);
                            p = players.FirstOrDefault((p) => p.Id == client.ConnectedID);

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
                    var pkg = new Packet().Add(Property.Type, PacketType.Request)
                        .Add(Property.TargetModule, Name)
                        .Add(Property.Method, "GameState")
                        .Add(Property.Data, player.Game.GetState());

                    if (players.Contains(player))
                        await roomsModule.GetClientsById(player.Game.Id, player.Id).SendAsync(pkg);
                }
            }
        }
        private void Player_OnAddCard(Player player, Card card)
        {
            var pkg = new Packet().Add(Property.Type, PacketType.Request)
                .Add(Property.TargetModule, Name)
                .Add(Property.Method, "AddCard")
                .Add(Property.Data, card);

            roomsModule.GetClientsById(player.Game.Id, player.Id).Send(pkg);
        }
        private void Player_OnAddRangeCard(Player player, List<Card> cards)
        {
            var pkg = new Packet().Add(Property.Type, PacketType.Request)
                .Add(Property.TargetModule, Name)
                .Add(Property.Method, "AddCards")
                .Add(Property.Data, cards);

            roomsModule.GetClientsById(player.Game.Id, player.Id).Send(pkg);
        }
        private void Player_OnRemoveCard(Player player, Card card)
        {
            var pkg = new Packet().Add(Property.Type, PacketType.Request)
                .Add(Property.TargetModule, Name)
                .Add(Property.Method, "RemoveCard")
                .Add(Property.Data, card);

            roomsModule.GetClientsById(player.Game.Id, player.Id).Send(pkg);
        }
        private void Player_OnChangeUno(Player player)
        {
            var pkg = new Packet().Add(Property.Type, PacketType.Request)
                .Add(Property.TargetModule, Name)
                .Add(Property.Method, "ChangeUno")
                .Add(Property.Data, player.IsUno);

            roomsModule.GetClientsById(player.Game.Id, player.Id).Send(pkg);
        }
    }

}
