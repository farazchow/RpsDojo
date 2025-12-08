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
    private Vector3 originalScale;
    [SerializeField] private CardState currentState;
    private Quaternion originalRotation;
    private Vector3 originalPosition;
    private int originalSiblingIndex;
    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;
    
    [SerializeField]private Vector3 moveTarget;
    [SerializeField] private float selectScale = 1.1f;
    [SerializeField] private Vector2 cardPlay;
    [SerializeField] private Vector2 cardUnplay;
    [SerializeField] private Vector3 playPosition;
    [SerializeField] private GameObject glowEffect;
    [SerializeField] private GameObject playArrow;
    [SerializeField] public float speed = 1f;

    void Awake()
    {
        eventSystem = FindAnyObjectByType<EventSystem>();
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        raycaster = GetComponentInParent<GraphicRaycaster>();
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
        // Mouse is down already
        if (Input.GetMouseButton(0))
        {
            return;
        }

        if (currentState == CardState.Default) 
        {
            currentState = CardState.Hover;
        }
        else if (currentState == CardState.Played)
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
        if ((currentState == CardState.PlayedDragged && rectTransform.localPosition.y > cardUnplay.y) || 
            (currentState == CardState.Dragged && rectTransform.localPosition.y > cardPlay.y))
        {
            currentState = CardState.Played;
            moveTarget = playPosition;
        } 
        else
        {
            TransitionToState0();
        }
    }

    // Handling Different States
    private void HandlePlayState()
    {   
        glowEffect.SetActive(false);
        playArrow.SetActive(true);
        rectTransform.localRotation = Quaternion.identity;
        SlideTo(playPosition);

        // Check to see if we are still hovering the card
        if (rectTransform.localPosition == playPosition)
        {
            PointerEventData pointerEventData = new PointerEventData(eventSystem); 
            pointerEventData.position = Input.mousePosition; 
            List<RaycastResult> results = new List<RaycastResult>(); 
            raycaster.Raycast(pointerEventData, results);
            foreach (RaycastResult result in results)
            {
                Debug.Log(result.gameObject.name);
            }

            if (results.Count != 0)
            {
                currentState = CardState.PlayedHovered;
            } else
            {
                currentState = CardState.Played;
            }
        }
    }

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

    // Utility Functions
    private void TransitionToState0()
    {
        currentState = CardState.Default;

        // Reset Transform
        rectTransform.localScale = originalScale;
        rectTransform.localRotation = originalRotation;
        // rectTransform.localPosition = originalPosition;
        moveTarget = originalPosition;
        SlideTo(originalPosition);
        rectTransform.SetSiblingIndex(originalSiblingIndex);
        glowEffect.SetActive(false);
        playArrow.SetActive(false);

        // Check to see if we are still hovering the card
        // Make sure mouse not clicked to ensure we don't hover new cards while dragging another
        // if (rectTransform.localPosition == originalPosition && !Input.GetMouseButton(0))
        // {
        //     PointerEventData pointerEventData = new PointerEventData(eventSystem); 
        //     pointerEventData.position = Input.mousePosition; 
        //     List<RaycastResult> results = new List<RaycastResult>(); 
        //     raycaster.Raycast(pointerEventData, results);
        //     if (results.Count != 0)
        //     {
        //         currentState = CardState.Hover;
        //     }
        // }
    }

    public void SlideTo(Vector3 newPos)
    {
        if (rectTransform.localPosition == newPos) { return; }

        float journeyFraction = Time.deltaTime * 1000 * speed / Vector3.Distance(rectTransform.localPosition, newPos);
        rectTransform.localPosition = Vector3.Lerp(rectTransform.localPosition, newPos, journeyFraction);
    }

    public void SaveTransform()
    {
        originalScale = rectTransform.localScale;
        originalRotation = rectTransform.localRotation;
        originalPosition = rectTransform.localPosition;
        originalSiblingIndex = rectTransform.GetSiblingIndex();
        moveTarget = rectTransform.localPosition;
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
