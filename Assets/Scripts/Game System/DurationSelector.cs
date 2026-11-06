
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DurationSelector : MonoBehaviour
{
    public Slider durationSlider;
    public TMP_Text durationValueText;

    void Start()
    {
        // A 0-minute time limit makes no sense, so the shortest game is 1 minute,
        // no matter what the slider was set to in the editor.
        if (durationSlider.minValue < 1f)
            durationSlider.minValue = 1f;
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
        int selectedMinutes = Mathf.Max(1, Mathf.RoundToInt(minutes));

        durationValueText.text = selectedMinutes + ":00";
        GameSettings.SelectedDuration = selectedMinutes * 60f;
    }

    void OnDestroy()
    {
        if (durationSlider != null)
            durationSlider.onValueChanged.RemoveListener(UpdateDuration);
    }
}
