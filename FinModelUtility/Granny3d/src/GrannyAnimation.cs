﻿using fin.schema.vector;

using schema;


namespace granny3d {
  public class GrannyAnimation : IGrannyAnimation, IDeserializable {
    public string Name { get; private set; }
    public float Duration { get; private set; }
    public float TimeStep { get; private set; }
    public float Oversampling { get; private set; }

    public IList<IGrannyTrackGroup> TrackGroups { get; } =
      new List<IGrannyTrackGroup>();

    public void Read(EndianBinaryReader er) {
      GrannyUtils.SubreadUInt64Pointer(
          er, ser => { this.Name = ser.ReadStringNT(); });

      this.Duration = er.ReadSingle();
      this.TimeStep = er.ReadSingle();
      this.Oversampling = er.ReadSingle();

      GrannyUtils.SubreadRefToArray(er, (ser, trackGroupCount) => {
        for (var i = 0; i < trackGroupCount; ++i) {
          var trackGroup = new GrannyTrackGroup();
          trackGroup.Read(ser);
          this.TrackGroups.Add(trackGroup);
        }
      });
    }
  }

  public class GrannyTrackGroup : IGrannyTrackGroup, IDeserializable {
    public string Name { get; private set; }

    public GrannyTransform InitialPlacement { get; } = new();
    public Vector3f LoopTranslation { get; } = new();
    public GrannyVariant ExtendedData { get; } = new();

    public void Read(EndianBinaryReader er) {
      GrannyUtils.SubreadUInt64Pointer(
          er, ser => this.Name = ser.ReadStringNT());

      // TODO: vector tracks header
      /*GrannyUtils.SubreadRefToArray(
          er, (ser, count) => { });
      // TODO: transform tracks header
      GrannyUtils.SubreadRefToArray(
          er, (ser, count) => { });
      // TODO: transform lod errors header
      GrannyUtils.SubreadRefToArray(
          er, (ser, count) => { });
      // TODO: text tracks header
      GrannyUtils.SubreadRefToArray(
          er, (ser, count) => { });

      this.InitialPlacement.Read(er);
      var flags = er.ReadUInt32();
      this.LoopTranslation.Read(er);
      // TODO: periodic loop ref
      GrannyUtils.SubreadUInt64Pointer(
          er, ser => { });
      // TODO: root motion ref
      GrannyUtils.SubreadUInt64Pointer(
          er, ser => { });
      this.ExtendedData.Read(er);*/
    }
  }

  public class GrannyVariant : IDeserializable {
    public void Read(EndianBinaryReader er) {
      // TODO: type
      GrannyUtils.SubreadUInt64Pointer(
          er, ser => { });
      // TODO: object
      GrannyUtils.SubreadUInt64Pointer(
          er, ser => { });
    }
  }
}