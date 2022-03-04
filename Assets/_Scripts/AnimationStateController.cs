using UnityEngine;

enum AnimationState
{
    IDLE,
    RUNNING,
    SWINGING,
    JUMPING,
    FALLING
}

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(TrailRenderer))]
public class AnimationStateController : MonoBehaviour
{
    private AnimationState state = AnimationState.IDLE;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private TrailRenderer trailRenderer;
    
    public float inputX { private get; set; }
    public Vector2 velocity { private get; set; }
    public float normalisedRunSpeed { private get; set; }
    private bool grounded = true;
    private bool attached;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    public void OnGrappleAttached()
    {
        grounded = false;
        animator.SetBool("grounded", grounded);
        attached = true;
        animator.SetBool("attached", attached);
        trailRenderer.enabled = true;
    }

    public void OnGrappleReleased()
    {
        attached = false;
        animator.SetBool("attached", false);
    }

    public void OnLanded()
    {
        grounded = true;
        animator.SetBool("grounded", grounded);
        trailRenderer.enabled = false;
    }

    private void Update()
    {
        FlipSprite();

        animator.SetFloat("normalisedRunSpeed", Mathf.Abs(normalisedRunSpeed));
        animator.SetFloat("y", velocity.y);
    }

    private void FlipSprite()
    {
        if (!spriteRenderer.flipX && inputX < 0f) {
            spriteRenderer.flipX = true;
        } else if (spriteRenderer.flipX && inputX > 0f) {
            spriteRenderer.flipX = false;
        }
    }
}
