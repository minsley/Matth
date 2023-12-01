using System.Numerics;
using Matth.SDF;

namespace Matth.Test
{
    public class CapsuleTests
    {
        private const float CapsuleRadius = 5f;
        private const float CapsuleBodyHeight = 20f;

        private readonly Capsule _capsule = new Capsule(
            originA: new Vector3(0f, -CapsuleBodyHeight / 2f, 0f),
            originB: new Vector3(0f, CapsuleBodyHeight / 2f, 0f),
            radius: CapsuleRadius);

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1A() // 1a. del>0, t0>0, t1>0: ray origin outside sphere, facing toward, 2 intersects (enter and exit)
        {
            var ray = new Ray
            {
                Origin = new Vector3(0f, 0f, -CapsuleRadius*2f),
                Direction = new Vector3(0f, 0f, 1f)
            };

            var hitEnter = new RayHitResult
            {
                HitType = SurfaceHitType.Enter,
                Ray = ray,
                DistanceAlongRay = CapsuleRadius,
                HitPoint = new Vector3(0f, 0f, -CapsuleRadius),
                SurfaceNormal = -ray.Direction
            };

            var hitExit = new RayHitResult
            {
                HitType = SurfaceHitType.Exit,
                Ray = ray,
                DistanceAlongRay = CapsuleRadius*3f,
                HitPoint = new Vector3(0f, 0f, CapsuleRadius),
                SurfaceNormal = ray.Direction
            };

            var dist = _capsule.Distance(ray.Origin);
            Assert.That(dist, Is.EqualTo(CapsuleRadius));

            var nHits = _capsule.Raycast(ray, out var hits);
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
                DistanceAlongRay = CapsuleRadius,
                HitPoint = new Vector3(0f, 0f, CapsuleRadius),
                SurfaceNormal = ray.Direction
            };

            var dist = _capsule.Distance(ray.Origin);
            Assert.That(dist, Is.EqualTo(-CapsuleRadius));

            var nHits = _capsule.Raycast(ray, out var hits);
            Assert.That(nHits, Is.EqualTo(1));
            Assert.That(hits, Has.Length.EqualTo(nHits));

            Assert.That(hits[0], Is.EqualTo(hitExit));
        }

        [Test]
        public void Test1C() // 1c. del>0, t0<0, t1<0: ray origin outside sphere, facing away, 0 intersects
        {
            var ray = new Ray
            {
                Origin = new Vector3(0f, 0f, -CapsuleRadius*2f),
                Direction = new Vector3(0f, 0f, -1f)
            };

            var dist = _capsule.Distance(ray.Origin);
            Assert.That(dist, Is.EqualTo(CapsuleRadius));

            var nHits = _capsule.Raycast(ray, out var hits);
            Assert.That(nHits, Is.EqualTo(0));
            Assert.That(hits, Has.Length.EqualTo(nHits));
        }

        [Test]
        public void Test2A() // 2a. del=0, t0=t1, t0>=0: ray origin on sphere, or outside facing toward, 1 intersect (tangent)
        {
            var ray = new Ray
            {
                Origin = new Vector3(-CapsuleRadius, 0f, -CapsuleRadius),
                Direction = new Vector3(0f, 0f, 1f)
            };

            var hitTangent = new RayHitResult
            {
                HitType = SurfaceHitType.Tangent,
                Ray = ray,
                DistanceAlongRay = CapsuleRadius,
                HitPoint = new Vector3(-CapsuleRadius, 0f, 0f),
                SurfaceNormal = new Vector3(-1f, 0f, 0f)
            };

            var nHits = _capsule.Raycast(ray, out var hits);
            Assert.That(nHits, Is.EqualTo(1));
            Assert.That(hits, Has.Length.EqualTo(nHits));

            Assert.That(hits[0], Is.EqualTo(hitTangent));
        }

        [Test]
        public void Test2B() // 2b. del=0, t0=t1, t0<0: ray origin outside sphere, facing away, 0 intersects
        {
            var ray = new Ray
            {
                Origin = new Vector3(-CapsuleRadius, 0f, -CapsuleRadius),
                Direction = new Vector3(0f, 0f, -1f)
            };

            var nHits = _capsule.Raycast(ray, out var hits);
            Assert.That(nHits, Is.EqualTo(0));
            Assert.That(hits, Has.Length.EqualTo(nHits));
        }

        [Test]
        public void Test3() // 3. del<0: line and sphere never intersect, so ray can't either
        {
            var ray = new Ray
            {
                Origin = new Vector3(-CapsuleRadius*2f, 0f, -CapsuleRadius*2f),
                Direction = new Vector3(0f, 0f, 1f)
            };

            var nHits = _capsule.Raycast(ray, out var hits);
            Assert.That(nHits, Is.EqualTo(0));
            Assert.That(hits, Has.Length.EqualTo(nHits));
        }
    }
}
