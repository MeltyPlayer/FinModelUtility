﻿using System;
using System.Runtime.CompilerServices;

using fin.image.formats;
using fin.util.color;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.io.tile {
  public readonly struct Dxt1aTileReader(
      int subTileCountInAxis = 2,
      int subTileSizeInAxis = 4,
      bool flipBlocksHorizontally = true)
      : ITileReader<Rgba32> {
    public IImage<Rgba32> CreateImage(int width, int height)
      => new Rgba32Image(PixelFormat.DXT1A, width, height);

    private readonly int tileSizeInAxis_
        = subTileCountInAxis * subTileSizeInAxis;

    public int TileWidth => this.tileSizeInAxis_;
    public int TileHeight => this.tileSizeInAxis_;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void Decode(IBinaryReader br,
                              Span<Rgba32> scan0,
                              int tileX,
                              int tileY,
                              int imageWidth,
                              int imageHeight) {
      Span<ushort> shortBuffer = stackalloc ushort[2];
      Span<Rgba32> paletteBuffer = stackalloc Rgba32[4];
      Span<byte> indicesBuffer = stackalloc byte[4];
      this.Decode(br,
                  scan0,
                  tileX,
                  tileY,
                  imageWidth,
                  imageHeight,
                  shortBuffer,
                  paletteBuffer,
                  indicesBuffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Decode(IBinaryReader br,
                       Span<Rgba32> scan0,
                       int tileX,
                       int tileY,
                       int imageWidth,
                       int imageHeight,
                       Span<ushort> shortBuffer,
                       Span<Rgba32> paletteBuffer,
                       Span<byte> indicesBuffer) {
      for (var j = 0; j < subTileCountInAxis; ++j) {
        for (var i = 0; i < subTileCountInAxis; ++i) {
          DecodeSubblock_(
              br,
              shortBuffer,
              scan0,
              paletteBuffer,
              indicesBuffer,
              tileX * TileWidth + i * subTileSizeInAxis,
              tileY * TileHeight + j * subTileSizeInAxis,
              imageWidth,
              imageHeight);
        }
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DecodeSubblock_(
        IBinaryReader br,
        Span<ushort> shortBuffer,
        Span<Rgba32> scan0,
        Span<Rgba32> paletteBuffer,
        Span<byte> indicesBuffer,
        int imageX,
        int imageY,
        int imageWidth,
        int imageHeight) {
      br.ReadUInt16s(shortBuffer);
      DecodePalette_(shortBuffer, paletteBuffer);

      br.ReadBytes(indicesBuffer);
      for (var j = 0; j < subTileSizeInAxis; ++j) {
        if (imageY + j >= imageHeight) {
          break;
        }

        var indices = indicesBuffer[j];
        var scan0Offset = (imageY + j) * imageWidth + imageX;

        for (var i = 0; i < subTileSizeInAxis; ++i) {
          if (imageX + i >= imageWidth) {
            break;
          }

          var shiftIndexAmount = !flipBlocksHorizontally ? i : 3 - i;
          var index = (indices >> (2 * shiftIndexAmount)) & 0b11;
          scan0[scan0Offset + i] = paletteBuffer[index];
        }
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DecodePalette_(
        ReadOnlySpan<ushort> colorValues,
        Span<Rgba32> palette) {
      var color1Value = colorValues[0];
      var color2Value = colorValues[1];

      ColorUtil.SplitRgb565(color1Value, out var r1, out var g1, out var b1);
      ColorUtil.SplitRgb565(color2Value, out var r2, out var g2, out var b2);

      palette[0] = new Rgba32(r1, g1, b1);
      palette[1] = new Rgba32(r2, g2, b2);

      if (color1Value > color2Value) {
        // 3rd color in palette is 1/3 from 1st to 2nd.
        palette[2] = new Rgba32(
            (byte) (((r1 << 1) + r2) / 3),
            (byte) (((g1 << 1) + g2) / 3),
            (byte) (((b1 << 1) + b2) / 3));
        // 4th color in palette is 2/3 from 1st to 2nd.
        palette[3] = new Rgba32(
            (byte) ((r1 + (r2 << 1)) / 3),
            (byte) ((g1 + (g2 << 1)) / 3),
            (byte) ((b1 + (b2 << 1)) / 3));
      } else {
        // 3rd color in palette is halfway between 1st and 2nd.
        var palette2 = palette[2] = new Rgba32(
            (byte) ((r1 + r2) >> 1),
            (byte) ((g1 + g2) >> 1),
            (byte) ((b1 + b2) >> 1));
        // 4th color in palette is transparency.
        // It might seem odd that we set the RGB channels for a pixel with 0
        // alpha, but occasionally the RGB channels will be selected for in
        // the shader and in those instances we need color values set.
        palette[3] = new Rgba32(
            palette2.R,
            palette2.G,
            palette2.B,
            0
        );
      }
    }
  }
}