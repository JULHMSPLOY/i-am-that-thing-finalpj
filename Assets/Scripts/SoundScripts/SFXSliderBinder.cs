using UnityEngine;
using UnityEngine.UI;

public class SFXSliderBinder : MonoBehaviour
{
    public Slider slider;

    void Start()
    {
        slider.value = 1f;
        SFXManager.Instance.SetVolume(slider.value);

        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnSliderChanged(float value)
    {
        Debug.Log("SFX Slider value = " + value);
        SFXManager.Instance.SetVolume(value);
    }
}
