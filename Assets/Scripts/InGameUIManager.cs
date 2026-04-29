using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameUIManager : MonoBehaviour
{
    public void RestartLevel()
    {
        // ดึงชื่อฉากปัจจุบันมาโหลดใหม่ผ่านระบบ Transition
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(currentSceneName);
        }
        else
        {
            // กันเหนียวถ้าหา Manager ไม่เจอ ให้โหลดแบบปกติ
            SceneManager.LoadScene(currentSceneName);
        }
    }

    public void GoToMainMenu()
    {
        // โหลดหน้าเมนูหลัก (Index 0) ผ่านระบบ Transition
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(0);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }
}