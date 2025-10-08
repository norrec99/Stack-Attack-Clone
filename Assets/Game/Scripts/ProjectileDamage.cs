using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ProjectileDamage : MonoBehaviour
{
    [SerializeField] private float damage = 5f;
    [SerializeField] private bool destroyOnHit = true;

    [SerializeField] private float speed = 25f;
    [SerializeField] private float lifeSeconds = 3f;

    private float life;

    private void OnEnable()
    {
        life = lifeSeconds;
    }

    private void Update()
    {
        transform.position += Vector3.forward * (speed * Time.deltaTime);

        life -= Time.deltaTime;
        if (life <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Find the StackEnemy even if we hit a child block
        StackEnemy stack = other.GetComponentInParent<StackEnemy>();
        if (stack != null)
        {
            stack.TakeTopDamage(damage);

            if (destroyOnHit == true)
            {
                Destroy(gameObject);
            }
            return;
        }
    }
}
