using GameCore.Enums;
using System.Collections.Generic;

namespace GameCore.Classes
{
    public static class DeckGenerator
    {
        public static List<Card> GenerateRandomDeck()
        {
            List<Card> cards = new List<Card>();

            List<CardSide> lightCardSides = new List<CardSide>();
            List<CardSide> darkCardSides = new List<CardSide>();

            for (int i = 0; i < 2; i++)
            {
                // Создание карт с номерами
                for (int j = 1; j < 10; j++)
                {
                    lightCardSides.Add(new CardSide(Color.Red, Action.Number, j));
                    lightCardSides.Add(new CardSide(Color.Yellow, Action.Number, j));
                    lightCardSides.Add(new CardSide(Color.Green, Action.Number, j));
                    lightCardSides.Add(new CardSide(Color.Blue, Action.Number, j));

                    darkCardSides.Add(new CardSide(Color.Pink, Action.Number, j));
                    darkCardSides.Add(new CardSide(Color.Orange, Action.Number, j));
                    darkCardSides.Add(new CardSide(Color.Turquoise, Action.Number, j));
                    darkCardSides.Add(new CardSide(Color.Purple, Action.Number, j));
                }

                // Создание Диких карт
                for (int j = 0; j < 2; j++)
                {
                    // Дикие карты
                    lightCardSides.Add(new CardSide(Color.None, Action.Wild));
                    darkCardSides.Add(new CardSide(Color.None, Action.Wild));


                    // Дикие карты с действием
                    lightCardSides.Add(new CardSide(Color.None, Action.WildGive, 2));
                    darkCardSides.Add(new CardSide(Color.None, Action.WildGiveForNow));
                }

                // С действием Взять карты
                lightCardSides.Add(new CardSide(Color.Red, Action.Give, 1));
                lightCardSides.Add(new CardSide(Color.Yellow, Action.Give, 1));
                lightCardSides.Add(new CardSide(Color.Green, Action.Give, 1));
                lightCardSides.Add(new CardSide(Color.Blue, Action.Give, 1));

                darkCardSides.Add(new CardSide(Color.Pink, Action.Give, 5));
                darkCardSides.Add(new CardSide(Color.Orange, Action.Give, 5));
                darkCardSides.Add(new CardSide(Color.Turquoise, Action.Give, 5));
                darkCardSides.Add(new CardSide(Color.Purple, Action.Give, 5));


                //С действием Сменить направление
                lightCardSides.Add(new CardSide(Color.Red, Action.ChangeDirection));
                lightCardSides.Add(new CardSide(Color.Yellow, Action.ChangeDirection));
                lightCardSides.Add(new CardSide(Color.Green, Action.ChangeDirection));
                lightCardSides.Add(new CardSide(Color.Blue, Action.ChangeDirection));

                darkCardSides.Add(new CardSide(Color.Pink, Action.ChangeDirection));
                darkCardSides.Add(new CardSide(Color.Orange, Action.ChangeDirection));
                darkCardSides.Add(new CardSide(Color.Turquoise, Action.ChangeDirection));
                darkCardSides.Add(new CardSide(Color.Purple, Action.ChangeDirection));


                // С действием Пропустить ход
                lightCardSides.Add(new CardSide(Color.Red, Action.SkipMove));
                lightCardSides.Add(new CardSide(Color.Yellow, Action.SkipMove));
                lightCardSides.Add(new CardSide(Color.Green, Action.SkipMove));
                lightCardSides.Add(new CardSide(Color.Blue, Action.SkipMove));

                darkCardSides.Add(new CardSide(Color.Pink, Action.SkipMoveAll));
                darkCardSides.Add(new CardSide(Color.Orange, Action.SkipMoveAll));
                darkCardSides.Add(new CardSide(Color.Turquoise, Action.SkipMoveAll));
                darkCardSides.Add(new CardSide(Color.Purple, Action.SkipMoveAll));


                // С действием Перевернуть
                lightCardSides.Add(new CardSide(Color.Red, Action.Flip));
                lightCardSides.Add(new CardSide(Color.Yellow, Action.Flip));
                lightCardSides.Add(new CardSide(Color.Green, Action.Flip));
                lightCardSides.Add(new CardSide(Color.Blue, Action.Flip));

                darkCardSides.Add(new CardSide(Color.Pink, Action.Flip));
                darkCardSides.Add(new CardSide(Color.Orange, Action.Flip));
                darkCardSides.Add(new CardSide(Color.Turquoise, Action.Flip));
                darkCardSides.Add(new CardSide(Color.Purple, Action.Flip));

            }

            lightCardSides.Flush();
            darkCardSides.Flush();

            for (int i = 0; i < lightCardSides.Count; i++)
                cards.Add(new Card(lightCardSides[i], darkCardSides[i]));

            cards.Flush();

            return cards;
        }
    }
}
