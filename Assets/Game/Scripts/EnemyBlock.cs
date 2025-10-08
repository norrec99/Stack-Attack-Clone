using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyBlock : MonoBehaviour
{
    [SerializeField] private float baseHp = 10f;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
        health.SetMax(baseHp, true);
    }

    public void SetHp(float hp)
    {
        baseHp = hp;
        health.SetMax(baseHp, true);
    }
}
