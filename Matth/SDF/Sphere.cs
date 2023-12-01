using System;
using System.Numerics;

namespace Matth.SDF
{
    public class Sphere : SDF, IRayCastable
    {
        internal float Radius;

        public Sphere(float radius) : base()
        {
            Radius = radius;
        }

        public override float Distance(Vector3 point) => Distance(point, Position, Radius);

        public static float Distance(Vector3 point, Vector3 sphereOrigin, float sphereRadius)
        {
            return (point - sphereOrigin).Length() - sphereRadius;
        }

        public int Raycast(Ray ray, out RayHitResult[] hits) => Raycast(ray.Origin, ray.Direction, out hits);

        public int Raycast(Vector3 rayOrigin, Vector3 rayDirection, out RayHitResult[] hits) => Raycast(rayOrigin, rayDirection, Position, Radius, out hits);

        public static int Raycast(Vector3 rayOrigin, Vector3 rayDirection, Vector3 sphereOrigin, float sphereRadius, out RayHitResult[] hits)
        {
            /*
            Rayyyyyyy Tracerrrrrrrrrrrr

            https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-sphere-intersection.html

            sphere eq:
                |p-so|^2 - r^2 = 0

            ray eq:
                p = ro + t*rd

            p is a point in space
            t is the ray length, and will be our intersect dist when solving for p that satisfies both the line and sphere equations

            combine the equations to solve for common points (intersects)...

            |ro + t*rd - so|^2 - sr^2 = 0
            = (ro-so)^2 + (rd*t)^2 + 2*rd*t*(ro-so) - sr^2 = 0
            = (ro - so)^2 + rd^2 * t^2 + 2*rd*t*(ro-so) - sr^2 = 0
            = (rd^2) * t^2 + (2*rd*(ro-so)) * t + ((ro-so)^2 - sr^2) = 0

            this mess is a quadratic:

            f(t) = a*t^2 + b*t + c,
                a = rd^2
                b = 2 * rd * (ro - so)
                c = (ro - so)^2 - sr^2

            the discriminant (del)'s sign tells us number of valid roots to the equation, and so number of line intersects

            del := b^2 - 4ac

            there are 3 LINE intersection cases:

                1. del > 0: 2 roots, t0 = ( -b + del^0.5 ) / 2a, t1 = ( -b - del^0.5 ) / 2a
                2. del = 0: 1 root,  t0 = t1 = -b / 2a
                3. del < 0: 0 roots, line does not intersect sphere

            RAY being only half of a LINE has additional cases to consider, specific to its origin and direction

                1a. del>0, t0>0,  t1>0:  ray origin outside sphere, facing toward, 2 intersects (enter and exit)
                1b. del>0, t0<0,  t1>0:  ray origin inside sphere, 1 intersect (exit only)
                1c. del>0, t0<0,  t1<0:  ray origin outside sphere, facing away, 0 intersects

                2a. del=0, t0=t1, t0>=0: ray origin on sphere, or outside facing toward, 1 intersect (tangent)
                2b. del=0, t0=t1, t0<0:  ray origin outside sphere, facing away, 0 intersects

                3.  del<0:               line and sphere never intersect, so ray can't either
            */

            var ro = rayOrigin;
            var rd = Vector3.Normalize(rayDirection);
            var so = sphereOrigin;
            var sr = Math.Abs(sphereRadius);

            // calculate our quadratic form shorthand values
            var rs = ro - so;

            var a = Vector3.Dot(rd, rd);
            var b = 2f * Vector3.Dot(rd, rs);
            var c = Vector3.Dot(rs, rs) - sr*sr;
            var del = b*b - 4*a*c;

            var a2 = 2f * a;
            var delRoot = (float)Math.Sqrt(del);

            // sign of the discriminant tells us if the line hit the sphere

            // discriminant < 0 means line never hits sphere
            if (del < 0f)
            {
                hits = Array.Empty<RayHitResult>();
                return 0;
            }

            // Discriminant > 0 means there are 2 line intersects, ray could have 0-2 hits
            if (del > 0f)
            {
                var t0 = (-b - delRoot) / a2;
                var t1 = (-b + delRoot) / a2;

                // origin is before sphere, 2 ray intersects (enter and exit)
                if (t0 > 0 && t1 > 0)
                {
                    var hit1 = new RayHitResult
                    {
                        Ray = new Ray(ro, rd),
                        DistanceAlongRay = t0,
                        HitType = SurfaceHitType.Enter,
                        HitPoint = ro + rd * t0
                    };
                    hit1.SurfaceNormal = Vector3.Normalize(hit1.HitPoint - so);

                    var hit2 = new RayHitResult
                    {
                        Ray = new Ray(ro, rd),
                        DistanceAlongRay = t1,
                        HitType = SurfaceHitType.Exit,
                        HitPoint = ro + rd * t1
                    };
                    hit2.SurfaceNormal = Vector3.Normalize(hit2.HitPoint - so);


                    hits = new[] { hit1, hit2 };
                    return hits.Length;
                }

                // origin is inside sphere, 1 ray intersect (exit only)
                if ((t0 < 0 && t1 > 0) || (t0 > 0 && t1 < 0))
                {
                    var hit = new RayHitResult
                    {
                        Ray = new Ray(ro, rd),
                        DistanceAlongRay = t0 > t1 ? t0 : t1,
                        HitType = SurfaceHitType.Exit,
                    };
                    hit.HitPoint = ro + rd * hit.DistanceAlongRay;
                    hit.SurfaceNormal = Vector3.Normalize(hit.HitPoint - so);

                    hits = new[] { hit };

                    return 1;
                }

                // origin is past sphere, 0 ray intersects
                if (t0 < 0 && t1 < 0)
                {
                    hits = Array.Empty<RayHitResult>();
                    return 0;
                }
            }

            // Discriminant == 0 is a single tangent hit, glancing the sphere, ray could have 0-1 hits
            if (del == 0f)
            {
                var t = -b / a2;

                // ray origin outside sphere, facing away, 0 intersects
                if (t < 0)
                {
                    hits = Array.Empty<RayHitResult>();
                    return 0;
                }
                else // ray origin either on sphere, or outside facing toward, 1 intersect (tangent)
                {
                    var hit = new RayHitResult
                    {
                        Ray = new Ray(ro, rd),
                        DistanceAlongRay = t,
                        HitType = SurfaceHitType.Tangent,
                        HitPoint = ro + rd * t
                    };
                    hit.SurfaceNormal = Vector3.Normalize(hit.HitPoint - so);

                    hits = new[] { hit };
                    return 1;
                }
            }

            throw new Exception("Raycast on Sphere has a discriminant that is not >, < or = to 0. Check for holes in the fabric of reality.");
        }
    }
}
