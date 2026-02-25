using System;
using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UIElements;
using ZMediaTask.Domain.Combat;
using ZMediaTask.Presentation.ViewModels;

namespace ZMediaTask.Presentation.Presenters
{
    public sealed class WrathCardPresenter : IDisposable
    {
        private const float ThresholdScreenFraction = 0.05f;
        private const float MaxDragPx = 100f;
        private const float MinScaleY = 0.85f;
        private const float SnapBackDuration = 0.35f;
        private const float TargetingTimeScale = 0.25f;

        private readonly WrathViewModel _wrathVm;
        private readonly Camera _camera;
        private readonly LayerMask _arenaLayer;
        private readonly Action<BattlePoint> _onCastWrath;
        private readonly WrathTargetingPresenter _targeting;

        private readonly VisualElement _card;
        private readonly VisualElement _fill;
        private readonly VisualElement _glow;
        private readonly Label _label;

        private readonly CompositeDisposable _disposables = new();

        private bool _isDragging;
        private int _activePointerId = -1;
        private float _dragStartScreenY;
        private float _currentTranslateY;
        private float _currentScaleY = 1f;
        private float _savedTimeScale = 1f;
        private Tween _snapBackTween;

        public WrathCardPresenter(
            VisualElement hudRoot,
            WrathViewModel wrathVm,
            Camera camera,
            LayerMask arenaLayer,
            Action<BattlePoint> onCastWrath,
            WrathTargetingPresenter targeting)
        {
            _wrathVm = wrathVm;
            _camera = camera;
            _arenaLayer = arenaLayer;
            _onCastWrath = onCastWrath;
            _targeting = targeting;

            _card = hudRoot.Q<VisualElement>("WrathCard");
            _fill = hudRoot.Q<VisualElement>("WrathCardFill");
            _glow = hudRoot.Q<VisualElement>("WrathCardGlow");
            _label = hudRoot.Q<Label>("WrathCardLabel");

            if (_card == null) return;

            _card.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _card.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _card.RegisterCallback<PointerUpEvent>(OnPointerUp);

            _wrathVm.ChargeNormalized.Subscribe(OnChargeChanged).AddTo(_disposables);
            _wrathVm.CanCast.Subscribe(OnCanCastChanged).AddTo(_disposables);
        }

        private void OnChargeChanged(float charge)
        {
            if (_fill != null)
            {
                _fill.style.height = new StyleLength(new Length(charge * 100f, LengthUnit.Percent));
            }

            if (_label != null)
            {
                _label.text = $"{(int)(charge * 100)}%";
            }
        }

        private void OnCanCastChanged(bool canCast)
        {
            if (_glow != null)
            {
                _glow.style.opacity = canCast ? 1f : 0f;
            }

            if (_card != null)
            {
                _card.EnableInClassList("wrath-card--disabled", !canCast);
            }
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!_wrathVm.CanCast.CurrentValue || _isDragging) return;

            _snapBackTween?.Kill();
            _snapBackTween = null;

            _isDragging = true;
            _activePointerId = evt.pointerId;
            _dragStartScreenY = evt.position.y;
            _card.CapturePointer(_activePointerId);
            _wrathVm.SetDragging(true);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_isDragging || evt.pointerId != _activePointerId) return;

            var dragDelta = _dragStartScreenY - evt.position.y;
            var screenHeight = Screen.height;

            var dragProgress = WrathCardDragMath.ComputeDragProgress(
                dragDelta, screenHeight, ThresholdScreenFraction);
            var translateY = WrathCardDragMath.ComputeTranslateY(dragProgress, MaxDragPx);
            var scaleY = WrathCardDragMath.ComputeScaleY(dragProgress, MinScaleY);

            _currentTranslateY = translateY;
            _currentScaleY = scaleY;
            _card.style.translate = new StyleTranslate(new Translate(0, translateY));
            _card.style.scale = new StyleScale(new Scale(new Vector3(1f, scaleY, 1f)));

            if (dragProgress >= 1f)
            {
                if (!_wrathVm.IsTargeting.CurrentValue)
                {
                    _savedTimeScale = Time.timeScale;
                    Time.timeScale = TargetingTimeScale;
                }

                _wrathVm.SetTargeting(true);
                if (_targeting != null && TryGetArenaPoint(evt.position, out var point))
                {
                    _targeting.Show(point);
                }
            }
            else
            {
                if (_wrathVm.IsTargeting.CurrentValue)
                {
                    Time.timeScale = _savedTimeScale;
                }

                _wrathVm.SetTargeting(false);
                _targeting?.Hide();
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_isDragging || evt.pointerId != _activePointerId) return;

            if (_wrathVm.IsTargeting.CurrentValue)
            {
                if (TryGetArenaPoint(evt.position, out var point))
                {
                    _onCastWrath?.Invoke(point);
                }

                PlayCastAnimation();
            }
            else
            {
                PlaySnapBack();
            }

