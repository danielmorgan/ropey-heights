using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GrappleController))]
[RequireComponent(typeof(RopeSystem))]
[RequireComponent(typeof(DistanceJoint2D))]
public class MovementController : MonoBehaviour
{
    private float swingForce = 2f;
    private float rappelSpeed = 3f;

    private Rigidbody2D rb;
    private GrappleController grappleController;
    private RopeSystem ropeSystem;
    private DistanceJoint2D joint;

    private Vector2 playerPos { get => (Vector2) this.transform.position; }
    private Vector2 anchorPos { get => (Vector2) ropeSystem.anchor.transform.position; }
    private float x;
    private float y;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        grappleController = GetComponentInParent<GrappleController>();
        ropeSystem = GetComponentInParent<RopeSystem>();
        joint = GetComponent<DistanceJoint2D>();
    }

    public void HandleSwing(InputAction.CallbackContext context)
    {
        if (!context.performed) {
            x = 0;
            return;
        }

        x = context.ReadValue<float>();
    }

    public void HandleRappel(InputAction.CallbackContext context)
    {
        if (!context.performed) {
            y = 0;
            return;
        }

        y = context.ReadValue<float>();
    }

    private void Update()
    {
        if (grappleController.state != GrappleState.Attached) return;

        // Swing
        Vector2 direction = (anchorPos - playerPos).normalized;
        Vector2 perpendicular;
        if (x > 0) {
            perpendicular = new Vector2(direction.y, -direction.x);
        } else {
            perpendicular = new Vector2(-direction.y, direction.x);
        }
        Debug.DrawRay(playerPos, perpendicular, Color.black);
        rb.AddForce(perpendicular * Mathf.Abs(x) * swingForce);

        // Rappel
        joint.distance -= y * rappelSpeed * Time.deltaTime;
    }
}
