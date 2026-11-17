using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Compact HUD adapter for interaction prompts, feedback, and player state.
/// It owns no gameplay logic and can be replaced by a proper HUD later.
/// </summary>
public sealed class InteractionUI : MonoBehaviour
{
    [SerializeField] private Text promptLabel;
    [SerializeField] private Text feedbackLabel;
    [SerializeField] private Text stateLabel;
    [SerializeField] private InteractionController interactionController;
    [SerializeField] private PlayerState playerState;
    [SerializeField] private NightPressureSystem nightPressure;
    [SerializeField] private GameSaveSystem gameSaveSystem;
    [SerializeField] private DistrictOutingSystem districtOutingSystem;

    private Coroutine feedbackRoutine;

    private void Start()
    {
        if (interactionController == null)
            interactionController = FindAnyObjectByType<InteractionController>();
        if (playerState == null)
            playerState = FindAnyObjectByType<PlayerState>();
        if (nightPressure == null)
            nightPressure = FindAnyObjectByType<NightPressureSystem>();
        if (gameSaveSystem == null)
            gameSaveSystem = FindAnyObjectByType<GameSaveSystem>();
        if (districtOutingSystem == null)
            districtOutingSystem = FindAnyObjectByType<DistrictOutingSystem>();

        if (interactionController != null)
        {
            interactionController.PromptChanged += RefreshPrompt;
            interactionController.FeedbackRaised += ShowFeedback;
        }

        if (playerState != null)
            playerState.StateChanged += RefreshState;
        if (nightPressure != null)
            nightPressure.NightResolved += ShowFeedback;
        if (gameSaveSystem != null)
            gameSaveSystem.Feedback += ShowFeedback;
        if (districtOutingSystem != null)
            districtOutingSystem.StatusChanged += ShowFeedback;

        RefreshPrompt(string.Empty);
        RefreshState();
    }

    private void OnDestroy()
    {
        if (interactionController != null)
        {
            interactionController.PromptChanged -= RefreshPrompt;
            interactionController.FeedbackRaised -= ShowFeedback;
        }

        if (playerState != null)
            playerState.StateChanged -= RefreshState;
        if (nightPressure != null)
            nightPressure.NightResolved -= ShowFeedback;
        if (gameSaveSystem != null)
            gameSaveSystem.Feedback -= ShowFeedback;
        if (districtOutingSystem != null)
            districtOutingSystem.StatusChanged -= ShowFeedback;
    }

    public void RefreshPrompt(string prompt)
    {
        if (promptLabel == null)
            return;

        promptLabel.text = prompt;
        promptLabel.gameObject.SetActive(!string.IsNullOrEmpty(prompt));
    }

    public void ShowFeedback(string message)
    {
        if (feedbackRoutine != null)
            StopCoroutine(feedbackRoutine);
        feedbackRoutine = StartCoroutine(ShowFeedbackRoutine(message));
    }

    private IEnumerator ShowFeedbackRoutine(string message)
    {
        if (feedbackLabel != null)
        {
            feedbackLabel.text = message;
            feedbackLabel.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(3f);

        if (feedbackLabel != null)
            feedbackLabel.gameObject.SetActive(false);
    }

    private void RefreshState()
    {
        if (stateLabel == null || playerState == null)
            return;

        stateLabel.text = $"CASH  {playerState.Money:000}\n" +
                          $"HP       {playerState.Health:000}\n" +
                          $"ZONE  {playerState.LocationName.ToUpperInvariant()}";
    }
}
