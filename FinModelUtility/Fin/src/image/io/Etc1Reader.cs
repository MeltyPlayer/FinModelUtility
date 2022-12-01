﻿using SharpGLTF.Schema2;
using System;
using System.IO;
using Buffer = System.Buffer;


namespace fin.image.io {
  /// <summary>
  ///   Stolen from:
  ///   https://github.com/xdanieldzd/Scarlet/blob/master/Scarlet/Drawing/Compression/ETC1.cs
  /// </summary>
  public class Etc1ImageReader : IImageReader<Rgba32Image> {
    private readonly int width_;
    private readonly int height_;
    private readonly bool hasAlpha_;

    /* Specs: https://www.khronos.org/registry/gles/extensions/OES/OES_compressed_ETC1_RGB8_texture.txt */

    /* Other implementations:
     * https://github.com/richgel999/rg-etc1/blob/master/rg_etc1.cpp
     * https://github.com/Gericom/EveryFileExplorer/blob/master/3DS/GPU/Textures.cs
     * https://github.com/gdkchan/Ohana3DS-Rebirth/blob/master/Ohana3DS%20Rebirth/Ohana/TextureCodec.cs */

    private static readonly int[,] ETC1_MODIFIER_TABLES_ = {
        {2, 8, -2, -8}, {5, 17, -5, -17}, {9, 29, -9, -29}, {13, 42, -13, -42},
        {18, 60, -18, -60}, {24, 80, -24, -80}, {33, 106, -33, -106},
        {47, 183, -47, -183}
    };

    public Etc1ImageReader(int width, int height, bool hasAlpha) {
      this.width_ = width;
      this.height_ = height;
      this.hasAlpha_ = hasAlpha;
    }

    public Rgba32Image Read(byte[] srcBytes) {
      var bytes =
          Decompress_(srcBytes, this.width_, this.height_, this.hasAlpha_);

      var output = new Rgba32Image(this.width_, this.height_);
      output.Mutate((_, setHandler) => {
        for (var y = 0; y < this.height_; ++y) {
          for (var x = 0; x < this.width_; ++x) {
            var i = 4 * (y * this.width_ + x);

            var r = bytes[i + 0];
            var g = bytes[i + 1];
            var b = bytes[i + 2];
            var a = bytes[i + 3];

            setHandler(x, y, r, g, b, a);
          }
        }
      });

      return output;
    }

    private static byte[] Decompress_(
        byte[] srcData,
        int width,
        int height,
        bool hasAlpha) {
      byte[] dstData = new byte[4 * width * height];

      var er =
          new EndianBinaryReader(new MemoryStream(srcData),
                                 Endianness.LittleEndian);

      for (int y = 0; y < height; y += 8) {
        for (int x = 0; x < width; x += 8) {
          Etc1ImageReader.DecodeETC1Tile_(er,
                                          dstData,
                                          x,
                                          y,
                                          width,
                                          height,
                                          hasAlpha);
        }
      }

      return dstData;
    }

    private static void DecodeETC1Tile_(
        EndianBinaryReader reader,
        byte[] pixelData,
        int x,
        int y,
        int width,
        int height,
        bool hasAlpha) {
      for (int by = 0; by < 8; by += 4) {
        for (int bx = 0; bx < 8; bx += 4) {
          if (reader.Length - reader.Position < 8) {
            break;
          }

          var alpha = 0xFFFFFFFFFFFFFFFF;
          if (hasAlpha) {
            alpha = reader.ReadUInt64();
          }
          var block = reader.ReadUInt64();

          using BinaryReader decodedReader =
              new BinaryReader(
                  new MemoryStream(Etc1ImageReader.DecodeETC1Block_(block)));
          for (int py = 0; py < 4; py++) {
            if (y + @by + py >= height) {
              break;
            }
            for (int px = 0; px < 4; px++) {
              if (x + bx + px >= width) {
                break;
              }

              int pixelOffset =
                  (int)((((y + @by + py) * width) + (x + bx + px)) * 4);
              Buffer.BlockCopy(decodedReader.ReadBytes(3),
                               0,
                               pixelData,
                               pixelOffset,
                               3);
              byte pixelAlpha =
                  (byte)((alpha >> (((px * 4) + py) * 4)) & 0xF);
              pixelData[pixelOffset + 3] =
                  (byte)(pixelAlpha * 17);
            }
          }
        }
      }
    }

