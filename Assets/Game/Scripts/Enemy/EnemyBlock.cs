using System;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyBlock : MonoBehaviour
{
    [SerializeField] private float baseHp = 10f;
    [SerializeField] private float deathScaleUp = 1.25f;
    [SerializeField] private float deathDur = 0.12f;
    public Health Health { get; private set; }
    public bool IsAlive { get; private set; } = true;

    private Rigidbody rb;

    private Vector3 baseScale;

    public event Action<EnemyBlock> Died;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        Health = GetComponent<Health>();
        Health.SetMax(baseHp, true);

        baseScale = transform.localScale;

        Health.onDeath.AddListener(() =>
        {
            IsAlive = false;

            transform.DOKill(false);

            // death sequence: scale up + quick fade, then destroy
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(baseScale * deathScaleUp, deathDur).SetEase(Ease.OutBack));

            sequence.OnComplete(() =>
            {
                Died?.Invoke(this);
                Destroy(gameObject);
            });
        });
    }

    public void SetHp(float hp)
    {
        baseHp = hp;
        Health.SetMax(baseHp, true);
        IsAlive = true;

        transform.DOKill(false);
        transform.localScale = baseScale;
    }

    private void OnDestroy()
    {
        transform.DOKill(false);
    }
}
