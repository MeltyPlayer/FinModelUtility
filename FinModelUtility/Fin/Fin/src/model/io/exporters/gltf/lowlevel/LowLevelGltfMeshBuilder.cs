﻿using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using fin.color;
using fin.math;

using SharpGLTF.Memory;
using SharpGLTF.Schema2;

using FinPrimitiveType = fin.model.PrimitiveType;
using GltfPrimitiveType = SharpGLTF.Schema2.PrimitiveType;


namespace fin.model.io.exporters.gltf.lowlevel {
  public class LowLevelGltfMeshBuilder {
    public bool UvIndices { get; set; }

    public IList<Mesh> BuildAndBindMesh(
        ModelRoot gltfModel,
        IModel model,
        float scale,
        Dictionary<IMaterial, (IList<byte>, Material)>
            finToTexCoordAndGltfMaterial) {
      var skin = model.Skin;
      var vertexAccessor = ConsistentVertexAccessor.GetAccessorForModel(model);

      var boneTransformManager = new BoneTransformManager();
      var boneToIndex = boneTransformManager.CalculateMatrices(
          model.Skeleton.Root,
          model.Skin.BoneWeights,
          null);
      boneTransformManager.InitModelVertices(model);

      var nullMaterial = gltfModel.CreateMaterial("null");
      nullMaterial.DoubleSided = false;
      nullMaterial.WithPBRSpecularGlossiness();

      var points = model.Skin.Vertices;
      var pointsCount = points.Count;

      var positionView = gltfModel.CreateBufferView(
          4 * 3 * pointsCount,
          0,
          BufferMode.ARRAY_BUFFER);
      var positionArray = new Vector3Array(positionView.Content);

      var normalView = gltfModel.CreateBufferView(
          4 * 3 * pointsCount,
          0,
          BufferMode.ARRAY_BUFFER);
      var normalArray = new Vector3Array(normalView.Content);

      for (var p = 0; p < pointsCount; ++p) {
        vertexAccessor.Target(points[p]);
        var point = vertexAccessor;

        boneTransformManager.ProjectVertexPositionNormal(
            point,
            out var outPosition,
            out var outNormal);
        var pos = positionArray[p];
        pos.X = outPosition.X * scale;
        pos.Y = outPosition.Y * scale;
        pos.Z = outPosition.Z * scale;
        positionArray[p] = pos;

        if (point.LocalNormal != null) {
          var norm = normalArray[p];

          norm.X = outNormal.X;
          norm.Y = outNormal.Y;
          norm.Z = outNormal.Z;
          normalArray[p] = norm;
        }

        /*if (point.Weights != null) {
          vertexBuilder = vertexBuilder.WithSkinning(
              point.Weights.Select(
                       boneWeight
                           => (boneToIndex[boneWeight.Bone],
                               boneWeight.Weight))
                   .ToArray());
        } else {
          vertexBuilder = vertexBuilder.WithSkinning(DEFAULT_SKINNING);
        }

        if (point.LocalNormal != null) {
          var tangent = point.LocalTangent;

          if (tangent == null) {
            vertexBuilder = vertexBuilder.WithGeometry(
                position,
                new Vector3(outNormal.X, outNormal.Y, outNormal.Z));
          } else {
            vertexBuilder = vertexBuilder.WithGeometry(
                position,
                new Vector3(outNormal.X, outNormal.Y, outNormal.Z),
                new Vector4(tangent.X, tangent.Y, tangent.Z, tangent.W));
          }
        }

        var finColor0 = point.GetColor(0);
        var hasColor0 = finColor0 != null;
        var assColor0 = hasColor0
                            ? LowLevelGltfMeshBuilder.FinToGltfColor_(
                                finColor0)
                            : new Vector4(1, 1, 1, 1);
        var finColor1 = point.GetColor(1);
        var hasColor1 = finColor1 != null;
        var assColor1 = hasColor1
                            ? LowLevelGltfMeshBuilder.FinToGltfColor_(
                                finColor1)
                            : new Vector4(1, 1, 1, 1);

        var hasColor = hasColor0 || hasColor1;

        var uvs = point.Uvs;
        var hasUvs = (uvs?.Count ?? 0) > 0;
        if (!this.UvIndices) {
          if (hasUvs) {
            var uv = uvs[0];
            vertexBuilder =
                vertexBuilder.WithMaterial(assColor0,
                                           assColor1,
                                           new Vector2(uv.U, uv.V));
          } else if (hasColor) {
            vertexBuilder =
                vertexBuilder.WithMaterial(assColor0, assColor1);
          }
        } else {
          // Importing the color directly via Assimp doesn't work for some
          // reason.
          vertexBuilder =
              vertexBuilder.WithMaterial(new Vector4(1, 1, 1, 1),
                                         new Vector2(
                                             hasUvs ? point.Index : -1,
                                             hasColor ? point.Index : -1));
        }

        vertices[p] = vertexBuilder;*/
      }

      var gltfMeshes = new List<Mesh>();
      foreach (var finMesh in skin.Meshes) {
        var gltfMesh = gltfModel.CreateMesh(finMesh.Name);

        foreach (var finPrimitive in finMesh.Primitives) {
          Material material;
          if (finPrimitive.Material != null) {
            (_, material) =
                finToTexCoordAndGltfMaterial[finPrimitive.Material];
          } else {
            material = nullMaterial;
          }

          var gltfPrimitive = gltfMesh.CreatePrimitive();
          gltfPrimitive.Material = material;

          // TODO: Use shared position/normal accessors?
          var positionAccessor = gltfModel.CreateAccessor();
          positionAccessor.SetVertexData(
              positionView,
              0,
              pointsCount);
          gltfPrimitive.SetVertexAccessor("POSITION", positionAccessor);

          var normalAccessor = gltfModel.CreateAccessor();
          normalAccessor.SetVertexData(
              normalView,
              0,
              pointsCount);
          gltfPrimitive.SetVertexAccessor("NORMAL", normalAccessor);

          if (finPrimitive.Type != FinPrimitiveType.QUADS &&
              finPrimitive.VertexOrder == VertexOrder.NORMAL) {
            gltfPrimitive.DrawPrimitiveType = finPrimitive.Type switch {
                FinPrimitiveType.TRIANGLES => GltfPrimitiveType.TRIANGLES,
                FinPrimitiveType.TRIANGLE_STRIP => GltfPrimitiveType
                    .TRIANGLE_STRIP,
                FinPrimitiveType.TRIANGLE_FAN => GltfPrimitiveType.TRIANGLE_FAN,
                FinPrimitiveType.LINES        => GltfPrimitiveType.LINES,
                FinPrimitiveType.POINTS       => GltfPrimitiveType.POINTS,
            };

            var finPrimitiveVertices = finPrimitive.Vertices;
            gltfPrimitive.SetIndexAccessor(
                CreateIndexAccessor_(
                    gltfModel,
                    finPrimitiveVertices.Select(vertex => vertex.Index)
                                        .ToArray()));
          } else {
            gltfPrimitive.DrawPrimitiveType = GltfPrimitiveType.TRIANGLES;

            var finTriangleVertexIndices =
                finPrimitive.GetOrderedTriangleVertexIndices().ToArray();
            gltfPrimitive.SetIndexAccessor(
                CreateIndexAccessor_(
                    gltfModel,
                    finTriangleVertexIndices));
            break;
          }
        }

        gltfMeshes.Add(gltfMesh);
      }

      // Vertex colors
      if (vertexAccessor.ColorCount > 0) {
        var colorView = gltfModel.CreateBufferView(
            4 * 4 * pointsCount,
            0,
            BufferMode.ARRAY_BUFFER);
        var colorArray = new ColorArray(colorView.Content);

        for (var p = 0; p < pointsCount; ++p) {
          vertexAccessor.Target(points[p]);
          var point = vertexAccessor;

          var finColor0 = point.GetColor(0);
          var hasColor0 = finColor0 != null;
          var assColor0 = hasColor0
              ? LowLevelGltfMeshBuilder.FinToGltfColor_(
                  finColor0)
              : new Vector4(1, 1, 1, 1);
          var col = colorArray[p];
          col.X = assColor0.X;
          col.Y = assColor0.Y;
          col.Z = assColor0.Z;
          col.W = assColor0.W;
          colorArray[p] = col;
        }

        var colorAccessor = gltfModel.CreateAccessor();
        colorAccessor.SetVertexData(
            colorView,
            0,
            pointsCount,
            DimensionType.VEC4);

        foreach (var gltfMesh in gltfMeshes) {
          foreach (var gltfPrimitive in gltfMesh.Primitives) {
            gltfPrimitive.SetVertexAccessor("COLOR_0", colorAccessor);
          }
        }
      }

      // UVs
      if (vertexAccessor.UvCount > 0) {
        var uvView = gltfModel.CreateBufferView(
            4 * 2 * pointsCount,
            0,
            BufferMode.ARRAY_BUFFER);
        var uvArray = new Vector2Array(uvView.Content);

        for (var p = 0; p < pointsCount; ++p) {
          vertexAccessor.Target(points[p]);
          var point = vertexAccessor;

          var finUv = point.GetUv(0);
          var uv = uvArray[p];
          uv.X = finUv.Value.U;
          uv.Y = finUv.Value.V;
          uvArray[p] = uv;
        }

        var uvAccessor = gltfModel.CreateAccessor();
        uvAccessor.SetVertexData(
            uvView,
            0,
            pointsCount,
            DimensionType.VEC2);

        foreach (var gltfMesh in gltfMeshes) {
          foreach (var gltfPrimitive in gltfMesh.Primitives) {
            gltfPrimitive.SetVertexAccessor("TEXCOORD_0", uvAccessor);
          }
        }
      }

      return gltfMeshes;
    }

