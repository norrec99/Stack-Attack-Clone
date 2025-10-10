using UnityEngine;

public class WeaponAutoFire : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform muzzle;                  // where the projectile spawns
    [SerializeField] private GameObject projectilePrefab;       // prefab with ProjectileForwardZ

    [Header("Firing")]
    [SerializeField] private float fireRate = 2f;               // shots per second
    [SerializeField] private bool fireOnPressInstantly = true;  // first shot fires immediately on touch

    private float cooldownTimer;
    private bool isHolding;

    private void OnEnable()
    {
        cooldownTimer = 0f;
        isHolding = false;
    }

    private void Update()
    {
        if (GameState.IsGameOver == true)
        {
            return;
        }
        
        UpdateHoldState();
        TickCooldown();

        if (isHolding == true)
        {
            TryFire();
        }
    }

    private void UpdateHoldState()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        if (Input.GetMouseButtonDown(0) == true)
        {
            isHolding = true;

            if (fireOnPressInstantly == true)
            {
                cooldownTimer = 0f;
            }
        }

        if (Input.GetMouseButtonUp(0) == true)
        {
            isHolding = false;
        }
#endif

        if (Application.isMobilePlatform == true)
        {
            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);

                if (t.phase == TouchPhase.Began)
                {
                    isHolding = true;

                    if (fireOnPressInstantly == true)
                    {
                        cooldownTimer = 0f;
                    }
                }

                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    // if there are still other touches, keep holding
                    if (Input.touchCount <= 1)
                    {
                        isHolding = false;
                    }
                }
            }
            else
            {
                isHolding = false;
            }
        }
    }

    private void TickCooldown()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer < 0f)
            {
                cooldownTimer = 0f;
            }
        }
    }

    private void TryFire()
    {
        if (cooldownTimer > 0f)
        {
            return;
        }

        FireOne();

        if (fireRate <= 0.01f)
        {
            cooldownTimer = 0.25f;
        }
        else
        {
            cooldownTimer = 1f / fireRate;
        }
    }

    private void FireOne()
    {
        if (projectilePrefab == null)
        {
            return;
        }

        Transform spawn = muzzle != null ? muzzle : transform;

        GameObject projectile = Instantiate(projectilePrefab, spawn.position, Quaternion.identity);
    }
}
