using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace ApartmentDeco
{
    public class ColorPickerElement : VisualElement
    {
        public Color Color;
        public event Action<Color> OnColorChange;

        public ColorPickerElement(Color color)
        {
            Color = color;

            style.width = 400;
            style.height = 500;
            style.backgroundColor = new Color(0.498f, 0.4392f, 0.5411765f);
            style.paddingBottom = 12;
            style.paddingLeft = 12;
            style.paddingRight = 12;
            style.paddingTop = 12;

            UxmlColorPicker colorPicker = new();
            colorPicker.SetCurrentColor(Color);
            Add(colorPicker);
            colorPicker.OnColorChange += SetColor;

            Button closeButton = new(() => { RemoveFromHierarchy(); });
            closeButton.text = "Close";
            Add(closeButton);
        }

        void SetColor(Color color)
        {
            Color = color;
            OnColorChange?.Invoke(color);
        }
    }
}