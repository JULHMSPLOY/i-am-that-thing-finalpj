using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    enum EnemyState { Patrol, Chase, Alert }
    EnemyState currentState;

    [Header("Movement Speeds")]
    public float patrolSpeed = 2.0f; // ความเร็วตอนเดินลาดตระเวนปกติ
    public float chaseSpeed = 3.5f;  // ความเร็วตอนวิ่งไล่ตามผู้เล่น

    [Header("Vision")]
    public float visionRange = 1f;
    public float visionAngle = 60f;
    public Transform target;

    [Header("Vision Cone Mesh")]
    public MeshFilter visionMeshFilter;
    public int visionSegments = 20;

    [Header("Patrol")]
    public float patrolDistance = 3f;

    Vector3 patrolStart;
    Vector3 patrolEnd;
    bool goingForward = true;

    [Header("Stealth")]
    public float hideTime = 3f;
    public float alertTime = 5f;

    float hideTimer = 0f;
    float alertTimer = 0f;

    Animator animator;
    NavMeshAgent agent;

    Mesh visionMesh;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        currentState = EnemyState.Patrol;

        if (agent != null)
            agent.speed = patrolSpeed;

        patrolStart = transform.position;
        Vector3 desiredEnd = transform.position + transform.forward * patrolDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(desiredEnd, out hit, 1.5f, NavMesh.AllAreas))
        {
            patrolEnd = hit.position;
        }
        else
        {
            patrolEnd = transform.position + transform.forward * (patrolDistance / 2f);
        }

        visionMesh = new Mesh();
        visionMeshFilter.mesh = visionMesh;

        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
            }
        }
    }

    void Update()
    {
        DrawVisionCone();

        switch (currentState)
        {
            case EnemyState.Patrol:
                PatrolUpdate();
                break;

            case EnemyState.Chase:
                ChaseUpdate();
                break;

            case EnemyState.Alert:
                AlertUpdate();
                break;
        }
    }

    void PatrolUpdate()
    {
        if (!agent.isOnNavMesh) return;

        Vector3 targetPoint = goingForward ? patrolEnd : patrolStart;

        agent.SetDestination(targetPoint);

        float dist = Vector3.Distance(transform.position, targetPoint);

        if (dist < 0.3f)
        {
            goingForward = !goingForward;
            transform.Rotate(0, 180f, 0);
        }

        if (CanSeeTarget())
        {
            currentState = EnemyState.Chase;
            hideTimer = 0f;
            agent.speed = chaseSpeed;
        }
    }

    void ChaseUpdate()
    {
        if (!agent.isOnNavMesh) return;

        if (target == null)
        {
            currentState = EnemyState.Patrol;
            agent.speed = patrolSpeed;
            return;
        }

        agent.SetDestination(target.position);

        if (CanSeeTarget())
        {
            hideTimer = 0f;
        }
        else
        {
            hideTimer += Time.deltaTime;

            if (hideTimer >= hideTime)
            {
                currentState = EnemyState.Alert;
                alertTimer = 0f;
            }
        }
    }

    void AlertUpdate()
    {
        if (!agent.isOnNavMesh) return;

        alertTimer += Time.deltaTime;

        if (CanSeeTarget())
        {
            currentState = EnemyState.Chase;
            hideTimer = 0f;
            agent.speed = chaseSpeed;
            return;
        }

        if (alertTimer >= alertTime)
        {
            currentState = EnemyState.Patrol;
            agent.speed = patrolSpeed;
        }
    }

    bool CanSeeTarget()
    {
        if (target == null) return false;

        Vector3 eyePos = transform.position + Vector3.up * 1.5f;
        Vector3 targetPos = target.position + Vector3.up * 0.5f;

        Vector3 dir = (targetPos - eyePos).normalized;

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > visionRange) return false;

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > visionAngle / 2f) return false;

        RaycastHit hit;

        if (Physics.Raycast(eyePos, dir, out hit, visionRange))
        {
            if (hit.transform == target ||
                hit.transform.IsChildOf(target) ||
                target.IsChildOf(hit.transform) ||
                hit.transform.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    void DrawVisionCone()
    {
        int vertexCount = visionSegments + 2;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[visionSegments * 3];

        vertices[0] = Vector3.zero;

        float angleStep = visionAngle / visionSegments;

        for (int i = 0; i <= visionSegments; i++)
        {
            float angle = -visionAngle / 2 + angleStep * i;
            float rad = Mathf.Deg2Rad * angle;

            Vector3 dir = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
            vertices[i + 1] = dir * visionRange;
        }

        for (int i = 0; i < visionSegments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        visionMesh.Clear();
        visionMesh.vertices = vertices;
        visionMesh.triangles = triangles;
        visionMesh.RecalculateNormals();
    }

    // --- เพิ่มระบบตรวจจับการชนเพื่อ Game Over ---
    private void OnCollisionEnter(Collision collision)
    {
        CheckGameOverCollision(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckGameOverCollision(other.gameObject);
    }

    private void CheckGameOverCollision(GameObject obj)
    {
        // เช็กว่าสิ่งที่ศัตรูชน มี Tag เป็น "Player" หรือไม่
        if (obj.CompareTag("Player"))
        {
            // พยายามดึงสคริปต์ PlayerManager ออกมาจาก Object ที่ชน
            PlayerManager playerManager = obj.GetComponent<PlayerManager>();

            // ถ้าไม่เจอที่ตัวมันเอง (เช่น ตอนสิงร่างอยู่ สคริปต์อาจจะอยู่ที่ตัวผี) 
            // ให้ลองดึงจาก PlayerManager ในฉากมาแทน
            if (playerManager == null)
            {
                // ใช้ FindObjectOfType แทน FindAnyObjectByType เพื่อความเข้ากันได้กับ Unity หลายเวอร์ชัน
                playerManager = FindAnyObjectByType<PlayerManager>();
            }

            // ถ้าเจอสคริปต์ PlayerManager ให้สั่งเรียกฟังก์ชัน TriggerGameOver()
            if (playerManager != null)
            {
                // หยุดศัตรูไม่ให้เดินต่อ
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                }

                // สั่ง Game Over
                playerManager.TriggerGameOver();
            }
        }
    }
}