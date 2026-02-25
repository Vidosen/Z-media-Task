using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace ZMediaTask.Presentation.Services
{
    public sealed class DamageFlashTracker
    {
        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");
        private const float FlashDuration = 0.15f;

        private readonly Dictionary<int, Tween> _tweens = new();

        public void Flash(int unitId, Renderer renderer)
        {
            if (renderer == null)
            {
                return;
            }

            if (_tweens.TryGetValue(unitId, out var existing))
            {
                existing?.Kill();
            }

            var mat = renderer.material;
            mat.SetFloat(FlashAmountId, 1f);

            var tween = DOTween.To(
                () => mat.GetFloat(FlashAmountId),
                v => mat.SetFloat(FlashAmountId, v),
                0f,
                FlashDuration);
            tween.SetEase(Ease.OutQuad);
            _tweens[unitId] = tween;
        }

        public void Clear()
        {
            foreach (var tween in _tweens.Values)
            {
                tween?.Kill();
            }

            _tweens.Clear();
        }
    }
}
