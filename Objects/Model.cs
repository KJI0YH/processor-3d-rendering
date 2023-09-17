using Lab1.Primitives;
using System.Collections.Generic;

namespace Lab1
{
    public class Model
    {
        public List<Vector3> Vertices { get; } = new List<Vector3>();
        public List<Vector3> VerticesTextures { get; } = new List<Vector3>();
        public List<Vector3> VerticesNormals { get; } = new List<Vector3>();
        public List<Polygon> Polygons { get; } = new List<Polygon>();

        private float xAxisRotate = 0;
        private float yAxisRotate = 0;
        private float zAxisRotate = 0;
        private float scale = 1.0f;
        public float ScaleStep = 0.1f;
        public float MoveStep = 1f;

        private Matrix4 ScaleMatrix;
        private Matrix4 RotationX;
        private Matrix4 RotationY;
        private Matrix4 RotationZ;
        private Matrix4 Move;
        public Vector3 Translation { get; private set; } = new Vector3(0, 0, 0);
        public Matrix4 Transformation { get; private set; }

        public float Scale
        {
            get { return scale; }
            set
            {
                if (scale != value)
                {
                    if (value > 0) scale = value;
                    else scale = 0;

                    ScaleMatrix = Matrix4.Scale(new Vector3(scale, scale, scale));
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
                    RotationX = Matrix4.RotateX(xAxisRotate);
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
                    RotationY = Matrix4.RotateY(yAxisRotate);
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
                    RotationZ = Matrix4.RotateZ(zAxisRotate);
                    UpdateTransformation();
                }
            }
        }

        public Model()
        {
            ScaleMatrix = Matrix4.Scale(new Vector3(scale, scale, scale));
            RotationX = Matrix4.RotateX(xAxisRotate);
            RotationY = Matrix4.RotateY(yAxisRotate);
            RotationZ = Matrix4.RotateZ(zAxisRotate);
            Move = Matrix4.Move(Translation);
            UpdateTransformation();
        }

        public void AddVertex(Vector3 vertex)
        {
            Vertices.Add(vertex);
        }

        public void AddVertexTexture(Vector3 vertexTexture)
        {
            VerticesTextures.Add(vertexTexture);
        }

        public void AddVertexNormal(Vector3 vectorNormal)
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
            Translation.X += moveStep;
            Move = Matrix4.Move(Translation);
            UpdateTransformation();
        }

        public void MoveByY(float moveStep)
        {
            Translation.Y += moveStep;
            Move = Matrix4.Move(Translation);
            UpdateTransformation();
        }

        public void MoveByZ(float moveStep)
        {
            Translation.Z += moveStep;
            Move = Matrix4.Move(Translation);
            UpdateTransformation();
        }
    }
}
