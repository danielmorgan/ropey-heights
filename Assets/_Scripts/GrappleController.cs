using UnityEngine;
using UnityEngine.InputSystem;

public enum HookState {
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
    private SpriteRenderer hook;

    [SerializeField]
    private float speed = 25f;
    [SerializeField]
    private FloatValue grappleRange;

    [SerializeField]
    public HookState state { get; private set; } = HookState.Inactive;

    private float t = 0;
    private Vector2 initialHookPosition;
    private Vector2 origin;
    private Vector2 target;
    private bool shouldAttach;
    private float distance {
        get { return Vector2.Distance(origin, target); }
    }

    private void Awake() {
        aimController = GetComponent<AimController>();
        initialHookPosition = hook.transform.position;
    }

    public void HandleGrappleFired(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (state != HookState.Inactive) {
            state = HookState.Returning;
            return;
        }

        if (aimController.aimDirection != Vector2.zero) {
            Fire();
        }
    }

    private void Update() {
        if (state == HookState.Inactive) return;

        Debug.DrawLine(origin, target, Color.black);

        if (state == HookState.Firing) {
            t += speed / distance * Time.deltaTime;
        }
        if (state == HookState.Returning) {
            t -= speed / distance * Time.deltaTime;
        }

        hook.transform.position = Vector2.Lerp(origin, target, t);

        if (shouldAttach && t >= 1) {
            state = HookState.Attached;
            return;
        }

        if (state != HookState.Returning && Vector2.Distance(origin, hook.transform.position) >= grappleRange.value - 0.1f) {
            state = HookState.Returning;
        } else if (state == HookState.Returning && Vector2.Distance(origin, hook.transform.position) <= 0.1f) {
            Deactivate();
        }
    }

    private void Fire()
    {
        hook.gameObject.SetActive(true);
        state = HookState.Firing;
        origin = hook.transform.position;
        t = 0;

        if (aimController.target != null) {
            target = (Vector2) aimController.target;
            shouldAttach = true;
        } else {
            target = origin + (aimController.aimDirection * grappleRange.value);
            shouldAttach = false;
        }

        Vector2 toTarget = target - origin;
        float angle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
        float correctedAngle = angle - 90f;
        hook.transform.rotation = Quaternion.Euler(0, 0, correctedAngle);
    }

    private void Deactivate()
    {
        hook.transform.position = initialHookPosition;
        hook.gameObject.SetActive(false);
        state = HookState.Inactive;
    }
}
