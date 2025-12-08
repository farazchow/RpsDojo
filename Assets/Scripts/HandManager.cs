using UnityEngine;
using RpsDojo;
using System.Collections.Generic;
using System;
using Unity.VisualScripting.FullSerializer;
using System.Runtime.CompilerServices;

public class HandManager : MonoBehaviour
{
    public GameObject attackPrefab;
    public GameObject effectPrefab;
    public Transform handTransform;
    public float fanAngle = 7.5f;
    public float fanSpacing = 100f;
    public float verticalSpacing = 10f;
    public DeckManager deckManager;
    readonly public List<GameObject> cardsInHand = new List<GameObject>();

    void OnValidate()
    {
        UpdateHandVisuals();
    }

    public void AddCardToHand(Card cardData)
    {
        GameObject cardPrefab = (cardData is EffectCard) ? effectPrefab : attackPrefab;
        GameObject newCard = Instantiate(cardPrefab, handTransform.position, Quaternion.identity, handTransform);
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
            cardsInHand[i].transform.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);

            float normalizedHandPos = 2f * i / (cardCount - 1) - 1f;
            float horizontalOffset = (i - (cardCount - 1)/2f) * fanSpacing;
            float verticalOffset = (1 - normalizedHandPos * normalizedHandPos) * verticalSpacing;
            cardsInHand[i].transform.localPosition = new Vector3(horizontalOffset, verticalOffset, 0f);
        }
    }
}
