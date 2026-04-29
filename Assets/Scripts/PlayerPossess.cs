using UnityEngine;

public class PlayerPossess : MonoBehaviour
{
    public float possessTimer = 20f;
    public bool isPossessing = false;

    void Update()
    {
        // ตรวจว่ากด E หรือไม่
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Pressed E");   // Debug ตรวจปุ่ม
            StartTimer();
        }

        if (isPossessing)
        {
            possessTimer -= Time.deltaTime;

            Debug.Log("Timer: " + possessTimer); // Debug เวลาที่กำลังนับ

            if (possessTimer <= 0)
            {
                possessTimer = 0;
                ExitProp();
            }
        }
    }

    void StartTimer()
    {
        Debug.Log("Timer Started");

        possessTimer = 20f;
        isPossessing = true;
    }

    void ExitProp()
    {
        Debug.Log("Timer Ended");

        isPossessing = false;
    }
}