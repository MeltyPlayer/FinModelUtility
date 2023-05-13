﻿using System.IO;

using fin.image;
using fin.image.io;
using fin.io.image.tile;

using j3d.schema.bmd.tex1;


namespace j3d.image {
  public class J3dImageReader : IImageReader {
    private readonly IImageReader impl_;

    public J3dImageReader(int width, int height, TextureFormat format) {
      this.impl_ = this.CreateImpl_(width, height, format);
    }

    private IImageReader CreateImpl_(int width,
                                     int height,
                                     TextureFormat format) {
      return format switch {
          // TODO: For some reason, this has to include alpha to look correct, but is this actually right??
          TextureFormat.I4 => TiledImageReader.New(
              width,
              height,
              8,
              8,
              new L2a4PixelReader()),
          // TODO: For some reason, this has to include alpha to look correct, but is this actually right??
          TextureFormat.I8 => TiledImageReader.New(
              width,
              height,
              8,
              4,
              new L2a8PixelReader()),
          TextureFormat.A4_I4 => TiledImageReader.New(
              width,
              height,
              8,
              4,
              new La8PixelReader()),
          TextureFormat.A8_I8 => TiledImageReader.New(
              width,
              height,
              4,
              4,
              new La16PixelReader()),
          TextureFormat.R5_G6_B5 => TiledImageReader.New(
              width,
              height,
              4,
              4,
              new Rgb565PixelReader()),
          TextureFormat.A3_RGB5 => TiledImageReader.New(
              width,
              height,
              4,
              4,
              new Rgba5553PixelReader()),
          TextureFormat.ARGB8 => TiledImageReader.New(
              width,
              height,
              4,
              4,
              new Rgba32PixelReader()),
          TextureFormat.S3TC1 => TiledImageReader.New(
              width,
              height,
              new CmprTileReader()),
      };
    }

    public IImage Read(IEndianBinaryReader er) => this.impl_.Read(er);

    public IImage Read(byte[] data, Endianness endianness)
      => this.impl_.Read(data, endianness);
  }
}