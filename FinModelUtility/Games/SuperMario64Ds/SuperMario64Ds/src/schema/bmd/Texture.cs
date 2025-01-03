﻿using fin.math;

using schema.binary;
using schema.binary.attributes;

namespace sm64ds.schema.bmd;

public enum TextureType {
  A3_I5 = 1,
  PALETTE_4 = 2,
  PALETTE_16 = 3,
  PALETTE_256 = 4,
  TEX_4X4 = 5,
  A5_I3 = 6,
  DIRECT = 7
}

/// <summary>
///   Shamelessly stolen from:
///   https://kuribo64.net/get.php?id=KBNyhM0kmNiuUBb3
/// </summary>
[BinarySchema]
public partial class Texture : IBinaryConvertible {
  private uint nameOffset_;

  [NullTerminatedString]
  [RAtPosition(nameof(nameOffset_))]
  public string Name { get; set; }

  private uint dataOffset_;
  private uint dataLength_;

  [RAtPosition(nameof(dataOffset_))]
  [RSequenceLengthSource(nameof(dataLength_))]
  public byte[] Data { get; set; }

  public ushort Width { get; set; }
  public ushort Height { get; set; }

  public uint Parameters { get; set; }

  [Skip]
  public TextureType TextureType
    => (TextureType) this.Parameters.ExtractFromRight(26, 3);

  [Skip]
  public bool UseTransparentColor0 => this.Parameters.GetBit(29);
}