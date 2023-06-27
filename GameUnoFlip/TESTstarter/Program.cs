using GameCore.Classes;
using GameCore.Enums;
using GameCore.Structs;
using Network;

namespace TESTstarter
{
    public class Program
    {
        static void Main(string[] args)
        {
            #region Для тестирования создания и входа в комнату
            Console.Write("Введите диапазон: \na: ");
            int a = int.Parse(Console.ReadLine());
            Console.Write("b: ");
            int b = int.Parse(Console.ReadLine());


            List<Task> tasks = new List<Task>();

            for (int i = a; i < b; i++)
            {
                string name = $"Client owner{i + 1}";
                bool isOwnerRoom = true;
                int roomId = i + 1;

                Task task = new Task(() =>
                {
                    EmilatorClient client = new EmilatorClient(name, isOwnerRoom, roomId);
                    client.Worker();
                });
                task.Start();

                tasks.Add(task);
            }

            Thread.Sleep(8000);

            for (int i = a; i < b; i++)
            {
                string name = $"Client{i + 1}";
                bool isOwnerRoom = false;
                int roomId = i + 1;

                Thread task = new Thread(() =>
                {
                    EmilatorClient client = new EmilatorClient(name, isOwnerRoom, roomId);
                    client.Worker();
                });
                task.Start();
            }

            tasks.ForEach(t => t.Wait());
            Console.Write("Конец");
            Console.ReadLine();
            #endregion
        }
    }

    #region Для тестирования создания и входа в комнату
    public class EmilatorClient
    {
        private bool _isOwnerRoom;
        private string _name;
        private int _roomId;
        private Client client;
        private PlayerState player = new PlayerState() { Id = -1, IsGive = false, IsUno = false, Cards = new List<Card>() };
        private GameState gameState = new GameState() { CurrentPlayer = -1 };

        List<double> ping = new List<double>();
        DateTime lastPing;

        public EmilatorClient(string name, bool isOwnerRoom, int roomId)
        {
            _name = name;
            _isOwnerRoom = isOwnerRoom;
            _roomId = roomId;
            client = new Client("109.195.67.94", 9999);


            Console.WriteLine($"{name} - {roomId}");
        }

        public void Start()
        {
            if (_isOwnerRoom)
            {
                client.Send(new Packet()
                    .Add(Property.Type, PacketType.Connect)
                    .Add(Property.Data, ""));

                Thread.Sleep(200);

                client.Send(new Packet()
                   .Add(Property.Type, PacketType.Request)
                   .Add(Property.TargetModule, "RoomsModule")
                   .Add(Property.Method, "create")
                   .Add(Property.Data, "Test Room # " + _roomId));
            }
            else
            {
                client.Send(new Packet()
                    .Add(Property.Type, PacketType.Connect)
                    .Add(Property.Data, "")); ;

                Thread.Sleep(200);

                client.Send(new Packet()
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
                    lastPing = DateTime.Now;
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
                        var averagePing = ping.Average();

                        Console.WriteLine($"[{_name}] Игра завершилась. средний пинг: {averagePing} мс");
                        client.Disconnect();
                    }
                    break;

                case "AddCard":
                    player.Cards.Add(packet.Get<Card>(Property.Data));
                    Console.WriteLine($"[{_name}] Получил карту");
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
                case "move":
                case "giveCard":
                    ping.Add((DateTime.Now - lastPing).Milliseconds);
                    break;
            }
        }

        public void Worker()
        {
            Start();
            while (client.Connected)
            {
                if (client.Available())
                {
                    var packet = client.Read();
                    OnRecive(packet);
                }
            }
        }

        private void CalculateMove()
        {
            var r = new Random();
            var cardisMove = player.Cards.Where(c => Game.IsMovePosible(gameState.LastCardPlayed, c, gameState.Side)).ToList();

            Card cardmove;
            if (cardisMove.Count > 0)
            {
                cardmove = cardisMove[r.Next(cardisMove.Count)];

                if (cardmove.Action(Side.Light).ToString().Contains("Wild") && player.Cards.Count > 1)
                {
                    cardmove.SetColor(Side.Light, (Color)r.Next(0, 4));

                    Console.WriteLine($"[{_name}] Выбрал цвет {cardmove.Color(Side.Light)}");
                }
                else if (cardmove.Action(Side.Dark).ToString().Contains("Wild") && player.Cards.Count > 1)
                {
                    cardmove.SetColor(Side.Dark, (Color)r.Next(4, 8));

                    Console.WriteLine($"[{_name}] Выбрал цвет {cardmove.Color(Side.Dark)}");
                }

                client.Send(new Packet()
                    .Add(Property.Type, PacketType.Response)
                    .Add(Property.TargetModule, "GamesModule")
                    .Add(Property.Method, "move")
                    .Add(Property.Data, cardmove));
                lastPing = DateTime.Now;

                Console.WriteLine($"[{_name}] Сделал ход картой {cardmove.ToString(gameState.Side)}");
            }
            else
            {
                if (player.IsGive) return;
                client.Send(new Packet()
                    .Add(Property.Type, PacketType.Response)
                    .Add(Property.TargetModule, "GamesModule")
                    .Add(Property.Method, "giveCard"));
                lastPing = DateTime.Now;
            }
        }
    }
    #endregion


