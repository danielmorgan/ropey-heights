using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ScrollingBackground : MonoBehaviour
{
    private float speed = 0.005f;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        Vector2 offset = new Vector2(Time.time * speed, 0);
        spriteRenderer.material.mainTextureOffset = offset;
    }
}
