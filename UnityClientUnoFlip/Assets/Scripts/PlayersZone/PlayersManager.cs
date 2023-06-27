using GameCore.Classes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayersManager : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    private List<Player> players;
    


    private void Start()
    {
        players = new List<Player>();
        players.AddRange(GetComponentsInChildren<Player>());
    }

    public void Initialized(int playerId, int[] playersIds) 
    {
        DistributeById(playerId, playersIds);
    }

    public void AvailableCards()
    {
        players[0].SelectCards();
    }

    public void ResetAvailableCards()
    {
        players[0].ResetSelectCards();
    }

    public void AddCardToPlayer(int playerId, Card card)
    {
        players.FirstOrDefault(p=>p.Get_ID() == playerId).AddCard(cardPrefab, card);
    }
    public void AddCardsToPlayer(int playerId, List<Card> cards)
    {
        players.FirstOrDefault(p => p.Get_ID() == playerId).AddCards(cardPrefab, cards);
    }
    public void RemoveCardFromPlayer(int playerId, Card card)
    {
        players.FirstOrDefault(p => p.Get_ID() == playerId).RemoveCard(card);
    }

    

    private void DistributeById(int playerId, int[] playersIds)
    {
        int count = playersIds.Length;
        int startIndex = 0;
        for (int i = 0; i < playersIds.Length; i++)
        {
            if (playerId == playersIds[i])
            {
                startIndex = i; 
                break;
            }
        }

        for (int i = 0; i < count; i++)
        {
            players[i].Set_ID(playersIds[(startIndex + i) % count]);
        }
    }
}
