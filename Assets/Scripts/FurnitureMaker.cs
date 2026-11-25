using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace ApartmentDeco
{
    public class FurnitureMaker : MonoBehaviour
    {
        [SerializeField] AudioClip _minimizeSound;
        [SerializeField] AudioClip _errorSound;

        AudioSource _audioSource;

        PanelRenderer _panelRenderer;
        VisualElement _main;

        Button _minimizeButton;
        VisualElement _characteristicsWrapper;

        TextField _nameField;
        EnumField _shapeEnumField;
        Button _colorButton;
        VisualElement _sizeContainer;
        List<FloatField> _sizeFields = new();
        Label _errorLabel;
        Button _addFurnitureButton;

        Color _color = Color.white;

        void OnEnable()
        {
            _audioSource = GetComponent<AudioSource>();

            _panelRenderer = GetComponentInChildren<PanelRenderer>();
            _panelRenderer.RegisterUIReloadCallback(OnUIReload);
        }

        void OnDisable()
        {
            _panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        }

        void OnUIReload(PanelRenderer panelRenderer, VisualElement root, int version)
        {
            _main = root.Q<VisualElement>("main");

            _minimizeButton = _main.Q<Button>("minimizeButton");
            _minimizeButton.clickable.clicked -= ToggleMinimize;
            _minimizeButton.clickable.clicked += ToggleMinimize;
            _characteristicsWrapper = _main.Q<VisualElement>("characteristicsWrapper");

            _nameField = _main.Q<TextField>("nameField");
            _shapeEnumField = _main.Q<EnumField>("shapeField");
            _sizeContainer = _main.Q<VisualElement>("sizeContainer");
            _errorLabel = _main.Q<Label>("errorLabel");
            _addFurnitureButton = _main.Q<Button>("addFurnitureButton");
            _colorButton = _main.Q<Button>("colorButton");
            _colorButton.style.backgroundColor = _color;

            _shapeEnumField.RegisterValueChangedCallback(OnShapeChanged);
            ResolveSizeInputs();

            _addFurnitureButton.clickable.clicked -= AddFurniture;
            _addFurnitureButton.clickable.clicked += AddFurniture;

            _colorButton.clickable.clicked -= ToggleColorPicker;
            _colorButton.clickable.clicked += ToggleColorPicker;
        }


        bool _isMinimized = false;

        void ToggleMinimize()
        {
            _audioSource.pitch = _isMinimized ? 0.8f : 1.2f;
            _audioSource.PlayOneShot(_minimizeSound);
            _isMinimized = !_isMinimized;
            _main.style.height = _isMinimized ? 100 : 600;
            _characteristicsWrapper.style.display = _isMinimized ? DisplayStyle.None : DisplayStyle.Flex;
            _minimizeButton.text = _isMinimized ? "+" : "-";
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

            _colorPicker = new(_color);
            _main.Add(_colorPicker);
            _colorPicker.style.position = Position.Absolute;
            _colorPicker.style.left = 500;
            _colorPicker.OnColorChange += ColorChanged;
        }

        void ColorChanged(Color c)
        {
            _colorButton.style.backgroundColor = c;
            _color = c;
        }

        void OnShapeChanged(ChangeEvent<Enum> evt)
        {
            ResolveSizeInputs();
        }

        void ResolveSizeInputs()
        {
            _sizeContainer.Clear();
            _sizeFields = new();
            if ((Shape)_shapeEnumField.value == Shape.Rectangle)
            {
                FloatField widthField = new("Width");
                FloatField heightField = new("Height");
                _sizeContainer.Add(widthField);
                _sizeContainer.Add(heightField);
                _sizeFields.Add(widthField);
                _sizeFields.Add(heightField);
            }

            if ((Shape)_shapeEnumField.value == Shape.Circle)
            {
                FloatField radiusField = new("Diameter (= Radius x 2)");
                _sizeContainer.Add(radiusField);
                _sizeFields.Add(radiusField);
            }

            if ((Shape)_shapeEnumField.value == Shape.L)
            {
                FloatField widthField = new("Width");
                FloatField heightField = new("Height");
                _sizeContainer.Add(widthField);
                _sizeContainer.Add(heightField);
                _sizeFields.Add(widthField);
                _sizeFields.Add(heightField);
                FloatField legWidthField = new("Leg Width");
                FloatField legHeightField = new("Leg Height");
                _sizeContainer.Add(legWidthField);
                _sizeContainer.Add(legHeightField);
                _sizeFields.Add(legWidthField);
                _sizeFields.Add(legHeightField);
            }
        }

        void AddFurniture()
        {
            if (!AreInputsValid()) return;
            // create furniture Scriptable Object and pass it to GameManager
            Furniture furniture = ScriptableObject.CreateInstance<Furniture>();

            furniture.Name = _nameField.value;
            furniture.Color = _color;
            furniture.Shape = (Shape)_shapeEnumField.value;

            if (furniture.Shape == Shape.Rectangle)
            {
                furniture.Width = _sizeFields[0].value * 0.01f;
                furniture.Height = _sizeFields[1].value * 0.01f;
            }

            if (furniture.Shape == Shape.Circle)
            {
                furniture.Width = _sizeFields[0].value * 0.01f;
            }

            if (furniture.Shape == Shape.L)
            {
                furniture.Width = _sizeFields[0].value * 0.01f;
                furniture.Height = _sizeFields[1].value * 0.01f;
                furniture.LegWidth = _sizeFields[2].value * 0.01f;
                furniture.LegHeight = _sizeFields[3].value * 0.01f;
            }

            GameManager.Instance.CreateFurniture(furniture);
        }

        IEnumerator _clearLabelCoroutine;

        bool AreInputsValid()
        {
            bool isError = false;
            if (string.IsNullOrEmpty(_nameField.value))
            {
                _errorLabel.text = "Name cannot be empty";
                isError = true;
            }

            Shape s = (Shape)_shapeEnumField.value;
            if (s == Shape.Rectangle && (_sizeFields[0].value <= 0 || _sizeFields[1].value <= 0))
            {
                _errorLabel.text = "Width and Height must be positive";
                isError = true;
            }

            if (s == Shape.Circle && _sizeFields[0].value <= 0)
            {
                _errorLabel.text = "Diameter must be positive";
                isError = true;
            }

            if (s == Shape.L && (_sizeFields[0].value <= 0 || _sizeFields[1].value <= 0 || _sizeFields[2].value <= 0 ||
                                 _sizeFields[3].value <= 0))
            {
                _errorLabel.text = "Width and Height must be positive";
                isError = true;
            }

            if (!isError) return true;

            _audioSource.pitch = Random.Range(0.8f, 1.2f);
            _audioSource.PlayOneShot(_errorSound);
            if (_clearLabelCoroutine != null) StopCoroutine(_clearLabelCoroutine);
            _clearLabelCoroutine = ClearErrorLabelCoroutine();
            StartCoroutine(_clearLabelCoroutine);
            return false;
        }

        IEnumerator ClearErrorLabelCoroutine()
        {
            yield return new WaitForSeconds(2);
            _errorLabel.text = "";
        }
    }
}