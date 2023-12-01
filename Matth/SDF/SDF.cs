using System;
using System.Collections.Generic;
using System.Numerics;

namespace Matth.SDF
{
    // https://iquilezles.org/articles/distfunctions/
    public interface ISDF
    {
        public float Distance(Vector3 point);
    }

    public abstract class SDF : ISDF
    {
        internal Vector3 Position = Vector3.Zero;
        internal Quaternion Rotation = Quaternion.Identity;
        internal float Scale = 1f;

        public abstract float Distance(Vector3 point);

        public static float UnionDistance(float d1, float d2) => Math.Min(d1, d2);
        public static float SubtractionDistance(float d1, float d2) => Math.Max(-d1, d2);
        public static float IntersectionDistance(float d1, float d2) => Math.Max(d1, d2);
        public static float XorDistance(float d1, float d2) => Math.Max(Math.Min(d1, d2), -Math.Max(d1, d2));
    }

    public class SdfGroup : SDF
    {
        internal List<ISDF> Shapes = new ();

        public SdfGroup(params ISDF[] shapes)
        {
            Shapes.AddRange(shapes);
        }

        public void Add(ISDF shape) => Shapes.Add(shape);

        public bool Remove(ISDF shape) => Shapes.Remove(shape);

        public override float Distance(Vector3 point)
        {
            if (Shapes.Count == 0) return float.NaN; // does anyone expect to handle NaN...?

            var min = Shapes[0].Distance(point);
            for (var i = 1; i < Shapes.Count; i++)
            {
                var dist = Shapes[i].Distance(point);
                if (dist < min) min = dist;
            }
            return min;
        }

        public float UnionDistance(SDF other, Vector3 point) => SDF.UnionDistance(this.Distance(point), other.Distance(point));
        public float SubtractionDistance(SDF other, Vector3 point) => SDF.SubtractionDistance(this.Distance(point), other.Distance(point));
        public float IntersectionDistance(SDF other, Vector3 point) => SDF.IntersectionDistance(this.Distance(point), other.Distance(point));
        public float XorDistance(SDF other, Vector3 point) => SDF.XorDistance(this.Distance(point), other.Distance(point));
    }
}
