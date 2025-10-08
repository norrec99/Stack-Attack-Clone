using UnityEngine;

public class StackEnemy : MonoBehaviour
{
    [Header("Stack Config")]
    [SerializeField] private EnemyBlock blockPrefab;
    [SerializeField] private int height = 6;
    [SerializeField] private float blockSpacingY = 0.1f;
    [SerializeField] private float hpBase = 10f;
    [SerializeField] private float hpPerRow = 2f;

    [Header("Motion (on Z axis)")]
    [SerializeField] private float moveSpeedZ = -2.0f; // negative = towards camera if +Z is away
    [SerializeField] private float despawnBeyondZ = -15f;

    private int aliveBlocks;

    private void Update()
    {
        // move along Z only
        transform.position += new Vector3(0f, 0f, moveSpeedZ * Time.deltaTime);

        if ((moveSpeedZ < 0f && transform.position.z < despawnBeyondZ) ||
            (moveSpeedZ > 0f && transform.position.z > despawnBeyondZ))
        {
            Destroy(gameObject);
        }
    }

    public void Build(int level)
    {
        // ensure y = 0 for the whole stack
        Vector3 p = transform.position;
        p.y = 0f;
        transform.position = p;

        // build vertical stack along Y, but gameplay plane stays Y=0
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        aliveBlocks = 0;
        for (int i = 0; i < height; i++)
        {
            EnemyBlock b = Instantiate(blockPrefab, transform);
            b.transform.localPosition = new Vector3(0f, i * blockSpacingY, 0f);

            float hp = hpBase + hpPerRow * (i + level);
            b.SetHp(hp);

            b.GetComponent<Health>().onDeath.AddListener(OnChildBlockDeath);
            aliveBlocks++;
        }
    }

    private void OnChildBlockDeath()
    {
        aliveBlocks--;
        if (aliveBlocks <= 0)
        {
            Destroy(gameObject);
        }
    }
}
