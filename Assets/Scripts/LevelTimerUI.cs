using UnityEngine;
using TMPro;

public class LevelTimerUI : MonoBehaviour
{
    public float levelTime = 180f;
    public TextMeshProUGUI timerText;

    // ตัวแปรเช็กว่า Game Over ไปหรือยัง จะได้ไม่เรียกฟังก์ชันซ้ำซ้อน
    private bool isGameOverTriggered = false;

    void Update()
    {
        // ถ้า Game Over ไปแล้ว ให้หยุดการอัปเดตเวลาทันที
        if (isGameOverTriggered) return;

        if (levelTime > 0)
        {
            levelTime -= Time.deltaTime;

            // ถ้าเวลาน้อยกว่าหรือเท่ากับ 0 แปลว่าหมดเวลาแล้ว
            if (levelTime <= 0)
            {
                levelTime = 0; // ล็อกตัวเลขเวลาไว้ที่ 0
                TriggerTimeOutGameOver(); // สั่ง Game Over
            }
        }

        // คำนวณและแสดงผลเวลา
        int minutes = Mathf.FloorToInt(levelTime / 60);
        int seconds = Mathf.FloorToInt(levelTime % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void TriggerTimeOutGameOver()
    {
        isGameOverTriggered = true;

        // ค้นหา PlayerManager ในฉากแบบเดียวกับที่ EnemyAI ทำ
        PlayerManager playerManager = FindAnyObjectByType<PlayerManager>();

        if (playerManager != null)
        {
            // สั่งเรียกหน้าจอ Game Over
            playerManager.TriggerGameOver();
        }
        else
        {
            Debug.LogWarning("หมดเวลาแล้ว แต่หา PlayerManager ในฉากไม่เจอ!");
        }
    }
}