using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Prop_Chair : MonoBehaviour, IPossessable
{
    private Rigidbody rb;

    [Header("Walking Settings (Waddle)")]
    public float moveSpeed = 4.0f;
    public float sprintMultiplier = 2.0f;
    public float liftForce = 1.0f;
    public float stepInterval = 0.2f;
    public float airControlTorque = 10f;
    public float jumpForce = 5f;

    private bool isPossessed = false;
    private Collider col;

    // --- ตัวแปรสำหรับระบบ Spam ปุ่ม ---
    private bool keyReleased = true;    // เช็กว่าปล่อยปุ่มหรือยัง
    private bool isStepping = false;    // เช็กว่ากำลังอยู่ในจังหวะก้าวหรือไม่
    private float stepTimer = 0f;
    private Vector3 currentStepVelocity;
    private bool isLeftStep = true;

    [Header("Status System (Stamina)")]
    public float maxStamina = 50f;
    private float currentStamina;
    public float walkStepCost = 2f;
    public float sprintStepCost = 8f;
    public float jumpCost = 15f;
    public float staminaRegenRate = 5f;
    public float regenDelay = 0.5f;
    private float lastStaminaUseTime;

    [Header("Status System (Stress)")]
    public float maxStress = 100f;
    private float currentStress = 0f;
    public float idleStressRate = 3f;
    public float walkStressRate = 5f;
    public float sprintStressRate = 12f;

    [Header("Audio Settings")]
    public AudioClip[] footstepSFXs; // ใน Inspector ให้ใส่ Size = 3 แล้วลาก 3 ไฟล์มาใส่

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.constraints = RigidbodyConstraints.None;

        // ดึงจุดศูนย์ถ่วงลงต่ำ ให้เก้าอี้ไม่ล้มง่าย
        rb.centerOfMass = new Vector3(0, -col.bounds.extents.y * 0.8f, 0);
        currentStamina = maxStamina;
    }

    void Update()
    {
        // ฟื้นฟู Stamina
        if (Time.time > lastStaminaUseTime + regenDelay && currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
        }
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        // ระบบ Stress พื้นฐาน (อยู่นิ่งๆ ก็เครียด)
        if (isPossessed)
        {
            currentStress += idleStressRate * Time.deltaTime;
            currentStress = Mathf.Clamp(currentStress, 0, maxStress);
        }
    }

    public void OnPossess(Transform playerTransform) { isPossessed = true; }

    public void OnUnpossess()
    {
        isPossessed = false;
        currentStress = 0f; // รีเซ็ต Stress
        isStepping = false;
    }

    public void Move(Vector3 moveDir, bool isSprinting)
    {
        if (!isPossessed) return;

        // 1. ตรวจจับการปล่อยปุ่ม (ถ้าทิศทางเข้าใกล้ 0 แปลว่าปล่อยปุ่มแล้ว)
        if (moveDir.magnitude < 0.1f)
        {
            keyReleased = true; // รีโหลด พร้อมให้กดก้าวต่อไป
        }

        if (isSprinting && currentStamina <= 0) isSprinting = false;

        // 2. ถ้ากำลังก้าวอยู่ -> บังคับความเร็วให้คงที่
        if (isStepping)
        {
            if (Time.time < stepTimer)
            {
                // รักษาความเร็วให้คงที่ตลอดช่วงเวลาของ 1 ก้าว (ไม่เร่งความเร็วสะสม)
                rb.linearVelocity = new Vector3(currentStepVelocity.x, rb.linearVelocity.y, currentStepVelocity.z);

                // เพิ่ม Stress เฉพาะตอนที่กำลังขยับตัว
                float currentStressRate = isSprinting ? sprintStressRate : walkStressRate;
                currentStress += currentStressRate * Time.deltaTime;
            }
            else
            {
                // หมดเวลา 1 ก้าว -> เบรกรถให้หยุดนิ่ง
                if (IsGrounded())
                {
                    rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                }
                isStepping = false;
            }
        }
        // 3. เริ่มก้าวใหม่ (ต้องปล่อยปุ่มก่อน + กดปุ่มทิศทาง + อยู่บนพื้น)
        else if (keyReleased && moveDir.magnitude >= 0.1f && IsGrounded())
        {
            float currentCost = isSprinting ? sprintStepCost : walkStepCost;

            if (currentStamina >= currentCost)
            {

                if (footstepSFXs != null && SFXManager.Instance != null)
                {
                    SFXManager.Instance.PlayRandomSFX(footstepSFXs);
                }
                // อัปเดตสถานะการก้าว
                keyReleased = false; // ล็อกไว้! ต้องปล่อยปุ่มก่อนถึงจะก้าวครั้งหน้าได้
                isStepping = true;
                stepTimer = Time.time + stepInterval;

                currentStamina -= currentCost;
                lastStaminaUseTime = Time.time;

                // กำหนดความเร็วสำหรับก้าวนี้
                float speedMult = isSprinting ? sprintMultiplier : 1f;
                currentStepVelocity = moveDir * moveSpeed * speedMult;
                rb.linearVelocity = new Vector3(currentStepVelocity.x, rb.linearVelocity.y, currentStepVelocity.z);

                // สร้างแรงงัดที่มุมเก้าอี้ให้มันโยกดุ๊กดิ๊ก
                float halfWidth = col.bounds.extents.x;
                float halfHeight = col.bounds.extents.y;
                float sideOffset = isLeftStep ? -halfWidth * 0.8f : halfWidth * 0.8f;
                Vector3 liftPoint = transform.position + (transform.right * sideOffset) + (transform.up * -halfHeight);

                rb.AddForceAtPosition(Vector3.up * liftForce, liftPoint, ForceMode.Impulse);
                isLeftStep = !isLeftStep;
            }
        }
        // 4. ควบคุมกลางอากาศ
        else if (!IsGrounded() && moveDir.magnitude > 0.1f)
        {
            Vector3 torqueDirection = new Vector3(moveDir.z, 0, -moveDir.x);
            rb.AddTorque(torqueDirection * airControlTorque, ForceMode.Acceleration);
        }
    }

    public void Action()
    {
        if (!isPossessed) return;

        if (currentStamina >= jumpCost && IsGrounded())
        {
            currentStamina -= jumpCost;
            lastStaminaUseTime = Time.time;
            currentStress += 15f;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private bool IsGrounded()
    {
        float distanceToBottom = col.bounds.extents.y + 0.1f;
        return Physics.Raycast(col.bounds.center, Vector3.down, distanceToBottom);
    }

    public float GetStaminaPercentage() => currentStamina / maxStamina;
    public float GetStressPercentage() => currentStress / maxStress;
    public void AddStress(float amount) { currentStress += amount; }
}