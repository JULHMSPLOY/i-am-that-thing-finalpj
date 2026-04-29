using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PlayerManager : MonoBehaviour
{
    [Header("UI System")]
    public GameObject statusUI;
    public Image staminaBarFill;
    public Image stressBarFill;

    // เพิ่มตัวแปรนี้สำหรับรูปภาพขอบจอมืด
    public Image stressVignette;

    [Header("Game Over System")]
    public float maxGhostTime = 10f; // ตั้งค่าเวลาที่อยู่ร่างผีได้ (วินาที) ใน Inspector
    private float currentGhostTime;
    public GameObject gameOverUI; // ลาก UI Game Over (game_over) มาใส่ช่องนี้
    private bool isGameOver = false;

    [Header("Ghost Movement")]
    public float ghostSpeed = 5f;

    [Header("Possession System")]
    public float possessRadius = 2.5f;
    public LayerMask possessableLayer;

    private Outline currentOutline;

    [Header("Animation & VFX Settings")]
    public float transitionDuration = 0.25f;
    public GameObject magicSparklePrefab;
    public GameObject smokePuffPrefab;
    public GameObject playerVisualEffect;

    private bool isTransitioning = false;
    private IPossessable currentProp;
    private GameObject currentPropObj;
    private string originalPropTag = "Untagged";

    private GameObject lastPossessedObj;

    private Rigidbody rb;
    private Collider col;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        currentGhostTime = maxGhostTime; // เริ่มต้นให้เวลาเต็ม
        if (gameOverUI != null) gameOverUI.SetActive(false); // ปิดหน้า Game Over ไว้ก่อน
    }

    void Update()
    {
        if (isGameOver) return; // ถ้า Game Over แล้ว หยุดการทำงานทุกอย่าง

        if (isTransitioning) return;

        DetectOutline();
        HandlePossess();

        if (currentProp == null)
        {
            // --- ระบบนับถอยหลังร่างผี ---
            currentGhostTime -= Time.deltaTime;
            if (currentGhostTime <= 0)
            {
                TriggerGameOver();
                return;
            }

            HandleGhostMovement();
            UpdateUI(false);
        }
        else
        {
            // รีเซ็ตเวลาให้เต็มเมื่อสิงร่างสำเร็จ
            currentGhostTime = maxGhostTime;

            HandlePossessedMovement();
            transform.position = currentPropObj.transform.position;
            UpdateUI(true);

            if (currentProp.GetStressPercentage() >= 1f)
            {
                ForceUnpossess();
            }
        }
    }

    public void TriggerGameOver()
    {
        isGameOver = true;
        rb.linearVelocity = Vector3.zero; // หยุดการเคลื่อนที่

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        // ปลดล็อกเมาส์ให้กดปุ่ม UI ได้ (ถ้าเกมคุณมีการล็อกเมาส์ไว้)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // ... (ฟังก์ชันอื่นๆ ด้านล่างนี้คือโค้ดเดิมทั้งหมด ไม่มีการเปลี่ยนแปลง) ...
    void UpdateUI(bool isPossessing)
    {
        if (statusUI != null) statusUI.SetActive(isPossessing);

        if (isPossessing && currentProp != null)
        {
            if (staminaBarFill != null)
                staminaBarFill.fillAmount = currentProp.GetStaminaPercentage();

            if (stressBarFill != null)
                stressBarFill.fillAmount = currentProp.GetStressPercentage();

            // --- เพิ่มระบบคำนวณความมืดของขอบจอตรงนี้ ---
            if (stressVignette != null)
            {
                float stress = currentProp.GetStressPercentage();
                float targetAlpha = 0f;

                // เปลี่ยนเงื่อนไขเป็น มากกว่าหรือเท่ากับ 50% (0.5f)
                if (stress >= 0.5f)
                {
                    // แปลงช่วง 0.5 - 1.0 (ระยะห่างคือ 0.5) ให้กลายเป็นความทึบ 0.0 - 0.8
                    targetAlpha = ((stress - 0.5f) / 0.5f) * 0.8f;
                }

                Color vColor = stressVignette.color;
                // ทำให้ค่า Alpha ค่อยๆ เปลี่ยนแบบนุ่มนวล (Lerp) จะได้ไม่กระตุก
                vColor.a = Mathf.Lerp(vColor.a, targetAlpha, Time.deltaTime * 5f);
                stressVignette.color = vColor;
            }
        }
        else
        {
            // ตอนออกจากร่าง (เป็นผี) ให้จอกลับมาสว่างเหมือนเดิม
            if (stressVignette != null)
            {
                Color vColor = stressVignette.color;
                vColor.a = Mathf.Lerp(vColor.a, 0f, Time.deltaTime * 10f);
                stressVignette.color = vColor;
            }
        }
    }

    void HandleGhostMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        float y = 0f;
        if (Input.GetKey(KeyCode.Space)) y = 1f;
        else if (Input.GetKey(KeyCode.LeftShift)) y = -1f;

        Transform camTransform = Camera.main.transform;
        Vector3 camForward = camTransform.forward;
        Vector3 camRight = camTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camForward * z + camRight * x).normalized;

        float currentYVel = rb.linearVelocity.y;
        if (y != 0)
        {
            currentYVel = y * ghostSpeed;
        }

        rb.linearVelocity = new Vector3(moveDir.x * ghostSpeed, currentYVel, moveDir.z * ghostSpeed);
    }

    void HandlePossessedMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Transform camTransform = Camera.main.transform;
        Vector3 camForward = camTransform.forward;
        Vector3 camRight = camTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camForward * z + camRight * x).normalized;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

        currentProp.Move(moveDir, isSprinting);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentProp.Action();
        }
    }

    void HandlePossess()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentProp != null)
                StartCoroutine(UnpossessRoutine());
            else
                TryPossessClosest();
        }
    }

    public void ForceUnpossess()
    {
        if (!isTransitioning && currentProp != null)
        {
            StartCoroutine(UnpossessRoutine());
        }
    }

    void TryPossessClosest()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, possessRadius, possessableLayer);
        float closestDistance = Mathf.Infinity;
        Collider closestCollider = null;

        foreach (Collider col in hitColliders)
        {
            if (col.gameObject == lastPossessedObj) continue;

            float distance = Vector3.Distance(transform.position, col.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCollider = col;
            }
        }

        if (closestCollider != null)
        {
            StartCoroutine(PossessRoutine(closestCollider));
        }
    }

    IEnumerator PossessRoutine(Collider targetCol)
    {
        isTransitioning = true;
        rb.isKinematic = true;
        col.enabled = false;

        Vector3 startPos = transform.position;
        Vector3 endPos = targetCol.transform.position;
        float timeElapsed = 0f;

        while (timeElapsed < transitionDuration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, timeElapsed / transitionDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        if (magicSparklePrefab != null)
        {
            GameObject vfx = Instantiate(magicSparklePrefab, endPos, Quaternion.identity);
            Destroy(vfx, 2f);
        }

        if (playerVisualEffect != null) playerVisualEffect.SetActive(false);

        currentProp = targetCol.GetComponent<IPossessable>();
        currentPropObj = targetCol.gameObject;
        originalPropTag = currentPropObj.tag;
        currentPropObj.tag = "Player";

        currentProp.OnPossess(transform);
        isTransitioning = false;
    }

    IEnumerator UnpossessRoutine()
    {
        isTransitioning = true;
        currentProp.OnUnpossess();

        if (currentPropObj != null) currentPropObj.tag = originalPropTag;

        Vector3 startPos = currentPropObj.transform.position;
        Vector3 endPos = startPos + Vector3.up * 2.0f;

        if (smokePuffPrefab != null)
        {
            GameObject vfx = Instantiate(smokePuffPrefab, startPos, Quaternion.identity);
            Destroy(vfx, 2f);
        }

        if (playerVisualEffect != null) playerVisualEffect.SetActive(true);

        lastPossessedObj = currentPropObj;
        currentProp = null;
        currentPropObj = null;

        float timeElapsed = 0f;

        while (timeElapsed < transitionDuration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, timeElapsed / transitionDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        rb.isKinematic = false;
        col.enabled = true;
        isTransitioning = false;
    }

    void DetectOutline()
    {
        if (currentProp != null)
        {
            if (currentOutline != null)
            {
                currentOutline.enabled = false;
                currentOutline = null;
            }
            return;
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, possessRadius, possessableLayer);

        Outline closestOutline = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider col in hitColliders)
        {
            if (col.gameObject == lastPossessedObj) continue;

            Outline outline = col.GetComponentInParent<Outline>();

            if (outline != null)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestOutline = outline;
                }
            }
        }

        if (currentOutline != null && currentOutline != closestOutline)
        {
            currentOutline.enabled = false;
        }

        if (closestOutline != null)
        {
            closestOutline.enabled = true;
        }

        currentOutline = closestOutline;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, possessRadius);
    }
}