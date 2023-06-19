using GameCore.Enums;
using ServerLib.GameContent;
using Action = GameCore.Enums.Action;

namespace TestDB
{
    public class Program
    {
        static AppDBContext dBContext;

        static void Main(string[] args)
        {
            dBContext = new AppDBContext();
            //CreateSides();
            //CreateCard();

            //dBContext.Sides.Select(s => s.ToString()).ToList().ForEach(s => Console.WriteLine(s));
            //dBContext.Cards.Select(c => c.Ligth.ToString()).ToList().ForEach(s => Console.WriteLine(s));

            Console.ReadLine();
        }

        /// <summary>
        /// Создает все стороны карт
        /// </summary>
        static void CreateSides()
        {
            // Создание всех светлых сторон

            List<t_Side> sides = new List<t_Side>();

            for (int i = 0; i < 4; i++)
            {
                sides.Add(new t_Side((Color)i, Action.SkipMove, -1));

                for (int j = 1; j < 10; j++)
                    sides.Add(new t_Side((Color)i, Action.Number, j));

                sides.Add(new t_Side((Color)i, Action.Give, 1));
                sides.Add(new t_Side((Color)i, Action.ChangeDirection, -1));
                sides.Add(new t_Side((Color)i, Action.Flip, -1));
            }

            sides.Add(new t_Side(Color.None, Action.Wild, -1));
            sides.Add(new t_Side(Color.None, Action.WildGive, 2));

            // Создание всех темных сторон
            for (int i = 4; i < 8; i++)
            {
                sides.Add(new t_Side((Color)i, Action.SkipMoveAll, -1));

                for (int j = 1; j < 10; j++)
                    sides.Add(new t_Side((Color)i, Action.Number, j));

                sides.Add(new t_Side((Color)i, Action.Give, 5));
                sides.Add(new t_Side((Color)i, Action.ChangeDirection, -1));
                sides.Add(new t_Side((Color)i, Action.Flip, -1));
            }

            sides.Add(new t_Side(Color.None, Action.Wild, -1));
            sides.Add(new t_Side(Color.None, Action.WildGiveForNow, -1));

            dBContext.Sides.AddRange(sides);
            dBContext.SaveChanges();
        }


