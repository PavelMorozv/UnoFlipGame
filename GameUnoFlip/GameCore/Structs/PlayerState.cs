using GameCore.Classes;
using System;
using GameCore.Enums;
using System.Collections.Generic;

namespace GameCore.Structs
{
    [Serializable]
    public struct PlayerState
    {
        public int Id { get; set; }
        public bool IsUno { get; set; }
        public List<Card>? Cards { get; set; }
        public bool IsGive { get; set; }

        public override string ToString()
        {
            string cards = "";
            foreach (Card card in Cards)
            {
                cards += card.ToString();
            }
            return $"Player: [{Id}][{IsUno}][{IsGive}][{cards}]";
        }

        public string ToString(Side sides)
        {
            string cards = "";
            if (Cards != null)
            {
                foreach (Card card in Cards)
                {
                    cards += card.ToString(sides);
                }
            }

            return $"Player: [{Id}][{IsUno}][{IsGive}][{cards}]";
        }
    }
}