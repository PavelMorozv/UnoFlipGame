using GameCore.Enums;
using GameCore.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using Action = GameCore.Enums.Action;

namespace GameCore.Classes
{
    public class Game
    {
        private GameStatus _gameStatus;
        private int _currentPlayer;
        private Side _currentSide;
        private Direction _direction;
        private List<Player> _players;
        private List<Card> _deck;

        public int Id { get; private set; } = -1;

        public Game()
        {
            Id = -1;
            _currentSide = Side.Light;
            _direction = Direction.Forward;
            _currentPlayer = -1;
            _players = new List<Player>();
            _deck = DeckGenerator.GenerateRandomDeck();
            _gameStatus = GameStatus.Initialization;
        }

        public Game(int id)
        {
            Id = id;
            _currentSide = Side.Light;
            _direction = Direction.Forward;
            _currentPlayer = -1;
            _players = new List<Player>();
            _deck = DeckGenerator.GenerateRandomDeck();
            _gameStatus = GameStatus.Initialization;
        }

        public Game(int id, List<Card> cards)
        {
            Id = id;
            _currentSide = Side.Light;
            _direction = Direction.Forward;
            _currentPlayer = -1;
            _players = new List<Player>();
            _deck = cards;
            _gameStatus = GameStatus.Initialization;
            for (int i = 0; i < 30; i++)
            {
                _deck.Shuffle();
            }
        }

        public void AddPlayer(Player player)
        {
            _players.Add(player);
        }

        public void Start()
        {
            _currentPlayer = new Random().Next(0, _players.Count);

            foreach (Player player in _players) HandOutCard(player, 7);

            _gameStatus = GameStatus.InProcess;
        }

        public int GetPlayerId(Player player)
        {
            return _players.IndexOf(player);
        }

        public bool Move(int playerID, Card card)
        {
            var player = _players.FirstOrDefault(p => p.Id == playerID);
            if (_gameStatus != GameStatus.InProcess) return false;
            if (_players.IndexOf(player) != _currentPlayer) return false;
            if (!IsMovePosible(LastCardPlayed(), card, _currentSide)) return false;



            _deck.Add(card);

            switch (card.Action(_currentSide))
            {
                case Action.Number:
                case Action.Wild:
                    break;

                case Action.Give:
                case Action.WildGive:
                    ActionGive(card.Value(_currentSide));
                    ActionEndMove();
                    break;

                case Action.Flip:
                    ActionFlip();
                    break;

                case Action.SkipMove:
                    ActionSkip();
                    break;

                case Action.SkipMoveAll:
                    ActionSkipAll();
                    break;

                case Action.ChangeDirection:
                    ActionChangeDirection();
                    break;

                case Action.WildGiveForNow:
                    ActionWildGiveForNow();
                    break;

                default: return false;
            }

            player.RemoveCard(card);

            if (player.Cards.Count == 0)
            {
                _gameStatus = GameStatus.EndGame;
            }
            else
            {
                ActionEndMove();
            }

            return true;
        }

        public List<Player> GetPlayers()
        {
            return _players;
        }

        public static bool IsMovePosible(Card lastCardPlayed, Card card, Side currentSide)
        {
            switch (card.Action(currentSide))
            {
                case Action.Wild:
                case Action.WildGive:
                case Action.WildGiveForNow:
                    return true;
            }

            if (lastCardPlayed.Color(currentSide) == card.Color(currentSide)) return true;

            if (lastCardPlayed.Action(currentSide) == card.Action(currentSide) &&
                lastCardPlayed.Action(currentSide) != Action.Number)
                return true;

            if (lastCardPlayed.Value(currentSide) == card.Value(currentSide) && lastCardPlayed.Action(currentSide) == card.Action(currentSide)
                && lastCardPlayed.Action(currentSide) == Action.Number)
                return true;

            return false;
        }

        public GameState GetState()
        {
            return new GameState()
            {
                Status = _gameStatus,
                //CurrentPlayer = _currentPlayer > -1 ? _players.ElementAt(_currentPlayer).Id : _currentPlayer,
                CurrentPlayer = _currentPlayer > -1 ? _players.ElementAt(_currentPlayer).Id : _currentPlayer,
                Direction = _direction,
                LastCardPlayed = LastCardPlayed(),
                NextCardInDeck = NextCardInDeck(),
                Side = _currentSide
            };
        }

        public void GetCard()
        {
            var player = _players[_currentPlayer];
            if (!player.IsGive)
            {
                player.IsGive = true;
                HandOutCard(player, 1);
            }
        }

        private Card LastCardPlayed()
        {
            return _deck.LastOrDefault();
        }

        private Card NextCardInDeck()
        {
            return _deck.FirstOrDefault();
        }

        private void HandOutCard(Player player, int countCard)
        {
            if (countCard == 1)
            {
                player.AddCard(_deck.GetRange(0, countCard)[0]);
                _deck.RemoveRange(0, countCard);
            }
            else
            {
                player.AddRangeCards(_deck.GetRange(0, countCard));
                _deck.RemoveRange(0, countCard);
            }
        }

        private Player NextPlayer()
        {
            int result = _currentPlayer + (int)_direction;
            if (result > _players.Count - 1) return _players[0];
            else if (result < 0) return _players[_players.Count - 1];

            return _players[result];
        }

        private void ActionChangeDirection()
        {
            _direction = _direction == Direction.Forward ? Direction.Backward : Direction.Forward;
        }

        private void ActionFlip()
        {
            _currentSide = _currentSide == Side.Light ? Side.Dark : Side.Light;
        }

        private void ActionGive(int count)
        {
            HandOutCard(NextPlayer(), count);
        }

        private void ActionSkip()
        {
            ActionEndMove();
        }

        private void ActionSkipAll()
        {
            for (int i = 0; i < _players.Count - 1; i++)
                _currentPlayer = _players.IndexOf(NextPlayer());
        }

        private void ActionWildGiveForNow()
        {
            while (!IsMovePosible(LastCardPlayed(), NextCardInDeck(), _currentSide))
            {
                ActionGive(1);
            }

            ActionGive(1);
        }

        public void ActionEndMove()
        {
            NextPlayer().IsGive = false;
            _currentPlayer = _players.IndexOf(NextPlayer());
        }
    }
}