using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GrappleController))]
[RequireComponent(typeof(RopeSystem))]
[RequireComponent(typeof(DistanceJoint2D))]
public class MovementController : MonoBehaviour
{
    private float swingForce = 15f;
    private float rappelSpeed = 0.2f;
    private float pushOffForce = 15f;
    [SerializeField]
    private FloatValue maxRopeLength;

    private Rigidbody2D rb;
    private GrappleController grappleController;
    private RopeSystem ropeSystem;
    private DistanceJoint2D joint;

    private Vector2 playerPos { get => (Vector2) this.transform.position; }
    private Vector2 anchorPos { get => (Vector2) ropeSystem.anchor.transform.position; }
    private float x;
    private float y;
    
    private bool anchorBelowPlayer;
    private float shouldJumpBuffer;

    private Vector2 collisionPoint;
    private Vector2 collisionNormal;
    private float collidedRecently;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        grappleController = GetComponentInParent<GrappleController>();
        ropeSystem = GetComponentInParent<RopeSystem>();
        joint = GetComponent<DistanceJoint2D>();
    }

    public void HandleSwing(InputAction.CallbackContext context)
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

    public void HandleRappel(InputAction.CallbackContext context)
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
        }
    }

    public void HandleJump(InputAction.CallbackContext context)
    {
        if (context.performed) {
            shouldJumpBuffer = 0.2f;
        }
    }

    private void FixedUpdate()
    {
        // Reset collisions
        collidedRecently -= Time.fixedDeltaTime;
        if (collidedRecently < 0) {
            collidedRecently = 0;
            collisionPoint = Vector2.zero;
            collisionNormal = Vector2.zero;
        }

        // Jump
        if (shouldJumpBuffer > 0) {
            shouldJumpBuffer -= Time.fixedDeltaTime;
            if (collisionNormal != Vector2.zero) {
                rb.AddForce(collisionNormal * pushOffForce, ForceMode2D.Impulse);
                shouldJumpBuffer = 0;
            }
        } else {
            shouldJumpBuffer = 0;
        }
     
        if (grappleController.state != GrappleState.Attached) {
            return;
        }

        // Swing
        if (x != 0) {
            Swing(x);
        }
        // rb.AddForce(rb.velocity * -0.1f);

        if (y != 0) {
            Rappel(y);
        }
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
        float desiredDistance = joint.distance + (y * rappelSpeed);

        // Lengthening
        if (desiredDistance > joint.distance) {
            // Stop if too long
            float ropeRemaining = maxRopeLength.value - ropeSystem.ropeLength;
            if (ropeRemaining <= 0) {
                return;
            }
            // Stop if colliding below
            if (collisionPoint != Vector2.zero && Vector2.Dot(Vector2.up, collisionPoint - playerPos) < 0) {
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

    private void OnCollisionEnter2D(Collision2D other)
    {
        collidedRecently = 0.5f;
        collisionPoint = other.contacts[0].point;
        collisionNormal = other.contacts[0].normal;
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        collisionPoint = Vector2.zero;
        collisionNormal = Vector2.zero;
    }

    // private void OnDrawGizmos() {
    //     Gizmos.color = Color.yellow;
    //     Gizmos.DrawWireSphere(collisionPoint, 0.5f);
    // }
}
