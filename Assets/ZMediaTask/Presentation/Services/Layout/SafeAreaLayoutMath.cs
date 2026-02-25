using UnityEngine;

namespace ZMediaTask.Presentation.Services
{
    public static class SafeAreaLayoutMath
    {
        public static Vector4 ComputeInsets(
            Rect safeAreaPx,
            int screenWidthPx,
            int screenHeightPx,
            float panelWidthPt,
            float panelHeightPt)
        {
            if (screenWidthPx <= 0 || screenHeightPx <= 0 || panelWidthPt <= 0f || panelHeightPt <= 0f)
            {
                return Vector4.zero;
            }

            var clampedXMin = Mathf.Clamp(safeAreaPx.xMin, 0f, screenWidthPx);
            var clampedXMax = Mathf.Clamp(safeAreaPx.xMax, 0f, screenWidthPx);
            var clampedYMin = Mathf.Clamp(safeAreaPx.yMin, 0f, screenHeightPx);
            var clampedYMax = Mathf.Clamp(safeAreaPx.yMax, 0f, screenHeightPx);

            // Convert pixel insets to panel points, so UI Toolkit layout stays correct on high-DPI screens.
            var scaleX = panelWidthPt / screenWidthPx;
            var scaleY = panelHeightPt / screenHeightPx;

            var left = clampedXMin * scaleX;
            var right = (screenWidthPx - clampedXMax) * scaleX;
            var bottom = clampedYMin * scaleY;
            var top = (screenHeightPx - clampedYMax) * scaleY;

            return new Vector4(left, top, right, bottom);
        }
    }
}
