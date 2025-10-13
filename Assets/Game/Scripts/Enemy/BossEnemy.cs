using UnityEngine;

[RequireComponent(typeof(Health))]
public class BossEnemy : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField] private float moveSpeedZ = -1.2f; // toward player if +Z is “forward”
    [SerializeField] private float stopAtZ = 4f;       // stop advancing when close

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void Update()
    {
        if (GameState.IsGameOver) return;

        Vector3 p = transform.position;

        if ((moveSpeedZ < 0f && p.z > stopAtZ) || (moveSpeedZ > 0f && p.z < stopAtZ))
        {
            p.z += moveSpeedZ * Time.deltaTime;
            transform.position = p;
        }
    }

    public Health GetHealth()
    {
        return health;
    }
}
