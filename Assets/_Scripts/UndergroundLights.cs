using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndergroundLights : MonoBehaviour
{
    [SerializeField]
    private BoxCollider2D trigger;
    [SerializeField]
    private GameObject lights;

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (GameObject.ReferenceEquals(trigger.gameObject, other.gameObject)) {
            lights.SetActive(false);
        }
    }
}
