using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalTrigger : MonoBehaviour
{
    public GameObject goalUI;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player reached Goal");

            if (goalUI != null)
            {
                goalUI.SetActive(true);
            }

            // สั่งโหลดฉากถัดไปผ่านระบบ Transition
            LoadNextSceneWithTransition();
        }
    }

    void LoadNextSceneWithTransition()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // เช็คว่ามี Scene ถัดไปใน Build Settings หรือไม่
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            // ถ้ามี SceneTransitionManager ให้ใช้โหลดฉากแบบมีอนิเมชัน
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene(nextSceneIndex);
            }
            else
            {
                // ถ้าไม่มี (กันพลาด) ให้โหลดแบบปกติ
                SceneManager.LoadScene(nextSceneIndex);
            }
        }
        else
        {
            Debug.Log("No next scene. Game finished.");
            // หรืออาจจะสั่งกลับหน้า Main Menu (Index 0) ก็ได้
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene(0);
            }
        }
    }
}