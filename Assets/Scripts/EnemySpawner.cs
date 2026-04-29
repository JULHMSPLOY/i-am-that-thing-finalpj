using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    [Header("Spawn Settings")]
    [Tooltip("โอกาสเกิดของศัตรูในแต่ละจุด (0.0 = ไม่เกิดเลย, 1.0 = เกิดแน่นอน 100%, 0.5 = โอกาส 50%)")]
    [Range(0f, 1f)] public float spawnChance = 0.5f;

    [Header("Room Spawns")]
    [Tooltip("ลากจุดกึ่งกลางห้อง (Empty GameObject) มาใส่ในนี้")]
    public Transform[] roomSpawnPoints;
    public float roomSpawnRadius = 4f; // รัศมีสุ่มเกิดรอบๆ จุดกึ่งกลางห้องนั้นๆ

    [Header("Player Distance")]
    public Transform player;
    public float minDistanceFromPlayer = 8f;

    [Header("Spacing & Placement")]
    public float minDistanceBetweenEnemies = 2.0f;
    public float minDistanceFromWall = 0.5f;

    List<GameObject> enemies = new List<GameObject>();

    void Start()
    {
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        int totalPoints = roomSpawnPoints.Length;

        if (roomSpawnPoints == null || totalPoints == 0)
        {
            Debug.LogError("ยังไม่ได้ใส่ Room Spawn Points! ศัตรูจะไม่เกิดครับ");
            return;
        }

        // 1. คำนวณจำนวนที่จะเกิดแน่นอนขั้นต่ำ (ป้องกันค่าติดลบในกรณีที่มีจุดเกิดน้อยกว่า 2 จุด)
        int minSpawns = Mathf.Max(0, totalPoints - 2);
        int actualSpawnCount = minSpawns;

        // 2. สุ่มโอกาสเกิดสำหรับจุดที่เหลือ (สูงสุดคือ 2 จุด)
        int optionalSpawns = totalPoints - minSpawns;
        for (int i = 0; i < optionalSpawns; i++)
        {
            if (Random.value <= spawnChance)
            {
                actualSpawnCount++;
            }
        }

        // 3. นำจุดเกิดทั้งหมดมาใส่ List แล้วสลับตำแหน่ง (Shuffle) 
        // เพื่อให้ศัตรูถูกสุ่มกระจายไปตามห้องต่างๆ แบบไม่ซ้ำจุด
        List<Transform> shuffledPoints = new List<Transform>(roomSpawnPoints);
        for (int i = 0; i < shuffledPoints.Count; i++)
        {
            Transform temp = shuffledPoints[i];
            int randomIndex = Random.Range(i, shuffledPoints.Count);
            shuffledPoints[i] = shuffledPoints[randomIndex];
            shuffledPoints[randomIndex] = temp;
        }

        // 4. สร้างศัตรูตามจำนวน actualSpawnCount ที่คำนวณไว้
        for (int i = 0; i < actualSpawnCount; i++)
        {
            Transform spawnPoint = shuffledPoints[i];

            // หาตำแหน่งที่เหมาะสมรอบๆ จุดเกิดนี้
            Vector3 spawnPos = GetValidSpawnPosition(spawnPoint.position);
            Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            GameObject enemy = Instantiate(enemyPrefab, spawnPos, randomRotation);

            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.Warp(spawnPos);
            }

            enemies.Add(enemy);
        }
    }

    // ฟังก์ชันนี้จะสุ่มหาพื้นที่สีฟ้ารอบๆ จุดกึ่งกลางห้องนั้นๆ
    Vector3 GetValidSpawnPosition(Vector3 roomCenter)
    {
        for (int i = 0; i < 100; i++)
        {
            Vector3 randomPoint = roomCenter + new Vector3(
                Random.Range(-roomSpawnRadius, roomSpawnRadius),
                0,
                Random.Range(-roomSpawnRadius, roomSpawnRadius)
            );

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 5f, NavMesh.AllAreas))
            {
                Vector3 pos = hit.position;
                if (IsFarFromPlayer(pos) && IsFarFromOtherEnemies(pos) && IsFarFromWalls(pos))
                {
                    return pos;
                }
            }
        }

        // Fallback: ถ้าหาจุดไม่ได้จริงๆ ให้เกิดตรงกลางห้องเป๊ะๆ
        NavMeshHit fallbackHit;
        if (NavMesh.SamplePosition(roomCenter, out fallbackHit, 10f, NavMesh.AllAreas))
        {
            return fallbackHit.position;
        }

        return roomCenter;
    }

    bool IsFarFromPlayer(Vector3 pos)
    {
        if (player == null) return true;
        return Vector3.Distance(pos, player.position) >= minDistanceFromPlayer;
    }

    bool IsFarFromOtherEnemies(Vector3 pos)
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null && Vector3.Distance(pos, enemy.transform.position) < minDistanceBetweenEnemies)
            {
                return false;
            }
        }
        return true;
    }

    bool IsFarFromWalls(Vector3 pos)
    {
        NavMeshHit edgeHit;
        if (NavMesh.FindClosestEdge(pos, out edgeHit, NavMesh.AllAreas))
        {
            return edgeHit.distance >= minDistanceFromWall;
        }
        return true;
    }
}