using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject settingPopup;
    public GameObject howToPlayPopup;

    public void PlayGame()
    {
        // แก้จาก SceneManager.LoadScene("Level02"); เป็นบรรทัดล่างนี้
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene("Level02");
        }
        else
        {
            SceneManager.LoadScene("Level02"); // กันพลาดถ้าลืมวาง SceneTransition ในฉากแรก
        }
    }

    // ===== Setting =====
    public void OpenSetting()
    {
        settingPopup.SetActive(true);
    }

    public void CloseSetting()
    {
        settingPopup.SetActive(false);
    }

    // ===== How To Play =====
    public void OpenHowToPlay()
    {
        howToPlayPopup.SetActive(true);
    }

    public void CloseHowToPlay()
    {
        howToPlayPopup.SetActive(false);
    }
}
