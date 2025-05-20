using UnityEngine;
    using System.Collections.Generic;

    [CreateAssetMenu(fileName = "NewWaveData", menuName = "Enemy Spawning/Wave Data")]
    public class WaveData : ScriptableObject
    {
        [System.Serializable]
        public struct EnemySpawnInfo
        {
            public GameObject enemyPrefab;
            [Tooltip("Probability weight for this enemy type in this wave. Higher values mean more likely.")]
            public float spawnWeight; // Bu düşmanın bu dalgada spawn olma ağırlığı
            [Tooltip("Number of this specific enemy type to spawn in this wave. Set to 0 if using totalEnemiesToSpawn and weights for random selection within that total.")]
            public int specificCount; // Bu düşmandan bu dalgada kaç tane spawn olacağı (opsiyonel)
        }

        [Tooltip("List of enemy types and their spawn weights/counts for this wave.")]
        public List<EnemySpawnInfo> enemiesInWave;

        [Tooltip("Total number of enemies to spawn in this wave. If specificCounts in EnemySpawnInfo are used, this might be ignored or used as a cap.")]
        public int totalEnemiesToSpawnInWave = 10;
        
        [Tooltip("Time duration over which all enemies in this wave will be spawned.")]
        public float waveDuration = 20f; // Bu dalgadaki tüm düşmanların spawn olacağı süre

        [Tooltip("Minimum time between individual enemy spawns within this wave.")]
        public float minSpawnIntervalInWave = 0.5f;
        [Tooltip("Maximum time between individual enemy spawns within this wave.")]
        public float maxSpawnIntervalInWave = 2.0f;

        [Tooltip("Time to wait after this wave completes before starting the next one.")]
        public float timeAfterWave = 15f;
    }