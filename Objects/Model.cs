using Rendering.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Rendering.Objects;

public class Model
{
    public readonly List<Position> Positions;
    public readonly List<Normal> Normals;
    public readonly List<Polygon> Polygons;

    private float _xAxisRotate;
    private float _yAxisRotate;
    private float _zAxisRotate;
    private float _xPosition;
    private float _yPosition;
    private float _zPosition;
    private float _scale = 1.0f;

    public float ScaleStep = 0.1f;
    public float MoveStep = 1f;
    public const float MOUSE_ROTATION_DELTA = MathF.PI / 36;

    private Matrix4x4 _scaleMatrix;
    private Matrix4x4 _rotationX;
    private Matrix4x4 _rotationY;
    private Matrix4x4 _rotationZ;
    private Matrix4x4 _move;
    public Matrix4x4 Transformation { get; private set; }

    public float XAxisRotate
    {
        get => _xAxisRotate;
        set
        {
            _xAxisRotate = value;
            _rotationX = Matrix4x4.CreateRotationX(_xAxisRotate);
            UpdateTransformation();
        }
    }

    public float YAxisRotate
    {
        get => _yAxisRotate;
        set
        {
            _yAxisRotate = value;
            _rotationY = Matrix4x4.CreateRotationY(_yAxisRotate);
            UpdateTransformation();
        }
    }

    public float ZAxisRotate
    {
        get => _zAxisRotate;
        set
        {
            _zAxisRotate = value;
            _rotationZ = Matrix4x4.CreateRotationZ(_zAxisRotate);
            UpdateTransformation();
        }
    }

    public float XPosition
    {
        get => _xPosition;
        set
        {
            _xPosition = value;
            _move = Matrix4x4.CreateTranslation(_xPosition, _yPosition, _zPosition);
            UpdateTransformation();
        }
    }

    public float YPosition
    {
        get => _yPosition;
        set
        {
            _yPosition = value;
            _move = Matrix4x4.CreateTranslation(_xPosition, _yPosition, _zPosition);
            UpdateTransformation();
        }
    }

    public float ZPosition
    {
        get => _zPosition;
        set
        {
            _zPosition = value;
            _move = Matrix4x4.CreateTranslation(_xPosition, _yPosition, _zPosition);
            UpdateTransformation();
        }
    }

    public float Scale
    {
        get => _scale;
        set
        {
            if (value > 0) _scale = value;
            else _scale = 0;

            _scaleMatrix = Matrix4x4.CreateScale(_scale);
            UpdateTransformation();
        }
    }

    public Model(List<Position> positions, List<Normal> normals, List<Polygon> polygons)
    {
        Positions = positions;
        Normals = normals;
        Polygons = polygons;
        _scaleMatrix = Matrix4x4.CreateScale(_scale);
        _rotationX = Matrix4x4.CreateRotationX(_xAxisRotate);
        _rotationY = Matrix4x4.CreateRotationY(_yAxisRotate);
        _rotationZ = Matrix4x4.CreateRotationZ(_zAxisRotate);
        _move = Matrix4x4.CreateTranslation(_xPosition, _yPosition, _zPosition);
        UpdateTransformation();
    }

    private void UpdateTransformation()
    {
        Transformation = _rotationX * _rotationY * _rotationZ * _scaleMatrix * _move;
    }

    public bool IsEmpty()
    {
        return Positions.Count == 0;
    }

    public void IncreaseScaleStep()
    {
        if (ScaleStep > 1) ScaleStep += 1;
        else if (ScaleStep > 0.1) ScaleStep += 0.1f;
        else if (ScaleStep > 0.01) ScaleStep += 0.01f;
        else if (ScaleStep > 0.001) ScaleStep += 0.001f;
        else ScaleStep += 0.0001f;
    }

    public void DecreaseScaleStep()
    {
        if (ScaleStep > 1) ScaleStep -= 1;
        else if (ScaleStep > 0.1) ScaleStep -= 0.01f;
        else if (ScaleStep > 0.01) ScaleStep -= 0.001f;
        else if (ScaleStep > 0.001) ScaleStep -= 0.0001f;
        else ScaleStep -= 0.00001f;
        if (ScaleStep < 0.00001) ScaleStep = 0.00001f;
    }

    public void IncreaseMoveStep()
    {
        if (MoveStep < 1) MoveStep += 0.1f;
        else if (MoveStep < 10) MoveStep += 1;
        else if (MoveStep < 100) MoveStep += 5;
        else MoveStep += 10;
    }

    public void DecreaseMoveStep()
    {
        if (MoveStep > 100) MoveStep -= 10;
        else if (MoveStep > 10) MoveStep -= 5;
        else if (MoveStep > 1) MoveStep -= 1;
        else MoveStep -= 0.1f;
        if (MoveStep < 0.1f) MoveStep = 0.1f;
    }

    public void MoveToWorldCenter()
    {
        var xMin = Positions.Min(v => v.Original.X);
        var xMax = Positions.Max(v => v.Original.X);
        var yMin = Positions.Min(v => v.Original.Y);
        var yMax = Positions.Max(v => v.Original.Y);
        var zMin = Positions.Min(v => v.Original.Z);
        var zMax = Positions.Max(v => v.Original.Z);
        XPosition = -(xMax + xMin) / 2;
        YPosition = -(yMax + yMin) / 2;
        ZPosition = -(zMax + zMin) / 2;
    }

    public void SetInitialPosition()
    {
        Scale = 1.0f;
        XAxisRotate = 0;
        YAxisRotate = 0;
        ZAxisRotate = 0;
        MoveToWorldCenter();
    }
}