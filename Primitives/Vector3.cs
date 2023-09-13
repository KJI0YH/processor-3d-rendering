using System;

namespace Lab1.Primitives
{
    public class Vector3
    {
        public float X { get; protected set; } = 0;
        public float Y { get; protected set; } = 0;
        public float Z { get; protected set; } = 0;
        public float W { get; protected set; } = 1;
        public static int Size { get; } = 3;

        public Vector3() { }

        public Vector3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Vector3(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return X;
                    case 1: return Y;
                    case 2: return Z;
                    case 3: return W;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        public float Magnitude
        {
            get
            {
                return MathF.Sqrt(X * X + Y * Y + Z * Z);
            }
        }

        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static Vector3 operator -(Vector3 vector)
        {
            return new Vector3(-vector.X, -vector.Y, -vector.Z);
        }

        public static Vector3 operator *(Vector3 vector, float scalar)
        {
            return new Vector3(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);
        }

        public static Vector3 operator /(Vector3 vector, float scalar)
        {
            scalar = 1.0f / scalar;
            return new Vector3(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);
        }

        public Vector3 Normalize()
        {
            return this / Magnitude;
        }

        public float Dot(Vector3 vector)
        {
            return X * vector.X + Y * vector.Y + Z * vector.Z;
        }

        public Vector3 Cross(Vector3 vector)
        {
            return new Vector3(Y * vector.Z - Z * vector.Y, Z * vector.X - X * vector.Z, X * vector.Y - Y * vector.X);
        }

        public Vector3 Scale(Vector3 scale)
        {
            return new Vector3(X * scale.X, Y * scale.Y, Z * scale.Z);
        }

        public void Update(Vector3 vector)
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
            W = vector.W;
        }
    }
}
