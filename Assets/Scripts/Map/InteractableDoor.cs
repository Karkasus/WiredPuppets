using UnityEngine;
using System.Collections;

public class InteractableDoor : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private GameObject doorModel; // The door model object that will be moved
    [SerializeField] private float openHeight = 3.0f; // Height to which the door opens
    [SerializeField] private float openSpeed = 2.0f; // Opening speed

    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.F; // Interaction key

    private bool isPlayerInRange = false;
    private bool isOpening = false;
    private bool isOpen = false;
    private Vector3 closedPosition;

    void Start()
    {
        if (doorModel == null)
        {
            Debug.LogError($"Door model missing on {gameObject.name} in InteractableDoor script!");
            enabled = false;
            return;
        }
        // Save the closed (initial) position of the door
        closedPosition = doorModel.transform.localPosition;
    }

    void Update()
    {
        // If player is in range and presses the interact key
        if (isPlayerInRange && !isOpening && !isOpen && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(OpenDoorRoutine());
        }
    }

    // Coroutine for door opening
    IEnumerator OpenDoorRoutine()
    {
        isOpening = true;
        Vector3 targetPosition = closedPosition + Vector3.up * openHeight;
        float elapsed = 0;

        // While not fully opened
        while (elapsed < 1.0f)
        {
            // Smoothly move (Lerp)
            doorModel.transform.localPosition = Vector3.Lerp(closedPosition, targetPosition, elapsed);
            elapsed += Time.deltaTime * openSpeed;
            yield return null; // Wait for next frame
        }

        // Snap to target position
        doorModel.transform.localPosition = targetPosition;

        // Disable collider so player can pass through
        Collider doorCollider = doorModel.GetComponent<Collider>();
        if (doorCollider != null && !doorCollider.isTrigger)
        {
            doorCollider.enabled = false;
        }

        isOpen = true;
        isOpening = false;
    }

    // Triggered when player enters the trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            // Optionally, show UI prompt like "Press F to open"
        }
    }

    // Triggered when player leaves the trigger
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            // Optionally, hide UI prompt
        }
    }
}