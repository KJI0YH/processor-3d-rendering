using Rendering.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Rendering.Objects
{
    public class Model
    {
        public List<Vertex> Vertices { get; } = new();
        public List<Polygon> Polygons { get; } = new List<Polygon>();

        private float xAxisRotate = 0;
        private float yAxisRotate = 0;
        private float zAxisRotate = 0;
        private float xPosition = 0;
        private float yPosition = 0;
        private float zPosition = 0;
        private float scale = 1.0f;

        public float ScaleStep = 0.1f;
        public float MoveStep = 1f;
        public float MouseRotationDelta = MathF.PI / 36;

        private Matrix4x4 ScaleMatrix;
        private Matrix4x4 RotationX;
        private Matrix4x4 RotationY;
        private Matrix4x4 RotationZ;
        private Matrix4x4 Move;
        public Matrix4x4 Transformation { get; private set; }


        public float XAxisRotate
        {
            get { return xAxisRotate; }
            set
            {
                if (xAxisRotate != value)
                {
                    xAxisRotate = value;
                    RotationX = Matrix4x4.CreateRotationX(xAxisRotate);
                    UpdateTransformation();
                }
            }
        }

        public float YAxisRotate
        {
            get { return yAxisRotate; }
            set
            {
                if (yAxisRotate != value)
                {
                    yAxisRotate = value;
                    RotationY = Matrix4x4.CreateRotationY(yAxisRotate);
                    UpdateTransformation();
                }
            }
        }

        public float ZAxisRotate
        {
            get { return zAxisRotate; }
            set
            {
                if (zAxisRotate != value)
                {
                    zAxisRotate = value;
                    RotationZ = Matrix4x4.CreateRotationZ(zAxisRotate);
                    UpdateTransformation();
                }
            }
        }

        public float XPosition
        {
            get => xPosition;
            set
            {
                if (xPosition != value)
                {
                    xPosition = value;
                    Move = Matrix4x4.CreateTranslation(xPosition, yPosition, zPosition);
                    UpdateTransformation();
                }
            }
        }

        public float YPosition
        {
            get => yPosition;
            set
            {
                if (yPosition != value)
                {
                    yPosition = value;
                    Move = Matrix4x4.CreateTranslation(xPosition, yPosition, zPosition);
                    UpdateTransformation();
                }
            }
        }

        public float ZPosition
        {
            get => zPosition;
            set
            {
                if (zPosition != value)
                {
                    zPosition = value;
                    Move = Matrix4x4.CreateTranslation(xPosition, yPosition, zPosition);
                    UpdateTransformation();
                }
            }
        }

        public float Scale
        {
            get { return scale; }
            set
            {
                if (scale != value)
                {
                    if (value > 0) scale = value;
                    else scale = 0;

                    ScaleMatrix = Matrix4x4.CreateScale(scale);
                    UpdateTransformation();
                }
            }
        }

        public Model()
        {
            ScaleMatrix = Matrix4x4.CreateScale(scale);
            RotationX = Matrix4x4.CreateRotationX(xAxisRotate);
            RotationY = Matrix4x4.CreateRotationY(yAxisRotate);
            RotationZ = Matrix4x4.CreateRotationZ(zAxisRotate);
            Move = Matrix4x4.CreateTranslation(xPosition, yPosition, zPosition);
            UpdateTransformation();
        }

        public void AddVertex(Vertex vertex)
        {
            Vertices.Add(vertex);
        }

        public void AddPolygon(Polygon polygon)
        {
            Polygons.Add(polygon);
        }

        public void AddPolygon(IEnumerable<Polygon> polygons)
        {
            Polygons.AddRange(polygons);
        }

        private void UpdateTransformation()
        {
            Transformation = RotationX * RotationY * RotationZ * ScaleMatrix * Move;
        }

        public bool IsEmpty()
        {
            return Vertices.Count == 0;
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
            float xMin = Vertices.Min(v => v.Original.X);
            float xMax = Vertices.Max(v => v.Original.X);
            float yMin = Vertices.Min(v => v.Original.Y);
            float yMax = Vertices.Max(v => v.Original.Y);
            float zMin = Vertices.Min(v => v.Original.Z);
            float zMax = Vertices.Max(v => v.Original.Z);
            xPosition = -(xMax + xMin) / 2;
            yPosition = -(yMax + yMin) / 2;
            zPosition = -(zMax + zMin) / 2;
            Move = Matrix4x4.CreateTranslation(new Vector3(xPosition, yPosition, zPosition));
            UpdateTransformation();
        }

        public void SetInitialPositioin()
        {
            Scale = 1.0f;
            xPosition = 0;
            yPosition = 0;
            zPosition = 0;
            XAxisRotate = 0;
            YAxisRotate = 0;
            ZAxisRotate = 0;
            Move = Matrix4x4.CreateTranslation(xPosition, yPosition, zPosition);
            MoveToWorldCenter();
        }
    }
}
