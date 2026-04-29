using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public Slider staminaBar;
    public Slider stressBar;

    public float maxStamina = 100f;
    public float stamina = 100f;

    public float stress = 0f;

    bool staminaDecreasing = false;
    bool stressIncreasing = false;

    void Start()
    {
        staminaBar.maxValue = maxStamina;
        staminaBar.value = stamina;

        stressBar.maxValue = 100f;
        stressBar.value = stress;
    }

    void Update()
    {
        // กดครั้งเดียวเพื่อสลับ
        if (Input.GetKeyDown(KeyCode.Z))
        {
            staminaDecreasing = !staminaDecreasing;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            stressIncreasing = !stressIncreasing;
        }

        // Stamina ลด / เพิ่ม
        if (staminaDecreasing)
        {
            stamina -= 20f * Time.deltaTime;
        }
        else
        {
            stamina += 10f * Time.deltaTime;
        }

        // Stress เพิ่ม / ลด
        if (stressIncreasing)
        {
            stress += 10f * Time.deltaTime;
        }
        else
        {
            stress -= 5f * Time.deltaTime;
        }

        stamina = Mathf.Clamp(stamina, 0, maxStamina);
        stress = Mathf.Clamp(stress, 0, 100);

        staminaBar.value = stamina;
        stressBar.value = stress;
    }
}