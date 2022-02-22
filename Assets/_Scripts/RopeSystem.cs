using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(GrappleController))]
[RequireComponent(typeof(DistanceJoint2D))]
public class RopeSystem : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private DistanceJoint2D joint;
    private GrappleController grappleController;
    private LayerMask terrainMask;
    public GameObject anchor { get; private set; }

    private Vector2 ropeOrigin { get => grappleController.hook.ropeOrigin; }
    private Vector2 playerPos { get => (Vector2) this.transform.position; }
    private Vector2 anchorPos { get => (Vector2) anchor.transform.position; }
    private List<Vector3> ropePositions = new List<Vector3>();
    private Dictionary<Vector3, int> wrapPoints = new Dictionary<Vector3, int>();
    private bool distanceSet;
    [SerializeField] public float ropeLength { get; private set; } = 0f;

    [SerializeField]
    private FloatValue grappleRange;
    private float ropeCollisionOffset = 0.25f;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        joint = GetComponent<DistanceJoint2D>();
        grappleController = GetComponent<GrappleController>();
        terrainMask = LayerMask.GetMask("Collidable Terrain");

        // Set up anchor point for DistanceJoint2D
        anchor = new GameObject("Anchor", typeof(Rigidbody2D));
        anchor.SetActive(false);
        Rigidbody2D anchorRb = anchor.GetComponent<Rigidbody2D>();
        anchorRb.isKinematic = true;
        joint.connectedBody = anchorRb;
    }

    public void OnGrappleAttached(Vector2 point)
    {
        UpdateAnchor(point);
    }

    public void OnGrappleReleased()
    {
        Reset();
    }

    private void FixedUpdate()
    {
        // Render
        lineRenderer.positionCount = ropePositions.Count;
        lineRenderer.SetPositions(ropePositions.ToArray());

        ropePositions.Clear();

        if (grappleController.state == GrappleState.Inactive) {
            lineRenderer.positionCount = 0;
            return;
        }

        // Add start point
        ropePositions.Add(ropeOrigin);
        // Add wrapped points
        Wrap();
        ropePositions.AddRange(wrapPoints.Keys);
        // Add end point
        ropePositions.Add(playerPos);
        // Remove any rope positions that have come unwrapped
        Unwrap();

        ropeLength = CalculateRopeLength();
    }

    private void Wrap()
    {
        if (grappleController.state != GrappleState.Attached) return;

        float length = (playerPos - ropeOrigin).magnitude;
        if (length > ropeCollisionOffset) {
            // Check for when the rope would pass through an object, but offset the ray from the player and the anchor point so we don't cause
            // it to get a hit when the rope is really short and the player is within floating-point error distance of the terrain.
            Vector2 rayOrigin = playerPos - ((playerPos - anchorPos).normalized * ropeCollisionOffset);
            Vector2 rayDestination = anchorPos - ((anchorPos - playerPos).normalized * ropeCollisionOffset);
            RaycastHit2D hit = Physics2D.Linecast(rayOrigin, rayDestination, terrainMask);
            if (hit.collider != null) {
                Vector2 closestVertex = FindClosestVertexOnCompositeCollider2D(hit);
                if (wrapPoints.ContainsKey(closestVertex)) {
                    Reset();
                    return;
                }

                distanceSet = false;
                wrapPoints.Add(closestVertex, 0);
                UpdateAnchor(closestVertex);
            }
        }
    }

    private void Unwrap()
    {
        if (ropePositions.Count <= 2) return;

        int anchorIndex = ropePositions.Count - 3;
        int hingeIndex = ropePositions.Count - 2;
        Vector2 anchorPosition = ropePositions[anchorIndex];
        Vector2 hingePosition = ropePositions[hingeIndex];
        Vector2 hingeDir = hingePosition - anchorPosition;
        float hingeAngle = Vector2.Angle(anchorPosition, hingeDir);
        Vector2 playerDir = playerPos - anchorPosition;
        float playerAngle = Vector2.Angle(anchorPosition, playerDir);

        if (!wrapPoints.ContainsKey(hingePosition)) {
            Debug.LogError("We were not tracking hingePosition (" + hingePosition + ") in the look up dictionary.");
            return;
        }

        if (playerAngle <= hingeAngle) {
            if (wrapPoints[hingePosition] == 1) {
                UnwrapRopePosition(anchorIndex, hingeIndex);
                return;
            }
            wrapPoints[hingePosition] = -1;
        } else {
            if (wrapPoints[hingePosition] == -1) {
                UnwrapRopePosition(anchorIndex, hingeIndex);
                return;
            }
            wrapPoints[hingePosition] = 1;
        }
    }

    private void UnwrapRopePosition(int anchorIndex, int hingeIndex)
    {
        distanceSet = false;
        UpdateAnchor(ropePositions[anchorIndex]);
        wrapPoints.Remove(ropePositions[hingeIndex]);
    }

    private void UpdateAnchor(Vector2 newAnchorPoint)
    {
        anchor.SetActive(true);
        joint.enabled = true;

        anchor.transform.position = newAnchorPoint;

        if (!distanceSet) {
            float desiredDistance = Vector2.Distance(playerPos, newAnchorPoint) - 0.25f;
            joint.distance = Mathf.Max(desiredDistance, 0.25f);
            distanceSet = true;
        }
    }

    public void Reset()
    {
        joint.enabled = false;

        anchor.SetActive(false);

        distanceSet = false;

        ropePositions.Clear();
        wrapPoints.Clear();

        grappleController.Deactivate();
    }



    private Vector2 FindClosestVertexOnCompositeCollider2D(RaycastHit2D hit)
    {
        Vector2 _closestVertex = Vector2.zero;
        CompositeCollider2D collider = hit.collider.GetComponent<CompositeCollider2D>();
        float minDistanceSqr = Mathf.Infinity;
        for (int i = 0; i < collider.pathCount; i++){
            Vector2[] pathVerts = new Vector2[collider.GetPathPointCount(i)];
            collider.GetPath(i, pathVerts);
            for (int j = 0; j < pathVerts.Length; j++) {
                Vector2 diff = collider.ClosestPoint(hit.point) - pathVerts[j];
                float distSqr = diff.sqrMagnitude;
                if (distSqr < minDistanceSqr) {
                    minDistanceSqr = distSqr;
                    _closestVertex = pathVerts[j];
                }
            }
        }
        return _closestVertex;
    }

    private float CalculateRopeLength()
    {
        float length = 0f;

        for (int i = 1; i < ropePositions.Count; i++) {
            length += Vector2.Distance(ropePositions[i - 1], ropePositions[i]);
        }

        return length;
    }
}
