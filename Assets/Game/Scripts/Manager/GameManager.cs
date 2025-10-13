using UnityEngine;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerLives playerLives;

    private void Awake()
    {
        GameState.Reset();
    }

    private void OnEnable()
    {
        if (playerLives != null)
        {
            playerLives.onPlayerDied += OnPlayerDied;
        }
    }

    private void OnDisable()
    {
        if (playerLives != null)
        {
            playerLives.onPlayerDied -= OnPlayerDied;
        }
    }

    private void OnPlayerDied()
    {
        GameState.IsGameOver = true;

        // Stop spawns via singleton
        if (FormationSpawner.Instance != null)
        {
            FormationSpawner.Instance.StopLoopAndDisable();
        }

        // Disable player input + firing
        var pc = playerLives.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.enabled = false;
        }

        var gun = playerLives.GetComponent<WeaponAutoFire>();
        if (gun != null)
        {
            gun.enabled = false;
        }

        LevelManager.Instance?.LevelFailed();

        // Pause DOTween (optional)
        DOTween.PauseAll();

        // TODO: Show Game Over UI / offer restart
    }
}
