using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GrappleController))]
[RequireComponent(typeof(RopeSystem))]
[RequireComponent(typeof(DistanceJoint2D))]
public class MovementController : MonoBehaviour
{
    private float swingForce = 500f;
    private float rappelSpeed = 5f;
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

    private void Update()
    {
        if (grappleController.state != GrappleState.Attached) return;

        Vector2 direction = (anchorPos - playerPos).normalized;
        anchorBelowPlayer = Mathf.Sign(direction.y) > 0;

        // Swing
        if (x != 0) {
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
            Debug.DrawRay(playerPos, perpendicular, Color.black);
            rb.AddForce(perpendicular * Mathf.Abs(x) * swingForce * Time.deltaTime);
        }
        // rb.AddForce(rb.velocity * -0.1f);

        // Rappel
        if (y != 0) {
            float desiredDistance = joint.distance;
            desiredDistance += (y * rappelSpeed * Time.deltaTime);
            joint.distance = Mathf.Clamp(desiredDistance, 0.2f, maxRopeLength.value);
        }
    }
}
