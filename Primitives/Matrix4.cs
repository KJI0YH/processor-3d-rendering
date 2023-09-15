using System;

namespace Lab1.Primitives
{
    public class Matrix4
    {
        private float[,] values = new float[4, 4];
        public static int Dimension { get; } = 4;

        public Matrix4()
        {
            for (int row = 0; row < Dimension; row++)
            {
                for (int col = 0; col < Dimension; col++)
                {
                    values[row, col] = 0.0f;
                }
            }
        }

        public float this[int row, int col]
        {
            get
            {
                return values[row, col];
            }
            set
            {
                values[row, col] = value;
            }
        }

        public static Matrix4 One()
        {
            Matrix4 matrix = new Matrix4();
            for (int index = 0; index < Dimension; index++)
            {
                matrix[index, index] = 1;
            }
            return matrix;
        }

        public static Matrix4 Move(Vector3 translation)
        {
            Matrix4 matrix = One();
            for (int i = 0; i < Vector3.Size; i++)
            {
                matrix[i, Dimension - 1] = translation[i];
            }
            return matrix;
        }

        public static Matrix4 Scale(Vector3 scale)
        {
            Matrix4 matrix = One();
            for (int i = 0; i < Vector3.Size; i++)
            {
                matrix[i, i] = scale[i];
            }
            return matrix;
        }

        public static Matrix4 RotateX(float angle)
        {
            Matrix4 matrix = One();

            float sin = MathF.Sin(angle);
            float cos = MathF.Cos(angle);

            matrix[1, 1] = cos;
            matrix[1, 2] = -sin;
            matrix[2, 1] = sin;
            matrix[2, 2] = cos;
            return matrix;
        }

        public static Matrix4 RotateY(float angle)
        {
            Matrix4 matrix = One();

            float sin = MathF.Sin(angle);
            float cos = MathF.Cos(angle);

            matrix[0, 0] = cos;
            matrix[0, 2] = sin;
            matrix[2, 0] = -sin;
            matrix[2, 2] = cos;
            return matrix;
        }

        public static Matrix4 RotateZ(float angle)
        {
            Matrix4 matrix = One();

            float sin = MathF.Sin(angle);
            float cos = MathF.Cos(angle);

            matrix[0, 0] = cos;
            matrix[0, 1] = -sin;
            matrix[1, 0] = sin;
            matrix[1, 1] = cos;
            return matrix;
        }

        public static Matrix4 Projection(float FOV, float aspect, float zNear, float zFar)
        {
            Matrix4 matrix = new Matrix4();
            float tan = MathF.Tan(FOV / 2);
            matrix[0, 0] = 1 / (aspect * tan);
            matrix[1, 1] = 1 / tan;
            matrix[2, 2] = zFar / (zNear - zFar);
            matrix[2, 3] = (zNear * zFar) / (zNear - zFar);
            matrix[3, 2] = -1;
            return matrix;
        }

        public static Matrix4 Viewport(float width, float height, float xMin, float yMin)
        {
            Matrix4 matrix = One();
            matrix[0, 0] = width / 2;
            matrix[1, 1] = -height / 2;
            matrix[0, 3] = xMin + matrix[0, 0];
            matrix[1, 3] = yMin - matrix[1, 1];
            return matrix;
        }

        public static Matrix4 operator *(Matrix4 a, Matrix4 b)
        {
            Matrix4 matrix = new Matrix4();
            for (int row = 0; row < Dimension; row++)
            {
                for (int col = 0; col < Dimension; col++)
                {
                    for (int common = 0; common < Dimension; common++)
                    {
                        matrix[row, col] += a[row, common] * b[common, col];
                    }
                }
            }
            return matrix;
        }

        public static Vector3 operator *(Matrix4 matrix, Vector3 vector)
        {
            float x = 0, y = 0, z = 0, w = 0;

            for (int col = 0; col < Dimension; col++)
            {
                x += matrix[0, col] * vector[col];
                y += matrix[1, col] * vector[col];
                z += matrix[2, col] * vector[col];
                w += matrix[3, col] * vector[col];
            }

            return new Vector3(x, y, z, w);
        }
    }
}
