using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Prop_Ball : MonoBehaviour, IPossessable
{
    private Rigidbody rb;
    public float rollSpeed = 15f;
    public float sprintMultiplier = 2f;
    public float jumpForce = 7f;
    private bool isPossessed = false;
    private Collider col;

    [Header("Status System (Stamina)")]
    public float maxStamina = 100f;
    private float currentStamina;
    public float walkDrainRate = 5f;
    public float sprintDrainRate = 15f;
    public float jumpCost = 15f;
    public float staminaRegenRate = 10f;
    public float regenDelay = 0.5f;
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

        rb.AddForce(moveDir * rollSpeed * speedMult);
    }

    public void Action()
    {
        if (!isPossessed) return;

        if (currentStamina >= jumpCost)
        {
            float distanceToBottom = col.bounds.extents.y + 0.1f;
            if (Physics.Raycast(transform.position, Vector3.down, distanceToBottom))
            {
                currentStamina -= jumpCost;
                lastStaminaUseTime = Time.time;
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                currentStress += 10f;
            }
        }
    }

    public float GetStaminaPercentage() => currentStamina / maxStamina;
    public float GetStressPercentage() => currentStress / maxStress;
    public void AddStress(float amount) { currentStress += amount; }
}