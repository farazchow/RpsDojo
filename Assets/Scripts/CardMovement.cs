using System;
using System.Collections.Generic;
using RpsDojo;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardMovement : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalLocalPointerPosition;
    private Vector3 originalPanelLocalPosition;
    [SerializeField] private Vector3 originalScale;
    [SerializeField] private CardState currentState;
    private Quaternion originalRotation;
    private Vector3 originalPosition;
    private int originalSiblingIndex;
    private HandManager handManager;
    private Vector3 playPosition;
    // private GraphicRaycaster raycaster;
    // private EventSystem eventSystem;
    
    [SerializeField]private Vector3 moveTarget;
    [SerializeField] private float selectScale = 1.1f;
    [SerializeField] private Vector2 cardPlay;
    [SerializeField] private GameObject glowEffect;
    [SerializeField] private GameObject playArrow;
    [SerializeField] public float speed = 1f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        handManager = GetComponentInParent<HandManager>();
        playPosition = handManager.playPosition;
        // eventSystem = FindAnyObjectByType<EventSystem>();
        // raycaster = GetComponentInParent<GraphicRaycaster>();
    }

    void Start()
    {
        SaveTransform();
    }

    void Update()
    {
        switch (currentState)
        {
            case CardState.Default:
                SlideTo(originalPosition);
                break;
            case CardState.Hover: 
                HandleHoverState();
                break;
            case CardState.Dragged:
                HandleDragState();
                break;
            case CardState.Played:
                HandlePlayState();
                break;
            case CardState.PlayedHovered:
                HandleHoverState();
                break;
            case CardState.PlayedDragged:
                HandleDragState();
                break;
        }
    }

    // Pointer Events
    public void OnPointerEnter(PointerEventData eventData)
    {
        // We already have a card selected
        if (handManager.selectedCard) { return; }

        if (currentState == CardState.Default) 
        {
            currentState = CardState.Hover;
        }
        else if (currentState == CardState.Played && rectTransform.localPosition == playPosition)
        {
            currentState = CardState.PlayedHovered;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentState == CardState.Hover)
        {
            TransitionToState0();
        } else if (currentState == CardState.PlayedHovered)
        {
            currentState = CardState.Played;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentState == CardState.Hover || currentState == CardState.PlayedHovered)
        {
            handManager.SetSelected(gameObject);
            currentState = (currentState == CardState.Hover) ? CardState.Dragged : CardState.PlayedDragged;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera,
                out originalLocalPointerPosition
            );
            originalPanelLocalPosition = rectTransform.localPosition;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (handManager.selectedCard != gameObject) { return; }

        if (currentState == CardState.Dragged || currentState == CardState.PlayedDragged)
        {
            Vector2 localPointerPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera,
                out localPointerPosition
            ))
            {
                // Move the Object to follow mouse
                Vector3 offsetToOriginal = localPointerPosition - originalLocalPointerPosition;
                moveTarget = originalPanelLocalPosition + offsetToOriginal;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (handManager.selectedCard != gameObject) { return; }
        if (currentState != CardState.PlayedDragged && currentState != CardState.Dragged) { return; }

        if (rectTransform.localPosition.y > cardPlay.y)
        {
            moveTarget = playPosition;
            if (currentState == CardState.Dragged)
            {
                handManager.PlayCard(gameObject);
            }
            currentState = CardState.Played;
        } else {
            if (currentState == CardState.PlayedDragged)
            {
                handManager.UnplayCard(gameObject);
            }
            TransitionToState0();
        }
    }

    // Handling Different States
    private void HandleHoverState()
    {
        glowEffect.SetActive(true);
        rectTransform.localScale = originalScale * selectScale;

        // Bring Card to the front
        rectTransform.SetAsLastSibling();
    }

    private void HandleDragState()
    {
        // Remove the card's rotation
        rectTransform.localRotation = Quaternion.identity;
        SlideTo(moveTarget);
    }

    private void HandlePlayState()
    {   
        glowEffect.SetActive(false);
        playArrow.SetActive(true);
        rectTransform.localRotation = Quaternion.identity;
        SlideTo(playPosition);
    }

    // Utility Functions
    private void TransitionToState0()
    {
        // Will Release if necessary
        handManager.ReleaseSelected(gameObject);
        currentState = CardState.Default;

        // Reset Transform
        rectTransform.localScale = originalScale;
        rectTransform.localRotation = originalRotation;
        // rectTransform.localPosition = originalPosition;
        moveTarget = originalPosition;
        
        rectTransform.SetSiblingIndex(originalSiblingIndex);
        glowEffect.SetActive(false);
        playArrow.SetActive(false);

    }

    public Boolean SlideTo(Vector3 newPos)
    {
        if (rectTransform.localPosition == newPos) {
            // if (!Input.GetMouseButton(0))
            // {
            //     PointerEventData pointerEventData = new PointerEventData(eventSystem); 
            //     pointerEventData.position = Input.mousePosition; 
            //     List<RaycastResult> results = new List<RaycastResult>(); 
            //     raycaster.Raycast(pointerEventData, results);
            //     if (results.Count != 0 && results)
            //     {
            //         currentState = CardState.Hover;
            //     }
            // }
            return true; 
        }

        float journeyFraction = Time.deltaTime * 1000 * speed / Vector3.Distance(rectTransform.localPosition, newPos);
        rectTransform.localPosition = Vector3.Lerp(rectTransform.localPosition, newPos, journeyFraction);
        return false;
    }

    public void SaveTransform()
    {
        originalScale = rectTransform.localScale;
        originalRotation = rectTransform.localRotation;
        originalPosition = rectTransform.localPosition;
        originalSiblingIndex = rectTransform.GetSiblingIndex();
        moveTarget = rectTransform.localPosition;
    }

    public void SetTransform(Vector3 position, Quaternion rotation, Vector2 scale, int index)
    {
        rectTransform.localRotation = rotation;
        rectTransform.localPosition = position;
        rectTransform.SetSiblingIndex(index);
        rectTransform.localScale = scale;
        SaveTransform();
    }

    public enum CardState
    {
        Default,
        Hover,
        Dragged,
        Played,
        PlayedHovered,
        PlayedDragged,
    }
}
