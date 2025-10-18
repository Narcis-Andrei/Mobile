using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI volToText;
    private const string PrefKey = "MasterVolume";

    private static void SavedVol()
    {
        float v = PlayerPrefs.HasKey(PrefKey) ? PlayerPrefs.GetFloat(PrefKey) : 0.4f;
        AudioListener.volume = Mathf.Clamp01(v);
    }

    private void Awake()
    {
        float v = PlayerPrefs.HasKey(PrefKey) ? PlayerPrefs.GetFloat(PrefKey) : 0.4f;

        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.value = v;
            slider.onValueChanged.AddListener(SetVolume);
        }

        Apply(v);
        UpdateText(v);
    }

    private void OnDestroy()
    {
        if (slider != null) slider.onValueChanged.RemoveListener(SetVolume);
    }

    public void SetVolume(float v)
    {
        Apply(v);
        PlayerPrefs.SetFloat(PrefKey, v);
        PlayerPrefs.Save();
        UpdateText(v);
    }

    private void Apply(float v)
    {
        AudioListener.volume = Mathf.Clamp01(v);
    }

    private void UpdateText(float v)
    {
        if (volToText != null)
            volToText.text = $"{Mathf.RoundToInt(v * 100)}";
    }
}
