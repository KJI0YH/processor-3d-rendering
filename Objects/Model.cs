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
        private Vector3 translation = new Vector3(0, 0, 0);

        private Matrix4 ScaleMatrix;
        private Matrix4 RotationX;
        private Matrix4 RotationY;
        private Matrix4 RotationZ;
        private Matrix4 Move;

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

        public Vector3 Translation
        {
            get { return translation; }
            set
            {
                if (translation != value)
                {
                    translation = value;
                    Move = Matrix4.Move(translation);
                }
            }
        }

        public Matrix4 Transformation { get; private set; }

        public Model()
        {
            ScaleMatrix = Matrix4.Scale(new Vector3(scale, scale, scale));
            RotationX = Matrix4.RotateX(xAxisRotate);
            RotationY = Matrix4.RotateY(yAxisRotate);
            RotationZ = Matrix4.RotateZ(zAxisRotate);
            Move = Matrix4.Move(translation);
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

    }
}
