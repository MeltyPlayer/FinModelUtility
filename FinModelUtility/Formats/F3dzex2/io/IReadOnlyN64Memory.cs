﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

using fin.data;
using fin.data.dictionaries;
using fin.decompression;
using fin.util.asserts;
using fin.util.hex;

using schema.binary;

namespace f3dzex2.io {
  public interface IReadOnlyN64Memory {
    Endianness Endianness { get; }
    EndianBinaryReader OpenAtSegmentedAddress(uint segmentedAddress);

    IEnumerable<EndianBinaryReader> OpenPossibilitiesAtSegmentedAddress(
        uint segmentedAddress);

    bool TryToOpenPossibilitiesAtSegmentedAddress(
        uint segmentedAddress,
        out IEnumerable<EndianBinaryReader> possibilities);

    EndianBinaryReader OpenSegment(uint segmentIndex);
    EndianBinaryReader OpenSegment(Segment segment, uint? offset = null);

    IEnumerable<EndianBinaryReader> OpenPossibilitiesForSegment(
        uint segmentIndex);

    bool IsValidSegment(uint segmentIndex);
    bool IsValidSegmentedAddress(uint segmentedAddress);
    bool IsSegmentCompressed(uint segmentIndex);
  }

  public interface IN64Memory : IReadOnlyN64Memory {
    void AddSegment(uint segmentIndex,
                    uint offset,
                    uint length,
                    IArrayDecompressor? decompressor = null);

    void AddSegment(uint segmentIndex, Segment segment);
  }

  public class N64Memory : IN64Memory {
    private readonly byte[] bytes_;
    private readonly ListDictionary<uint, Segment> segments_ = new();

    public N64Memory(byte[] data, Endianness endianness) {
      this.bytes_ = data;
      this.Endianness = endianness;
    }

    public Endianness Endianness { get; }

    public EndianBinaryReader OpenAtSegmentedAddress(uint segmentedAddress)
      => this.OpenPossibilitiesAtSegmentedAddress(segmentedAddress).Single();

    public IEnumerable<EndianBinaryReader> OpenPossibilitiesAtSegmentedAddress(
        uint segmentedAddress) {
      Asserts.True(
          this.TryToOpenPossibilitiesAtSegmentedAddress(
              segmentedAddress,
              out var possibilities),
          $"Expected 0x{segmentedAddress.ToHex()} to be a valid segmented address.");
      return possibilities;
    }

    public bool TryToOpenPossibilitiesAtSegmentedAddress(
        uint segmentedAddress,
        out IEnumerable<EndianBinaryReader> possibilities) {
      if (!this.TryToGetSegmentsAtSegmentedAddress_(
              segmentedAddress,
              out var offset,
              out var validSegments)) {
        possibilities = default;
        return false;
      }

      possibilities =
          validSegments.Select(segment => this.OpenSegment(segment, offset));
      return true;
    }

    public EndianBinaryReader OpenSegment(uint segmentIndex)
      => this.OpenPossibilitiesForSegment(segmentIndex).Single();

    public EndianBinaryReader OpenSegment(Segment segment,
                                           uint? offset = null) {
      var er = new EndianBinaryReader(
          new MemoryStream(this.bytes_,
                           (int) segment.Offset,
                           (int) segment.Length),
          this.Endianness);
      if (offset != null) {
        er.Position = offset.Value;
      }

      return er;
    }

    public IEnumerable<EndianBinaryReader> OpenPossibilitiesForSegment(
        uint segmentIndex)
      => this
         .segments_[segmentIndex]
         .Select(segment => this.OpenSegment(segment));


    public bool IsValidSegment(uint segmentIndex)
      => this.segments_.HasList(segmentIndex);

    public bool IsValidSegmentedAddress(uint segmentedAddress) {
      IoUtils.SplitSegmentedAddress(segmentedAddress,
                                    out var segmentIndex,
                                    out var offset);
      if (!this.segments_.TryGetList(segmentIndex, out var segments)) {
        return false;
      }

      var offsetInSegment = offset;
      return segments!.Any(segment => offsetInSegment < segment.Length);
    }

    public bool IsSegmentCompressed(uint segmentIndex)
      => this.segments_[segmentIndex].Single().Decompressor != null;

    public void AddSegment(uint segmentIndex,
                           uint offset,
                           uint length,
                           IArrayDecompressor? decompressor = null)
      => this.AddSegment(segmentIndex,
                         new Segment {
                             Offset = offset,
                             Length = length,
                             Decompressor = decompressor,
                         });

    public void AddSegment(uint segmentIndex, Segment segment)
      => this.segments_.Add(segmentIndex, segment);

    private bool TryToGetSegmentsAtSegmentedAddress_(
        uint segmentedAddress,
        out uint offset,
        out IEnumerable<Segment> validSegments) {
      IoUtils.SplitSegmentedAddress(segmentedAddress,
                                    out var segmentIndex,
                                    out offset);
      var offsetInSegment = offset;

      if (!this.segments_.TryGetList(segmentIndex, out var segments)) {
        validSegments = default;
        return false;
      }

      validSegments =
          segments!.Where(segment => offsetInSegment < segment.Length);
      return segments!.Any();
    }
  }

  public readonly struct Segment {
    public required uint Offset { get; init; }
    public required uint Length { get; init; }
    public IArrayDecompressor? Decompressor { get; init; }
  }
}