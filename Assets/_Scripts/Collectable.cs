using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class Collectable : MonoBehaviour
{
    private Animator animator;
    private AudioSource audioSource;
    private AudioClip pickupSound;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        pickupSound = Resources.Load<AudioClip>("pickup");
    }

    void OnTriggerEnter2D(Collider2D target)
    {
        Debug.Log("OnTriggerEnter2D");
        animator.SetTrigger("collect");
        audioSource.clip = pickupSound;
        if (!audioSource.isPlaying) {
            Debug.Log("play");
            audioSource.Play();
        }
    }
}
