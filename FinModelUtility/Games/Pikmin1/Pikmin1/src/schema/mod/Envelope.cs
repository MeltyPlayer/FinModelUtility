﻿using schema.binary;
using schema.binary.attributes;

namespace pikmin1.schema.mod;

[BinarySchema]
public partial class IndexAndWeight : IBinaryConvertible {
  public ushort index;
  public float weight;
}

[BinarySchema]
public partial class Envelope : IBinaryConvertible {
  [SequenceLengthSource(SchemaIntegerType.UINT16)]
  public IndexAndWeight[] indicesAndWeights;
}