    private static byte[] DecodeETC1Block_(ulong block) {
      byte r1, g1, b1, r2, g2, b2;

      byte tableIndex1 = (byte)((block >> 37) & 0x07);
      byte tableIndex2 = (byte)((block >> 34) & 0x07);
      byte diffBit = (byte)((block >> 33) & 0x01);
      byte flipBit = (byte)((block >> 32) & 0x01);

      if (diffBit == 0x00) {
        /* Individual mode */
        r1 = (byte)(((block >> 60) & 0x0F) << 4 | (block >> 60) & 0x0F);
        g1 = (byte)(((block >> 52) & 0x0F) << 4 | (block >> 52) & 0x0F);
        b1 = (byte)(((block >> 44) & 0x0F) << 4 | (block >> 44) & 0x0F);

        r2 = (byte)(((block >> 56) & 0x0F) << 4 | (block >> 56) & 0x0F);
        g2 = (byte)(((block >> 48) & 0x0F) << 4 | (block >> 48) & 0x0F);
        b2 = (byte)(((block >> 40) & 0x0F) << 4 | (block >> 40) & 0x0F);
      } else {
        /* Differential mode */

        /* 5bit base values */
        byte r1a = (byte)(((block >> 59) & 0x1F));
        byte g1a = (byte)(((block >> 51) & 0x1F));
        byte b1a = (byte)(((block >> 43) & 0x1F));

        /* Subblock 1, 8bit extended */
        r1 = (byte)((r1a << 3) | (r1a >> 2));
        g1 = (byte)((g1a << 3) | (g1a >> 2));
        b1 = (byte)((b1a << 3) | (b1a >> 2));

        /* 3bit modifiers */
        sbyte dr2 = (sbyte)((block >> 56) & 0x07);
        sbyte dg2 = (sbyte)((block >> 48) & 0x07);
        sbyte db2 = (sbyte)((block >> 40) & 0x07);
        if (dr2 >= 4) dr2 -= 8;
        if (dg2 >= 4) dg2 -= 8;
        if (db2 >= 4) db2 -= 8;

        /* Subblock 2, 8bit extended */
        r2 = (byte)((r1a + dr2) << 3 | (r1a + dr2) >> 2);
        g2 = (byte)((g1a + dg2) << 3 | (g1a + dg2) >> 2);
        b2 = (byte)((b1a + db2) << 3 | (b1a + db2) >> 2);
      }

      var decodedData = new byte[(4 * 4) * 3];
      var decodedPos = 0;

      for (int py = 0; py < 4; py++) {
        for (int px = 0; px < 4; px++) {
          int index = (int)(((block >> ((px * 4) + py)) & 0x1) |
                            ((block >> (((px * 4) + py) + 16)) & 0x1) << 1);

          if ((flipBit == 0x01 && py < 2) || (flipBit == 0x00 && px < 2)) {
            int modifier =
                Etc1ImageReader.ETC1_MODIFIER_TABLES_[tableIndex1, index];
            Etc1ImageReader.WriteClampedByte_(decodedData, ref decodedPos,
                                              r1 + modifier);
            Etc1ImageReader.WriteClampedByte_(decodedData, ref decodedPos,
                                              g1 + modifier);
            Etc1ImageReader.WriteClampedByte_(decodedData, ref decodedPos,
                                              b1 + modifier);
          } else {
            int modifier =
                Etc1ImageReader.ETC1_MODIFIER_TABLES_[tableIndex2, index];
            Etc1ImageReader.WriteClampedByte_(decodedData, ref decodedPos,
                                              r2 + modifier);
            Etc1ImageReader.WriteClampedByte_(decodedData, ref decodedPos,
                                              g2 + modifier);
            Etc1ImageReader.WriteClampedByte_(decodedData, ref decodedPos,
                                              b2 + modifier);
          }
        }
      }

      return decodedData;
    }

    private static void WriteClampedByte_(byte[] decodedData,
                                          ref int pos,
                                          int value) {
      value = Math.Clamp(value, byte.MinValue, byte.MaxValue);
      decodedData[pos++] = (byte)value;
    }
  }
}