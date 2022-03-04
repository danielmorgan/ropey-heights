using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GrappleController))]
[RequireComponent(typeof(RopeSystem))]
[RequireComponent(typeof(DistanceJoint2D))]
[RequireComponent(typeof(AnimationStateController))]
public class MovementController : MonoBehaviour
{
    [SerializeField]
    private LayerMask terrainMask;
    [SerializeField]
    private FloatValue maxRopeLength;

    private float runSpeed = 15f;
    private float runAcceleration = 4f;
    private float runDeceleration = 1f;
    private float swingForce = 10f;
    private float rappelSpeed = 0.15f;
    private float jumpForce = 13f;

    private Rigidbody2D rb;
    private GrappleController grappleController;
    private RopeSystem ropeSystem;
    private DistanceJoint2D joint;
    private AnimationStateController animationStateController;

    private Vector2 playerPos { get => (Vector2) this.transform.position; }
    private Vector2 anchorPos { get => (Vector2) ropeSystem.anchor.transform.position; }
    private float x;
    private float y;
    
    private bool anchorBelowPlayer;

    private bool grounded;
    private float shouldJumpBuffer;

    public UnityEvent Landed;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        grappleController = GetComponentInParent<GrappleController>();
        ropeSystem = GetComponentInParent<RopeSystem>();
        joint = GetComponent<DistanceJoint2D>();
        animationStateController = GetComponent<AnimationStateController>();
    }

    public void HandleX(InputAction.CallbackContext context)
    {
        if (context.started) {
            return;
        }

        if (context.performed) {
            x = context.ReadValue<float>();
            return;
        }

        if (context.canceled) {
            x = 0;
            return;
        }
    }

    public void HandleY(InputAction.CallbackContext context)
    {
        if (!context.performed) {
            y = 0;
            return;
        }

        y = context.ReadValue<float>();
    }

    public void HandleRelease(InputAction.CallbackContext context)
    {
        if (context.performed) {
            ropeSystem.Reset();
            grappleController.Release();
        }
    }

    public void HandleJump(InputAction.CallbackContext context)
    {
        if (grappleController.state == GrappleState.Attached) {
            ropeSystem.Reset();
            grappleController.Release();
        }

        if (context.performed) {
            shouldJumpBuffer = 0.2f;
        }
    }

    private void FixedUpdate()
    {
        CheckGrounded();

        Jump();
     
        if (grappleController.state == GrappleState.Attached && x != 0) {
            Swing(x);
        }
        if (grappleController.state == GrappleState.Attached && y != 0) {
            Rappel(y);
        }
        if (grounded) {
            Run(x);
        }
        if (grappleController.state != GrappleState.Attached && !grounded) {
            ControlInAir(x);
        }

        animationStateController.inputX = x;
        animationStateController.velocity = rb.velocity;
        animationStateController.normalisedRunSpeed = rb.velocity.x / runSpeed;
    }

    private void Jump()
    {
        if (shouldJumpBuffer > 0) {
            shouldJumpBuffer -= Time.fixedDeltaTime;
            if (grounded) {
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                shouldJumpBuffer = 0;
            }
        } else {
            shouldJumpBuffer = 0;
        }
    }

    private void CheckGrounded()
    {
        bool wasGrounded = grounded;
        grounded = false;

        Collider2D collider = Physics2D.OverlapCircle(playerPos + Vector2.down, 0.25f, terrainMask);
        if (collider != null) {
            grounded = true;
            if (!wasGrounded && grounded) {
                Landed.Invoke();
            }
        }
    }

    private void Run(float x)
    {
        float targetSpeed = x * runSpeed;
        float speedDiff = targetSpeed - rb.velocity.x;
        float acceleration = Mathf.Abs(targetSpeed) > 0 ? runAcceleration : runDeceleration;
        float force = Mathf.Pow(Mathf.Abs(speedDiff) * acceleration, 0.95f) * Mathf.Sign(speedDiff);
        rb.AddForce(Vector2.right * force);
    }

    private void ControlInAir(float x)
    {
        float targetSpeed = x * (runSpeed / 1);
        float speedDiff = targetSpeed - rb.velocity.x;
        float acceleration = Mathf.Abs(targetSpeed) > 0 ? runAcceleration : runDeceleration;
        float force = Mathf.Pow(Mathf.Abs(speedDiff) * acceleration, 0.95f) * Mathf.Sign(speedDiff);
        rb.AddForce(Vector2.right * force);
    }

    private void Swing(float x)
    {
        Vector2 direction = (anchorPos - playerPos).normalized;
        anchorBelowPlayer = Mathf.Sign(direction.y) > 0;
        Vector2 perpendicular = Vector2.zero;
        if (!anchorBelowPlayer && x > 0) {
            perpendicular = new Vector2(-direction.y, direction.x);
        } else if (!anchorBelowPlayer && x < 0) {
            perpendicular = new Vector2(direction.y, -direction.x);
        } else if (anchorBelowPlayer && x > 0) {
            perpendicular = new Vector2(direction.y, -direction.x);
        } else if (anchorBelowPlayer && x < 0) {
            perpendicular = new Vector2(-direction.y, direction.x);
        }
        // Debug.DrawRay(playerPos, perpendicular, Color.black);
        rb.AddForce(perpendicular * Mathf.Abs(x) * swingForce);
    }

    private void Rappel(float y)
    {
        Vector2 direction = (anchorPos - playerPos).normalized;
        anchorBelowPlayer = Mathf.Sign(direction.y) > 0;

        float desiredDistance = joint.distance + (y * rappelSpeed);

        // Lengthening
        if (desiredDistance > joint.distance) {
            // Stop if too long
            float ropeRemaining = maxRopeLength.value - ropeSystem.ropeLength;
            if (ropeRemaining <= 0) {
                return;
            }
            // Stop if colliding below and anchor is above
            if (grounded && anchorBelowPlayer) {
                return;
            }
        }

        // Shortening
        if (desiredDistance < joint.distance) {
            // Stop if too short
            if (desiredDistance < 0.5f) {
                return;
            }
        }

        joint.distance = desiredDistance;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(playerPos + Vector2.down, 0.25f);
    }
}
