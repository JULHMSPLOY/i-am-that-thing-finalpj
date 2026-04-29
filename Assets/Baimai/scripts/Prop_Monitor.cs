using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Prop_Monitor : MonoBehaviour, IPossessable
{
    private Rigidbody rb;
    private Collider col;
    private bool isPossessed = false;

    [Header("Movement")]
    public float moveSpeed = 5f; // เปลี่ยนจาก slideForce เป็น moveSpeed
    public float sprintMultiplier = 2.5f;
    public float turnSpeed = 10f;

    [Header("Action")]
    public float jumpForce = 6f;

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

        // ล็อกการหมุนแกน X และ Z เพื่อป้องกันไม่ให้หน้าจอล้มคว่ำ
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
        if (!isPossessed) return;

        // หยุดนิ่งทันทีเมื่อปล่อยปุ่มทิศทาง (และต้องอยู่บนพื้น)
        if (moveDir.magnitude == 0)
        {
            if (IsGrounded())
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
            return;
        }

        if (isSprinting && currentStamina <= 0) isSprinting = false;

        if (moveDir.magnitude > 0.1f)
        {
            float currentStressRate = isSprinting ? sprintStressRate : walkStressRate;
            currentStress += currentStressRate * Time.deltaTime;
        }

        float currentDrain = isSprinting ? sprintDrainRate : walkDrainRate;

        if (currentStamina <= 0 && currentDrain > 0) return;

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

        // ใช้การเซ็ตความเร็ว (Velocity) ตรงๆ แทนการใช้แรงผลัก (AddForce)
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