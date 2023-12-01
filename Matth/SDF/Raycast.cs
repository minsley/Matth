using System.Numerics;

namespace Matth.SDF
{
    public interface IRayCastable
    {
        public int Raycast(Vector3 rayOrigin, Vector3 rayDirection, out RayHitResult[] hits);
        public int Raycast(Ray ray, out RayHitResult[] hits);
    }

    public struct Ray
    {
        public Vector3 Origin;
        public Vector3 Direction;

        public Ray(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            Direction = direction;
        }
    }

    public struct RayHitResult
    {
        public SurfaceHitType HitType;
        public Ray Ray;
        public float DistanceAlongRay;
        public Vector3 HitPoint;
        public Vector3 SurfaceNormal;
    }

    public enum SurfaceHitType
    {
        Enter,
        Exit,
        Tangent
    }
}
