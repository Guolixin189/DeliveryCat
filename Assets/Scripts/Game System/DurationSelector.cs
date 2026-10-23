
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DurationSelector : MonoBehaviour
{
    public Slider durationSlider;
    public TMP_Text durationValueText;

    void Start()
    {
        durationSlider.value = Mathf.Clamp(
            GameSettings.SelectedDuration / 60f,
            durationSlider.minValue,
            durationSlider.maxValue
        );

        durationSlider.onValueChanged.AddListener(UpdateDuration);
        UpdateDuration(durationSlider.value);
    }

    public void UpdateDuration(float minutes)
    {
        int selectedMinutes = Mathf.RoundToInt(minutes);

        durationValueText.text = selectedMinutes + ":00";
        GameSettings.SelectedDuration = selectedMinutes * 60f;
    }

    void OnDestroy()
    {
        if (durationSlider != null)
            durationSlider.onValueChanged.RemoveListener(UpdateDuration);
    }
}
