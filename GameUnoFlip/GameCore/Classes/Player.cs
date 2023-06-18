using GameCore.Structs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameCore.Classes
{
    public class Player
    {
        public event Action<Player, Card>? OnAddCard;
        public event Action<Player, List<Card>>? OnAddRangeCard;
        public event Action<Player, Card>? OnRemoveCard;
        public event Action<Player>? OnChangeUno;

        public int Id { get; private set; }
        public bool IsGive { get; set; }
        public bool IsUno { get; private set; }
        public Game Game { get; private set; }

        public List<Card> Cards { get; private set; } = new List<Card>();

        public Player(int id, Game game)
        {
            Id = id;
            IsUno = false;
            IsGive = false;
            Game = game;
        }

        public void SetUno()
        {
            IsUno = true;
            OnChangeUno?.Invoke(this);
        }

        public void ResetUno()
        {
            IsUno = false;
            OnChangeUno?.Invoke(this);
        }

        public void AddCard(Card card)
        {
            Cards.Add(card);
            OnAddCard?.Invoke(this, card);
        }

        public void AddRangeCards(List<Card> cards)
        {
            Cards.AddRange(cards);
            OnAddRangeCard?.Invoke(this, cards);
        }

        public void RemoveCard(Card card)
        {
            var tempCard = Cards.First(c => c.Id == card.Id);
            Cards.Remove(tempCard);
            OnRemoveCard?.Invoke(this, card);
        }

        public bool CheckAvailableCard(Card card)
        {
            return Cards.Contains(card);
        }

        public PlayerState GetState()
        {
            return new PlayerState()
            {
                Id = Game.GetPlayerId(this),
                IsUno = IsUno,
                Cards = Cards,
                IsGive = IsGive,
            };
        }

        public override string ToString()
        {
            string cards = "";
            foreach (Card card in Cards)
            {
                cards += card.ToString();
            }
            return $"{{ id: {Id}, isUno: {IsUno}, isGive: {IsGive}, Cards: [ {cards} ] }}";
        }
    }
}