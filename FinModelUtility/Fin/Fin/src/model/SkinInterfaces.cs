﻿using System.Collections.Generic;

using fin.color;
using fin.data;
using fin.math.matrix;
using fin.model.impl;
using System;
using System.Drawing;
using System.Numerics;

using fin.util.hash;


namespace fin.model {
  public interface ISkin {
    IReadOnlyList<IVertex> Vertices { get; }
    IVertex AddVertex(Position position);
    IVertex AddVertex(Vector3 position);
    IVertex AddVertex(IVector3 position);
    IVertex AddVertex(float x, float y, float z);

    IReadOnlyList<IMesh> Meshes { get; }
    IMesh AddMesh();

    IReadOnlyList<IBoneWeights> BoneWeights { get; }

    IBoneWeights GetOrCreateBoneWeights(
        VertexSpace vertexSpace,
        IBone bone);

    IBoneWeights GetOrCreateBoneWeights(
        VertexSpace vertexSpace,
        params IBoneWeight[] weights);

    IBoneWeights CreateBoneWeights(
        VertexSpace vertexSpace,
        params IBoneWeight[] weights);
  }

  public interface IMesh {
    string Name { get; set; }

    IReadOnlyList<IPrimitive> Primitives { get; }

    IPrimitive AddTriangles(params (IVertex, IVertex, IVertex)[] triangles);
    IPrimitive AddTriangles(params IVertex[] vertices);

    IPrimitive AddTriangleStrip(params IVertex[] vertices);
    IPrimitive AddTriangleFan(params IVertex[] vertices);

    IPrimitive AddQuads(params (IVertex, IVertex, IVertex, IVertex)[] quads);
    IPrimitive AddQuads(params IVertex[] vertices);

    ILinesPrimitive AddLines(params (IVertex, IVertex)[] lines);
    ILinesPrimitive AddLines(params IVertex[] lines);

    IPointsPrimitive AddPoints(params IVertex[] points);
  }


  public interface IBoneWeights : IIndexable, IEquatable<IBoneWeights> {
    VertexSpace VertexSpace { get; }
    IReadOnlyList<IBoneWeight> Weights { get; }

    bool Equals(VertexSpace vertexSpace, IReadOnlyList<IBoneWeight> weights);
  }

  public interface IBoneWeight {
    IBone Bone { get; }
    IReadOnlyFinMatrix4x4? InverseBindMatrix { get; }
    float Weight { get; }
  }

  public record BoneWeight(
    IBone Bone,
    // TODO: This should be moved to the bone interface instead.
    IReadOnlyFinMatrix4x4? InverseBindMatrix,
    float Weight) : IBoneWeight {

    public override int GetHashCode() {
      int hash = 216613626;
      var sub = 16780669;

      hash = hash * sub ^ Bone.Index.GetHashCode();
      if (InverseBindMatrix != null) {
        hash = hash * sub ^ InverseBindMatrix.GetHashCode();
      }
      hash = hash * sub ^ Weight.GetHashCode();
      
      return hash;
    }
  }

  public interface ITexCoord {
    float U { get; }
    float V { get; }
  }

  public enum VertexSpace {
    WORLD,
    WORLD_RELATIVE_TO_ROOT,
    BONE,
  }

  public interface IVertex : IIndexable {
    // TODO: Allow caching vertex builders directly on this type.

    IBoneWeights? BoneWeights { get; }
    IVertex SetBoneWeights(IBoneWeights boneWeights);

    Position LocalPosition { get; }
    IVertex SetLocalPosition(Position localPosition);
    IVertex SetLocalPosition(Vector3 localPosition);
    IVertex SetLocalPosition(IVector3 localPosition);
    IVertex SetLocalPosition(float x, float y, float z);

    Normal? LocalNormal { get; }
    IVertex SetLocalNormal(Normal? localNormal);
    IVertex SetLocalNormal(Vector3? localNormal);
    IVertex SetLocalNormal(IVector3? localNormal);
    IVertex SetLocalNormal(float x, float y, float z);

    Tangent? LocalTangent { get; }
    IVertex SetLocalTangent(Tangent? localTangent);
    IVertex SetLocalTangent(Vector4? localTangent);
    IVertex SetLocalTangent(IVector4? localTangent);
    IVertex SetLocalTangent(float x, float y, float z, float w);

    IVertexAttributeArray<IColor>? Colors { get; }
    IVertex SetColor(Color? color);
    IVertex SetColor(IColor? color);
    IVertex SetColor(Vector4? color);
    IVertex SetColor(IVector4? color);
    IVertex SetColor(int colorIndex, IColor? color);
    IVertex SetColorBytes(byte r, byte g, byte b, byte a);
    IVertex SetColorBytes(int colorIndex, byte r, byte g, byte b, byte a);
    IColor? GetColor();
    IColor? GetColor(int colorIndex);

    IVertexAttributeArray<ITexCoord>? Uvs { get; }
    IVertex SetUv(ITexCoord? uv);
    IVertex SetUv(Vector2? uv);
    IVertex SetUv(IVector2? uv);
    IVertex SetUv(float u, float v);
    IVertex SetUv(int uvIndex, ITexCoord? uv);
    IVertex SetUv(int uvIndex, float u, float v);
    ITexCoord? GetUv();
    ITexCoord? GetUv(int uvIndex);
  }

  public enum PrimitiveType {
    TRIANGLES,
    TRIANGLE_STRIP,
    TRIANGLE_FAN,
    QUADS,
    LINES,
    POINTS,
    // TODO: Other types.
  }

  public enum VertexOrder {
    NORMAL,
    FLIP,
  }

  public interface ILinesPrimitive : IPrimitive {
    float LineWidth { get; }
    ILinesPrimitive SetLineWidth(float width);
  }

  public interface IPointsPrimitive : IPrimitive {
    float Radius { get; }
    IPointsPrimitive SetRadius(float radius);
  }

  public interface IPrimitive {
    PrimitiveType Type { get; }
    IReadOnlyList<IVertex> Vertices { get; }

    IMaterial Material { get; }
    IPrimitive SetMaterial(IMaterial material);

    VertexOrder VertexOrder { get; }
    IPrimitive SetVertexOrder(VertexOrder vertexOrder);

    /// <summary>
    ///   Rendering priority when determining what order to draw in. Lower
    ///   values will be prioritized higher.
    /// </summary>
    uint InversePriority { get; }

    IPrimitive SetInversePriority(uint inversePriority);
  }
}