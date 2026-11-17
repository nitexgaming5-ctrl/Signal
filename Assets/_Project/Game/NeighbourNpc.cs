using Signal.Core;
using UnityEngine;

namespace Signal.Game
{
    /// <summary>
    /// First ambient citizen. The Core brain decides what Milo wants to do; this component
    /// only moves and presents him in whichever shared city location is active.
    /// </summary>
    public sealed class NeighbourNpc : MonoBehaviour
    {
        private static readonly Vector2 WorkPosition = new Vector2(1000f, 1000.4f);
        private static readonly Vector2 RestPosition = new Vector2(3.5f, -17.5f);
        private static readonly Vector2 SocialPosition = new Vector2(18.2f, -11.6f);
        private static readonly Vector2 HomePosition = new Vector2(1016.5f, 1000f);

        [SerializeField] private float walkingSpeed = 1.6f;

        private readonly NpcUtilityBrain _brain = new NpcUtilityBrain();
        private GameClock _clock;
        private Vector2 _destination;
        private NpcActivity _activity;
        private SpriteRenderer _bodyRenderer;
        private bool _initialized;
        private string _currentPlace = "Hideout";

        public string DisplayName => "Milo";
        public NpcActivity Activity => _activity;
        public Vector2 Position => transform.position;

        public void Bind(GameClock clock)
        {
            if (clock == null)
                return;

            if (_clock != null)
                _clock.Changed -= OnClockChanged;

            _clock = clock;
            _clock.Changed += OnClockChanged;
            BuildVisual();
            RefreshFromClock(true);
            _initialized = true;
        }

        public void SetCurrentPlace(string place)
        {
            _currentPlace = place ?? string.Empty;
            transform.position = new Vector3(_destination.x, _destination.y, 0f);
        }

        public bool CanTalkFrom(Vector2 position)
        {
            return _initialized && Vector2.Distance(position, Position) <= 1.85f;
        }

        public bool TryTalk(Vector2 playerPosition, out string dialogue)
        {
            dialogue = null;
            if (!CanTalkFrom(playerPosition))
                return false;

            switch (_activity)
            {
                case NpcActivity.Work:
                    dialogue = "Milo: Morning. I'm the baker at the corner shop. The ovens are on before most of the street wakes up.";
                    break;
                case NpcActivity.Rest:
                    dialogue = "Milo: Give me five minutes. Those ovens run hot, and my knees complain louder every year.";
                    break;
                case NpcActivity.Socialize:
                    dialogue = "Milo: The bakery is the part everyone sees. After closing, I help a few people with things they can't ask for at the counter.";
                    break;
                default:
                    dialogue = "Milo: Shop's closed for today. I start before sunrise. Catch me when the shutters are up.";
                    break;
            }

            return true;
        }

        private void OnEnable()
        {
            if (_initialized)
                RefreshFromClock(true);
        }

        private void OnDestroy()
        {
            if (_clock != null)
                _clock.Changed -= OnClockChanged;
        }

        private void Update()
        {
            if (!_initialized)
                return;

            Vector3 destination = new Vector3(_destination.x, _destination.y, transform.position.z);
            Vector3 current = transform.position;
            if (Mathf.Abs(destination.x - current.x) > 0.02f)
                _bodyRenderer.flipX = destination.x < current.x;

            transform.position = Vector3.MoveTowards(current, destination, walkingSpeed * Time.deltaTime);
        }

        private void OnClockChanged(GameClock clock)
        {
            RefreshFromClock(false);
        }

        private void RefreshFromClock(bool snapToDestination)
        {
            if (_clock == null)
                return;

            NpcActivity next = _brain.AdvanceTo(_clock.Day, _clock.MinuteOfDay);
            if (next != _activity || snapToDestination)
            {
                _activity = next;
                _destination = PositionFor(next);
                if (snapToDestination || ActivityPlace(next) != _currentPlace)
                    transform.position = new Vector3(_destination.x, _destination.y, 0f);
            }
        }

        private static string ActivityPlace(NpcActivity activity)
        {
            switch (activity)
            {
                case NpcActivity.Work: return "CornerShop";
                case NpcActivity.Home: return "Apartment";
                default: return "Street";
            }
        }

        private static Vector2 PositionFor(NpcActivity activity)
        {
            switch (activity)
            {
                case NpcActivity.Work: return WorkPosition;
                case NpcActivity.Rest: return RestPosition;
                case NpcActivity.Socialize: return SocialPosition;
                default: return HomePosition;
            }
        }

        private void BuildVisual()
        {
            if (_bodyRenderer != null)
                return;

            _bodyRenderer = CreatePart("Coat", new Vector2(0f, -0.12f), new Vector2(0.58f, 0.76f),
                                       new Color32(65, 78, 83, 255), 8);
            CreatePart("Apron", new Vector2(0f, -0.18f), new Vector2(0.34f, 0.5f),
                       new Color32(190, 178, 151, 255), 9);
            CreatePart("Face", new Vector2(0f, 0.42f), new Vector2(0.43f, 0.43f),
                       new Color32(190, 145, 111, 255), 10);
            CreatePart("Hair", new Vector2(0f, 0.61f), new Vector2(0.46f, 0.2f),
                       new Color32(57, 49, 43, 255), 11);
            CreatePart("BakerCap", new Vector2(0f, 0.73f), new Vector2(0.48f, 0.16f),
                       new Color32(218, 213, 195, 255), 12);
        }

        private SpriteRenderer CreatePart(string partName, Vector2 localPosition, Vector2 size, Color color, int sortingOrder)
        {
            var part = new GameObject(partName);
            part.transform.SetParent(transform, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = new Vector3(size.x, size.y, 1f);

            SpriteRenderer renderer = part.AddComponent<SpriteRenderer>();
            renderer.sprite = PixelSprites.Solid(color);
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }
    }
}