using UnityEngine;
using UnityEngine.UIElements;

namespace ApartmentDeco
{
    public class FurnitureCardElement : VisualElement
    {
        FurnitureController _furnitureController;
        Button _colorButton;

        public FurnitureCardElement(FurnitureController controller)
        {
            _furnitureController = controller;

            AddNameField();
            AddSizeContainer();
            AddColorButton();
            AddRotateButtons();

            Button destroyButton = new();
            destroyButton.AddToClassList("common__destroy-button");
            destroyButton.AddToClassList("common__menu-button");
            Add(destroyButton);
            destroyButton.clickable.clicked += _furnitureController.DestroySelf;
        }

        void AddNameField()
        {
            TextField nameField = new("Name");
            nameField.value = _furnitureController.Furniture.Name;
            Add(nameField);
            nameField.RegisterValueChangedCallback(e => _furnitureController.SetName(e.newValue));
        }

        void AddSizeContainer()
        {
            FloatField widthField = new("Diameter");
            widthField.value = _furnitureController.Furniture.Width;
            Add(widthField);
            widthField.RegisterValueChangedCallback(e => _furnitureController.SetWidth(e.newValue));

            if (_furnitureController.Furniture.Shape == Shape.Circle) return;
            widthField.label = "Width";
            FloatField heightField = new("Height");
            heightField.value = _furnitureController.Furniture.Height;
            Add(heightField);
            heightField.RegisterValueChangedCallback(e => _furnitureController.SetHeight(e.newValue));

            if (_furnitureController.Furniture.Shape == Shape.Rectangle) return;

            FloatField legWidthField = new("Leg Width");
            legWidthField.value = _furnitureController.Furniture.LegWidth;
            Add(legWidthField);
            legWidthField.RegisterValueChangedCallback(e => _furnitureController.SetLegWidth(e.newValue));

            FloatField legHeightField = new("Leg Height");
            legHeightField.value = _furnitureController.Furniture.LegHeight;
            Add(legHeightField);
            legHeightField.RegisterValueChangedCallback(e => _furnitureController.SetLegHeight(e.newValue));
        }

        void AddColorButton()
        {
            VisualElement container = new();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.justifyContent = Justify.SpaceBetween;
            Add(container);

            Label colorLabel = new("Color");
            container.Add(colorLabel);

            _colorButton = new();
            _colorButton.style.backgroundColor = _furnitureController.Furniture.Color;
            _colorButton.style.width = 306;
            container.Add(_colorButton);
            _colorButton.text = "";
            _colorButton.clickable.clicked -= ToggleColorPicker;
            _colorButton.clickable.clicked += ToggleColorPicker;
        }

        ColorPickerElement _colorPicker;

        void ToggleColorPicker()
        {
            if (_colorPicker != null)
            {
                _colorPicker.OnColorChange -= ColorChanged;
                _colorPicker.RemoveFromHierarchy();
                _colorPicker = null;
                return;
            }

            _colorPicker = new(_furnitureController.Furniture.Color);
            Add(_colorPicker);
            _colorPicker.style.position = Position.Absolute;
            _colorPicker.style.left = resolvedStyle.width + 15;
            _colorPicker.style.top = -160;
            _colorPicker.OnColorChange += ColorChanged;
        }

        void ColorChanged(Color c)
        {
            _colorButton.style.backgroundColor = c;
            _furnitureController.SetColor(c);
        }

        void AddRotateButtons()
        {
            VisualElement container = new();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.justifyContent = Justify.SpaceAround;
            Add(container);

            Label rotateLabel = new("Rotate");
            container.Add(rotateLabel);

            Button rotateButton = new();
            rotateButton.AddToClassList("common__rotate-clockwise-button");
            container.Add(rotateButton);
            rotateButton.clickable.clicked += () => { _furnitureController.Rotate(30); };

            Button rotateAntiClockwiseButton = new();
            rotateAntiClockwiseButton.AddToClassList("common__rotate-counter-clockwise-button");
            container.Add(rotateAntiClockwiseButton);
            rotateAntiClockwiseButton.clickable.clicked += () => { _furnitureController.Rotate(-15); };
        }
    }
}