        /// <summary>
        /// Обьеденяет стороны карт вкарты из оригинальной колоды
        /// </summary>
        static void CreateCard()
        {
            dBContext.Cards.AddRange(new[]
            {
                new t_Card() { LigthId = 1,   DarkId = 107},               new t_Card() { LigthId = 1,   DarkId = 78},
                new t_Card() { LigthId = 2,   DarkId = 84},                new t_Card() { LigthId = 2,   DarkId = 96},
                new t_Card() { LigthId = 3,   DarkId = 79},                new t_Card() { LigthId = 3,   DarkId = 104},
                new t_Card() { LigthId = 4,   DarkId = 88},                new t_Card() { LigthId = 4,   DarkId = 108},
                new t_Card() { LigthId = 5,   DarkId = 80},                new t_Card() { LigthId = 5,   DarkId = 104},
                new t_Card() { LigthId = 6,   DarkId = 83},                new t_Card() { LigthId = 6,   DarkId = 60},
                new t_Card() { LigthId = 7,   DarkId = 81},                new t_Card() { LigthId = 7,   DarkId = 77},
                new t_Card() { LigthId = 8,   DarkId = 99},                new t_Card() { LigthId = 8,   DarkId = 69},
                new t_Card() { LigthId = 9,   DarkId = 105},               new t_Card() { LigthId = 9,   DarkId = 62},
                new t_Card() { LigthId = 10,  DarkId = 99},                new t_Card() { LigthId = 10,  DarkId = 66},
                new t_Card() { LigthId = 11,  DarkId = 85},                new t_Card() { LigthId = 11,  DarkId = 84},
                new t_Card() { LigthId = 12,  DarkId = 97},                new t_Card() { LigthId = 12,  DarkId = 62},
                new t_Card() { LigthId = 13,  DarkId = 97},                new t_Card() { LigthId = 13,  DarkId = 89},
                new t_Card() { LigthId = 14,  DarkId = 56},                new t_Card() { LigthId = 14,  DarkId = 90},
                new t_Card() { LigthId = 15,  DarkId = 94},                new t_Card() { LigthId = 15,  DarkId = 94},
                new t_Card() { LigthId = 16,  DarkId = 76},                new t_Card() { LigthId = 16,  DarkId = 87},
                new t_Card() { LigthId = 17,  DarkId = 57},                new t_Card() { LigthId = 17,  DarkId = 102},
                new t_Card() { LigthId = 18,  DarkId = 95},                new t_Card() { LigthId = 18,  DarkId = 65},
                new t_Card() { LigthId = 19,  DarkId = 79},                new t_Card() { LigthId = 19,  DarkId = 90},
                new t_Card() { LigthId = 20,  DarkId = 105},               new t_Card() { LigthId = 20,  DarkId = 55},
                new t_Card() { LigthId = 21,  DarkId = 71},                new t_Card() { LigthId = 21,  DarkId = 68},
                new t_Card() { LigthId = 22,  DarkId = 66},                new t_Card() { LigthId = 22,  DarkId = 59},
                new t_Card() { LigthId = 23,  DarkId = 73},                new t_Card() { LigthId = 23,  DarkId = 106},
                new t_Card() { LigthId = 24,  DarkId = 61},                new t_Card() { LigthId = 24,  DarkId = 87},
                new t_Card() { LigthId = 25,  DarkId = 72},                new t_Card() { LigthId = 25,  DarkId = 107},
                new t_Card() { LigthId = 26,  DarkId = 101},               new t_Card() { LigthId = 26,  DarkId = 100},
                new t_Card() { LigthId = 27,  DarkId = 67},                new t_Card() { LigthId = 27,  DarkId = 71},
                new t_Card() { LigthId = 28,  DarkId = 81},                new t_Card() { LigthId = 28,  DarkId = 107},
                new t_Card() { LigthId = 29,  DarkId = 56},                new t_Card() { LigthId = 29,  DarkId = 63},
                new t_Card() { LigthId = 30,  DarkId = 95},                new t_Card() { LigthId = 30,  DarkId = 91},
                new t_Card() { LigthId = 31,  DarkId = 106},               new t_Card() { LigthId = 31,  DarkId = 91},
                new t_Card() { LigthId = 32,  DarkId = 103},               new t_Card() { LigthId = 32,  DarkId = 63},
                new t_Card() { LigthId = 33,  DarkId = 68},                new t_Card() { LigthId = 33,  DarkId = 108},
                new t_Card() { LigthId = 34,  DarkId = 70},                new t_Card() { LigthId = 34,  DarkId = 100},
                new t_Card() { LigthId = 35,  DarkId = 82},                new t_Card() { LigthId = 35,  DarkId = 70},
                new t_Card() { LigthId = 36,  DarkId = 60},                new t_Card() { LigthId = 36,  DarkId = 98},
                new t_Card() { LigthId = 37,  DarkId = 102},               new t_Card() { LigthId = 37,  DarkId = 82},
                new t_Card() { LigthId = 38,  DarkId = 67},                new t_Card() { LigthId = 38,  DarkId = 107},
                new t_Card() { LigthId = 39,  DarkId = 76},                new t_Card() { LigthId = 39,  DarkId = 85},
                new t_Card() { LigthId = 40,  DarkId = 77},                new t_Card() { LigthId = 40,  DarkId = 98},
                new t_Card() { LigthId = 41,  DarkId = 73},                new t_Card() { LigthId = 41,  DarkId = 80},
                new t_Card() { LigthId = 42,  DarkId = 55},                new t_Card() { LigthId = 42,  DarkId = 65},
                new t_Card() { LigthId = 43,  DarkId = 93},                new t_Card() { LigthId = 43,  DarkId = 96},
                new t_Card() { LigthId = 44,  DarkId = 89},                new t_Card() { LigthId = 44,  DarkId = 64},
                new t_Card() { LigthId = 45,  DarkId = 59},                new t_Card() { LigthId = 45,  DarkId = 75},
                new t_Card() { LigthId = 46,  DarkId = 86},                new t_Card() { LigthId = 46,  DarkId = 108},
                new t_Card() { LigthId = 47,  DarkId = 74},                new t_Card() { LigthId = 47,  DarkId = 57},
                new t_Card() { LigthId = 48,  DarkId = 92},                new t_Card() { LigthId = 48,  DarkId = 64},
                new t_Card() { LigthId = 49,  DarkId = 92},                new t_Card() { LigthId = 49,  DarkId = 78},
                new t_Card() { LigthId = 50,  DarkId = 61},                new t_Card() { LigthId = 50,  DarkId = 74},
                new t_Card() { LigthId = 51,  DarkId = 69},                new t_Card() { LigthId = 51,  DarkId = 88},
                new t_Card() { LigthId = 52,  DarkId = 108},               new t_Card() { LigthId = 52,  DarkId = 58},

                new t_Card() { LigthId = 53,  DarkId = 101},               new t_Card() { LigthId = 53,  DarkId = 86},
                new t_Card() { LigthId = 53,  DarkId = 93},                new t_Card() { LigthId = 53,  DarkId = 58},
                new t_Card() { LigthId = 54,  DarkId = 103},               new t_Card() { LigthId = 54,  DarkId = 75},
                new t_Card() { LigthId = 54,  DarkId = 72},                new t_Card() { LigthId = 54,  DarkId = 83},
            });
            dBContext.SaveChanges();
        }
    }
}