            EndDrag();
        }

        private bool TryGetArenaPoint(Vector3 panelPos, out BattlePoint point)
        {
            point = default;
            var cam = _camera != null ? _camera : Camera.main;
            if (cam == null) return false;

            // Panel coords are in UI Toolkit points; screen coords are in pixels.
            // Scale by the panel's pixel ratio to bridge the gap on high-DPI displays.
            var scale = _card.panel?.scaledPixelsPerPoint ?? 1f;
            var screenPos = new Vector2(panelPos.x * scale, Screen.height - panelPos.y * scale);
            var ray = cam.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out var hit, 200f, _arenaLayer))
            {
                point = new BattlePoint(hit.point.x, hit.point.z);
                return true;
            }

            if (ray.direction.y != 0f)
            {
                var t = -ray.origin.y / ray.direction.y;
                if (t > 0f)
                {
                    var p = ray.origin + ray.direction * t;
                    point = new BattlePoint(p.x, p.z);
                    return true;
                }
            }

            return false;
        }

        private void PlaySnapBack()
        {
            _snapBackTween?.Kill();

            var startTranslateY = _currentTranslateY;
            var startScaleY = _currentScaleY;

            var progress = 0f;
            _snapBackTween = DOTween.To(
                    () => progress,
                    x =>
                    {
                        progress = x;
                        var ty = Mathf.Lerp(startTranslateY, 0f, x);
                        var sy = Mathf.Lerp(startScaleY, 1f, x);
                        _currentTranslateY = ty;
                        _currentScaleY = sy;
                        _card.style.translate = new StyleTranslate(new Translate(0, ty));
                        _card.style.scale = new StyleScale(new Scale(new Vector3(1f, sy, 1f)));
                    },
                    1f,
                    SnapBackDuration)
                .SetEase(Ease.OutBack);
        }

        private void PlayCastAnimation()
        {
            _snapBackTween?.Kill();

            var startTranslateY = _currentTranslateY;

            var seq = DOTween.Sequence();

            var pulseProgress = 0f;
            seq.Append(DOTween.To(
                    () => pulseProgress,
                    x =>
                    {
                        pulseProgress = x;
                        var s = Mathf.Lerp(1f, 1.1f, x);
                        _currentScaleY = s;
                        _card.style.scale = new StyleScale(new Scale(new Vector3(s, s, 1f)));
                    },
                    1f,
                    0.1f)
                .SetEase(Ease.OutQuad));

            var returnProgress = 0f;
            seq.Append(DOTween.To(
                    () => returnProgress,
                    x =>
                    {
                        returnProgress = x;
                        var ty = Mathf.Lerp(startTranslateY, 0f, x);
                        var s = Mathf.Lerp(1.1f, 1f, x);
                        _currentTranslateY = ty;
                        _currentScaleY = s;
                        _card.style.translate = new StyleTranslate(new Translate(0, ty));
                        _card.style.scale = new StyleScale(new Scale(new Vector3(s, s, 1f)));
                    },
                    1f,
                    0.25f)
                .SetEase(Ease.InBack));

            _snapBackTween = seq;
        }

        private void EndDrag()
        {
            if (_wrathVm.IsTargeting.CurrentValue)
            {
                Time.timeScale = _savedTimeScale;
            }

            if (_card.HasPointerCapture(_activePointerId))
            {
                _card.ReleasePointer(_activePointerId);
            }

            _isDragging = false;
            _activePointerId = -1;
            _wrathVm.SetDragging(false);
            _wrathVm.SetTargeting(false);
            _targeting?.Hide();
        }

        public void Dispose()
        {
            if (_wrathVm.IsTargeting.CurrentValue)
            {
                Time.timeScale = _savedTimeScale;
            }

            _snapBackTween?.Kill();
            _disposables.Dispose();

            if (_card != null)
            {
                _card.UnregisterCallback<PointerDownEvent>(OnPointerDown);
                _card.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
                _card.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            }
        }
    }

    public static class WrathCardDragMath
    {
        public static float ComputeDragProgress(float dragDelta, float screenHeight, float thresholdFraction)
        {
            if (screenHeight <= 0f || thresholdFraction <= 0f) return 0f;
            return Mathf.Clamp01(dragDelta / (screenHeight * thresholdFraction));
        }

        public static float ComputeTranslateY(float dragProgress, float maxDragPx)
        {
            return -Mathf.Lerp(0f, maxDragPx, dragProgress);
        }

        public static float ComputeScaleY(float dragProgress, float minScaleY = 0.85f)
        {
            return 1f - (1f - minScaleY) * Mathf.Sin(dragProgress * Mathf.PI);
        }
    }
}
