using UnityEngine;
using UnityEngine.InputSystem;

public class AimController : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer reticle;

    public Vector2 aimDirection { get; private set; }
    
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

    private void Update()
    {
        // Not aiming
        if (aimDirection.magnitude < 0.1f) {
            aimDirection = Vector2.zero;
            reticle.gameObject.SetActive(false);
            return;
        }

        // Aiming at terrain
        RaycastHit2D hit = Physics2D.Raycast(transform.position, aimDirection, 999f, terrainMask);
        if (hit.collider != null && hit.collider.gameObject.tag == "Hookable") {
            reticle.transform.position = hit.point;
            Debug.DrawLine(transform.position, hit.point, Color.red);
            return;
        }

        // Aiming at blank space
        reticle.transform.position = (Vector2) transform.position + (aimDirection * 5f);
        Debug.DrawLine(transform.position, reticle.transform.position, Color.red);
    }
}
