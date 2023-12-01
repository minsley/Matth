using System;
using System.Numerics;

namespace Matth.SDF
{
    public class Capsule : SDF, IRayCastable
    {
        internal Vector3 OriginA;
        internal Vector3 OriginB;
        internal float Radius;

        public Capsule(Vector3 originA, Vector3 originB, float radius)
        {
            OriginA = originA;
            OriginB = originB;
            Radius = radius;
        }

        public override float Distance(Vector3 point)
        {
            return Distance(point, OriginA, OriginB, Radius);
        }

        public static float Distance(Vector3 point, Vector3 capsuleOriginA, Vector3 capsuleOriginB, float capsuleRadius)
        {
            var pa = point - capsuleOriginA;
            var ba = capsuleOriginB - capsuleOriginA;
            var h = Math.Clamp(Vector3.Dot(pa, ba) / Vector3.Dot(ba, ba), 0f, 1f);
            return (pa - ba * h).Length() - capsuleRadius;
        }

        public int Raycast(Ray ray, out RayHitResult[] hits) => Raycast(ray.Origin, ray.Direction, out hits);

        public int Raycast(Vector3 rayOrigin, Vector3 rayDirection, out RayHitResult[] hits) => Raycast(rayOrigin, rayDirection, OriginA, OriginB, Radius, out hits);

        public static int Raycast(Vector3 rayOrigin, Vector3 rayDirection, Vector3 capsuleOriginA, Vector3 capsuleOriginB, float capsuleRadius, out RayHitResult[] hits)
        {
            // https://www.shadertoy.com/view/Xt3SzX
            // throw new NotImplementedException();

            var ro = rayOrigin;
            var rd = Vector3.Normalize(rayDirection);
            var pa = capsuleOriginA;
            var pb = capsuleOriginB;
            var r = Math.Abs(capsuleRadius);

            var ba = pb - pa;
            var oa = ro - pa;
            var ob = ro - pb;

            var baba = Vector3.Dot(ba, ba);
            var bard = Vector3.Dot(ba, rd);
            var baoa = Vector3.Dot(ba, oa);
            var rdoa = Vector3.Dot(rd, oa);
            var oaoa = Vector3.Dot(oa, oa);

            var a = baba - bard * bard;
            var b = baba * rdoa - baoa * bard;
            var c = baba * oaoa - baoa * baoa - r * r * baba;
            var del = b * b - a * c;
            var delRoot = (float) Math.Sqrt(del);


            // if( del>=0.0 )
            // {
            //     float t = (-b-sqrt(del))/a;
            //     float y = baoa + t*bard; // body
            //     if( y>0.0 && y<baba ) return t; // caps
            //
            //     vec3 oc = (y<=0.0) ? oa : ro - pb;
            //     b = dot(rd,oc);
            //     c = dot(oc,oc) - r*r;
            //     del = b*b - c;
            //     if( del>0.0 ) return -b - sqrt(del);
            //
            // }

            // discriminant < 0 means line never hits
            if (del < 0f)
            {
                hits = Array.Empty<RayHitResult>();
                return 0;
            }

            // Discriminant > 0 means there are 2 line intersects, ray could have 0-2 hits
            if (del > 0f)
            {
                var t0 = (-b + delRoot) / a;
                var t1 = (-b - delRoot) / a;
                var y0 = baoa + bard * t0;
                var y1 = baoa + bard * t1;

                if (y0 > 0f && y0 < baba) // hit 1 is on body
                {
                }
                else // else caps
                {
                    var oc = y0 <= 0f ? oa : ob; // are we hitting a cap or b cap
                    var bCap = Vector3.Dot(rd, oc);
                    var cCap = Vector3.Dot(oc, oc) - r * r;
                    var delCap = bCap * bCap - cCap;

                    if (delCap > 0f)
                    {
                        var t1Cap = -bCap + (float) Math.Sqrt(delCap);
                    }
                }

                // origin is before sphere, 2 ray intersects (enter and exit)
                if (t0 > 0 && t1 > 0)
                {
                    var hit1 = new RayHitResult
                    {
                        Ray = new Ray(ro, rd),
                        DistanceAlongRay = t0,
                        HitType = t0 < t1 ? SurfaceHitType.Enter : SurfaceHitType.Exit,
                        HitPoint = ro + rd * t0
                    };
                    // hit1.HitNormal = Vector3.Normalize(hit1.HitPoint - so);


                    var hit2 = new RayHitResult
                    {
                        Ray = new Ray(ro, rd),
                        DistanceAlongRay = t1,
                        HitType = t1 < t0 ? SurfaceHitType.Enter : SurfaceHitType.Exit,
                        HitPoint = ro + rd * t1
                    };
                    // hit2.HitNormal = Vector3.Normalize(hit2.HitPoint - so);

                    hits = new[] {hit1, hit2};

                    return hits.Length;
                }

                // origin is inside sphere, 1 ray intersect (exit only)
                if ((t0 < 0 && t1 > 0) || (t0 > 0 && t1 < 0))
                {
                    var hit = new RayHitResult
                    {
                        Ray = new Ray(ro, rd),
                        DistanceAlongRay = t0 > t1 ? t0 : t1,
                        HitType = SurfaceHitType.Exit
                    };
                    hit.HitPoint = ro + rd * hit.DistanceAlongRay;
                    // hit.HitNormal = Vector3.Normalize(hit.HitPoint - so);

                    hits = new[] {hit};

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
                var t = -b / (2f * a);

                // ray origin outside sphere, facing away, 0 intersects
                if (t < 0)
                {
                    hits = Array.Empty<RayHitResult>();
                    return 0;
                }

                // ray origin either on sphere, or outside facing toward, 1 intersect (tangent)
                var hit = new RayHitResult
                {
                    Ray = new Ray(ro, rd),
                    DistanceAlongRay = t,
                    HitType = SurfaceHitType.Tangent,
                    HitPoint = ro + rd * t
                };
                // hit.HitNormal = Vector3.Normalize(hit.HitPoint - so);

                hits = new[] {hit};
                return 1;
            }

            throw new Exception("Raycast on Sphere has a discriminant that is not >, < or = to 0. Check for holes in the fabric of reality.");
        }
    }
}
