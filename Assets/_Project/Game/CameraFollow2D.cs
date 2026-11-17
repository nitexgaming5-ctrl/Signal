using UnityEngine;

namespace Signal.Game
{
    /// <summary>Keeps an orthographic camera on a target without showing anything outside the bounds.</summary>
    public sealed class CameraFollow2D : MonoBehaviour
    {
        private Transform _target;
        private Rect _bounds;
        private Camera _camera;

        public void Configure(Transform target, Rect bounds)
        {
            _target = target;
            _bounds = bounds;
            _camera = GetComponent<Camera>();
            Follow();
        }

        private void LateUpdate()
        {
            Follow();
        }

        private void Follow()
        {
            if (_target == null || _camera == null)
                return;

            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;

            float x = ClampAxis(_target.position.x, _bounds.xMin + halfWidth, _bounds.xMax - halfWidth, _bounds.center.x);
            float y = ClampAxis(_target.position.y, _bounds.yMin + halfHeight, _bounds.yMax - halfHeight, _bounds.center.y);

            transform.position = new Vector3(x, y, transform.position.z);
        }

        // When the room is smaller than the view, the camera simply centres on it.
        private static float ClampAxis(float value, float min, float max, float centre)
        {
            return min > max ? centre : Mathf.Clamp(value, min, max);
        }
    }
}
