using UnityEngine;
using UnityEngine.InputSystem;

public enum HookState {
    Disabled,
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
    private float speed = 4f;
    [SerializeField]
    private float maxDistance = 5f;

    public HookState state { get; private set; } = HookState.Disabled;

    private float t;
    private Vector2 initialHookPosition;
    private Vector2 origin;
    private Vector2 target;
    private bool shouldAttach;

    private void Awake() {
        aimController = GetComponent<AimController>();
        initialHookPosition = hook.transform.position;
    }

    public void HandleGrappleFired(InputAction.CallbackContext context)
    {
        if (context.performed) {
            if (state != HookState.Disabled) {
                Cancel();
            }
            Fire();
        }
    }

    private void Update() {
        if (state == HookState.Attached) return;

        if (state != HookState.Returning) {
            t += speed * Time.deltaTime;
        } else {
            t -= speed * Time.deltaTime;
        }

        hook.transform.position = Vector2.Lerp(origin, target, t);

        if (shouldAttach && t >= 1) {
            state = HookState.Attached;
            return;
        }
        
        if (state != HookState.Returning && Vector2.Distance(origin, hook.transform.position) >= maxDistance - 0.1f) {
            state = HookState.Returning;
        } else if (state == HookState.Returning && Vector2.Distance(origin, hook.transform.position) <= 0.1f) {
            Cancel();
        }
    }

    private void Fire()
    {
        hook.gameObject.SetActive(true);
        state = HookState.Firing;
        origin = hook.transform.position;
        shouldAttach = true;
        target = origin + (aimController.aimDirection * maxDistance);
        
        Debug.DrawLine(origin, target, Color.black, 1f);
    }

    private void Cancel()
    {
        hook.transform.position = initialHookPosition;
        hook.gameObject.SetActive(false);
        state = HookState.Disabled;
    }
}
