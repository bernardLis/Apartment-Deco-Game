using System;
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

    public Vector3 Position;
    public Vector3 Rotation;

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

    #region Serialization

    public FurnitureData SerializeSelf()
    {
        FurnitureData data = new FurnitureData
        {
            Id = Id,
            Position = Position,
            Rotation = Rotation,
            Name = Name,
            Color = Color,
            ShapeIndex = (int)Shape,
            Width = Width,
            Height = Height,
            LegWidth = LegWidth,
            LegHeight = LegHeight
        };

        return data;
    }

    public void LoadFromData(FurnitureData data)
    {
        Position = data.Position;
        Rotation = data.Rotation;
        Name = data.Name;
        Color = data.Color;
        Shape = (Shape)data.ShapeIndex;
        Width = data.Width;
        Height = data.Height;
        LegWidth = data.LegWidth;
        LegHeight = data.LegHeight;
    }

    #endregion
}

[Serializable]
public struct FurnitureData
{
    public string Id;
    public string Name;
    public Vector3 Position;
    public Vector3 Rotation;
    public Color Color;
    public int ShapeIndex;
    public float Width;
    public float Height;
    public float LegWidth;
    public float LegHeight;
}


public enum Shape
{
    Rectangle = 0,
    Circle = 1,
    L = 2
}