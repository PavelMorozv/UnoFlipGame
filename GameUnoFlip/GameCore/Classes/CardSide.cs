using GameCore.Enums;
using System;
using Action = GameCore.Enums.Action;

namespace GameCore.Classes
{
    [Serializable]
    public class CardSide
    {
        private static int cardsCount = 0;
        public int Id { get; set; }
        public Color Color { get; set; }
        public Action Action { get; set; }
        public int Value { get; set; }

        public CardSide() { }

        public CardSide(Color colors = Color.None, Action actions = Action.None, int value = -1)
        {
            Id = cardsCount++;
            Color = colors;
            Action = actions;
            Value = value;
        }

        public override string ToString()
        {
            //return $"[{Action}][{Color}][{Value}]";
            return $"{{ action: {Action}, color: {Color}, value: {Value} }}";
        }

        public override bool Equals(object obj)
        {
            CardSide temp = (CardSide)obj;
            if (temp == null) return false;

            if (Action == temp.Action && Action.ToString().Contains("Wild"))
                return Action == temp.Action && Value == temp.Value;

            if (Action == temp.Action && Color == temp.Color && Value == temp.Value)
                return true;

            return false;
        }
    }
}
