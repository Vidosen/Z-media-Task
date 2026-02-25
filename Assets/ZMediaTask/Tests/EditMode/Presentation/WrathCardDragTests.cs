using NUnit.Framework;
using ZMediaTask.Presentation.Presenters;
using ZMediaTask.Presentation.ViewModels;

namespace ZMediaTask.Tests.EditMode.Presentation
{
    public class WrathCardDragTests
    {
        #region ComputeDragProgress

        [Test]
        public void DragProgress_ZeroDelta_ReturnsZero()
        {
            var result = WrathCardDragMath.ComputeDragProgress(0f, 1280f, 0.35f);
            Assert.AreEqual(0f, result, 0.001f);
        }

        [Test]
        public void DragProgress_AtThreshold_ReturnsOne()
        {
            var screenHeight = 1280f;
            var threshold = 0.35f;
            var dragDelta = screenHeight * threshold;

            var result = WrathCardDragMath.ComputeDragProgress(dragDelta, screenHeight, threshold);
            Assert.AreEqual(1f, result, 0.001f);
        }

        [Test]
        public void DragProgress_BeyondThreshold_ClampedToOne()
        {
            var screenHeight = 1280f;
            var threshold = 0.35f;
            var dragDelta = screenHeight * threshold * 2f;

            var result = WrathCardDragMath.ComputeDragProgress(dragDelta, screenHeight, threshold);
            Assert.AreEqual(1f, result, 0.001f);
        }

        [Test]
        public void DragProgress_NegativeDelta_ClampedToZero()
        {
            var result = WrathCardDragMath.ComputeDragProgress(-100f, 1280f, 0.35f);
            Assert.AreEqual(0f, result, 0.001f);
        }

        [Test]
        public void DragProgress_ZeroScreenHeight_ReturnsZero()
        {
            var result = WrathCardDragMath.ComputeDragProgress(100f, 0f, 0.35f);
            Assert.AreEqual(0f, result, 0.001f);
        }

        [Test]
        public void DragProgress_ZeroThreshold_ReturnsZero()
        {
            var result = WrathCardDragMath.ComputeDragProgress(100f, 1280f, 0f);
            Assert.AreEqual(0f, result, 0.001f);
        }

        [Test]
        public void DragProgress_HalfThreshold_ReturnsHalf()
        {
            var screenHeight = 1280f;
            var threshold = 0.35f;
            var dragDelta = screenHeight * threshold * 0.5f;

            var result = WrathCardDragMath.ComputeDragProgress(dragDelta, screenHeight, threshold);
            Assert.AreEqual(0.5f, result, 0.001f);
        }

        #endregion

        #region ComputeTranslateY

        [Test]
        public void TranslateY_ZeroProgress_ReturnsZero()
        {
            var result = WrathCardDragMath.ComputeTranslateY(0f, 200f);
            Assert.AreEqual(0f, result, 0.001f);
        }

        [Test]
        public void TranslateY_FullProgress_ReturnsNegativeMaxDrag()
        {
            var result = WrathCardDragMath.ComputeTranslateY(1f, 200f);
            Assert.AreEqual(-200f, result, 0.001f);
        }

        [Test]
        public void TranslateY_HalfProgress_ReturnsHalfMaxDrag()
        {
            var result = WrathCardDragMath.ComputeTranslateY(0.5f, 200f);
            Assert.AreEqual(-100f, result, 0.001f);
        }

        #endregion

        #region ComputeScaleY

        [Test]
        public void ScaleY_ZeroProgress_ReturnsOne()
        {
            var result = WrathCardDragMath.ComputeScaleY(0f, 0.85f);
            Assert.AreEqual(1f, result, 0.001f);
        }

        [Test]
        public void ScaleY_FullProgress_ReturnsOne()
        {
            var result = WrathCardDragMath.ComputeScaleY(1f, 0.85f);
            Assert.AreEqual(1f, result, 0.01f);
        }

        [Test]
        public void ScaleY_MidProgress_DipsToMinimum()
        {
            var result = WrathCardDragMath.ComputeScaleY(0.5f, 0.85f);
            Assert.AreEqual(0.85f, result, 0.001f);
        }

        [Test]
        public void ScaleY_QuarterProgress_BetweenOneAndMin()
        {
            var result = WrathCardDragMath.ComputeScaleY(0.25f, 0.85f);
            Assert.Greater(result, 0.85f);
            Assert.Less(result, 1f);
        }

        #endregion

        #region WrathViewModel.IsTargeting

        [Test]
        public void WrathViewModel_IsTargeting_DefaultsFalse()
        {
            var vm = new WrathViewModel();
            Assert.IsFalse(vm.IsTargeting.CurrentValue);
            vm.Dispose();
        }

        [Test]
        public void WrathViewModel_SetTargeting_True_UpdatesProperty()
        {
            var vm = new WrathViewModel();
            vm.SetTargeting(true);
            Assert.IsTrue(vm.IsTargeting.CurrentValue);
            vm.Dispose();
        }

        [Test]
        public void WrathViewModel_SetTargeting_BackToFalse_UpdatesProperty()
        {
            var vm = new WrathViewModel();
            vm.SetTargeting(true);
            vm.SetTargeting(false);
            Assert.IsFalse(vm.IsTargeting.CurrentValue);
            vm.Dispose();
        }

        [Test]
        public void WrathViewModel_IsTargeting_EmitsOnChange()
        {
            var vm = new WrathViewModel();

            Assert.IsFalse(vm.IsTargeting.CurrentValue);
            vm.SetTargeting(true);
            Assert.IsTrue(vm.IsTargeting.CurrentValue);
            vm.SetTargeting(false);
            Assert.IsFalse(vm.IsTargeting.CurrentValue);

            vm.Dispose();
        }

        #endregion
    }
}
