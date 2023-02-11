﻿using System.IO;

using fin.image;

using SixLabors.ImageSharp.PixelFormats;


namespace cmb.schema.cmb.image {
  /// <summary>
  ///   Stolen from:
  ///   https://github.com/magcius/noclip.website/blob/master/src/oot3d/pica_texture.ts
  /// </summary>
  public class Rgba32ImageReader : BTiledImageReader<Rgba32> {
    public Rgba32ImageReader(int width, int height) : base(width, height) { }

    protected override IImage<Rgba32> CreateImage_(int width, int height)
      => new Rgba32Image(width, height);

    protected override unsafe void Decode(IEndianBinaryReader er,
                                          Rgba32* scan0,
                                          int offset) {
      var a = er.ReadByte();
      var b = er.ReadByte();
      var g = er.ReadByte();
      var r = er.ReadByte();
      scan0[offset] = new Rgba32(r, g, b);
    }
  }
}