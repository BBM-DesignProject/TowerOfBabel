using UnityEngine;
using System.Collections.Generic; // List<T> ve IEnumerator için
using System.Linq; // Sum() gibi LINQ metodları için

public class EnemySpawner : MonoBehaviour
{
    [Header("Wave Configuration")]
    [Tooltip("List of WaveData ScriptableObjects defining each wave in order.")]
    public List<WaveData> waves;
    [Tooltip("Initial delay before the first wave starts.")]
    public float initialWaveDelay = 5.0f;

    [Header("Spawn Points")]
    [Tooltip("List of transforms representing spawn points. Enemies will spawn at these locations.")]
    public List<Transform> spawnPoints;
    
    [Header("Global Settings")]
    [Tooltip("Maximum number of enemies allowed in the scene at once. Spawning pauses if this limit is reached.")]
    public int maxEnemiesOnScreen = 30;
    [Tooltip("Tag used to count enemies on screen. Ensure your enemy prefabs have this tag.")]
    public string enemyTag = "Enemy";

    private int currentWaveIndex = -1; 
    private bool isSpawningPhaseActive = false; // Dalganın spawn etme aşamasının aktif olup olmadığını belirtir
    private float nextWaveStartTime;
    private List<GameObject> activeEnemiesInCurrentWave;

    public static event System.Action<int> OnWaveStarted; // Dalga başladığında (dalga index'i + 1)
    public static event System.Action<int> OnWaveSpawnPhaseCompleted; // Dalganın spawn aşaması bittiğinde
    public static event System.Action<int> OnWaveCleared; // Dalga tamamen temizlendiğinde (tüm düşmanlar öldü)
    public static event System.Action OnAllWavesCompletedAndCleared; // Tüm dalgalar bittiğinde ve tüm düşmanlar temizlendiğinde

