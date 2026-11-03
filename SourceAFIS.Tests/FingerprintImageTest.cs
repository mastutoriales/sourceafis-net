// Part of SourceAFIS for .NET: https://sourceafis.machinezoo.com/net
using System;
using NUnit.Framework;
using SourceAFIS.Engine.Primitives;

namespace SourceAFIS
{
    public class FingerprintImageTest
    {
        [Test]
        public void DecodePng() => new FingerprintImage(TestResources.Png());

        void AssertSimilar(DoubleMatrix matrix, DoubleMatrix reference)
        {
            Assert.AreEqual(reference.Width, matrix.Width);
            Assert.AreEqual(reference.Height, matrix.Height);
            double delta = 0, max = -1, min = 1;
            for (int x = 0; x < matrix.Width; ++x)
            {
                for (int y = 0; y < matrix.Height; ++y)
                {
                    delta += Math.Abs(matrix[x, y] - reference[x, y]);
                    max = Math.Max(max, matrix[x, y]);
                    min = Math.Min(min, matrix[x, y]);
                }
            }
            Assert.IsTrue(max > 0.75);
            Assert.IsTrue(min < 0.1);
            Assert.IsTrue(delta / (matrix.Width * matrix.Height) < 0.01);
        }
        void AssertSimilar(byte[] image, byte[] reference) => AssertSimilar(new FingerprintImage(image).Matrix, new FingerprintImage(reference).Matrix);

        [Test]
        public void DecodeJpeg() => AssertSimilar(TestResources.Jpeg(), TestResources.Png());
        [Test]
        public void DecodeBmp() => AssertSimilar(TestResources.Bmp(), TestResources.Png());

        // A 3x2 RGBA PNG encoded independently of SkiaSharp. Rows contain:
        // (0,0,0,255), (255,255,255,255), (255,0,0,255)
        // (12,34,56,128), (90,120,150,0), (10,30,80,64).
        static byte[] RgbaPng() => Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAMAAAACCAYAAACddGYaAAAAH0lEQVR4nGNgYGD4DwYMDP8ZeJQsGqIqpjFwyQU4AACWSwoA/3zg4gAAAABJRU5ErkJggg==");

        [Test]
        public void DecodeRgbAndIgnoreAlpha()
        {
            var matrix = new FingerprintImage(RgbaPng()).Matrix;
            Assert.AreEqual(3, matrix.Width);
            Assert.AreEqual(2, matrix.Height);
            double[] expected = { 1, 0, 2.0 / 3, 13.0 / 15, 9.0 / 17, 43.0 / 51 };
            for (int y = 0; y < matrix.Height; ++y)
                for (int x = 0; x < matrix.Width; ++x)
                    Assert.That(matrix[x, y], Is.EqualTo(expected[y * matrix.Width + x]).Within(1e-12));
        }

        [Test]
        public void PreserveStoredJpegOrientation()
        {
            var original = TestResources.Jpeg();
            // EXIF APP1 segment: little-endian TIFF with Orientation = 6 (rotate 90 degrees).
            byte[] exif = {
                0xff, 0xe1, 0x00, 0x22, 0x45, 0x78, 0x69, 0x66, 0x00, 0x00,
                0x49, 0x49, 0x2a, 0x00, 0x08, 0x00, 0x00, 0x00, 0x01, 0x00,
                0x12, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };
            var oriented = new byte[original.Length + exif.Length];
            Array.Copy(original, 0, oriented, 0, 2);
            Array.Copy(exif, 0, oriented, 2, exif.Length);
            Array.Copy(original, 2, oriented, 2 + exif.Length, original.Length - 2);
            var reference = new FingerprintImage(original).Matrix;
            var matrix = new FingerprintImage(oriented).Matrix;
            Assert.AreEqual(reference.Width, matrix.Width);
            Assert.AreEqual(reference.Height, matrix.Height);
            for (int y = 0; y < matrix.Height; ++y)
                for (int x = 0; x < matrix.Width; ++x)
                    Assert.AreEqual(reference[x, y], matrix[x, y]);
        }

