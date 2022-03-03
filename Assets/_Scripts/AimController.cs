using UnityEngine;
using UnityEngine.InputSystem;

public class AimController : MonoBehaviour
{
    [SerializeField]
    private Camera mainCam;
    [SerializeField]
    private SpriteRenderer reticle;
    [SerializeField]
    private LineRenderer aimLine;
    [SerializeField]
    private FloatValue grappleRange;
    [SerializeField]
    private Color defaultLineColor = Color.white;
    [SerializeField]
    private Color validTargetLineColor = Color.green;

    private Vector2 playerPos { get => (Vector2) this.transform.position; }
    public Vector2 aimDirection { get; private set; }
    public Vector2? target { get; private set; }
    private Vector2 aimPoint;
    
    private LayerMask terrainMask;
    private Material aimLineMaterial;

    private void Awake()
    {
        terrainMask = LayerMask.GetMask("Attachable Terrain");
        aimLineMaterial = aimLine.GetComponent<Renderer>().material;
    }

    public void HandleAim(InputAction.CallbackContext context)
    {
        aimDirection = context.ReadValue<Vector2>();
        reticle.gameObject.SetActive(true);
        aimLine.gameObject.SetActive(true);
    }

    public void HandlePointAim(InputAction.CallbackContext context)
    {
        aimPoint = context.ReadValue<Vector2>();
        aimPoint = mainCam.ScreenToWorldPoint(aimPoint);
        aimDirection = (aimPoint - playerPos).normalized;
        reticle.gameObject.SetActive(true);
        aimLine.gameObject.SetActive(true);
    }

    private void Update()
    {
        // Not aiming
        if (aimDirection.magnitude < 0.1f) {
            aimDirection = Vector2.zero;
            target = null;
            reticle.gameObject.SetActive(false);
            aimLine.gameObject.SetActive(false);
            aimLine.positionCount = 0;
            return;
        }

        // Aiming at something hookable
        RaycastHit2D hit = Physics2D.Raycast(playerPos, aimDirection, grappleRange.value, terrainMask);
        // if (hit.collider != null && hit.collider.gameObject.tag == "Hookable") {
        if (hit.collider != null) {
            reticle.transform.position = hit.point;
            aimLine.positionCount = 2;
            aimLine.SetPosition(0, playerPos);
            aimLine.SetPosition(1, hit.point);
            aimLineMaterial.SetColor("_Color", validTargetLineColor);
            target = hit.point;
            return;
        }

        // Aiming at nothing
        reticle.transform.position = playerPos + (aimDirection * grappleRange.value);
        aimLine.positionCount = 2;
        aimLine.SetPosition(0, playerPos);
        aimLine.SetPosition(1, reticle.transform.position);
        aimLineMaterial.SetColor("_Color", defaultLineColor);
        target = null;
    }
}
