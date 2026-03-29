using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.AR.Inputs;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class TouchMechanic : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler
{
    [SerializeField] private List<Material> materials;
    [SerializeField] private List<Color> colors;
    [SerializeField] private float rotationSpeed = 0.5f;
    [SerializeField] private float zoomSpeed = 0.001f;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 2.0f;

    private int _currentIndex = 0;
    private Vector2 _previousScrollValue = Vector2.zero;
    private float _previousPinchDistance = 0f;


    private float previousMagnitude = 0f;

    private void Start()
    {
        foreach (var mat in materials)
        {
            mat.SetColor("_Color", colors[0]);
        }

        var touch0pos = new InputAction
        (
            type: InputActionType.Value,
            binding: "<Touchscreen>/touch0/position"
        );
        touch0pos.Enable();
        var touch1pos = new InputAction
        (
            type: InputActionType.Value,
            binding: "<Touchscreen>/touch1/position"
        );
        touch1pos.Enable();
        touch1pos.performed += _ =>
        {
            var magnitude = (touch0pos.ReadValue<Vector2>() - touch1pos.ReadValue<Vector2>()).magnitude;
            if (previousMagnitude == 0)
            {
                previousMagnitude = magnitude;
            }

            var difference = magnitude - previousMagnitude;
            previousMagnitude = magnitude;
            ApplyZoom(difference);
        };
    }


    private void Update()
    {
        if (Mouse.current != null)
        {
            Vector2 scrollValue = Mouse.current.scroll.ReadValue();

            if (scrollValue != _previousScrollValue)
            {
                Debug.Log($"Scroll detectado: {scrollValue}");
                ApplyZoom(scrollValue.y);
                _previousScrollValue = scrollValue;
            }
        }
    }

    private void ApplyZoom(float increment)
    {
        Vector3 currentScale = transform.localScale;

        float nextScale = currentScale.x + (increment * zoomSpeed);
        nextScale = Mathf.Clamp(nextScale, minScale, maxScale);
        transform.localScale = new Vector3(nextScale, nextScale, nextScale);
        Debug.Log($"Zoom aplicado: {nextScale}");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!eventData.dragging && Input.touchCount < 2)
        {
            _currentIndex = (_currentIndex + 1) % colors.Count;
            foreach (var mat in materials)
            {
                mat.SetColor("_BaseColor", colors[_currentIndex]);
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
    }

    public void OnDrag(PointerEventData eventData)
    {
        float rotationX = eventData.delta.x * rotationSpeed;
        float rotationY = eventData.delta.y * rotationSpeed;

        transform.Rotate(Vector3.up, rotationY, Space.World);
        transform.Rotate(Vector3.right, rotationX, Space.World);
    }
}