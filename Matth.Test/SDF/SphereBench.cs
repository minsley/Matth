using System.Numerics;
using Matth.SDF;

namespace Matth.Test
{
    public class SphereBench
    {
        private const int MultiTestCount = 1_000_000;

        private System.Diagnostics.Stopwatch _timer;
        private readonly Random _random = new ();

        private Sphere _sphere;
        private Vector3 _point;
        private Ray _ray;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _timer = new System.Diagnostics.Stopwatch();
        }

        [SetUp]
        public void Setup()
        {
            _sphere = new Sphere(radius: _random.Next(1, 5));

            _point = new Vector3(_random.Next(-20, 20), _random.Next(-20, 20), _random.Next(-20, 20));

            _ray = new Ray
            {
                Origin = new Vector3(0f, 0f, -20f),
                Direction = new Vector3(0f, 0f, 1f)
            };

            _timer.Reset();
        }

        // multiple runs to account for .net being smart about stack allocations
        [Test] [TestCase][TestCase][TestCase][TestCase][TestCase]
        public void BenchDistance()
        {
            _timer.Start();

            var dist = _sphere.Distance(_point);

            _timer.Stop();

            Assert.That(dist, Is.GreaterThanOrEqualTo(0f));
            Assert.Pass($"{_timer.Elapsed.TotalMicroseconds:N} us");
        }

        [Test] [TestCase][TestCase][TestCase][TestCase][TestCase]
        public void BenchDistanceMulti()
        {
            _timer.Start();

            float dist = 0;
            for (var i = 0; i < MultiTestCount; i++)
            {
                dist = _sphere.Distance(_point);
            }

            _timer.Stop();

            Assert.That(dist, Is.GreaterThanOrEqualTo(0f));
            Assert.Pass($"{_timer.Elapsed.TotalMilliseconds:N} ms for {MultiTestCount:N0} runs, avg {_timer.Elapsed.TotalMicroseconds / MultiTestCount:N} us per run");
        }

        [Test] [TestCase][TestCase][TestCase][TestCase][TestCase]
        public void BenchRaycast()
        {
            _timer.Start();

            var nHits = _sphere.Raycast(_ray, out var hits);

            _timer.Stop();

            Assert.That(nHits, Is.EqualTo(hits.Length));
            Assert.Pass($"{_timer.Elapsed.TotalMicroseconds:N} us");
        }

        [Test] [TestCase][TestCase][TestCase][TestCase][TestCase]
        public void BenchRaycastMulti()
        {
            _timer.Start();

            var nHits = 0;
            var hits = Array.Empty<RayHitResult>();
            for (var i = 0; i < MultiTestCount; i++)
            {
                nHits = _sphere.Raycast(_ray, out hits);
            }

            _timer.Stop();

            Assert.That(nHits, Is.EqualTo(hits.Length));
            Assert.Pass($"{_timer.Elapsed.TotalMilliseconds:N} ms for {MultiTestCount:N0} runs, avg {_timer.Elapsed.TotalMicroseconds / MultiTestCount:N} us per run");
        }
    }
}
