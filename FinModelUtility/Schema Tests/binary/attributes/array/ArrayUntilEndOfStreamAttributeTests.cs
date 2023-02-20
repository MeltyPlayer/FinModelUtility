﻿using NUnit.Framework;


namespace schema.binary.attributes.array {
  internal class ArrayUntilEndOfStreamAttribute {
    [Test] public void TestArrayUntilEndOfStream() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes.array;

namespace foo.bar {
  [BinarySchema]
  public partial class Wrapper : IBinaryConvertible {
    [ArrayUntilEndOfStream]
    public byte[] Field { get; set; }
  }
}",
                                     @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class Wrapper {
    public void Read(IEndianBinaryReader er) {
      {
        var temp = new List<byte>();
        while (!er.Eof) {
          temp.Add(er.ReadByte());
        }
        this.Field = temp.ToArray();
      }
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class Wrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteBytes(this.Field);
    }
  }
}
");
    }
  }
}