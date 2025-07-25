﻿using fin.schema.color;

using schema.binary;
using schema.binary.attributes;


namespace marioartist.schema;

[BinarySchema]
[Endianness(Endianness.BigEndian)]
public partial class Tstlt : IBinaryDeserializable {
  public Argb1555Image Thumbnail { get; } = new Argb1555Image(24, 24);

  [SequenceLengthSource(12)]
  public uint[] Unk { get; private set; }

  public Argb1555Image FaceTextures { get; } = new Argb1555Image(128, 141);

  public AnotherHeader AnotherHeader { get; } = new();
}

[BinarySchema]
public partial class AnotherHeader : IBinaryDeserializable {
  public uint unkCount0;
  public uint unkCount1;
  public uint unk2;
  public Rgba32 SkinColor { get; set; }
}
