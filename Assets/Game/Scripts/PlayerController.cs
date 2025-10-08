using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Camera mainCam;
    [SerializeField] private float sensitivity = 1f;
    [SerializeField] private float smoothTime = 0.06f;
    [SerializeField] private float extraEdgePaddingWorld = 0.1f; // optional margin beyond half width

    private float minX;
    private float maxX;

    private bool isDragging;
    private float targetX;
    private float xVelocity;
    private float startWorldX;
    private float startPlayerX;
    private float dragPlaneY;

    private void Awake()
    {
        if (mainCam == null)
        {
            mainCam = Camera.main;
        }
    }

    private void Start()
    {
        CalculateBounds();
        targetX = transform.position.x;
    }

    private void Update()
    {
        HandleInput();
        ApplyMovement();
    }

    private void CalculateBounds()
    {

        dragPlaneY = transform.position.y;
        Plane plane = new Plane(Vector3.up, new Vector3(0f, dragPlaneY, 0f));

        float sampleY = Screen.height * 0.35f;
        float enter;

        // Project left edge
        Ray leftRay = mainCam.ScreenPointToRay(new Vector3(0f, sampleY, 0f));
        if (plane.Raycast(leftRay, out enter) == true)
        {
            minX = leftRay.GetPoint(enter).x;
        }

        // Project right edge
        Ray rightRay = mainCam.ScreenPointToRay(new Vector3(Screen.width, sampleY, 0f));
        if (plane.Raycast(rightRay, out enter) == true)
        {
            maxX = rightRay.GetPoint(enter).x;
        }

        // Shrink by player half width so the visible mesh never leaves screen
        float halfWidth = GetPlayerHalfWidthX();
        float pad = halfWidth + extraEdgePaddingWorld;

        minX += pad;
        maxX -= pad;

        // Safety in edge cases
        if (minX > maxX)
        {
            float mid = (minX + maxX) * 0.5f;
            minX = mid - 0.01f;
            maxX = mid + 0.01f;
        }

        targetX = Mathf.Clamp(targetX, minX, maxX);
    }

    private float GetPlayerHalfWidthX()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers != null && renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;

            for (int i = 1; i < renderers.Length; i++)
            {
                b.Encapsulate(renderers[i].bounds);
            }

            return b.extents.x;
        }

        Collider[] colliders = GetComponentsInChildren<Collider>();
        if (colliders != null && colliders.Length > 0)
        {
            Bounds b = colliders[0].bounds;

            for (int i = 1; i < colliders.Length; i++)
            {
                b.Encapsulate(colliders[i].bounds);
            }

            return b.extents.x;
        }

        return 0.5f;
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            BeginDrag(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            if (isDragging == true)
            {
                ContinueDrag(Input.mousePosition);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging == true)
            {
                isDragging = false;
            }
        }
    }

    private void BeginDrag(Vector2 screenPos)
    {
        isDragging = true;

        Vector3 world = ScreenToWorldOnPlane(screenPos, dragPlaneY);
        startWorldX = world.x;
        startPlayerX = transform.position.x;
    }

    private void ContinueDrag(Vector2 screenPos)
    {
        Vector3 world = ScreenToWorldOnPlane(screenPos, dragPlaneY);
        float worldDeltaX = (world.x - startWorldX) * sensitivity;

        float desired = startPlayerX + worldDeltaX;
        targetX = Mathf.Clamp(desired, minX, maxX);
    }

    private void ApplyMovement()
    {
        Vector3 pos = transform.position;

        if (smoothTime > 0.0001f)
        {
            float newX = Mathf.SmoothDamp(pos.x, targetX, ref xVelocity, smoothTime);
            pos.x = newX;
        }
        else
        {
            pos.x = targetX;
        }

        transform.position = pos;
    }

    private Vector3 ScreenToWorldOnPlane(Vector2 screenPos, float planeY)
    {
        Ray ray = mainCam.ScreenPointToRay(screenPos);
        Plane plane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));
        float enter;

        if (plane.Raycast(ray, out enter) == true)
        {
            return ray.GetPoint(enter);
        }

        return transform.position;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;

        Vector3 a = new Vector3(minX, transform.position.y, transform.position.z - 5f);
        Vector3 b = new Vector3(minX, transform.position.y, transform.position.z + 5f);
        Vector3 c = new Vector3(maxX, transform.position.y, transform.position.z - 5f);
        Vector3 d = new Vector3(maxX, transform.position.y, transform.position.z + 5f);

        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(c, d);
    }
}


