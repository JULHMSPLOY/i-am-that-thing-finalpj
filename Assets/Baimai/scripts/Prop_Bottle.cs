using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Prop_Bottle : MonoBehaviour, IPossessable
{
    private Rigidbody rb;

    [Header("Movement")]
    public float moveSpeed = 15f;
    public float turnSpeed = 25f;
    public float sprintMultiplier = 2f;
    public float jumpForce = 7f;
    public float airControlTorque = 15f;
    private bool isPossessed = false;
    private Collider col;

    [Header("Status System (Stamina)")]
    public float maxStamina = 80f;
    private float currentStamina;
    public float walkDrainRate = 5f;
    public float sprintDrainRate = 20f;
    public float jumpCost = 15f;
    public float staminaRegenRate = 10f;
    public float regenDelay = 0.5f;
    private float lastStaminaUseTime;

    [Header("Status System (Stress)")]
    public float maxStress = 100f;
    private float currentStress = 0f;
    public float idleStressRate = 3f;
    public float walkStressRate = 5f;
    public float sprintStressRate = 12f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.constraints = RigidbodyConstraints.None;
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
        if (!isPossessed || moveDir.magnitude == 0) return;

        if (isSprinting && currentStamina <= 0)
        {
            isSprinting = false;
        }

        float speedMult = 1f;
        float currentDrain = isSprinting ? sprintDrainRate : walkDrainRate;

        if (currentStamina <= 0 && currentDrain > 0)
        {
            return;
        }

        if (moveDir.magnitude > 0.1f)
        {
            float currentStressRate = isSprinting ? sprintStressRate : walkStressRate;
            currentStress += currentStressRate * Time.deltaTime;
        }

        if (currentDrain > 0)
        {
            currentStamina -= currentDrain * Time.deltaTime;
            currentStamina = Mathf.Max(0, currentStamina);
            lastStaminaUseTime = Time.time;
        }

        if (isSprinting) speedMult = sprintMultiplier;

        if (IsGrounded())
        {
            rb.AddForce(moveDir * moveSpeed * speedMult);

            if (Mathf.Abs(transform.up.y) < 0.8f)
            {
                Vector3 longAxis = new Vector3(transform.up.x, 0, transform.up.z).normalized;
                Vector3 targetAxis = new Vector3(-moveDir.z, 0, moveDir.x).normalized;
                if (Vector3.Dot(longAxis, targetAxis) < 0) targetAxis = -targetAxis;

                Vector3 twistTorque = Vector3.Cross(longAxis, targetAxis);
                rb.AddTorque(twistTorque * turnSpeed, ForceMode.Acceleration);
            }
        }
        else
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
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            currentStress += 12f;
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