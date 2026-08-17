using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableObjectDetector : MonoBehaviour
{
    [SerializeField] private InputActionReference _interactAction;
    private List<IInteractableObject> _nearbyInteractableObjects;

    public IInteractableObject NearestInteractableObject { get; private set; }
    public event Action<InteractionPromptInfo> InteractionPromptChanged;

    private void Awake()
    {
        _nearbyInteractableObjects = new();
        NearestInteractableObject = null;
    }

    private void OnEnable()
    {
        _interactAction.action.Enable();
        _interactAction.action.performed += OnInteract;
    }

    private void OnDisable()
    {
        _interactAction.action.performed -= OnInteract;
        _interactAction.action.Disable();
    }

    private void Update()
    {
        IInteractableObject currentNearestInteractableObject = NearestInteractableObject;
        SetNearestInteractableObject();
        if (currentNearestInteractableObject != NearestInteractableObject)
            InteractionPromptChanged?.Invoke(new InteractionPromptInfo(NearestInteractableObject != null, _interactAction.action.GetBindingDisplayString(group: "Keyboard&Mouse"), NearestInteractableObject?.InteractionPromptText ?? ""));
    }

    private void OnTriggerEnter(Collider other)
    {
        // Don't detect the player transform
        if (other.transform.root == transform.root)
            return;

        IInteractableObject interactableObject = other.GetComponent<IInteractableObject>();
        if (interactableObject != null && !_nearbyInteractableObjects.Contains(interactableObject))
            _nearbyInteractableObjects.Add(interactableObject);
    }

    private void OnTriggerExit(Collider other)
    {
        IInteractableObject interactableObject = other.GetComponent<IInteractableObject>();
        if (interactableObject != null && _nearbyInteractableObjects.Contains(interactableObject))
            _nearbyInteractableObjects.Remove(interactableObject);
    }

    private void SetNearestInteractableObject()
    {
        // Prune any recently destroyed objects from the list
        _nearbyInteractableObjects.RemoveAll(interactableObject =>
            interactableObject == null ||
            interactableObject is UnityEngine.Object unityObject && unityObject == null
         );

        if (_nearbyInteractableObjects.Count < 1)
        {
            NearestInteractableObject = null;
            return;
        }

        IInteractableObject closest = null;
        float closestDistance = Mathf.Infinity;
        foreach (IInteractableObject interactableObject in _nearbyInteractableObjects)
        {
            float squaredDistance = (interactableObject.WorldPosition - transform.position).sqrMagnitude;
            if (squaredDistance < closestDistance)
            {
                closest = interactableObject;
                closestDistance = squaredDistance;
            }
        }
        NearestInteractableObject = closest;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        InteractWithNearestObject();
    }

    public void InteractWithNearestObject()
    {
        if (NearestInteractableObject == null)
            return;
        IInteractableObject currentNearestInteractableObject = NearestInteractableObject;
        currentNearestInteractableObject.Interact();
    }

    public void SetInteractionEnabled(bool enabled)
    {
        if (enabled)
            _interactAction.action.Enable();
        else
            _interactAction.action.Disable();
    }

    public bool IsInRange(IInteractableObject interactableObject)
    {
        if (interactableObject == null || _nearbyInteractableObjects == null)
            return false;
        return _nearbyInteractableObjects.Contains(interactableObject);
    }

}
