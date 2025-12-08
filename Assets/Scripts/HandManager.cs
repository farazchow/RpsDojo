using UnityEngine;
using RpsDojo;
using System.Collections.Generic;
using System;
using Unity.VisualScripting.FullSerializer;
using System.Runtime.CompilerServices;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;

public class HandManager : MonoBehaviour
{
    public GameObject attackPrefab;
    public GameObject effectPrefab;
    public float fanAngle = 7.5f;
    public float fanSpacing = 100f;
    public float verticalSpacing = 10f;
    public DeckManager deckManager;
    readonly public List<GameObject> cardsInHand = new List<GameObject>();
    public Transform playContainer;
    public List<GameObject> cardsPlayed = new List<GameObject>();
    public Vector3 playPosition = new Vector3(-500, 600, 0);
    public GameObject selectedCard;
    private float cardScale = 100f;


    void OnValidate()
    {
        UpdateHandVisuals();
    }

    public void AddCardToHand(Card cardData)
    {
        GameObject cardPrefab = (cardData is EffectCard) ? effectPrefab : attackPrefab;
        GameObject newCard = Instantiate(cardPrefab, transform.position, Quaternion.identity, transform);
        cardsInHand.Add(newCard);
        newCard.GetComponent<CardDisplay>().cardData = cardData;
        UpdateHandVisuals();
    }

    private void UpdateHandVisuals()
    {
        int cardCount = cardsInHand.Count;
        if (cardCount == 1)
        {
            cardsInHand[0].transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            cardsInHand[0].transform.localPosition = new Vector3(0f, 0f, 0f);
            return;
        }

        for (int i = 0; i < cardCount; i++)
        {
            float rotationAngle = fanAngle * (i - (cardCount - 1)/2f);
            Quaternion localRotation = Quaternion.Euler(0f, 0f, rotationAngle);

            float normalizedHandPos = 2f * i / (cardCount - 1) - 1f;
            float horizontalOffset = (i - (cardCount - 1)/2f) * fanSpacing;
            float verticalOffset = (1 - normalizedHandPos * normalizedHandPos) * verticalSpacing;
            Vector3 localPosition = new Vector3(horizontalOffset, verticalOffset, 0f);
            Vector2 originalScale = new Vector2(cardScale, cardScale);
            cardsInHand[i].GetComponent<CardMovement>().SetTransform(localPosition, localRotation, originalScale, i);
        }
    }

    public void SetSelected(GameObject card)
    {
        selectedCard = card;

        // Ensure it is drawn on top
        selectedCard.transform.SetAsLastSibling();
    }

    public void ReleaseSelected(GameObject card)
    {
        if (selectedCard == card)
        {
            selectedCard = null;        
        }
    }

    public void PlayCard(GameObject card)
    {
        cardsInHand.Remove(card);
        cardsPlayed.Add(card);
        card.transform.SetParent(playContainer);
        ReleaseSelected(card);
        UpdateHandVisuals();
    }

    public void UnplayCard(GameObject card)
    {
        cardsPlayed.Remove(card);
        card.transform.SetParent(transform);
        cardsInHand.Add(card);
        ReleaseSelected(card);
        UpdateHandVisuals();
    }
}
