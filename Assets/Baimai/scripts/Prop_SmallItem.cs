using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Prop_SmallItem : MonoBehaviour, IPossessable
{
    private Rigidbody rb;
    private Collider col;
    private bool isPossessed = false;

    [Header("Movement")]
    public float moveSpeed = 5f; // เปลี่ยนจาก slideForce เป็น moveSpeed
    public float sprintMultiplier = 2.5f;
    public float turnSpeed = 10f;

    [Header("Action")]
    public float flipJumpForce = 6f;

    [Header("Status System (Stamina)")]
    public float maxStamina = 60f;
    private float currentStamina;
    public float walkDrainRate = 2f;
    public float sprintDrainRate = 10f;
    public float actionCost = 15f;
    public float staminaRegenRate = 15f;
    public float regenDelay = 0.3f;
    private float lastStaminaUseTime;

    [Header("Status System (Stress)")]
    public float maxStress = 100f;
    private float currentStress = 0f;
    public float idleStressRate = 2f;
    public float walkStressRate = 4f;
    public float sprintStressRate = 10f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        currentStamina = maxStamina;
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
        if (!isPossessed) return;

        // แก้ไข: รวมเงื่อนไขหยุดนิ่งเมื่อปล่อยปุ่ม หรือ เมื่อ Stamina หมด (< 0)
        // เพื่อบังคับเซ็ต Velocity แกนแนวนอนให้เป็น 0 ทันที
        if (moveDir.magnitude == 0 || currentStamina <= 0)
        {
            if (IsGrounded())
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
            return;
        }

        // ป้องกันกรณีที่กด Shift ค้างไว้ตอน Stamina เพิ่งจะหมด
        if (isSprinting && currentStamina <= 0) isSprinting = false;

        if (moveDir.magnitude > 0.1f)
        {
            float currentStressRate = isSprinting ? sprintStressRate : walkStressRate;
            currentStress += currentStressRate * Time.deltaTime;
        }

        float currentDrain = isSprinting ? sprintDrainRate : walkDrainRate;

        if (currentDrain > 0)
        {
            currentStamina -= currentDrain * Time.deltaTime;
            lastStaminaUseTime = Time.time;
        }

        float speedMult = isSprinting ? sprintMultiplier : 1f;

        if (moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.deltaTime));
        }

        // ใช้การเซ็ตความเร็ว (Velocity) ตรงๆ
        Vector3 targetVelocity = moveDir * moveSpeed * speedMult;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }

    public void Action()
    {
        if (!isPossessed) return;

        if (currentStamina >= actionCost && IsGrounded())
        {
            currentStamina -= actionCost;
            lastStaminaUseTime = Time.time;
            currentStress += 15f;

            // ใส่แรงกระโดดขึ้นด้านบนอย่างเดียว
            rb.AddForce(Vector3.up * flipJumpForce, ForceMode.Impulse);

            // แก้ไข: ลบบรรทัด rb.AddTorque ออก เพื่อไม่ให้ Object หมุนตีลังกา
            // rb.AddTorque(transform.right * flipJumpForce, ForceMode.Impulse); 
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