using System.Numerics;
using Matth.SDF;

namespace Matth.Test
{
    public class CapsuleBench
    {
        private const int MultiTestCount = 1_000_000;

        private System.Diagnostics.Stopwatch _timer;
        private readonly Random _random = new ();

        private Capsule _capsule;
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
            var x = _random.Next(-10, 10);
            var y = _random.Next(-10, 10);

            _capsule = new Capsule(
                radius: _random.Next(1, 5),
                originA: new Vector3(x, y, 0f),
                originB: new Vector3(-x, -y, 0f));

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

            var dist = _capsule.Distance(_point);

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
                dist = _capsule.Distance(_point);
            }

            _timer.Stop();

            Assert.That(dist, Is.GreaterThanOrEqualTo(0f));
            Assert.Pass($"{_timer.Elapsed.TotalMilliseconds:N} ms for {MultiTestCount:N0} runs, avg {_timer.Elapsed.TotalMicroseconds / MultiTestCount:N} us per run");
        }

        [Test] [TestCase][TestCase][TestCase][TestCase][TestCase]
        public void BenchRaycast()
        {
            _timer.Start();

            var nHits = _capsule.Raycast(_ray, out var hits);

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
                nHits = _capsule.Raycast(_ray, out hits);
            }

            _timer.Stop();

            Assert.That(nHits, Is.EqualTo(hits.Length));
            Assert.Pass($"{_timer.Elapsed.TotalMilliseconds:N} ms for {MultiTestCount:N0} runs, avg {_timer.Elapsed.TotalMicroseconds / MultiTestCount:N} us per run");
        }
    }
}
