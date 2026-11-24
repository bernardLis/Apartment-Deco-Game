using System;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

[UxmlElement]
public partial class UxmlColorPicker : VisualElement
{
    private const string ColorDisplay = "colorDisplay";

    // runtime render constants:
    private const float PositionMarkerOutlineThickness = 2f;
    private const float ImageMarkerRadiusFactor = 0.05f;
    private const float MarkerLineWidth = 3f;
    private const float ColorWheelThickness = 0.1f;
    private const float ColorWheelCircleRadiusFactor = 0.425f;
    private const float ImageRectSizePercent = 50f;

    #region Properties

    private float _hue;

    [UxmlAttribute, Range(0f, 0.999f)]
    public float Hue
    {
        get => _hue;
        set
        {
            _hue = Math.Clamp(value, 0f, 1f);
            UpdateGradientTexture();
            OnColorUpdate();
            _colorWheel.MarkDirtyRepaint();
        }
    }

    private float _saturation = 1;

    [UxmlAttribute, Range(0f, 0.999f)]
    public float Saturation
    {
        get => _saturation;
        set
        {
            _saturation = Math.Clamp(value, 0f, 1f);
            _innerImage.MarkDirtyRepaint();
            OnColorUpdate();
        }
    }

    private float _brightness = 1;

    [UxmlAttribute, Range(0f, 0.999f)]
    public float Brightness
    {
        get => _brightness;
        set
        {
            _brightness = Math.Clamp(value, 0f, 1f);
            _innerImage.MarkDirtyRepaint();
            OnColorUpdate();
        }
    }

    public Color Color => Color.HSVToRGB(Hue, Saturation, Brightness);

    private Gradient CircleGradient { get; } = new()
    {
        mode = GradientMode.Blend,
        colorKeys = new GradientColorKey[]
        {
            new(Color.HSVToRGB(0, 1, 1), 0),
            new(Color.HSVToRGB(1 * 1f / 6f, 1, 1), 1 * 1f / 6f),
            new(Color.HSVToRGB(2 * 1f / 6f, 1, 1), 2 * 1f / 6f),
            new(Color.HSVToRGB(3 * 1f / 6f, 1, 1), 3 * 1f / 6f),
            new(Color.HSVToRGB(4 * 1f / 6f, 1, 1), 4 * 1f / 6f),
            new(Color.HSVToRGB(5 * 1f / 6f, 1, 1), 5 * 1f / 6f),
            new(Color.HSVToRGB(1, 1, 1), 1)
        }
    };

    /// <summary>
    /// Direction as a computed property. It converts the Hue value into a direction vector for the wheel marker
    /// and the other way around.
    /// </summary>
    private Vector2 Direction
    {
        get
        {
            var radians = (float)(Hue * 2 * Math.PI);
            return new Vector2(Mathf.Cos(radians), -Mathf.Sin(radians));
        }
        set
        {
            var degrees = Vector2.SignedAngle(Vector2.right, new Vector2(value.x, -value.y));
            Hue = (degrees + 360) % 360 / 360f;
        }
    }

    private Vector2 Position
    {
        get
        {
            var width = _innerImage.resolvedStyle.width;
            var height = _innerImage.resolvedStyle.height;
            var minSide = Mathf.Min(width, height);
            var widthReduction = (width - minSide) / 2;
            var heightReduction = (height - minSide) / 2;

            var posX = Saturation * minSide + widthReduction;
            var posY = (1 - Brightness) * minSide + heightReduction;

            return new Vector2(posX, posY);
        }
        set
        {
            var width = _innerImage.resolvedStyle.width;
            var height = _innerImage.resolvedStyle.height;
            var minSide = Mathf.Min(width, height);
            var widthReduction = (width - minSide) / 2;
            var heightReduction = (height - minSide) / 2;

            Saturation = (value.x - widthReduction) / (width - 2 * widthReduction);
            Brightness = 1 - (value.y - heightReduction) / (height - 2 * heightReduction);
        }
    }

