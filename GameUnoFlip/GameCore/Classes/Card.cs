﻿using GameCore.Enums;
using System;
using Action = GameCore.Enums.Action;

namespace GameCore.Classes
{
    [Serializable]
    public class Card
    {
        public int Id { get; set; } = -1;
        public CardSide[] Sides { get; set; }

        public Card() { }

        public Card(CardSide light, CardSide dark)
        {
            Sides = new CardSide[] { light, dark };
        }
        public Card(int cardId, CardSide light, CardSide dark)
        {
            Id = cardId;
            Sides = new CardSide[] { light, dark };
        }

        public void SetLightSide(CardSide side)
        {
            Sides[(int)Side.Light] = side;
        }

        public void SetDarkSide(CardSide side)
        {
            Sides[(int)Side.Dark] = side;
        }

        public CardSide GetSide(Side side)
        {
            return Sides[(int)side];
        }

        public Color Color(Side cardSides)
        {
            return Sides[(int)cardSides].Color;
        }

        public void SetColor(Side cardSides, Color newColor)
        {
            Sides[(int)cardSides].Color = newColor;
        }

        public Action Action(Side cardSides)
        {
            return Sides[(int)cardSides].Action;
        }

        public int Value(Side cardSides)
        {
            return Sides[(int)cardSides].Value;
        }

        public string ToString(Side cardSides)
        {
            return $"{(cardSides == Side.Light ? Sides[(int)Side.Light] : Sides[(int)Side.Dark])}";
        }

        public override string ToString()
        {
            return $"{{ light: {Sides[(int)Side.Light]}, dark: {Sides[(int)Side.Dark]} }}";
        }

        public override bool Equals(object obj)
        {
            Card temp = (Card)obj;
            if (temp == null) return false;

            if (Sides[0].Equals(temp.Sides[0]) && Sides[1].Equals(temp.Sides[1])) return true;
            else return false;
        }
    }
}
