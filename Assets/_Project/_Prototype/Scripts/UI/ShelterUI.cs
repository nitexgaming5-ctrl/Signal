using UnityEngine;
using UnityEngine.UI;

/// <summary>Displays the active rentable base and its prototype modifiers.</summary>
public sealed class ShelterUI : MonoBehaviour
{
    [SerializeField] private Text shelterLabel;
    [SerializeField] private ShelterManager shelterManager;

    private void Start()
    {
        if (shelterManager == null)
            shelterManager = FindAnyObjectByType<ShelterManager>();
        if (shelterManager != null)
            shelterManager.ShelterChanged += Refresh;
        Refresh(shelterManager == null ? null : shelterManager.Current);
    }

    private void OnDestroy()
    {
        if (shelterManager != null)
            shelterManager.ShelterChanged -= Refresh;
    }

    private void Refresh(ShelterOption shelter)
    {
        if (shelterLabel == null || shelter == null)
            return;

        shelterLabel.text = $"BASE  {shelter.displayName.ToUpperInvariant()}\n" +
                            $"STORAGE  {shelter.storageCapacity:00}   COMFORT  {shelter.comfort:00}";
    }
}
