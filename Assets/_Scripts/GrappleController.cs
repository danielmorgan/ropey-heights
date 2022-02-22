using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public enum GrappleState {
    Inactive,
    Firing,
    Returning,
    Attached,
};

[RequireComponent(typeof(AimController))]
public class GrappleController : MonoBehaviour
{
    private AimController aimController;
    [SerializeField]
    private Hook hookPrefab;
    public Hook hook { get; private set; }
    [SerializeField]
    private FloatValue grappleRange;
    private float speed = 30f;

    public GrappleState state { get; private set; } = GrappleState.Inactive;
    
    private float t = 0;
    private Vector2 playerPos { get => (Vector2) this.transform.position; }
    private Vector2 origin;
    private Vector2 target;
    private bool shouldAttach;
    private float distance {
        get { return Vector2.Distance(origin, target); }
    }

    [Space]
    [Header("Events")]
    public UnityEvent<Vector2> GrappleAttached;
    public UnityEvent GrappleReleased;

    private void Awake()
    {
        aimController = GetComponent<AimController>();
        hook = Instantiate<Hook>(hookPrefab);
        hook.gameObject.SetActive(false);
    }

    public void HandleGrappleFired(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (state != GrappleState.Inactive) {
            Release();
        }

        if (aimController.aimDirection != Vector2.zero) {
            Fire();
        }
    }

    private void Update()
    {
        if (state == GrappleState.Inactive) return;

        // Animate hook
        if (state == GrappleState.Firing) {
            t += speed / distance * Time.deltaTime;
            hook.transform.position = Vector2.Lerp(origin, target, t);
            hook.transform.rotation = GetHookRotation(origin, target);
        }
        if (state == GrappleState.Returning) {
            t -= speed / distance * Time.deltaTime;
            hook.transform.position = Vector2.Lerp(playerPos, target, t);
            hook.transform.rotation = GetHookRotation(playerPos, target);
        }
        // Hook reached target
        if (state == GrappleState.Firing && shouldAttach && t >= 1) {
            Attach();
            return;
        }

        // Range reached without hitting anything
        if (state != GrappleState.Returning && Vector2.Distance(origin, hook.transform.position) >= grappleRange.value - 0.1f) {
            Return();
            return;
        }
        
        // Return finished
        if (state == GrappleState.Returning && Vector2.Distance(playerPos, hook.transform.position) <= 0.1f) {
            Deactivate();
            return;
        }
    }

    private void Fire()
    {
        hook.gameObject.SetActive(true);
        origin = playerPos;
        t = 0;

        if (aimController.target != null) {
            target = (Vector2) aimController.target;
            shouldAttach = true;
        } else {
            target = origin + (aimController.aimDirection * grappleRange.value);
            shouldAttach = false;
        }

        state = GrappleState.Firing;
    }

    private void Release()
    {
        GrappleReleased.Invoke();
        Return();
    }

    private void Return()
    {
        state = GrappleState.Returning;
    }

    private void Attach()
    {
        shouldAttach = false;
        GrappleAttached.Invoke(target);
        state = GrappleState.Attached;
    }

    public void Deactivate()
    {
        t = 0;
        hook.transform.localPosition = Vector2.zero;
        hook.gameObject.SetActive(false);
        state = GrappleState.Inactive;
    }

    private Quaternion GetHookRotation(Vector2 origin, Vector2 target)
    {
        Vector2 toTarget = target - origin;
        float angle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
        float correctedAngle = angle - 90f;
        return Quaternion.Euler(0, 0, correctedAngle);
    }
}
