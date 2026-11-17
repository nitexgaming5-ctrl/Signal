using UnityEngine;

/// <summary>
/// Applies the active rental tier to the prototype shelter footprint.
/// Later tiers can swap to authored prefab variants without changing rent logic.
/// </summary>
public sealed class ShelterVisualController : MonoBehaviour
{
    [SerializeField] private ShelterManager shelterManager;
    [SerializeField] private SpriteRenderer floor;

    private Transform north;
    private Transform west;
    private Transform east;
    private Transform southLeft;
    private Transform southRight;

    private void Awake()
    {
        if (shelterManager == null)
            shelterManager = FindAnyObjectByType<ShelterManager>();

        north = transform.Find("ShelterWallNorth");
        west = transform.Find("ShelterWallWest");
        east = transform.Find("ShelterWallEast");
        southLeft = transform.Find("ShelterWallSouthLeft");
        southRight = transform.Find("ShelterWallSouthRight");

        if (floor == null)
        {
            var floorObject = transform.Find("ShelterFloor");
            if (floorObject != null)
                floor = floorObject.GetComponent<SpriteRenderer>();
        }
    }

    private void Start()
    {
        if (shelterManager != null)
        {
            shelterManager.ShelterChanged += Apply;
            Apply(shelterManager.Current);
        }
    }

    private void OnDestroy()
    {
        if (shelterManager != null)
            shelterManager.ShelterChanged -= Apply;
    }

    private void Apply(ShelterOption shelter)
    {
        if (shelter == null)
            return;

        var index = shelterManager.CurrentIndex;
        var width = 10f + index * 1.5f;
        var height = 7f + index * 0.8f;
        var wallThickness = 0.55f;
        var openingWidth = 2f;
        var sideWidth = (width - openingWidth) * 0.5f;
        var wallColor = Color.Lerp(new Color(0.18f, 0.25f, 0.28f), new Color(0.35f, 0.40f, 0.40f), index / 4f);

        if (floor != null)
        {
            floor.size = new Vector2(width, height);
            floor.color = Color.Lerp(new Color(0.62f, 0.66f, 0.67f), new Color(0.82f, 0.84f, 0.82f), index / 4f);
        }

        SetWall(north, new Vector3(0f, height * 0.5f, 0.2f), new Vector2(width, wallThickness), wallColor);
        SetWall(west, new Vector3(-width * 0.5f, 0f, 0.2f), new Vector2(wallThickness, height), wallColor);
        SetWall(east, new Vector3(width * 0.5f, 0f, 0.2f), new Vector2(wallThickness, height), wallColor);
        SetWall(southLeft, new Vector3(-(openingWidth + sideWidth) * 0.5f, -height * 0.5f, 0.2f), new Vector2(sideWidth, wallThickness), wallColor);
        SetWall(southRight, new Vector3((openingWidth + sideWidth) * 0.5f, -height * 0.5f, 0.2f), new Vector2(sideWidth, wallThickness), wallColor);
    }

    private static void SetWall(Transform wall, Vector3 position, Vector2 size, Color color)
    {
        if (wall == null)
            return;

        wall.position = position;
        var renderer = wall.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.size = size;
            renderer.color = color;
        }

        var collider = wall.GetComponent<BoxCollider2D>();
        if (collider != null)
            collider.size = size;
    }
}
