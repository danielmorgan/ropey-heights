using System.Collections;
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
    private float runDeceleration = 0.5f;
    private float swingForce = 55f;
    private float rappelAcceleration = 0.005f;
    private float rappelDeceleration = 0.005f;
    private float rappelMaxSpeed = 0.15f;
    private float jumpForce = 12f;
    private float defaultJumpBuffer = 0.1f;
    private float hangTime = 0.2f;

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
    private float defaultGravity;
    // private Tweener rappelTween;
    // private bool rappelLengthShouldChange;
    // private float desiredRappelDistance;
    // private float lastRappelInput;
    // private float currentDistance;
    // private float rappelSpeed;

    public UnityEvent Landed;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        defaultGravity = rb.gravityScale;
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

    public void HandleJump(InputAction.CallbackContext context)
    {
        if (context.performed && grappleController.state != GrappleState.Attached) {
            shouldJumpBuffer = defaultJumpBuffer;
        }
    }

    public void OnGrappleReleased()
    {
        if (rb.velocity.y > 3f) {
            StartCoroutine(LowerGravity());
        }
    }

    // private void Update()
    // {
    //     if (y != 0) {
    //         desiredRappelDistance = joint.distance + (y * rappelMaxSpeed);
    //     }
    // }

    private void FixedUpdate()
    {
        // DebugText.Instance.Set(rb.velocity.y.ToString(), rb.velocity.y > 3f ? Color.green : Color.white);
        // float magnitude = 1f / rb.velocity.magnitude;
        // DebugText.Instance.Set(magnitude.ToString("0"));

        CheckGrounded();

        Jump();
     
        if (grappleController.state == GrappleState.Attached && x != 0) {
            Swing(x);
        }
        if (grappleController.state == GrappleState.Attached) {
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

    // private void Update()
    // {
    //     float duration = 3f;
    //     float time = Mathf.Clamp(Mathf.Abs(lastRappelInput - Time.time), 0, 1);
    //     // time = Mathf.Sin(time * 0.5f * Mathf.PI);
    //     DebugText.Instance.Set(currentDistance.ToString() + " -> " + desiredRappelDistance.ToString() + ", " + time.ToString() + " = " + Mathf.Lerp(currentDistance, desiredRappelDistance, time));
    //     joint.distance = Mathf.Lerp(currentDistance, desiredRappelDistance, time);
    // }

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
        // Make the swing stronger when we're moving slower, so that we can build up momentum quicker from a stand still,
        // but limit how much force we can apply when moving fast. This should have the effect of applying the most force
        // at the extreme positions, like leaning on a swing when you're at the highest point.
        float modifiedSwingForce = Mathf.Clamp((1f / rb.velocity.magnitude), 0f, 1f) * swingForce;
        rb.AddForce(perpendicular * Mathf.Abs(x) * modifiedSwingForce);
    }

    private void Rappel(float y)
    {
        // if (Mathf.Abs(y) > 0) {
        //     rappelSpeed += rappelAcceleration;
        // } else {
        //     rappelSpeed -= rappelDeceleration;
        // }
        // rappelSpeed = Mathf.Clamp(rappelSpeed, 0, rappelMaxSpeed);
        // DebugText.Instance.Set(rappelSpeed.ToString());
        float desiredDistance = joint.distance + (y * rappelMaxSpeed);

        // Handle input
        Vector2 direction = (anchorPos - playerPos).normalized;
        anchorBelowPlayer = Mathf.Sign(direction.y) > 0;

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
        // joint.distance = Mathf.Lerp(joint.distance, desiredRappelDistance, rappelSpeed);

        // desiredRappelDistance = desiredDistance;
        // currentDistance = joint.distance;

        // if (currentDistance != desiredRappelDistance) {

            
        //     float time = Mathf.Clamp(Mathf.Abs(lastRappelInput - Time.time), 0, 1);
        //     time = Mathf.Sin(time * 0.5f * Mathf.PI);
        //     DebugText.Instance.Set(currentDistance.ToString() + " -> " + desiredRappelDistance.ToString() + ", " + time.ToString() + " = " + Mathf.Lerp(currentDistance, desiredRappelDistance, time));
        //     joint.distance = Mathf.Lerp(currentDistance, desiredRappelDistance, time);

        //     // float duration = 3f;
        //     // float time = Mathf.Clamp(Mathf.Abs(lastRappelInput - Time.time), 0, 1);
        //     // // time = Mathf.Sin(time * 0.5f * Mathf.PI);
        //     // DebugText.Instance.Set(joint.distance.ToString() + " -> " + desiredDistance.ToString() + ", " + time.ToString() + " = " + Mathf.Lerp(joint.distance, desiredDistance, time));
        //     // joint.distance = Mathf.Lerp(joint.distance, desiredDistance, time);
        // }
    }

    private IEnumerator LowerGravity()
    {
        rb.gravityScale = defaultGravity / 2f;
        yield return new WaitForSeconds(hangTime);
        rb.gravityScale = defaultGravity;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(playerPos + Vector2.down, 0.25f);
    }
}
