using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormationSpawner : MonoBehaviour
{
    // -------- Types --------
    public enum Formation
    {
        HorizontalLine,
        VerticalLine,
        Circle
    }

    // -------- Refs --------
    [Header("Refs")]
    [SerializeField] private Camera mainCam;
    [SerializeField] private StackEnemy stackPrefab;

    // -------- Gameplay Plane / Depth --------
    [Header("Gameplay Plane / Depth")]
    [SerializeField] private float planeY = 0f;
    [SerializeField] private float spawnZCenter = 18f;
    [SerializeField] private float spawnZJitter = 0f;
    [SerializeField] private int level = 1;

    // computed screen-to-plane bounds
    private float minX;
    private float maxX;
    private int lastW;
    private int lastH;

    // measured prefab footprint
    private float stackWidthX = 1f;
    private float stackDepthZ = 1f;

    // -------- Horizontal Line --------
    [Header("Horizontal Line (randomized)")]
    [SerializeField] private int hCountMin = 3;
    [SerializeField] private int hCountMax = 7;

    // -------- Vertical Line --------
    [Header("Vertical Line (randomized)")]
    [SerializeField] private int vCountMin = 3;
    [SerializeField] private int vCountMax = 7;

    // -------- Circle --------
    [Header("Circle (randomized)")]
    [SerializeField] private int cCountMin = 5;
    [SerializeField] private int cCountMax = 12;
    [SerializeField] private float cRadiusMin = 4f;
    [SerializeField] private float cRadiusMax = 10f;
    [SerializeField] private bool faceOutward = true;

    // -------- Formation Randomization --------
    [Header("Formation Randomization")]
    [SerializeField] private bool randomizeFormationEachWave = true;
    [SerializeField] private float weightHorizontal = 1f;
    [SerializeField] private float weightVertical = 1f;
    [SerializeField] private float weightCircle = 1f;
    [SerializeField] private Formation loopFormation = Formation.HorizontalLine;

    // -------- Loop control --------
    [Header("Loop (optional)")]
    [SerializeField] private bool runLoop = true;
    [SerializeField] private float firstDelay = 1f;
    [SerializeField] private float interval = 2f;

    private Coroutine loopCo;
    public bool IsRunning { get; private set; }

    public System.Action<StackEnemy> OnSpawnedEnemy;

    // -------- Singleton --------
    public static FormationSpawner Instance { get; private set; }

    private void Awake()
    {
        if (mainCam == null)
        {
            mainCam = Camera.main;
        }

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        RecalculateBounds();
        MeasureFootprint();

        if (runLoop == true)
        {
            StartLoop();
        }
    }

    private void Update()
    {
        if (Screen.width != lastW || Screen.height != lastH)
        {
            RecalculateBounds();
        }
    }

    public void StartLoop()
    {
        if (loopCo != null)
        {
            StopCoroutine(loopCo);
        }

        IsRunning = true;
        loopCo = StartCoroutine(Loop());
    }

    public void StopLoop()
    {
        IsRunning = false;

        if (loopCo != null)
        {
            StopCoroutine(loopCo);
            loopCo = null;
        }
    }

    public void StopLoopAndDisable()
    {
        StopLoop();
        enabled = false;
    }

    public void SpawnRandomFormationNow()
    {
        if (GameState.IsGameOver == true)
        {
            return;
        }

        SpawnFormation(ChooseRandomFormation());
    }

    public void SpawnFormation(Formation f)
    {
        if (GameState.IsGameOver == true)
        {
            return;
        }

        switch (f)
        {
            case Formation.HorizontalLine:
                SpawnHorizontalLineRandom();
                break;

            case Formation.VerticalLine:
                SpawnVerticalLineRandom();
                break;

            case Formation.Circle:
                SpawnCircleRandom();
                break;
        }
    }

    private IEnumerator Loop()
    {
        yield return new WaitForSeconds(firstDelay);

        while (true)
        {
            if (GameState.IsGameOver == true)
            {
                StopLoop();
                yield break;
            }

            Formation f = randomizeFormationEachWave ? ChooseRandomFormation() : loopFormation;
            SpawnFormation(f);

            level += 1;

            float t = 0f;
            while (t < interval)
            {
                if (GameState.IsGameOver == true)
                {
                    StopLoop();
                    yield break;
                }

                t += Time.deltaTime;
                yield return null;
            }
        }
    }

    private void SpawnHorizontalLineRandom()
    {
        int count = Mathf.Clamp(Random.Range(hCountMin, hCountMax + 1), 1, 1000);

        float totalWidth = count * stackWidthX;
        float half = totalWidth * 0.5f;
        float edgePad = stackWidthX * 0.5f;

        float minCenter = minX + edgePad + half;
        float maxCenter = maxX - edgePad - half;
        if (minCenter > maxCenter) return;

        float centerX = Random.Range(minCenter, maxCenter);
        float z = spawnZCenter + Random.Range(-spawnZJitter, spawnZJitter);

        float startX = centerX - half + stackWidthX * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float x = startX + i * stackWidthX;
            Vector3 pos = new Vector3(x, 0f, z);
            StackEnemy e = Instantiate(stackPrefab, pos, Quaternion.identity);
            e.Build(level);
            OnSpawnedEnemy?.Invoke(e);
        }
    }

    private void SpawnVerticalLineRandom()
    {
        int count = Mathf.Clamp(Random.Range(vCountMin, vCountMax + 1), 1, 1000);

        float edgePad = stackWidthX * 0.5f;
        float minXCenter = minX + edgePad;
        float maxXCenter = maxX - edgePad;
        if (minXCenter > maxXCenter) return;

        float x = Random.Range(minXCenter, maxXCenter);

        float totalDepth = count * stackDepthZ;
        float zCenter = spawnZCenter + Random.Range(-spawnZJitter, spawnZJitter);
        float startZ = zCenter - totalDepth * 0.5f + stackDepthZ * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float z = startZ + i * stackDepthZ;
            Vector3 pos = new Vector3(x, 0f, z);
            StackEnemy e = Instantiate(stackPrefab, pos, Quaternion.identity);
            e.Build(level);
            OnSpawnedEnemy?.Invoke(e);
        }
    }

    private void SpawnCircleRandom()
    {
        int count = Mathf.Clamp(Random.Range(cCountMin, cCountMax + 1), 3, 360);

        float edgePad = stackWidthX * 0.5f;
        float visibleWidth = (maxX - minX) - 2f * edgePad;
        if (visibleWidth <= 0.01f) return;

        float chordNeeded = Mathf.Max(stackWidthX, stackDepthZ); // widest footprint
        float rTouch = chordNeeded / (2f * Mathf.Sin(Mathf.PI / count));

        float rMaxFit = visibleWidth * 0.5f;
        float rRandom = Random.Range(cRadiusMin, cRadiusMax);
        float R = Mathf.Min(rMaxFit, Mathf.Max(rRandom, rTouch));

        float minCenter = minX + edgePad + R;
        float maxCenter = maxX - edgePad - R;
        if (minCenter > maxCenter) return;

        float centerX = Random.Range(minCenter, maxCenter);
        float centerZ = spawnZCenter + Random.Range(-spawnZJitter, spawnZJitter);

        float startDeg = Random.Range(0f, 360f);
        float step = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float deg = startDeg + i * step;
            float rad = deg * Mathf.Deg2Rad;

            float x = centerX + Mathf.Cos(rad) * R;
            float z = centerZ + Mathf.Sin(rad) * R;

            Vector3 pos = new Vector3(x, 0f, z);
            StackEnemy e = Instantiate(stackPrefab, pos, Quaternion.identity);
            e.Build(level);
            OnSpawnedEnemy?.Invoke(e);

            if (faceOutward == true)
            {
                Vector3 outward = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));
                if (outward.sqrMagnitude > 0.0001f)
                {
                    e.transform.rotation = Quaternion.LookRotation(outward, Vector3.up);
                }
            }
        }
    }

    private Formation ChooseRandomFormation()
    {
        float wH = Mathf.Max(0f, weightHorizontal);
        float wV = Mathf.Max(0f, weightVertical);
        float wC = Mathf.Max(0f, weightCircle);

        float total = wH + wV + wC;
        if (total <= 0f)
        {
            return loopFormation;
        }

        float r = Random.value * total;

        if (r < wH)
        {
            return Formation.HorizontalLine;
        }

        r -= wH;

        if (r < wV)
        {
            return Formation.VerticalLine;
        }

        return Formation.Circle;
    }

    private void RecalculateBounds()
    {
        lastW = Screen.width;
        lastH = Screen.height;

        if (mainCam == null)
        {
            minX = -5f;
            maxX = 5f;
            return;
        }

        Plane plane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));

        // slightly below mid-screen so rays tilt downward even with level camera
        float sampleY = Screen.height * 0.35f;
        float enter;

        Ray leftRay = mainCam.ScreenPointToRay(new Vector3(0f, sampleY, 0f));
        if (plane.Raycast(leftRay, out enter) == true)
        {
            minX = leftRay.GetPoint(enter).x;
        }

        Ray rightRay = mainCam.ScreenPointToRay(new Vector3(Screen.width, sampleY, 0f));
        if (plane.Raycast(rightRay, out enter) == true)
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

    private void MeasureFootprint()
    {
        // Spawn a probe offscreen, build it, read bounds, destroy.
        Vector3 probePos = new Vector3(10000f, 0f, 10000f);
        StackEnemy probe = Instantiate(stackPrefab, probePos, Quaternion.identity);
        probe.Build(level);

        Bounds? combined = null;
        Renderer[] rends = probe.GetComponentsInChildren<Renderer>();

        for (int i = 0; i < rends.Length; i++)
        {
            if (combined == null)
            {
                combined = rends[i].bounds;
            }
            else
            {
                Bounds b = combined.Value;
                b.Encapsulate(rends[i].bounds);
                combined = b;
            }
        }

        if (combined.HasValue == true)
        {
            stackWidthX = Mathf.Max(0.001f, combined.Value.size.x);
            stackDepthZ = Mathf.Max(0.001f, combined.Value.size.z);
        }
        else
        {
            stackWidthX = 1f;
            stackDepthZ = 1f;
        }

        Destroy(probe.gameObject);
    }
}