        [Test]
        public void DecodeGrayPixelValues()
        {
            var matrix = new FingerprintImage(3, 2, new byte[] { 0, 255, 51, 102, 153, 204 }).Matrix;
            Assert.AreEqual(3, matrix.Width);
            Assert.AreEqual(2, matrix.Height);
            double[] expected = { 1, 0, 0.8, 0.6, 0.4, 0.2 };
            for (int y = 0; y < matrix.Height; ++y)
                for (int x = 0; x < matrix.Width; ++x)
                    Assert.That(matrix[x, y], Is.EqualTo(expected[y * matrix.Width + x]).Within(1e-12));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void DefaultDpi(bool raw)
        {
            var image = raw ? new FingerprintImage(1, 1, new byte[] { 0 }) : new FingerprintImage(RgbaPng());
            Assert.AreEqual(500, image.Dpi);
        }

        [TestCase(false, 250)]
        [TestCase(false, 1000)]
        [TestCase(true, 250)]
        [TestCase(true, 1000)]
        public void CustomDpi(bool raw, double dpi)
        {
            var options = new FingerprintImageOptions { Dpi = dpi };
            var image = raw ? new FingerprintImage(1, 1, new byte[] { 0 }, options) : new FingerprintImage(RgbaPng(), options);
            Assert.AreEqual(dpi, image.Dpi);
        }

        [Test]
        public void RejectNullImage()
        {
            var error = Assert.Throws<ArgumentNullException>(() => new FingerprintImage(null));
            Assert.AreEqual("image", error.ParamName);
        }

        [Test]
        public void RejectNullPixels()
        {
            var error = Assert.Throws<ArgumentNullException>(() => new FingerprintImage(1, 1, null));
            Assert.AreEqual("pixels", error.ParamName);
        }

        void AssertDecodeFailure(byte[] image)
        {
            var error = Assert.Throws<ArgumentException>(() => new FingerprintImage(image));
            Assert.AreEqual("Failed to decode fingerprint image.", error.Message);
            Assert.That(error.InnerException, Is.Not.Null);
        }

        [Test]
        public void RejectEmptyImage() => AssertDecodeFailure(Array.Empty<byte>());

        [Test]
        public void RejectInvalidImage() => AssertDecodeFailure(new byte[] { 1, 2, 3, 4, 5 });

        [TestCase("PNG")]
        [TestCase("JPEG")]
        [TestCase("BMP")]
        public void RejectTruncatedImage(string format)
        {
            var image = format switch {
                "PNG" => TestResources.Png(),
                "JPEG" => TestResources.Jpeg(),
                "BMP" => TestResources.Bmp(),
                _ => throw new ArgumentOutOfRangeException(nameof(format))
            };
            Array.Resize(ref image, image.Length / 2);
            AssertDecodeFailure(image);
        }

        public static FingerprintImage Probe() => new FingerprintImage(TestResources.Probe());
        public static FingerprintImage Matching() => new FingerprintImage(TestResources.Matching());
        public static FingerprintImage Nonmatching() => new FingerprintImage(TestResources.Nonmatching());
        public static FingerprintImage ProbeGray() => new FingerprintImage(332, 533, TestResources.ProbeGray());
        public static FingerprintImage MatchingGray() => new FingerprintImage(320, 407, TestResources.MatchingGray());
        public static FingerprintImage NonmatchingGray() => new FingerprintImage(333, 435, TestResources.NonmatchingGray());

        [Test]
        public void DecodeGray()
        {
            double score = new FingerprintMatcher(new FingerprintTemplate(ProbeGray()))
                .Match(new FingerprintTemplate(MatchingGray()));
            Assert.That(score, Is.GreaterThan(40));
        }
    }
}
