using System;
using UnityEngine;

/// <summary>
/// Switches between the exterior city and the private shelter interior.
/// Both spaces stay in one scene for this prototype, but are isolated like
/// separate maps so the player never sees the city while inside the base.
/// </summary>
public sealed class ShelterSpaceController : MonoBehaviour
{
    [SerializeField] private GameObject worldRoot;
    [SerializeField] private GameObject cityRoot;
    [SerializeField] private GameObject shelterRoot;
    [SerializeField] private GameObject player;
    [SerializeField] private bool startInside = true;

    public bool IsInside { get; private set; }
    public event Action<bool> SpaceChanged;

    private void Awake()
    {
        if (worldRoot == null)
            worldRoot = GameObject.Find("--- WORLD ---");
        if (cityRoot == null)
            cityRoot = GameObject.Find("--- CITY ---");
        if (shelterRoot == null)
            shelterRoot = GameObject.Find("--- SHELTER ---");
        if (player == null)
            player = GameObject.Find("Player");

        // Publish the authored initial state before any other component's Start.
        IsInside = startInside;
    }

    private void Start()
    {
        SetInside(startInside, false, false);
    }

    public void RestoreSpace(bool inside)
    {
        SetInside(inside, false, false);
    }

    public void Toggle(InteractionController interactor)
    {
        SetInside(!IsInside, true, true);
        interactor.ShowFeedback(IsInside
            ? "Shelter secured. The city can wait outside."
            : "Back on the street. Keep your head down.");
    }

    private void SetInside(bool inside, bool movePlayer, bool notifyListeners)
    {
        var changed = IsInside != inside;
        IsInside = inside;

        if (worldRoot != null)
            worldRoot.SetActive(!inside);
        if (cityRoot != null)
            cityRoot.SetActive(!inside);
        if (shelterRoot != null)
            shelterRoot.SetActive(inside);

        if (movePlayer && player != null)
        {
            var destination = inside ? new Vector3(0f, 0.2f, 0f) : new Vector3(0f, -1.35f, 0f);
            player.transform.position = destination;
            var body = player.GetComponent<Rigidbody2D>();
            if (body != null)
                body.position = new Vector2(destination.x, destination.y);
        }

        var state = player == null ? null : player.GetComponent<PlayerState>();
        if (state != null)
        {
            var shelter = FindAnyObjectByType<ShelterManager>();
            state.SetLocation(inside && shelter != null && shelter.Current != null
                ? shelter.Current.displayName
                : "Contested Street");
        }

        if (changed && notifyListeners)
            SpaceChanged?.Invoke(inside);
    }
}