    private static Vector4 FinToGltfColor_(IColor? color)
      => new(color?.Rf ?? 1, color?.Gf ?? 0, color?.Bf ?? 1, color?.Af ?? 1);

    private static Accessor CreateIndexAccessor_(
        ModelRoot gltfModelRoot,
        int[] vertexIndices) {
      int bytesPerIndex = vertexIndices.Max() switch {
          < byte.MaxValue   => 1,
          < ushort.MaxValue => 2,
          _                 => 4
      };

      var indexEncodingType = bytesPerIndex switch {
          1 => IndexEncodingType.UNSIGNED_BYTE,
          2 => IndexEncodingType.UNSIGNED_SHORT,
          4 => IndexEncodingType.UNSIGNED_INT,
      };

      var indexView = gltfModelRoot.CreateBufferView(
          bytesPerIndex * vertexIndices.Length,
          0,
          BufferMode.ELEMENT_ARRAY_BUFFER);
      var indexArray = new IntegerArray(indexView.Content, indexEncodingType);

      int i = 0;
      foreach (var v in vertexIndices) {
        indexArray[i++] = (uint) v;
      }

      var indexAccessor = gltfModelRoot.CreateAccessor();
      indexAccessor.SetIndexData(indexView,
                                 0,
                                 vertexIndices.Length,
                                 indexEncodingType);

      return indexAccessor;
    }
  }
}