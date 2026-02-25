using UnityEngine;
using UnityEngine.UIElements;
using ZMediaTask.Presentation.Services;

namespace ZMediaTask.Presentation.Presenters
{
    public sealed class SafeAreaController
    {
        private readonly VisualElement _uiRoot;
        private readonly VisualElement _safeAreaRoot;

        private Rect _lastSafeAreaPx;
        private int _lastScreenWidth = -1;
        private int _lastScreenHeight = -1;
        private float _lastPanelWidth = -1f;
        private float _lastPanelHeight = -1f;

        public SafeAreaController(VisualElement uiRoot, VisualElement safeAreaRoot)
        {
            _uiRoot = uiRoot;
            _safeAreaRoot = safeAreaRoot;
            _uiRoot.RegisterCallback<GeometryChangedEvent>(OnRootGeometryChanged);
            Refresh(force: true);
        }

        public void Refresh(bool force = false)
        {
            if (_safeAreaRoot == null || _uiRoot == null) return;

            var panelWidth = _uiRoot.resolvedStyle.width;
            var panelHeight = _uiRoot.resolvedStyle.height;
            if (panelWidth <= 0f || panelHeight <= 0f) return;

            var safeArea = Screen.safeArea;
            var screenWidth = Screen.width;
            var screenHeight = Screen.height;

            if (!force &&
                screenWidth == _lastScreenWidth &&
                screenHeight == _lastScreenHeight &&
                Mathf.Approximately(panelWidth, _lastPanelWidth) &&
                Mathf.Approximately(panelHeight, _lastPanelHeight) &&
                RectApproximatelyEqual(safeArea, _lastSafeAreaPx))
            {
                return;
            }

            var insets = SafeAreaLayoutMath.ComputeInsets(
                safeArea, screenWidth, screenHeight, panelWidth, panelHeight);

            _safeAreaRoot.style.paddingLeft = insets.x;
            _safeAreaRoot.style.paddingTop = insets.y;
            _safeAreaRoot.style.paddingRight = insets.z;
            _safeAreaRoot.style.paddingBottom = insets.w;

            _lastSafeAreaPx = safeArea;
            _lastScreenWidth = screenWidth;
            _lastScreenHeight = screenHeight;
            _lastPanelWidth = panelWidth;
            _lastPanelHeight = panelHeight;
        }

        public void Dispose()
        {
            _uiRoot?.UnregisterCallback<GeometryChangedEvent>(OnRootGeometryChanged);
        }

        private void OnRootGeometryChanged(GeometryChangedEvent _) => Refresh(force: true);

        private static bool RectApproximatelyEqual(Rect a, Rect b)
        {
            return Mathf.Approximately(a.x, b.x) &&
                   Mathf.Approximately(a.y, b.y) &&
                   Mathf.Approximately(a.width, b.width) &&
                   Mathf.Approximately(a.height, b.height);
        }
    }
}