    //public class EmilatorClient
    //{
    //    private string _name;
    //    private Client client;
    //    private PlayerState player = new PlayerState() { Id = -1, IsGive = false, IsUno = false, Cards = new List<Card>() };
    //    private GameState gameState = new GameState() { CurrentPlayer = -1 };
    //    private Task task;
    //    private readonly object lockClient = new object();


    //    public EmilatorClient(string name)
    //    {
    //        _name = name;
    //        client = new Client("109.195.67.94", 9999);


    //        Console.WriteLine($"{name}");
    //        Thread.Sleep(200);

    //        task = new Task(async () =>
    //        {
    //            await Worker();
    //        });
    //    }

    //    public async Task Start()
    //    {
    //        await client.SendAsync(new Packet()
    //            .Add(Property.Type, PacketType.Connect)
    //            .Add(Property.Data, ""));

    //        await Task.Delay(500);

    //        await client.SendAsync(new Packet()
    //            .Add(Property.Type, PacketType.Request)
    //            .Add(Property.TargetModule, "RoomsModule")
    //            .Add(Property.Method, "FastGame"));

    //    }

    //    private void OnRecive(Packet packet)
    //    {
    //        switch (packet.Get<string>(Property.Method))
    //        {
    //            case "GameInit":
    //                Console.WriteLine($"[{_name}] Получил пакет инициализации");

    //                gameState = packet.Get<GameState>(Property.Data);
    //                break;

    //            case "GameState":
    //                gameState = packet.Get<GameState>(Property.Data);
    //                if (gameState.CurrentPlayer == player.Id && gameState.Status == GameStatus.InProcess)
    //                {
    //                    CalculateMove();
    //                }
    //                if (gameState.Status == GameStatus.EndGame)
    //                {
    //                    Console.WriteLine($"[{_name}] Игра завершилась");
    //                    client.Disconnect();
    //                }
    //                break;

    //            case "AddCard":
    //                player.Cards.Add(packet.Get<Card>(Property.Data));
    //                Console.WriteLine($"[{_name}] Получил карту");
    //                break;

    //            case "ChangeUno":
    //                break;

    //            case "AddCards":
    //                player.Cards.AddRange(packet.Get<List<Card>>(Property.Data));
    //                Console.WriteLine($"[{_name}] Получил несколько карт");
    //                break;

    //            case "RemoveCard":
    //                var rescard = packet.Get<Card>(Property.Data);
    //                var removeCard = player.Cards.First(c => c.Id == rescard.Id);
    //                player.Cards.Remove(removeCard);
    //                break;

    //            case "playerState":
    //                player = packet.Get<PlayerState>(Property.Data);
    //                break;
    //        }
    //    }

    //    public async Task Worker()
    //    {
    //        await Start();
    //        while (client.Connected)
    //        {
    //            if (client.Available())
    //            {
    //                var packet = await client.ReadAsync();
    //                await Task.Run(() => OnRecive(packet));
    //            }
    //            await Task.Delay(100);
    //        }
    //    }

    //    async Task CalculateMove()
    //    {
    //        var r = new Random();
    //        var cardisMove = player.Cards.Where(c => Game.IsMovePosible(gameState.LastCardPlayed, c, gameState.Side)).ToList();

    //        await Task.Delay(100);
    //        Card cardmove;
    //        if (cardisMove.Count > 0)
    //        {
    //            cardmove = cardisMove[r.Next(cardisMove.Count)];

    //            if (cardmove.Action(Side.Light).ToString().Contains("Wild") && player.Cards.Count > 1)
    //            {
    //                cardmove.SetColor(Side.Light, player.Cards
    //                    .Where(c => c != cardmove)
    //                    .GroupBy(c => c.Color(Side.Light))
    //                    .ToDictionary(gr => gr.Key, gr => gr.Count())
    //                    .OrderByDescending(gr => gr.Value)
    //                    .Select(gr => gr.Key)
    //                    .FirstOrDefault());

    //                Console.WriteLine($"[{_name}] Выбрал цвет {cardmove.Color(Side.Light)}");
    //            }
    //            else if (cardmove.Action(Side.Dark).ToString().Contains("Wild") && player.Cards.Count > 1)
    //            {
    //                cardmove.SetColor(Side.Dark, player.Cards
    //                    .Where(c => c != cardmove)
    //                    .GroupBy(c => c.Color(Side.Dark))
    //                    .ToDictionary(gr => gr.Key, gr => gr.Count())
    //                    .OrderByDescending(gr => gr.Value)
    //                    .Select(gr => gr.Key)
    //                    .FirstOrDefault());

    //                Console.WriteLine($"[{_name}] Выбрал цвет {cardmove.Color(Side.Dark)}");
    //            }

    //            await client.SendAsync(new Packet()
    //                .Add(Property.Type, PacketType.Response)
    //                .Add(Property.TargetModule, "GamesModule")
    //                .Add(Property.Method, "move")
    //                .Add(Property.Data, cardmove));

    //            Console.WriteLine($"[{_name}] Сделал ход картой {cardmove.ToString(gameState.Side)}");
    //        }
    //        else
    //        {
    //            if (player.IsGive) return;
    //            await client.SendAsync(new Packet()
    //                .Add(Property.Type, PacketType.Response)
    //                .Add(Property.TargetModule, "GamesModule")
    //                .Add(Property.Method, "giveCard"));
    //        }
    //    }
    //}
}