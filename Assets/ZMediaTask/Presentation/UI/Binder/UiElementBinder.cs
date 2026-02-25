using System;
using R3;
using UnityEngine.UIElements;

namespace ZMediaTask.Presentation.UI.Binder
{
    public static class UiElementBinder
    {
        public static T Require<T>(VisualElement root, string name) where T : VisualElement
        {
            var element = root.Q<T>(name);
            if (element == null)
            {
                throw new InvalidOperationException(
                    $"Required UI element '{name}' of type {typeof(T).Name} not found.");
            }

            return element;
        }

        public static IDisposable BindText(Label label, ReadOnlyReactiveProperty<string> property)
        {
            label.text = property.CurrentValue;
            return property.Subscribe(value => label.text = value);
        }

        public static IDisposable BindVisible(VisualElement element, ReadOnlyReactiveProperty<bool> property)
        {
            SetVisible(element, property.CurrentValue);
            return property.Subscribe(value => SetVisible(element, value));
        }

        public static IDisposable BindEnabled(Button button, ReadOnlyReactiveProperty<bool> property)
        {
            button.SetEnabled(property.CurrentValue);
            return property.Subscribe(value => button.SetEnabled(value));
        }

        public static IDisposable BindProgressBar(ProgressBar bar, ReadOnlyReactiveProperty<float> property)
        {
            bar.value = property.CurrentValue;
            return property.Subscribe(value => bar.value = value);
        }

        private static void SetVisible(VisualElement element, bool visible)
        {
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
