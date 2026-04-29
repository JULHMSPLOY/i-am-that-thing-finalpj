using UnityEngine;
using TMPro;

public class PropTimerUI : MonoBehaviour
{
    public PlayerPossess player;
    public TextMeshProUGUI timerText;

    void Start()
    {
        timerText.text = ""; // เริ่มต้นไม่แสดง
    }

    void Update()
    {
        if (player != null && player.isPossessing)
        {
            int time = Mathf.CeilToInt(player.possessTimer);
            timerText.text = "Only " + time + " seconds left as the object!"; // แสดงเวลาที่เหลือ
        }
        else
        {
            timerText.text = ""; // ซ่อน UI
        }
    }
}