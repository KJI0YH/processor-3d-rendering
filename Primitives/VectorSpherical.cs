﻿using System;
using System.Numerics;

namespace Rendering.Primitives
{
    public class VectorSpherical
    {
        public float R;
        public float AzimuthAngle;
        public float ZenithAngle;

        public VectorSpherical(float r, float azimuthAngle, float elevationAngle)
        {
            R = r;
            AzimuthAngle = azimuthAngle;
            ZenithAngle = elevationAngle;
        }

        public Vector3 ToCartesian()
        {
            (float sinAzimuth, float cosAzimuth) = MathF.SinCos(AzimuthAngle);
            (float sinElevation, float cosElevation) = MathF.SinCos(ZenithAngle);
            return new Vector3(R * sinElevation * sinAzimuth, R * cosElevation, R * sinElevation * cosAzimuth);
        }
    }
}
