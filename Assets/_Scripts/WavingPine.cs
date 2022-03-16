using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class WavingPine : MonoBehaviour
{
    private void Awake()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.material.SetFloat("_OffsetTime", Random.Range(0f, 2f));
    }
}