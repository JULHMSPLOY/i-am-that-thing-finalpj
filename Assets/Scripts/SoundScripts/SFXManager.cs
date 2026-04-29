using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    public AudioSource sfxSource;

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
        sfxSource.volume = 1f; // ค่าเริ่มต้น 80%
    }

    public void SetVolume(float value)
    {
        sfxSource.volume = value;
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
    // เพิ่มฟังก์ชันนี้ในไฟล์ SFXManager.cs
    public void PlayRandomSFX(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;

        // สุ่มเลือก index จากจำนวนไฟล์ที่มี
        int randomIndex = Random.Range(0, clips.Length);

        // สุ่ม Pitch เล็กน้อยเพื่อให้เสียงดูธรรมชาติ (0.9 ถึง 1.1)
        sfxSource.pitch = Random.Range(0.9f, 1.1f);

        sfxSource.PlayOneShot(clips[randomIndex]);

        // คืนค่า pitch เป็นปกติ
        sfxSource.pitch = 1f;
    }

}
