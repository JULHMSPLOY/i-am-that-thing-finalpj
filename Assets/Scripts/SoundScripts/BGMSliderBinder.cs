using UnityEngine;
using UnityEngine.UI;

public class BGMSliderBinder : MonoBehaviour
{
    public Slider slider;

    void Start()
    {
        slider.value = 1f;
        BGMManager.Instance.SetVolume(slider.value);

        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnSliderChanged(float value)
    {
        Debug.Log("Slider value = " + value);
        BGMManager.Instance.SetVolume(value);
    }
}
