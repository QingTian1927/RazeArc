using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Object")]
    public GameObject doorMesh;

    [Header("Open Condition")]
    public bool isNormalDoor = false;
    public string requiredKey = "None";
    public bool isRoomCleared = false;
    [Tooltip("Check this if the door should lock BEHIND the player until enemies are dead.")]
    public bool isArenaDoor = false;

    [Tooltip("Drag the enemies for this specific room into this list")]
    public GameObject[] roomEnemies;

    [Header("Close Settings")]
    public bool autoClose = true;

    [Header("Animation Settings")]
    public float slideHeight = 5f;
    public float slideSpeed = 5f;

    private bool isOpening = false;
    private bool isClosing = false;
    private Vector3 targetPosition;
    private Vector3 startPosition;

    private bool isLockedInArena = false;

    void Start()
    {
        if (doorMesh != null)
        {
            startPosition = doorMesh.transform.position;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CheckDoorConditions(other.gameObject);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && autoClose)
        {
            CloseDoor();

            // Lock the door behind the player if it's an arena door and enemies are still alive
            if (isArenaDoor && !isLockedInArena && !AreEnemiesDead())
            {
                isLockedInArena = true;
            }
        }
    }

    private void CheckDoorConditions(GameObject player)
    {
        // 1. Arena Door Logic
        if (isArenaDoor)
        {
            if (isLockedInArena)
            {
                if (AreEnemiesDead())
                {
                    isLockedInArena = false; // Unlock permanently
                    OpenDoor();
                    return;
                }
                else
                {
                    Debug.Log("Arena Locked: Defeat all enemies to escape!");
                    return;
                }
            }
            else
            {
                OpenDoor(); // Open freely the first time to let the player in
                return;
            }
        }

        // 2. Normal Door Logic
        if (isNormalDoor)
        {
            OpenDoor();
            return;
        }

        // 3. Keycard Door Logic
        if (requiredKey != "None")
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();

            if ((requiredKey == "Red" && inventory.hasRedKey) ||
                (requiredKey == "Blue" && inventory.hasBlueKey) ||
                (requiredKey == "Green" && inventory.hasGreenKey))
            {
                OpenDoor();
                return;
            }
            else
            {
                Debug.Log("Door Locked: You need the " + requiredKey + " key!");
                return;
            }
        }

        // 4. Pre-cleared Room Logic
        if (isRoomCleared)
        {
            if (AreEnemiesDead())
            {
                OpenDoor();
                return;
            }
            else
            {
                Debug.Log("Door Locked: You must clear the room first!");
                return;
            }
        }
    }

    private bool AreEnemiesDead()
    {
        if (roomEnemies.Length == 0) return true;

        foreach (GameObject enemy in roomEnemies)
        {
            if (enemy != null)
            {
                return false; // Found an enemy that is still alive
            }
        }

        return true; // All enemies are destroyed (null)
    }

    private void OpenDoor()
    {
        targetPosition = startPosition + new Vector3(0, slideHeight, 0);
        isOpening = true;
        isClosing = false;
        this.enabled = true;
    }

    private void CloseDoor()
    {
        targetPosition = startPosition;
        isOpening = false;
        isClosing = true;
        this.enabled = true;
    }

    void Update()
    {
        if (isOpening || isClosing)
        {
            doorMesh.transform.position = Vector3.MoveTowards(
                doorMesh.transform.position,
                targetPosition,
                slideSpeed * Time.deltaTime
            );

            if (doorMesh.transform.position == targetPosition)
            {
                isOpening = false;
                isClosing = false;
                this.enabled = false;
            }
        }
    }
}