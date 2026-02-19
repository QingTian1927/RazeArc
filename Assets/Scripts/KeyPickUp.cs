using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPickUp : MonoBehaviour
{
    public string keyColor;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInventory inventory = GetComponent<PlayerInventory>();

            if (inventory != null)
            {
                if (keyColor == "Red") inventory.hasRedKey = true;
                if (keyColor == "Blue") inventory.hasBlueKey = true;
                if (keyColor == "Green") inventory.hasGreenKey = true;

                Debug.Log("Player picked up the " + keyColor + " key!");

                Destroy(gameObject);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
