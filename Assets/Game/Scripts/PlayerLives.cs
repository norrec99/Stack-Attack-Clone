using System;
using System.Collections;
using UnityEngine;

public class PlayerLives : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField] private int startingLives = 3;
    [SerializeField] private int maxLives = 9;

    [Header("Invulnerability (Shield)")]
    [SerializeField] private float shieldDuration = 1.0f;
    [SerializeField] private GameObject shieldVfx;
    [SerializeField] private bool flashOnShield = false;

    [Header("Collision Filters")]
    [Tooltip("If true, only collisions with EnemyBlock/StackEnemy will damage the player.")]
    [SerializeField] private bool onlyEnemyDamage = true;

    public int CurrentLives { get; private set; }
    public bool IsInvulnerable { get; private set; }

    public Action<int> onLivesChanged;
    public Action onPlayerDied;
    public Action onShieldStart;
    public Action onShieldEnd;

    private Coroutine shieldRoutine;

    private void Awake()
    {
        CurrentLives = Mathf.Max(0, startingLives);

        if (shieldVfx != null)
        {
            shieldVfx.SetActive(false);
        }

        IsInvulnerable = false;
    }

    public void AddLife(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        int newLives = Mathf.Clamp(CurrentLives + amount, 0, maxLives);
        CurrentLives = newLives;

        if (onLivesChanged != null)
        {
            onLivesChanged.Invoke(CurrentLives);
        }
    }

    public void TakeHit()
    {
        if (IsInvulnerable == true)
        {
            return;
        }

        if (CurrentLives <= 0)
        {
            return;
        }

        CurrentLives -= 1;

        if (onLivesChanged != null)
        {
            onLivesChanged.Invoke(CurrentLives);
        }

        if (CurrentLives <= 0)
        {
            HandleDeath();
            return;
        }

        StartShield();
    }

    private void HandleDeath()
    {
        if (onPlayerDied != null)
        {
            onPlayerDied.Invoke();
        }
        Time.timeScale = 0f;
    }

    private void StartShield()
    {
        if (shieldRoutine != null)
        {
            StopCoroutine(shieldRoutine);
        }

        shieldRoutine = StartCoroutine(ShieldCoroutine());
    }

    private IEnumerator ShieldCoroutine()
    {
        IsInvulnerable = true;

        if (shieldVfx != null)
        {
            shieldVfx.SetActive(true);
        }

        if (onShieldStart != null)
        {
            onShieldStart.Invoke();
        }

        float t = 0f;
        while (t < shieldDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (shieldVfx != null)
        {
            shieldVfx.SetActive(false);
        }

        IsInvulnerable = false;

        if (onShieldEnd != null)
        {
            onShieldEnd.Invoke();
        }

        shieldRoutine = null;
    }

    private bool IsEnemyCollider(Collider other)
    {
        if (onlyEnemyDamage == false)
        {
            return true;
        }

        if (other.GetComponentInParent<StackEnemy>() != null)
        {
            return true;
        }

        if (other.GetComponent<EnemyBlock>() != null)
        {
            return true;
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsInvulnerable == true)
        {
            return;
        }

        if (IsEnemyCollider(other) == true)
        {
            TakeHit();
        }
    }
}
