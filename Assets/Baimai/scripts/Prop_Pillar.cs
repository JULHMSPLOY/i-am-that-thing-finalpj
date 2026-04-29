using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Prop_Pillar : MonoBehaviour, IPossessable
{
    private Rigidbody rb;
    private Collider col;
    private bool isPossessed = false;

    [Header("Heavy Movement Settings")]
    public float hopForwardForce = 8f;
    public float hopUpwardForce = 5f;
    public float sprintMultiplier = 1.5f;
    public float hopInterval = 0.5f;
    private float nextHopTime = 0f;

    [Header("Action Settings")]
    public float heavySmashForce = 15f;
    public float actionCooldown = 0.5f; // เพิ่มหน่วงเวลาป้องกันสแปมกระโดด
    private float nextActionTime = 0f;  // เก็บเวลาที่จะกระโดดครั้งต่อไปได้

    [Header("Status System (Stamina)")]
    public float maxStamina = 120f;
    private float currentStamina;
    public float walkHopCost = 10f;
    public float sprintHopCost = 20f;
    public float actionCost = 30f;
    public float staminaRegenRate = 8f;
    public float regenDelay = 1.0f;
    private float lastStaminaUseTime;

    [Header("Status System (Stress)")]
    public float maxStress = 100f;
    private float currentStress = 0f;
    public float idleStressRate = 5f;
    public float walkStressRate = 6f;
    public float sprintStressRate = 15f;

    [Header("Audio Settings")]
    public AudioClip[] footstepSFXs; // ใน Inspector ให้ใส่ Size = 3 แล้วลาก 3 ไฟล์มาใส่

void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        currentStamina = maxStamina;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        if (isPossessed)
        {
            currentStress += idleStressRate * Time.deltaTime;
            currentStress = Mathf.Clamp(currentStress, 0, maxStress);
        }

        if (Time.time > lastStaminaUseTime + regenDelay && currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
        }
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }

    public void OnPossess(Transform playerTransform) { isPossessed = true; }

    public void OnUnpossess()
    {
        isPossessed = false;
        currentStress = 0f;
    }

    public void Move(Vector3 moveDir, bool isSprinting)
    {
        if (!isPossessed || moveDir.magnitude == 0) return;

        if (isSprinting && currentStamina <= 0) isSprinting = false;

        if (moveDir.magnitude > 0.1f)
        {
            float currentStressRate = isSprinting ? sprintStressRate : walkStressRate;
            currentStress += currentStressRate * Time.deltaTime;
        }

        float currentCost = isSprinting ? sprintHopCost : walkHopCost;

        if (Time.time >= nextHopTime && currentStamina >= currentCost && IsGrounded())
        {
            float speedMult = isSprinting ? sprintMultiplier : 1f;

            // ในฟังก์ชัน Move() ตรงจังหวะก้าว/กระโดด ให้เปลี่ยนเป็น:
            if (footstepSFXs != null && SFXManager.Instance != null)
            {
                SFXManager.Instance.PlayRandomSFX(footstepSFXs);
            }

            if (moveDir.magnitude > 0.1f)
            {
                // === โค้ดส่วนที่แก้ใหม่เพื่อล็อกไม่ให้ล้ม ===
                // 1. หาว่า "ด้านหน้า" ของโมเดลตอนนี้ชี้ไปทางไหนบนพื้นราบ
                Vector3 visualForward = transform.forward;
                visualForward.y = 0;

                // ถ้า transform.forward ชี้ขึ้นฟ้า/ลงดิน (เพราะโมเดลหมุน -90 มา) ให้สลับไปใช้แกน Up แทน
                if (visualForward.sqrMagnitude < 0.01f)
                {
                    visualForward = transform.up;
                    visualForward.y = 0;
                }
                visualForward.Normalize();

                // 2. คำนวณว่าต้องหมุนซ้าย/ขวาอีกกี่องศา เพื่อให้หันไปตรงกับทิศทางที่ผู้เล่นกดเดิน
                float angleToTurn = Vector3.SignedAngle(visualForward, moveDir, Vector3.up);

                // 3. สั่งหมุนรอบแกน Y ของโลกเท่านั้น! (วิธีนี้แกน X และ Z จะถูกล็อกไว้อย่างปลอดภัย ไม่หน้าทิ่มแน่นอน)
                transform.Rotate(Vector3.up, angleToTurn, Space.World);
                // =====================================
            }

            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

            // ใช้ moveDir เพื่อกระโดดไปข้างหน้าเสมอ
            Vector3 hopForce = (moveDir * hopForwardForce * speedMult) + (Vector3.up * hopUpwardForce);
            rb.AddForce(hopForce, ForceMode.Impulse);

            currentStamina -= currentCost;
            lastStaminaUseTime = Time.time;
            nextHopTime = Time.time + hopInterval;
        }
    }

    public void Action()
    {
        if (!isPossessed) return;

        // เช็กเวลาว่าเลยช่วง Cooldown หรือยัง + ต้องอยู่บนพื้น
        if (Time.time >= nextActionTime && currentStamina >= actionCost && IsGrounded())
        {
            currentStamina -= actionCost;
            lastStaminaUseTime = Time.time;
            currentStress += 20f;

            rb.linearVelocity = Vector3.zero;
            rb.AddForce(Vector3.up * heavySmashForce, ForceMode.Impulse);

            // ตั้งเวลาดีเลย์สำหรับการเคลื่อนที่ (Hop) และ การกระโดด (Action) ครั้งถัดไป
            nextHopTime = Time.time + 1.0f;
            nextActionTime = Time.time + actionCooldown;
        }
    }

    private bool IsGrounded()
    {
        // ขยับจุดยิง Raycast ลงมาด้านล่างสุดของ Collider เพื่อป้องกันการยิงชนตัวเอง
        Vector3 rayStart = col.bounds.center;
        rayStart.y -= (col.bounds.extents.y - 0.1f);

        // ยิง Raycast ลงไปสั้นๆ แค่ 0.3f
        return Physics.Raycast(rayStart, Vector3.down, 0.3f);
    }

    public float GetStaminaPercentage() => currentStamina / maxStamina;
    public float GetStressPercentage() => currentStress / maxStress;
    public void AddStress(float amount) { currentStress += amount; }
}