using System.Numerics;
using Matth.SDF;

namespace Matth.Test
{
    public class SphereTests
    {
        private const float SphereRadius = 5f;

        private readonly Sphere _sphere = new (radius: SphereRadius);

        [SetUp]
        public void Setup()
        {
        }

        /*
        there are 3 SPHERE/LINE intersection cases:

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

        [Test]
        public void Test1A() // 1a. del>0, t0>0, t1>0: ray origin outside sphere, facing toward, 2 intersects (enter and exit)
        {
            var ray = new Ray
            {
                Origin = new Vector3(0f, 0f, -SphereRadius*2f),
                Direction = new Vector3(0f, 0f, 1f)
            };

            var hitEnter = new RayHitResult
            {
                HitType = SurfaceHitType.Enter,
                Ray = ray,
                DistanceAlongRay = SphereRadius,
                HitPoint = new Vector3(0f, 0f, -SphereRadius),
                SurfaceNormal = -ray.Direction
            };

            var hitExit = new RayHitResult
            {
                HitType = SurfaceHitType.Exit,
                Ray = ray,
                DistanceAlongRay = SphereRadius*3f,
                HitPoint = new Vector3(0f, 0f, SphereRadius),
                SurfaceNormal = ray.Direction
            };

            var dist = _sphere.Distance(ray.Origin);
            Assert.That(dist, Is.EqualTo(SphereRadius));

            var nHits = _sphere.Raycast(ray, out var hits);
            Assert.That(nHits, Is.EqualTo(2));
            Assert.That(hits, Has.Length.EqualTo(nHits));

            Assert.That(hits[0], Is.EqualTo(hitEnter));
            Assert.That(hits[1], Is.EqualTo(hitExit));
        }

        [Test]
        public void Test1B() // 1b. del>0, t0<0, t1>0: ray origin inside sphere, 1 intersect (exit only)
        {
            var ray = new Ray
            {
                Origin = Vector3.Zero,
                Direction = new Vector3(0f, 0f, 1f)
            };

            var hitExit = new RayHitResult
            {
                HitType = SurfaceHitType.Exit,
                Ray = ray,
                DistanceAlongRay = SphereRadius,
                HitPoint = new Vector3(0f, 0f, SphereRadius),
                SurfaceNormal = ray.Direction
            };

            var dist = _sphere.Distance(ray.Origin);
            Assert.That(dist, Is.EqualTo(-SphereRadius));

            var nHits = _sphere.Raycast(ray, out var hits);
            Assert.That(nHits, Is.EqualTo(1));
            Assert.That(hits, Has.Length.EqualTo(nHits));

            Assert.That(hits[0], Is.EqualTo(hitExit));
        }

        [Test]
        public void Test1C() // 1c. del>0, t0<0, t1<0: ray origin outside sphere, facing away, 0 intersects
        {
            var ray = new Ray
            {
                Origin = new Vector3(0f, 0f, -SphereRadius*2f),
                Direction = new Vector3(0f, 0f, -1f)
            };

            var dist = _sphere.Distance(ray.Origin);
            Assert.That(dist, Is.EqualTo(SphereRadius));

            var nHits = _sphere.Raycast(ray, out var hits);
            Assert.That(nHits, Is.EqualTo(0));
            Assert.That(hits, Has.Length.EqualTo(nHits));
        }

        [Test]
        public void Test2A() // 2a. del=0, t0=t1, t0>=0: ray origin on sphere, or outside facing toward, 1 intersect (tangent)
        {
            var ray = new Ray
            {
                Origin = new Vector3(-SphereRadius, 0f, -SphereRadius),
                Direction = new Vector3(0f, 0f, 1f)
            };

            var hitTangent = new RayHitResult
            {
                HitType = SurfaceHitType.Tangent,
                Ray = ray,
                DistanceAlongRay = SphereRadius,
                HitPoint = new Vector3(-SphereRadius, 0f, 0f),
                SurfaceNormal = new Vector3(-1f, 0f, 0f)
            };

            var nHits = _sphere.Raycast(ray, out var hits);
            Assert.That(nHits, Is.EqualTo(1));
            Assert.That(hits, Has.Length.EqualTo(nHits));

            Assert.That(hits[0], Is.EqualTo(hitTangent));
        }

        [Test]
        public void Test2B() // 2b. del=0, t0=t1, t0<0: ray origin outside sphere, facing away, 0 intersects
        {
            var ray = new Ray
            {
                Origin = new Vector3(-SphereRadius, 0f, -SphereRadius),
                Direction = new Vector3(0f, 0f, -1f)
            };

            var nHits = _sphere.Raycast(ray, out var hits);
            Assert.That(nHits, Is.EqualTo(0));
            Assert.That(hits, Has.Length.EqualTo(nHits));
        }

        [Test]
        public void Test3() // 3. del<0: line and sphere never intersect, so ray can't either
        {
            var ray = new Ray
            {
                Origin = new Vector3(-SphereRadius*2f, 0f, -SphereRadius*2f),
                Direction = new Vector3(0f, 0f, 1f)
            };

            var nHits = _sphere.Raycast(ray, out var hits);
            Assert.That(nHits, Is.EqualTo(0));
            Assert.That(hits, Has.Length.EqualTo(nHits));
        }
    }
}