    void Start()
    {
        if (waves == null || waves.Count == 0)
        {
            Debug.LogError("EnemySpawner: No waves assigned in the Inspector! Disabling spawner.", this);
            enabled = false;
            return;
        }

        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError("EnemySpawner: No spawn points assigned! Disabling spawner.", this);
            enabled = false;
            return;
        }
        activeEnemiesInCurrentWave = new List<GameObject>();
        nextWaveStartTime = Time.time + initialWaveDelay;
    }

    void Update()
    {
        if (isSpawningPhaseActive) 
            return;

        // Tüm dalgalar bittiyse ve aktif düşman kalmadıysa kontrolü ReportEnemyDeath'e taşındı.
        // if (currentWaveIndex >= waves.Count -1 && activeEnemiesInCurrentWave.Count == 0)
        // {
        //     // Bu kontrol ReportEnemyDeath içinde daha mantıklı
        //     return;
        // }

        if (Time.time >= nextWaveStartTime && currentWaveIndex < waves.Count -1) // Henüz son dalgaya gelinmediyse
        {
            StartNextWave();
        }
    }

    void StartNextWave()
    {
        currentWaveIndex++;
        if (currentWaveIndex < waves.Count)
        {
            isSpawningPhaseActive = true;
            WaveData currentWaveData = waves[currentWaveIndex];
            Debug.Log($"Starting Wave {currentWaveIndex + 1}: {currentWaveData.name}");
            OnWaveStarted?.Invoke(currentWaveIndex + 1);
            StartCoroutine(SpawnEnemiesForWaveCoroutine(currentWaveData));
        }
    }

    System.Collections.IEnumerator SpawnEnemiesForWaveCoroutine(WaveData waveData)
    {
        int enemiesSpawnedThisWaveCount = 0;
        float waveStartTime = Time.time;

        while (enemiesSpawnedThisWaveCount < waveData.totalEnemiesToSpawnInWave)
        {
            if (waveData.waveDuration > 0 && Time.time > waveStartTime + waveData.waveDuration)
            {
                Debug.Log($"Wave {currentWaveIndex + 1} duration ended.");
                break; 
            }

            GameObject[] currentSceneEnemies = GameObject.FindGameObjectsWithTag(enemyTag);
            if (currentSceneEnemies.Length < maxEnemiesOnScreen)
            {
                GameObject spawnedEnemy = SpawnSingleEnemyFromWave(waveData);
                if (spawnedEnemy != null)
                {
                    activeEnemiesInCurrentWave.Add(spawnedEnemy);
                    enemiesSpawnedThisWaveCount++;
                }
            }
            
            float spawnDelay = Random.Range(waveData.minSpawnIntervalInWave, waveData.maxSpawnIntervalInWave);
            yield return new WaitForSeconds(spawnDelay);
        }

        Debug.Log($"Wave {currentWaveIndex + 1} spawn phase completed. Spawned {enemiesSpawnedThisWaveCount} enemies. Waiting for them to be cleared.");
        isSpawningPhaseActive = false; 
        OnWaveSpawnPhaseCompleted?.Invoke(currentWaveIndex + 1);

        // Eğer spawn bittiğinde hiç aktif düşman kalmadıysa (hepsi anında öldü veya hiç spawn olamadı)
        // WaveCleared event'ini hemen tetikle.
        if (activeEnemiesInCurrentWave.Count == 0)
        {
            HandleWaveCleared();
        }
    }

    GameObject SpawnSingleEnemyFromWave(WaveData waveData)
    {
        if (waveData.enemiesInWave == null || waveData.enemiesInWave.Count == 0 || spawnPoints.Count == 0)
        {
            Debug.LogWarning($"Wave {waveData.name} has no enemies defined or no spawn points available.", this);
            return null;
        }

        GameObject enemyToSpawnPrefab = GetRandomEnemyWeighted(waveData.enemiesInWave);
        if (enemyToSpawnPrefab == null)
        {
            Debug.LogWarning($"Could not select an enemy to spawn for wave {waveData.name} based on weights.", this);
            return null;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        return Instantiate(enemyToSpawnPrefab, spawnPoint.position, spawnPoint.rotation);
    }
    
    public void ReportEnemyDeath(GameObject deadEnemy)
    {
        if (activeEnemiesInCurrentWave != null && activeEnemiesInCurrentWave.Contains(deadEnemy))
        {
            activeEnemiesInCurrentWave.Remove(deadEnemy);
            // Debug.Log($"{deadEnemy.name} removed from active wave enemies. Remaining: {activeEnemiesInCurrentWave.Count}");

            // Sadece spawn aşaması bittiyse ve aktif düşman kalmadıysa dalga temizlenmiş sayılır.
            if (!isSpawningPhaseActive && activeEnemiesInCurrentWave.Count == 0)
            {
                HandleWaveCleared();
            }
        }
    }

    private void HandleWaveCleared()
    {
        Debug.Log($"Wave {currentWaveIndex + 1} CLEARED! All enemies defeated.");
        OnWaveCleared?.Invoke(currentWaveIndex + 1);

        if (currentWaveIndex < waves.Count - 1) // Son dalga değilse
        {
            WaveData completedWaveData = waves[currentWaveIndex];
            nextWaveStartTime = Time.time + completedWaveData.timeAfterWave;
            Debug.Log($"Next wave ({currentWaveIndex + 2}) scheduled for: {nextWaveStartTime}");
        }
        else // Bu son dalgaydı ve temizlendi
        {
            Debug.Log("All waves and all enemies cleared! Game Over (Win) or proceed to next stage.");
            OnAllWavesCompletedAndCleared?.Invoke();
            // Spawner'ı burada devre dışı bırakabiliriz.
            // enabled = false; 
        }
    }

    GameObject GetRandomEnemyWeighted(List<WaveData.EnemySpawnInfo> enemyInfos)
    {
        float totalWeight = 0;
        foreach (var info in enemyInfos)
        {
            totalWeight += info.spawnWeight;
        }

        if (totalWeight <= 0) 
        {
            return enemyInfos.Count > 0 ? enemyInfos[0].enemyPrefab : null;
        }

        float randomPoint = Random.value * totalWeight;

        foreach (var info in enemyInfos)
        {
            if (randomPoint < info.spawnWeight)
            {
                return info.enemyPrefab;
            }
            else
            {
                randomPoint -= info.spawnWeight;
            }
        }
        return enemyInfos.Count > 0 ? enemyInfos[enemyInfos.Count - 1].enemyPrefab : null; 
    }
}