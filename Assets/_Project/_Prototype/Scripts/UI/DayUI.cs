using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Minimal day display. It listens to GameManager rather than polling so future
/// HUD elements can subscribe to the same progression event.
/// </summary>
public sealed class DayUI : MonoBehaviour
{
    [SerializeField]
    private Text dayLabel;

    private void Start()
    {
        if (dayLabel == null)
            dayLabel = GetComponentInChildren<Text>();

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("DayUI could not find GameManager.");
            return;
        }

        GameManager.Instance.DayChanged += Refresh;
        GameManager.Instance.DayRestored += Refresh;
        Refresh(GameManager.Instance.CurrentDay);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.DayChanged -= Refresh;
            GameManager.Instance.DayRestored -= Refresh;
        }
    }

    private void Refresh(int day)
    {
        if (dayLabel != null)
            dayLabel.text = $"DAY {day}   F5 SAVE / F9 LOAD";
    }
}
