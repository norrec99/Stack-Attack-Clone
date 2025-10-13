using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Levels")]
    [SerializeField] private List<LevelConfig> levels = new List<LevelConfig>();
    [SerializeField] private int startLevelIndex = 0;

    [Header("Debug / UI Hooks")]
    public Action<int> onLevelStarted;          // sends levelIndex
    public Action<int> onEnemySpawnedCount;     // sends spawned count during enemy phase
    public Action<int> onEnemyQuotaSet;         // sends quota for this level
    public Action<int> onLevelCompleted;        // sends levelIndex
    public Action<int> onLevelFailed;           // sends levelIndex

    private int currentLevel = -1;
    private int spawnedThisPhase = 0;
    private bool enemyPhaseRunning = false;
    private bool bossPhaseRunning = false;
    private GameObject currentBoss = null;

    public static LevelManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("LevelManager: duplicate instance, destroying.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        if (FormationSpawner.Instance != null)
        {
            FormationSpawner.Instance.OnSpawnedEnemy += HandleSpawnedEnemy;
        }
    }

    private void OnDisable()
    {
        if (FormationSpawner.Instance != null)
        {
            FormationSpawner.Instance.OnSpawnedEnemy -= HandleSpawnedEnemy;
        }
    }

    private void Start()
    {
        if (levels == null || levels.Count == 0)
        {
            Debug.LogWarning("LevelManager: No LevelConfig assets assigned.");
            return;
        }

        StartLevel(startLevelIndex);
    }

    // ---------- Public Controls ----------

    public void StartLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
            Debug.LogWarning("LevelManager: Level index out of range.");
            return;
        }

        currentLevel = levelIndex;

        if (onLevelStarted != null)
        {
            onLevelStarted.Invoke(currentLevel);
        }

        StopAllCoroutines();
        StartCoroutine(RunLevel(levels[currentLevel]));
    }

    public void RestartCurrentLevel()
    {
        if (currentLevel >= 0)
        {
            StartLevel(currentLevel);
        }
    }

    public void StartNextLevel()
    {
        int next = currentLevel + 1;

        if (next >= levels.Count)
        {
            // all levels done → you can show a “You Win” screen
            Debug.Log("All levels complete!");
            return;
        }

        StartLevel(next);
    }

    // ---------- Core Loop ----------

    private IEnumerator RunLevel(LevelConfig cfg)
    {
        // guard
        if (GameState.IsGameOver) yield break;
        if (FormationSpawner.Instance == null)
        {
            Debug.LogWarning("LevelManager: No FormationSpawner found.");
            yield break;
        }

        // reset counters
        spawnedThisPhase = 0;
        enemyPhaseRunning = true;
        bossPhaseRunning = false;

        // show quota in UI if desired
        if (onEnemyQuotaSet != null)
        {
            onEnemyQuotaSet.Invoke(cfg.enemyQuota);
        }

        // start spawner loop (if not running already)
        if (!FormationSpawner.Instance.IsRunning)
        {
            FormationSpawner.Instance.enabled = true;
            FormationSpawner.Instance.StartLoop();
        }

        // run enemy phase for cfg.spawnDuration or until quota reached
        float t = 0f;
        while (t < cfg.spawnDuration && spawnedThisPhase < cfg.enemyQuota)
        {
            if (GameState.IsGameOver)
            {
                yield break;
            }

            t += Time.deltaTime;
            yield return null;
        }

        // stop spawning new enemies
        FormationSpawner.Instance.StopLoop();

        enemyPhaseRunning = false;

        // small buffer before boss
        float waitIntro = Mathf.Max(0f, cfg.bossIntroDelay);
        float te = 0f;
        while (te < waitIntro)
        {
            if (GameState.IsGameOver)
            {
                yield break;
            }

            te += Time.deltaTime;
            yield return null;
        }

        // spawn boss
        bossPhaseRunning = true;
        currentBoss = SpawnBoss(cfg);
        if (currentBoss == null)
        {
            // no boss assigned → auto-complete level
            bossPhaseRunning = false;
            yield return new WaitForSeconds(cfg.postBossWinDelay);
            LevelComplete();
            yield break;
        }

        // wait for boss death
        Health bossHealth = currentBoss.GetComponent<Health>();
        bool bossDead = false;

        if (bossHealth != null)
        {
            bossHealth.onDeath.AddListener(() => { bossDead = true; });
        }
        else
        {
            // fallback if prefab missing Health
            Debug.LogWarning("Boss prefab has no Health; completing level immediately.");
            bossDead = true;
        }

        while (!bossDead)
        {
            if (GameState.IsGameOver)
            {
                yield break;
            }

            yield return null;
        }

        bossPhaseRunning = false;

        // small delay, then progress
        yield return new WaitForSeconds(cfg.postBossWinDelay);
        LevelComplete();
    }

    private GameObject SpawnBoss(LevelConfig cfg)
    {
        if (cfg.bossPrefab == null)
        {
            Debug.LogWarning("LevelManager: No bossPrefab assigned in LevelConfig.");
            return null;
        }

        GameObject boss = Instantiate(cfg.bossPrefab, cfg.bossSpawnPos, Quaternion.identity);
        return boss;
    }

    // ---------- Events / Helpers ----------

    private void HandleSpawnedEnemy(StackEnemy e)
    {
        if (enemyPhaseRunning == false)
        {
            return;
        }

        spawnedThisPhase += 1;

        if (onEnemySpawnedCount != null)
        {
            onEnemySpawnedCount.Invoke(spawnedThisPhase);
        }

        // If we’ve hit the quota early, stop the spawner now.
        // (Level still waits for the time window or goes straight to boss? You asked for a time-limited phase,
        // but if you prefer immediate boss when quota reached, uncomment the early stop logic below.)

        if (spawnedThisPhase >= levels[currentLevel].enemyQuota)
        {
            FormationSpawner.Instance.StopLoop();
            // If you want to skip straight to boss as soon as quota is reached, you can:
            // enemyPhaseRunning = false;
        }
    }

    private void LevelComplete()
    {
        onLevelCompleted?.Invoke(currentLevel);
        Debug.Log("LevelManager: Level " + currentLevel + " complete.");

        // auto-progress
        // StartNextLevel();
    }

    public void LevelFailed()
    {
        if (onLevelFailed != null)
        {
            onLevelFailed.Invoke(currentLevel);
        }

        // stop spawning just in case
        if (FormationSpawner.Instance != null)
        {
            FormationSpawner.Instance.StopLoop();
        }
    }
}
