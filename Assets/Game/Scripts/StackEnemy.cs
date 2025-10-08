using System.Collections.Generic;
using DG.Tweening;
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

    [Header("Settle Animation")]
    [SerializeField] private float settleDuration = 0.12f;
    [SerializeField] private Ease settleEase = Ease.OutCubic;

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

            b.Died += OnBlockDied;

            blocks.Add(b);
            aliveBlocks++;
        }

        // ensure list is sorted by height (highest last)
        blocks.Sort((a, b) =>
        {
            if (a == null || b == null)
            {
                return 0;
            }
            return a.transform.localPosition.y.CompareTo(b.transform.localPosition.y);
        });
    }

    private void OnBlockDied(EnemyBlock dead)
    {
        aliveBlocks -= 1;


        if (aliveBlocks <= 0)
        {
            Destroy(gameObject);
            return;
        }

        transform.DOKill(false);
        Sequence nudge = DOTween.Sequence();
        nudge.Append(transform.DOLocalMoveY(-0.05f, 0.05f));
        nudge.Append(transform.DOLocalMoveY(0f, 0.06f)).SetEase(Ease.OutCubic);

        SettleBlocksDOTween();
    }

    private void SettleBlocksDOTween()
    {
        // collect alive in current vertical order (bottom → top)
        List<EnemyBlock> alive = new List<EnemyBlock>(blocks.Count);

        for (int i = 0; i < blocks.Count; i++)
        {
            EnemyBlock b = blocks[i];

            if (b != null && b.IsAlive == true)
            {
                alive.Add(b);
            }
        }

        // tween each alive block to its compacted slot
        for (int i = 0; i < alive.Count; i++)
        {
            Transform t = alive[i].transform;

            Vector3 target = new Vector3(0f, i * blockSpacingY, 0f);

            t.DOKill(false);
            t.DOLocalMove(target, settleDuration).SetEase(settleEase);
        }

        // rebuild list order bottom → top
        blocks.Clear();
        blocks.AddRange(alive);
        blocks.Sort((a, b) =>
        {
            if (a == null || b == null)
            {
                return 0;
            }
            return a.transform.localPosition.y.CompareTo(b.transform.localPosition.y);
        });
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

    private void OnDestroy()
    {
        // kill any tweens targeting children to avoid leaks
        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i] != null)
            {
                blocks[i].transform.DOKill(false);
            }
        }
        transform.DOKill(false);
    }
}
