using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Camera mainCam;
    [SerializeField] private StackEnemy stackPrefab;

    [Header("Spawn Position")]
    [SerializeField] private float spawnZ = 20f;
    [SerializeField] private float planeY = 0f;

    [Header("Timing")]
    [SerializeField] private float firstDelay = 0.8f;
    [SerializeField] private float interval = 1.0f;
    [SerializeField] private int stacksPerWave = 2;
    [SerializeField] private int level = 1;

    private float minX, maxX;

    private void Awake()
    {
        if (mainCam == null)
        {
            mainCam = Camera.main;
        }
    }

    private void Start()
    {
        CalcScreenXBoundsOnPlane();
        StartCoroutine(SpawnLoop());
    }

    private void CalcScreenXBoundsOnPlane()
    {
        Plane plane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));
        float sampleY = Screen.height * 0.35f; // avoids parallel-ray case
        float enter;

        Ray leftRay = mainCam.ScreenPointToRay(new Vector3(0f, sampleY, 0f));
        if (plane.Raycast(leftRay, out enter))
        {
            minX = leftRay.GetPoint(enter).x;
        }

        Ray rightRay = mainCam.ScreenPointToRay(new Vector3(Screen.width, sampleY, 0f));
        if (plane.Raycast(rightRay, out enter))
        {
            maxX = rightRay.GetPoint(enter).x;
        }

        if (minX > maxX)
        {
            float t = minX;
            minX = maxX;
            maxX = t;
        }
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(firstDelay);

        while (true)
        {
            for (int i = 0; i < stacksPerWave; i++)
            {
                float x = Random.Range(minX, maxX);
                Vector3 pos = new Vector3(x, 0f, spawnZ);      // <<< y fixed to 0
                StackEnemy e = Instantiate(stackPrefab, pos, Quaternion.identity);
                e.Build(level);
            }

            level++;
            yield return new WaitForSeconds(interval);
        }
    }
}
