using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyBlock : MonoBehaviour
{
    [SerializeField] private float baseHp = 10f;
    public Health Health { get; private set; }
    public bool IsAlive { get; private set; } = true;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Health = GetComponent<Health>();
        Health.SetMax(baseHp, true);
        Health.onDeath.AddListener(() =>
        {
            IsAlive = false;
        });
    }

    public void SetHp(float hp)
    {
        baseHp = hp;
        Health.SetMax(baseHp, true);
        IsAlive = true;
    }
}
