﻿using System.IO;
using System.Security.Cryptography.X509Certificates;

using SixLabors.ImageSharp.PixelFormats;


namespace fin.image.io {
  public static class PixelImageReader {
    public static PixelImageReader<TPixel> New<TPixel>(
        int width,
        int height,
        IPixelReader<TPixel> pixelReader)
        where TPixel : unmanaged, IPixel<TPixel>
      => New(width,
             height,
             new BasicPixelIndexer(width),
             pixelReader);

    public static PixelImageReader<TPixel> New<TPixel>(
        int width,
        int height,
        IPixelIndexer pixelIndexer,
        IPixelReader<TPixel> pixelReader)
        where TPixel : unmanaged, IPixel<TPixel>
      => new(width,
             height,
             pixelIndexer,
             pixelReader);
  }

  public class PixelImageReader<TPixel> : IImageReader<IImage<TPixel>>
      where TPixel : unmanaged, IPixel<TPixel> {
    private readonly int width_;
    private readonly int height_;
    private readonly IPixelIndexer pixelIndexer_;
    private readonly IPixelReader<TPixel> pixelReader_;

    public PixelImageReader(int width,
                            int height,
                            IPixelIndexer pixelIndexer,
                            IPixelReader<TPixel> pixelReader) {
      this.width_ = width;
      this.height_ = height;
      this.pixelIndexer_ = pixelIndexer;
      this.pixelReader_ = pixelReader;
    }

    public IImage<TPixel> Read(
        byte[] srcBytes,
        Endianness endianness = Endianness.LittleEndian) {
      using var er = new EndianBinaryReader(srcBytes, endianness);
      return Read(er);
    }

    public unsafe IImage<TPixel> Read(IEndianBinaryReader er) {
      var image = this.pixelReader_.CreateImage(this.width_, this.height_);
      using var imageLock = image.Lock();
      var scan0 = imageLock.pixelScan0;

      for (var i = 0;
           i < this.width_ * this.height_;
           i += this.pixelReader_.PixelsPerRead) {
        this.pixelIndexer_.GetPixelCoordinates(i, out var x, out var y);
        var dstOffs = y * this.width_ + x;
        this.pixelReader_.Decode(er, scan0, dstOffs);
      }

      return image;
    }
  }
}