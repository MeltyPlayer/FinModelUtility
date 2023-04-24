﻿using f3dzex2.io;

using fin.decompression;
using fin.util.asserts;
using fin.util.enumerables;

using SuperMario64.schema;

namespace SuperMario64.memory {
  public interface IReadOnlySm64Memory : IReadOnlyN64Memory {
    byte? AreaId { get; }
  }

  public interface ISm64Memory : IN64Memory, IReadOnlySm64Memory {
    new byte? AreaId { get; set; }
  }

  public class Sm64Memory : ISm64Memory {
    public byte? AreaId { get; set; }

    public Endianness Endianness => Endianness.BigEndian;
    public byte[] Bytes => ROM.Instance.Bytes;

    public IEnumerable<IEndianBinaryReader> OpenPossibilitiesAtSegmentedAddress(
        uint address)
      => this.OpenAtSegmentedAddress(address).Yield();

    public IEnumerable<IEndianBinaryReader> OpenPossibilitiesForSegment(uint segmentIndex) {
      throw new NotImplementedException();
    }

    public bool IsValidSegment(uint segmentIndex) {
      throw new NotImplementedException();
    }

    public bool IsValidSegmentedAddress(uint segmentedAddress) {
      throw new NotImplementedException();
    }

    public IEndianBinaryReader OpenAtSegmentedAddress(uint segmentedAddress) {
      IoUtils.SplitSegmentedAddress(segmentedAddress,
                                   out var segment,
                                   out var offset);
      var er = new EndianBinaryReader(
          Asserts.CastNonnull(ROM.Instance.getSegment(segment, this.AreaId)),
          SchemaConstants.SM64_ENDIANNESS);
      er.Position = offset;
      return er;
    }

    public IEndianBinaryReader OpenSegment(uint segmentIndex) {
      throw new NotImplementedException();
    }

    public bool IsSegmentCompressed(uint segmentIndex) {
      throw new NotImplementedException();
    }

    public void AddSegment(uint segmentIndex,
                           uint offset,
                           uint length,
                           IDecompressor? decompressor = null) {
      throw new NotImplementedException();
    }
  }
}