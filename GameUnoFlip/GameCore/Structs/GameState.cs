using GameCore.Classes;
using System;
using GameCore.Enums;

namespace GameCore.Structs
{
    [Serializable]
    public struct GameState
    {
        public GameStatus Status { get; set; }
        public int CountPlayers { get; set; }
        public int CurrentPlayer { get; set; }
        public Side Side { get; set; }
        public Direction Direction { get; set; }
        public Card? LastCardPlayed { get; set; }
        public Card? NextCardInDeck { get; set; }
        public override string ToString()
        {
            return $"Game State ---(Players: {CountPlayers})------\n"
                + $"  Status:.........{Status}\n"
                + $"  Side:...........{Side}\n"
                + $"  Direction:......{Direction}\n"
                + $"  Curent player:..{CurrentPlayer}\n"
                + $"  NextCardInDeck..{NextCardInDeck?.ToString(Side == Side.Light ? Side.Dark : Side.Light)}\n"
                + $"  LastDroppedCard:{LastCardPlayed?.ToString(Side)}";
        }
    }
}
