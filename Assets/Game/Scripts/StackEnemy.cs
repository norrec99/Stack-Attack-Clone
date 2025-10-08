using System.Collections.Generic;
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
    [SerializeField] private float moveSpeedZ = -2.0f;
    [SerializeField] private float despawnBeyondZ = -15f;

    private readonly List<EnemyBlock> blocks = new List<EnemyBlock>();
    private int aliveBlocks;

    private void Update()
    {
        transform.position += new Vector3(0f, 0f, moveSpeedZ * Time.deltaTime);

        if ((moveSpeedZ < 0f && transform.position.z < despawnBeyondZ) ||
            (moveSpeedZ > 0f && transform.position.z > despawnBeyondZ))
        {
            Destroy(gameObject);
        }
    }

    public void Build(int level)
    {
        Vector3 p = transform.position;
        p.y = 0f;
        transform.position = p;

        // clear old
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        blocks.Clear();
        aliveBlocks = 0;

        // spawn from bottom to top (y increases upward)
        for (int i = 0; i < height; i++)
        {
            EnemyBlock b = Instantiate(blockPrefab, transform);
            b.transform.localPosition = new Vector3(0f, i * blockSpacingY, 0f);
            float hp = hpBase + hpPerRow * (i + level);
            b.SetHp(hp);

            // count alive on death
            b.Health.onDeath.AddListener(OnChildBlockDeath);

            blocks.Add(b);
            aliveBlocks++;
        }

        // ensure list is sorted by height (highest last)
        blocks.Sort((a, b) => a.transform.localPosition.y.CompareTo(b.transform.localPosition.y));
    }

    private void OnChildBlockDeath()
    {
        aliveBlocks--;
        if (aliveBlocks <= 0)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Apply damage to the highest (topmost) alive block.
    /// </summary>
    public void TakeTopDamage(float amount)
    {
        // scan from top to bottom
        for (int i = blocks.Count - 1; i >= 0; i--)
        {
            var b = blocks[i];
            if (b != null && b.IsAlive)
            {
                b.Health.TakeDamage(amount);
                return;
            }
        }
        // no alive blocks -> destroy stack (safety)
        Destroy(gameObject);
    }
}
