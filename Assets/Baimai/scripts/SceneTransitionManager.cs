using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    // สร้างเป็น Singleton เพื่อให้เรียกใช้จากไฟล์ไหนก็ได้ในเกม
    public static SceneTransitionManager Instance;

    [Header("UI References")]
    public CanvasGroup transitionCanvasGroup;

    [Header("Settings")]
    public float fadeDuration = 1f; // ความเร็วตอนเฟดมืด/สว่าง
    public float waitTime = 1f; // ระยะเวลาแช่หน้าจอให้คนอ่านชื่อเกม

    private void Awake()
    {
        // เช็กว่ามี Transition Manager อยู่ในเกมหรือยัง ถ้ายังให้ตั้งตัวนี้เป็นตัวหลัก
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // คำสั่งเทพ: ห้ามทำลาย Object นี้เวลาเปลี่ยนฉาก!
        }
        else
        {
            Destroy(gameObject); // ถ้ามีซ้ำซ้อนให้ทำลายทิ้ง
        }
    }


    // เรียกใช้ฟังก์ชันนี้ตอนโหลดด้วยชื่อด่าน
    public void LoadScene(string sceneName)
    {
        StartCoroutine(TransitionRoutine(sceneName));
    }

    // เรียกใช้ฟังก์ชันนี้ตอนโหลดด้วย Index (เช่น 0, 1, 2)
    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(TransitionRoutineIndex(sceneIndex));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        yield return StartCoroutine(FadeIn());

        // โหลดฉากใหม่แบบเบื้องหลัง (เกมจะไม่ค้าง)
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone) yield return null;

        yield return StartCoroutine(FadeOut());
    }

    private IEnumerator TransitionRoutineIndex(int sceneIndex)
    {
        yield return StartCoroutine(FadeIn());

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        while (!asyncLoad.isDone) yield return null;

        yield return StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        // 1. เช็กก่อนเลยว่า CanvasGroup ยังอยู่ดีไหม
        if (transitionCanvasGroup == null) yield break;

        transitionCanvasGroup.blocksRaycasts = true;
        float time = 0;
        while (time < fadeDuration)
        {
            // 2. เช็กซ้ำใน Loop เพราะตอนเปลี่ยนฉาก Object อาจโดนทำลายกะทันหัน
            if (transitionCanvasGroup == null) yield break;

            transitionCanvasGroup.alpha = Mathf.Lerp(0, 1, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        if (transitionCanvasGroup != null) transitionCanvasGroup.alpha = 1f;
        yield return new WaitForSeconds(waitTime);
    }

    private IEnumerator FadeOut()
    {
        // 3. เช็กความพร้อมก่อนเริ่มเฟดออก
        if (transitionCanvasGroup == null) yield break;

        float time = 0;
        while (time < fadeDuration)
        {
            // 4. ถ้าจังหวะนี้โหลดฉากเสร็จแล้วเกิดอะไรขึ้นกับ Object ให้หยุด Coroutine ทันที
            if (transitionCanvasGroup == null) yield break;

            transitionCanvasGroup.alpha = Mathf.Lerp(1, 0, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.alpha = 0f;
            transitionCanvasGroup.blocksRaycasts = false;
        }
    }
}