    #endregion

    private readonly Texture2D _gradientTexture;
    private readonly VisualElement _colorWheel;

    private readonly Image _innerImage = new Image
    {
        style =
        {
            width = Length.Percent(ImageRectSizePercent),
            height = Length.Percent(ImageRectSizePercent),
            marginTop = Length.Auto(),
            marginRight = Length.Auto(),
            marginBottom = Length.Auto(),
            marginLeft = Length.Auto(),
        }
    };

    public event Action<Color> OnColorChange;

    public UxmlColorPicker()
    {
        style.flexGrow = 1;

        var colorDisplay = new VisualElement()
        {
            name = ColorDisplay
        };
        this.Add(colorDisplay);

        // Adding color wheel
        _colorWheel = new VisualElement
        {
            style =
            {
                flexGrow = 1
            }
        };
        this.Add(_colorWheel);

        _colorWheel.generateVisualContent += OnGenerateVisualContent;
        SetWheelMouseCallbackEvents();

        // Creating gradient texture amd adding inner image
        _gradientTexture = new Texture2D(16, 16, TextureFormat.RGB24, false)
        {
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave,
            wrapMode = TextureWrapMode.Clamp
        };
        _innerImage.image = _gradientTexture;
        UpdateGradientTexture();
        _colorWheel.Add(_innerImage);

        _innerImage.generateVisualContent += ImageGenerateVisualContent;
        SetImageMouseCallbackEvents();

        OnColorUpdate();
    }

    ~UxmlColorPicker()
    {
        Object.Destroy(_gradientTexture);
    }

    private void OnColorUpdate()
    {
        OnColorChange?.Invoke(Color);
    }

    public void SetCurrentColor(Color color)
    {
        Color.RGBToHSV(color, out var hue, out var saturation, out var brightness);
        Hue = hue;
        Saturation = saturation;
        Brightness = brightness;
    }

    #region Image

    private void SetImageMouseCallbackEvents()
    {
        _innerImage.RegisterCallback<PointerDownEvent>(evt =>
        {
            if (!IsPointerOnImage(_innerImage.WorldToLocal(evt.position)))
                return;

            _innerImage.CaptureMouse();
            SetImagePosition(_innerImage.WorldToLocal(evt.position));
        });

        _innerImage.RegisterCallback<PointerMoveEvent>(evt =>
        {
            if (_innerImage.HasMouseCapture())
            {
                SetImagePosition(_innerImage.WorldToLocal(evt.position));
            }
        });

        _innerImage.RegisterCallback<PointerUpEvent>(evt => { _innerImage.ReleaseMouse(); });
    }

    private bool IsPointerOnImage(Vector2 pointer)
    {
        var width = _innerImage.resolvedStyle.width;
        var height = _innerImage.resolvedStyle.height;
        var minSide = Mathf.Min(width, height);
        var widthReduction = (width - minSide) / 2;
        var heightReduction = (height - minSide) / 2;

        return (pointer.x >= widthReduction && pointer.x <= width - widthReduction &&
                pointer.y >= heightReduction && pointer.y <= height - heightReduction);
    }

    private Vector2 ImageClampPosition(Vector2 position)
    {
        var width = _innerImage.resolvedStyle.width;
        var height = _innerImage.resolvedStyle.height;
        var minSide = Mathf.Min(width, height);
        var widthReduction = (width - minSide) / 2;
        var heightReduction = (height - minSide) / 2;

        var clampedWidth = Math.Clamp(position.x, widthReduction, width - widthReduction);
        var clampedHeight = Math.Clamp(position.y, heightReduction, height - heightReduction);
        return new Vector2(clampedWidth, clampedHeight);
    }

    private void SetImagePosition(Vector2 eventPos)
    {
        var position = ImageClampPosition(eventPos);
        this.Position = position;
        _innerImage.MarkDirtyRepaint();
    }

