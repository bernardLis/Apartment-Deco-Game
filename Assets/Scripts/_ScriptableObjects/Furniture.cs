using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "ScriptableObjects/Furniture")]
public class Furniture : BaseScriptableObject
{
    public Color Color;
    public Shape Shape;
    [Tooltip("For Circle Just Width Counts")]
    public float Width;
    [FormerlySerializedAs("Length")]
    public float Height;
    [Header("For L Shape")]
    public float LegWidth;
    [FormerlySerializedAs("LegLength")]
    public float LegHeight;

    public Vector2 Position;
    public Vector2 Rotation;

    public void SetName(string newName)
    {
        Name = newName;
    }

    public void SetColor(Color newColor)
    {
        Color = newColor;
    }

    public void SetWidth(float newWidth)
    {
        Width = newWidth;
    }

    public void SetHeight(float newHeight)
    {
        Height = newHeight;
    }

    public void SetLegWidth(float newWidth)
    {
        LegWidth = newWidth;
    }

    public void SetLegHeight(float newHeight)
    {
        LegHeight = newHeight;
    }

    public void SetPosition(Vector2 newPosition)
    {
        Position = newPosition;
    }

    public void SetRotation(Vector2 newRotation)
    {
        Rotation = newRotation;
    }
}

public enum Shape
{
    Rectangle = 0,
    Circle = 1,
    L = 2
}