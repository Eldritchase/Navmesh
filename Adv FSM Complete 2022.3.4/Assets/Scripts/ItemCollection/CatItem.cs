using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatItem : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            FindObjectOfType<GameManager>().addCollectedItem();
            gameObject.SetActive(false);
        }
    }
}
