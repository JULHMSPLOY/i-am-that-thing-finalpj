using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    public AudioSource bgmSource;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // เพิ่มการตรวจสอบเล็กน้อย กัน Error
        if (bgmSource == null) bgmSource = GetComponent<AudioSource>();

        if (bgmSource != null)
        {
            bgmSource.volume = 1f;
            bgmSource.Play();
        }
    }

    public void SetVolume(float value)
    {
        Debug.Log("Slider value = " + value);
        if (bgmSource != null) bgmSource.volume = value;
    }

    // ⭐ เพิ่ม 2 ฟังก์ชันนี้สำหรับ QTE System
    public void PauseBGM()
    {
        if (bgmSource != null) bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        if (bgmSource != null) bgmSource.UnPause();
    }
}