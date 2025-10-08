using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private bool destroyOnDeath = false;

    public UnityEvent onDamaged;
    public UnityEvent onDeath;

    private float current;

    private void Awake()
    {
        current = maxHealth;
    }

    public void SetMax(float value, bool fillCurrent = true)
    {
        maxHealth = Mathf.Max(0.01f, value);

        if (fillCurrent == true)
        {
            current = maxHealth;
        }
    }

    public void TakeDamage(float amount)
    {
        if (current <= 0f)
        {
            return;
        }

        current -= amount;

        if (current <= 0f)
        {
            current = 0f;
            onDeath?.Invoke();

            if (destroyOnDeath == true)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            onDamaged?.Invoke();
        }
    }
}
