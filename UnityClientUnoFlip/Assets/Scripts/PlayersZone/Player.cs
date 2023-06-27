using GameCore.Classes;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private int id = -1;
    [SerializeField] public bool isPlayer;
    [SerializeField] private GameObject deck;
    [SerializeField] private GameObject gameManagerObject;

    private MyGameManager gameManager;

    public void Set_ID(int id)
    {
        this.id = id;
    }

    public int Get_ID()
    {
        return id;
    }

    public void AddCard(GameObject cardPrefab, Card card)
    {
        var cardObject = Instantiate(cardPrefab, deck.transform, false);
        cardObject.GetComponent<CardDesign>().Initialized(gameManager, card);
    }

    public void AddCards(GameObject cardPrefab, List<Card> cards)
    {
        foreach (var card in cards)
        {
            AddCard(cardPrefab, card);
        }
    }

    public void RemoveCard(Card card)
    {
        var cardD = GetComponentsInChildren<CardDesign>();
        foreach (var c in cardD)
        {
            if (c.Info.Id == card.Id) Destroy(c.gameObject);
        }
    }

    void Start()
    {
        gameManager = gameManagerObject.GetComponent<MyGameManager>();
    }

    public void SelectCards()
    {
        var cardD = GetComponentsInChildren<CardDesign>();
        foreach (var card in cardD)
        {
            if (Game.IsMovePosible(gameManager.gameState.LastCardPlayed, card.Info, gameManager.gameState.Side))
            {
                card.gameObject.GetComponent<CardMovement>().isMove = true;
            }
        }
    }

    public void ResetSelectCards()
    {
        var cardD = GetComponentsInChildren<CardDesign>();
        foreach (var card in cardD)
        {
            card.gameObject.GetComponent<CardMovement>().isMove = false;
        }
    }
}