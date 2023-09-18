using Lab1.Primitives;
using System.Collections.Generic;
using System.Numerics;

namespace Lab1
{
    public class Model
    {
        public List<Vector4> Vertices { get; } = new List<Vector4>();
        public List<Vector4> VerticesTextures { get; } = new List<Vector4>();
        public List<Vector4> VerticesNormals { get; } = new List<Vector4>();
        public List<Polygon> Polygons { get; } = new List<Polygon>();

        private float xAxisRotate = 0;
        private float yAxisRotate = 0;
        private float zAxisRotate = 0;
        private float scale = 1.0f;
        public float ScaleStep = 0.1f;
        public float MoveStep = 1f;
        public float XPosition = 0;
        public float YPosition = 0;
        public float ZPosition = 0;

        private Matrix4x4 ScaleMatrix;
        private Matrix4x4 RotationX;
        private Matrix4x4 RotationY;
        private Matrix4x4 RotationZ;
        private Matrix4x4 Move;
        public Matrix4x4 Transformation { get; private set; }

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

        public Model()
        {
            ScaleMatrix = Matrix4x4.CreateScale(scale);
            RotationX = Matrix4x4.CreateRotationX(xAxisRotate);
            RotationY = Matrix4x4.CreateRotationY(yAxisRotate);
            RotationZ = Matrix4x4.CreateRotationZ(zAxisRotate);
            Move = Matrix4x4.CreateTranslation(XPosition, YPosition, ZPosition);
            UpdateTransformation();
        }

        public void AddVertex(Vector4 vertex)
        {
            Vertices.Add(vertex);
        }

        public void AddVertexTexture(Vector4 vertexTexture)
        {
            VerticesTextures.Add(vertexTexture);
        }

        public void AddVertexNormal(Vector4 vectorNormal)
        {
            VerticesNormals.Add(vectorNormal);
        }

        public void AddPolygon(Polygon polygon)
        {
            Polygons.Add(polygon);
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

        public void MoveByX(float moveStep)
        {
            XPosition += moveStep;
            Move = Matrix4x4.CreateTranslation(XPosition, YPosition, ZPosition);
            UpdateTransformation();
        }

        public void MoveByY(float moveStep)
        {
            YPosition += moveStep;
            Move = Matrix4x4.CreateTranslation(XPosition, YPosition, ZPosition);
            UpdateTransformation();
        }

        public void MoveByZ(float moveStep)
        {
            ZPosition += moveStep;
            Move = Matrix4x4.CreateTranslation(XPosition, YPosition, ZPosition);
            UpdateTransformation();
        }
    }
}
