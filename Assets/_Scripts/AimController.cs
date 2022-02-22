using UnityEngine;
using UnityEngine.InputSystem;

public class AimController : MonoBehaviour
{
    [SerializeField]
    private Camera mainCam;
    [SerializeField]
    private SpriteRenderer reticle;
    [SerializeField]
    private FloatValue grappleRange;

    private Vector2 playerPos { get => (Vector2) this.transform.position; }
    public Vector2 aimDirection { get; private set; }
    public Vector2? target { get; private set; }
    Vector2 aimPoint;
    
    private LayerMask terrainMask;

    void Awake()
    {
        terrainMask = LayerMask.GetMask("Collidable Terrain");
    }

    public void HandleAim(InputAction.CallbackContext context)
    {
        aimDirection = context.ReadValue<Vector2>();
        reticle.gameObject.SetActive(true);
    }

    public void HandlePointAim(InputAction.CallbackContext context)
    {
        aimPoint = context.ReadValue<Vector2>();
        aimPoint = mainCam.ScreenToWorldPoint(aimPoint);
        aimDirection = (aimPoint - playerPos).normalized;
        reticle.gameObject.SetActive(true);
    }

    private void Update()
    {
        // Not aiming
        if (aimDirection.magnitude < 0.1f) {
            aimDirection = Vector2.zero;
            target = null;
            reticle.gameObject.SetActive(false);
            return;
        }

        // Aiming at something hookable
        RaycastHit2D hit = Physics2D.Raycast(playerPos, aimDirection, grappleRange.value, terrainMask);
        if (hit.collider != null && hit.collider.gameObject.tag == "Hookable") {
            reticle.transform.position = hit.point;
            target = hit.point;
            Debug.DrawLine(playerPos, hit.point, Color.red);
            return;
        }

        // Aiming at nothing
        reticle.transform.position = playerPos + (aimDirection * grappleRange.value);
        target = null;
        Debug.DrawLine(playerPos, reticle.transform.position, Color.magenta);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(aimPoint, 0.25f);
    }
}