    private void ImageGenerateVisualContent(MeshGenerationContext context)
    {
        var rect = _innerImage.contentRect;
        var swh = Mathf.Min(rect.width, rect.height);
        var paint = context.painter2D;

        // Image position marker
        paint.BeginPath();
        {
            paint.Arc(Position, swh * ImageMarkerRadiusFactor - PositionMarkerOutlineThickness, 0, 360);

            paint.lineWidth = MarkerLineWidth + PositionMarkerOutlineThickness;
            paint.strokeColor = Color.black;
            paint.Stroke();

            paint.lineWidth = MarkerLineWidth;
            paint.strokeColor = Color.white;
            paint.Stroke();
        }
        paint.ClosePath();
    }

    #endregion

    #region Color Wheel

    private void SetWheelMouseCallbackEvents()
    {
        _colorWheel.RegisterCallback<PointerDownEvent>(evt =>
        {
            if (!IsPointerOnWheel(_colorWheel.WorldToLocal(evt.position)))
                return;

            _colorWheel.CaptureMouse();
            SetWheelHue(_colorWheel.WorldToLocal(evt.position));
        });

        _colorWheel.RegisterCallback<PointerMoveEvent>(evt =>
        {
            if (_colorWheel.HasMouseCapture())
            {
                SetWheelHue(_colorWheel.WorldToLocal(evt.position));
            }
        });

        _colorWheel.RegisterCallback<PointerUpEvent>(evt => { _colorWheel.ReleaseMouse(); });
    }

    private bool IsPointerOnWheel(Vector2 pointer)
    {
        var direction = pointer - _colorWheel.contentRect.center;

        var rect = _colorWheel.contentRect;
        var swh = Mathf.Min(rect.width, rect.height);
        var circleRadius = swh * ColorWheelCircleRadiusFactor;
        var gradientOffset = swh * ColorWheelThickness / 2;
        var length = direction.magnitude;

        return length < circleRadius + gradientOffset && length > circleRadius - gradientOffset;
    }

    private void SetWheelHue(Vector2 eventPosition)
    {
        var direction = eventPosition - _colorWheel.contentRect.center;

        Direction = direction;
    }

    private void OnGenerateVisualContent(MeshGenerationContext context)
    {
        var rect = _colorWheel.contentRect;
        var swh = Mathf.Min(rect.width, rect.height);
        var paint = context.painter2D;
        var circleRadius = swh * ColorWheelCircleRadiusFactor;
        var gradientWidth = swh * ColorWheelThickness;

        // Color ring
        paint.BeginPath();
        {
            paint.Arc(rect.center, circleRadius, 270f, -90f, ArcDirection.CounterClockwise);

            paint.lineWidth = gradientWidth;
            paint.strokeGradient = CircleGradient;
            paint.Stroke();
        }
        paint.ClosePath();

        // Hue position marker
        paint.BeginPath();
        {
            paint.Arc(
                rect.center + Direction.normalized * circleRadius,
                gradientWidth / 2 - PositionMarkerOutlineThickness, 0, 360
            );

            paint.lineWidth = MarkerLineWidth + PositionMarkerOutlineThickness;
            paint.strokeColor = Color.black;
            paint.Stroke();

            paint.lineWidth = MarkerLineWidth;
            paint.strokeColor = Color.white;
            paint.Stroke();
        }
        paint.ClosePath();
    }

    private void UpdateGradientTexture()
    {
        if (!_gradientTexture) return;
        var pixels = new Color[_gradientTexture.width * _gradientTexture.height];

        for (var x = 0; x < _gradientTexture.width; x++)
        {
            for (var y = 0; y < _gradientTexture.height; y++)
            {
                pixels[x * _gradientTexture.width + y] = Color.HSVToRGB(
                    Hue,
                    (float)y / _gradientTexture.height,
                    (float)x / _gradientTexture.width
                );
            }
        }

        _gradientTexture.SetPixels(pixels);
        _gradientTexture.Apply();
    }

    #endregion
}