using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RpsDojo;

public class CardDisplay : MonoBehaviour
{
    public Card cardData;
    public Image cardImage;
    public TMP_Text nameText;
    public TMP_Text damageText;
    public TMP_Text staminaText;

    // Start is called before the first frame update
    void Start()
    {
        if (cardData)
        {
            UpdateCardDisplay();
        }
    }

    public void UpdateCardDisplay()
    {
        nameText.text = cardData.cardName;
        if (cardData.GetType() == typeof(AttackCard))
        {
            damageText.text = (cardData as AttackCard).damage.ToString();
        } 
        else if (cardData.GetType() == typeof(EffectCard))
        {
            staminaText.text = (cardData as EffectCard).staminaCost.ToString();
        }
        cardImage.sprite = cardData.cardSprite;
    }
}
