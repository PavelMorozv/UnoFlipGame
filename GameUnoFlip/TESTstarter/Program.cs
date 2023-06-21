using GameCore.Classes;
using GameCore.Enums;
using GameCore.Structs;
using Network;

namespace TESTstarter
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Введите количество запускаемых проектов: ");
            int count = int.Parse(Console.ReadLine());

            List<Task> tasks = new List<Task>();

            for (int i = 0; i < count; i++)
            {
                string name = $"Client owner{i + 1}";
                bool isOwnerRoom = true;
                int roomId = i + 1;

                Task task = Task.Run(() =>
                {
                    EmilatorClient client = new EmilatorClient(name, isOwnerRoom, roomId);
                    client.Worker().GetAwaiter().GetResult();
                });

                tasks.Add(task);
            }

            await Task.Delay(2000);

            for (int i = 0; i < count; i++)
            {
                string name = $"Client{i + 1}";
                bool isOwnerRoom = false;
                int roomId = i + 1;

                Task task = Task.Run(() =>
                {
                    EmilatorClient client = new EmilatorClient(name, isOwnerRoom, roomId);
                    client.Worker().GetAwaiter().GetResult();
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
            Console.ReadKey();
        }
    }

    public class EmilatorClient
    {
        private bool _isOwnerRoom;
        private string _name;
        private int _roomId;
        private Client client;
        private PlayerState player = new PlayerState() { Id = -1, IsGive = false, IsUno = false, Cards = new List<Card>() };
        private GameState gameState = new GameState() { CurrentPlayer = -1 };
        private Task task;
        private readonly object lockClient = new object();


        public EmilatorClient(string name, bool isOwnerRoom, int roomId)
        {
            _name = name;
            _isOwnerRoom = isOwnerRoom;
            _roomId = roomId;
            client = new Client("109.195.67.94", 9999);


            Console.WriteLine($"{name} - {roomId}");
            Thread.Sleep(200);

            task = new Task(async () =>
            {
                await Worker();
            });
        }

        public async Task Start()
        {
            if (_isOwnerRoom)
            {
                await client.SendAsync(new Packet()
                    .Add(Property.Type, PacketType.Connect)
                    .Add(Property.Data, ""));

                await Task.Delay(500);

                await client.SendAsync(new Packet()
                    .Add(Property.Type, PacketType.Request)
                    .Add(Property.TargetModule, "RoomsModule")
                    .Add(Property.Method, "create")
                    .Add(Property.Data, "Test Room # " + _roomId));
            }
            else
            {
                await client.SendAsync(new Packet()
                    .Add(Property.Type, PacketType.Connect)
                    .Add(Property.Data, ""));

                await client.SendAsync(new Packet()
                    .Add(Property.Type, PacketType.Request)
                    .Add(Property.TargetModule, "RoomsModule")
                    .Add(Property.Method, "join")
                    .Add(Property.Data, _roomId));
            }
        }

        private void OnRecive(Packet packet)
        {
            switch (packet.Get<string>(Property.Method))
            {
                case "GameInit":
                    Console.WriteLine($"[{_name}] Получил пакет инициализации");
                    gameState = packet.Get<GameState>(Property.Data);
                    break;

                case "GameState":
                    gameState = packet.Get<GameState>(Property.Data);
                    if (gameState.CurrentPlayer == player.Id && gameState.Status == GameStatus.InProcess)
                    {
                        CalculateMove();
                    }
                    if (gameState.Status == GameStatus.EndGame)
                    {
                        Console.WriteLine($"[{_name}] Игра завершилась");
                        client.Disconnect();
                    }
                    break;

                case "AddCard":
                    player.Cards.Add(packet.Get<Card>(Property.Data));
                    Console.WriteLine($"[{_name}] Получил карту");
                    break;

                case "ChangeUno":
                    break;

                case "AddCards":
                    player.Cards.AddRange(packet.Get<List<Card>>(Property.Data));
                    Console.WriteLine($"[{_name}] Получил несколько карт");
                    break;

                case "RemoveCard":
                    var rescard = packet.Get<Card>(Property.Data);
                    var removeCard = player.Cards.First(c => c.Id == rescard.Id);
                    player.Cards.Remove(removeCard);
                    break;

                case "playerState":
                    player = packet.Get<PlayerState>(Property.Data);
                    break;
            }
        }

        public async Task Worker()
        {
            await Start();
            while (client.Connected)
            {
                if (client.Available())
                {
                    var packet = await client.ReadAsync();
                    await Task.Run(() => OnRecive(packet));
                }
                await Task.Delay(100);
            }
        }

        async Task CalculateMove()
        {
            var r = new Random();
            var cardisMove = player.Cards.Where(c => Game.IsMovePosible(gameState.LastCardPlayed, c, gameState.Side)).ToList();

            await Task.Delay(100);
            Card cardmove;
            if (cardisMove.Count > 0)
            {
                cardmove = cardisMove[r.Next(cardisMove.Count)];

                if (cardmove.Action(Side.Light).ToString().Contains("Wild") && player.Cards.Count > 1)
                {
                    cardmove.SetColor(Side.Light, player.Cards
                        .Where(c => c != cardmove)
                        .GroupBy(c => c.Color(Side.Light))
                        .ToDictionary(gr => gr.Key, gr => gr.Count())
                        .OrderByDescending(gr => gr.Value)
                        .Select(gr => gr.Key)
                        .FirstOrDefault());

                    Console.WriteLine($"[{_name}] Выбрал цвет {cardmove.Color(Side.Light)}");
                }
                else if (cardmove.Action(Side.Dark).ToString().Contains("Wild") && player.Cards.Count > 1)
                {
                    cardmove.SetColor(Side.Dark, player.Cards
                        .Where(c => c != cardmove)
                        .GroupBy(c => c.Color(Side.Dark))
                        .ToDictionary(gr => gr.Key, gr => gr.Count())
                        .OrderByDescending(gr => gr.Value)
                        .Select(gr => gr.Key)
                        .FirstOrDefault());

                    Console.WriteLine($"[{_name}] Выбрал цвет {cardmove.Color(Side.Dark)}");
                }

                await client.SendAsync(new Packet()
                    .Add(Property.Type, PacketType.Response)
                    .Add(Property.TargetModule, "GamesModule")
                    .Add(Property.Method, "move")
                    .Add(Property.Data, cardmove));

                Console.WriteLine($"[{_name}] Сделал ход картой {cardmove.ToString(gameState.Side)}");
            }
            else
            {
                if (player.IsGive) return;
                await client.SendAsync(new Packet()
                    .Add(Property.Type, PacketType.Response)
                    .Add(Property.TargetModule, "GamesModule")
                    .Add(Property.Method, "giveCard"));
            }
        }
    }
}