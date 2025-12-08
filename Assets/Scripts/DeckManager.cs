using UnityEngine;
using RpsDojo;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using System;

public class DeckManager : MonoBehaviour
{
    public List<Card> allCards = new List<Card>();
    public int initialHandSize = 6;
    public int maxHandSize = 12;
    public HandManager handManager;

    private int currentIndex = 0;

    void Start()
    {
        // Load all Card Assets from the Resources folder
        Card[] cards = Resources.LoadAll<Card>("CardData");
        allCards.AddRange(cards);

        for (int i = 0; i < initialHandSize; i++)
        {
            DrawCard();
        }
    }

    public void DrawCard()
    {
        if (allCards.Count == 0 || handManager.cardsInHand.Count >= maxHandSize) { return; }
        if (!handManager) {throw new Exception("Hand Manager not set");}

        Card nextCard = allCards[currentIndex];
        handManager.AddCardToHand(nextCard);
        currentIndex = (currentIndex + 1) % allCards.Count;
    }
}
