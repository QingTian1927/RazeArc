using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Object")]
    public GameObject doorMesh;

    [Header("Open Condition")]
    public bool isNormalDoor = false; //Open whenever entering the trigger zone
    public string requiredKey = "None";
    public bool isRoomCleared = false;
    [Tooltip("Drag the enemies for this specific room into this list")]
    public GameObject[] roomEnemies;

    [Header("Animation Settings")]
    public float slideHeight = 5f;
    public float slideSpeed = 5f;

    private bool isOpening = false;
    private Vector3 targetPosition;

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("SOMETHING TOUCHED THE DOOR: " + other.gameObject.name);
        Debug.Log("ITS TAG IS: " + other.gameObject.tag);
        if (other.CompareTag("Player"))
        {
            CheckDoorConditions(other.gameObject);
        }
    }

    private void OpenDoor()
    {
        Debug.Log("Opening Door");

        targetPosition = doorMesh.transform.position + new Vector3(0, slideHeight, 0);

        isOpening = true;

        GetComponent<Collider>().enabled = false;
    }

    private void CheckDoorConditions(GameObject player)
    {
        if (isNormalDoor)
        {
            OpenDoor();
            return;
        }

        if (requiredKey != "None")
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();

            if (requiredKey == "Red" && inventory.hasRedKey)
            {
                OpenDoor();
                return;
            }
            else if (requiredKey == "Blue" && inventory.hasBlueKey)
            {
                OpenDoor();
                return;
            }
            else if (requiredKey == "Green" && inventory.hasGreenKey)
            {
                OpenDoor();
                return;
            }
            else
            {
                Debug.Log("Door Locked: You need the " + requiredKey + " key!");
            }   
        }

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
                return false;
            }
        }

        return true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isOpening)
        {
            doorMesh.transform.position = Vector3.MoveTowards(
                doorMesh.transform.position,
                targetPosition,
                slideSpeed * Time.deltaTime
            );
        }

        if (doorMesh.transform.position == targetPosition)
        {
            isOpening = false;
            this.enabled = false;
            Debug.Log("Door finished opening. Script disabled.");
        }
    }
}
