using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Scriptable Objects/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    [Header("Enemy Phase")]
    public float spawnDuration = 20f;         // seconds to run the spawner
    public int enemyQuota = 40;               // max enemies to spawn during this phase

    [Header("Boss")]
    public GameObject bossPrefab;             // required
    public Vector3 bossSpawnPos = new Vector3(0f, 0f, 16f);
    public float bossIntroDelay = 3.0f;       // small pause before boss appears

    [Header("Tweaks")]
    public float postBossWinDelay = 1.0f;     // small pause before advancing
}
