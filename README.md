[![SWUbanner](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/banner2-direct.svg)](https://github.com/vshymanskyy/StandWithUkraine/blob/main/docs/README.md)

# SourceAFIS for .NET

[![Build status](https://github.com/robertvazan/sourceafis-net/actions/workflows/build.yml/badge.svg)](https://github.com/robertvazan/sourceafis-net/actions/workflows/build.yml)
[![Nuget](https://img.shields.io/nuget/v/SourceAFIS)](https://www.nuget.org/packages/SourceAFIS/)

SourceAFIS for .NET is a C# port of [SourceAFIS](https://sourceafis.machinezoo.com/),
an algorithm for recognition of human fingerprints.
It can compare two fingerprints 1:1 or search a large database 1:N for matching fingerprint.
It takes fingerprint images on input and produces similarity score on output.
Similarity score is then compared to customizable match threshold.

More on [homepage](https://sourceafis.machinezoo.com/net).

## Status

Stable and maintained.

## Getting started

See [homepage](https://sourceafis.machinezoo.com/net).

## SkiaSharp image decoding

This variant targets .NET 8 and uses SkiaSharp 4.154.0 to decode PNG, JPEG, and BMP images.
TIFF is not supported. The public SourceAFIS API, fingerprint recognition algorithm, and template serialization are unchanged.
Raw 8-bit grayscale pixels are still accepted through `FingerprintImage(width, height, pixels, options)`.

Image decoding requires SkiaSharp's native runtime libraries. Windows assets are supplied transitively by SkiaSharp,
and this package also depends on `SkiaSharp.NativeAssets.Linux.NoDependencies` 4.154.0 for Linux.
The Linux build does not require Fontconfig, but still requires the platform's standard system libraries.
Keep the native assets included in your application's build or publish output when deploying it.
The build workflow compiles, tests, and packages this variant on Windows x64 and Ubuntu Linux x64.

Decoding preserves stored image dimensions and orientation without applying EXIF rotation or color space conversion.
Intensity is calculated from the average of the stored RGB channels; alpha is ignored without blending onto a background.
Incomplete image decodes are rejected. Set `FingerprintImageOptions.Dpi` explicitly when the resolution differs
from the default of 500 DPI; embedded image DPI metadata is not used.

## Documentation

* [SourceAFIS for .NET](https://sourceafis.machinezoo.com/net)
* [XML doc comments](https://github.com/robertvazan/sourceafis-net/tree/master/SourceAFIS)
* [SourceAFIS overview](https://sourceafis.machinezoo.com/)
* [Algorithm](https://sourceafis.machinezoo.com/algorithm)

## Feedback

Bug reports and pull requests are welcome. See [CONTRIBUTING.md](https://github.com/robertvazan/sourceafis-net/blob/master/CONTRIBUTING.md).

## License

Distributed under [Apache License 2.0](https://github.com/robertvazan/sourceafis-net/blob/master/LICENSE).
