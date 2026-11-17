using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Signal.Game
{
    /// <summary>Pointer-driven tabletop prop used by the workbench close-up.</summary>
    public sealed class WorkbenchDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IPointerClickHandler
    {
        private RectTransform _surface;
        private Canvas _canvas;
        private RectTransform _rect;
        private string _label;
        private string _description;
        private Action<string> _onSelected;

        public void Configure(RectTransform surface, Canvas canvas, string label, string description, Action<string> onSelected)
        {
            _surface = surface;
            _canvas = canvas;
            _rect = transform as RectTransform;
            _label = label;
            _description = description;
            _onSelected = onSelected;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_surface == null || _rect == null)
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _surface, eventData.position, _canvas == null ? null : _canvas.worldCamera, out Vector2 localPoint))
            {
                Rect bounds = _surface.rect;
                float halfWidth = _rect.rect.width * 0.5f;
                float halfHeight = _rect.rect.height * 0.5f;
                localPoint.x = Mathf.Clamp(localPoint.x, bounds.xMin + halfWidth, bounds.xMax - halfWidth);
                localPoint.y = Mathf.Clamp(localPoint.y, bounds.yMin + halfHeight, bounds.yMax - halfHeight);
                _rect.anchoredPosition = localPoint;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onSelected?.Invoke(_label + " — " + _description);
        }
    }
}