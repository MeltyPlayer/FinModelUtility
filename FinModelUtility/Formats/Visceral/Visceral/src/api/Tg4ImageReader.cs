﻿using System.Drawing;

using BCnEncoder.Decoder;
using BCnEncoder.Shared;

using CommunityToolkit.HighPerformance;

using fin.image;
using fin.image.formats;
using fin.io;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace visceral.api;

public record Tg4ImageFileBundle {
  public required IReadOnlyTreeFile Tg4hFile { get; init; }
  public required IReadOnlyTreeFile Tg4dFile { get; init; }
}

public class Tg4ImageReader {
  public IImage ReadImage(Tg4ImageFileBundle bundle) {
    var headerFile = bundle.Tg4hFile;
    using var headerEr =
        new SchemaBinaryReader(headerFile.OpenRead(),
                               Endianness.LittleEndian);
    headerEr.Position = 0x20;
    var width = headerEr.ReadUInt16();
    var height = headerEr.ReadUInt16();
    var format = headerEr.SubreadStringNTAt(0x4b);

    CompressionFormat? compressionFormat = format switch {
        "DXT1c"   => CompressionFormat.Bc1,
        "DXT1a"   => CompressionFormat.Bc1WithAlpha,
        "DXT5"    => CompressionFormat.Bc3,
        "DXT5_NM" => CompressionFormat.Bc3,
        _         => null,
    };

    if (compressionFormat == null) {
      return FinImage.Create1x1FromColor(Color.Magenta);
    }

    var isNormal = format == "DXT5_NM";

    var imageFormat = compressionFormat switch {
        CompressionFormat.Bc1          => PixelFormat.DXT1,
        CompressionFormat.Bc1WithAlpha => PixelFormat.DXT1A,
        CompressionFormat.Bc3          => PixelFormat.DXT5,
        _                              => throw new ArgumentOutOfRangeException()
    };

    var dataFile = bundle.Tg4dFile;

    var bcDecoder = new BcDecoder();
    bcDecoder.Options.IsParallel = true;

    using var dataS = dataFile.OpenRead();
    var loadedDxt = bcDecoder
                    .DecodeRaw(dataS, width, height, compressionFormat.Value)
                    .AsSpan();

    var rgbaImage = new Rgba32Image(imageFormat, width, height);
    using var dstLock = rgbaImage.Lock();

    if (!isNormal) {
      loadedDxt.AsBytes().CopyTo(dstLock.Bytes);
    } else {
      var dst = dstLock.Pixels;
      for (var y = 0; y < height; y++) {
        for (var x = 0; x < width; ++x) {
          var i = y * width + x;

          var src = loadedDxt[i];
          dst[i] = new Rgba32(src.a, src.g, (byte) (255 - src.b), 255);
        }
      }
    }

    return rgbaImage;
  }
}