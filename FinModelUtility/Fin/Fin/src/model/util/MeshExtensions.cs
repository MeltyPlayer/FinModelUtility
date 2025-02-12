﻿using System;
using System.Numerics;

using fin.math;
using fin.math.floats;
using fin.math.matrix.two;

namespace fin.model.util;

public static class MeshExtensions {
  public static void AddSimpleQuad<TVertex>(
      this IMesh mesh,
      ISkin<TVertex> skin,
      Vector3 ul,
      Vector3 ur,
      Vector3 lr,
      Vector3 ll,
      IReadOnlyMaterial? material = null,
      IReadOnlyBone? bone = null,
      (float, float)? repeat = null)
      where TVertex : INormalVertex, ISingleUvVertex
    => mesh.AddSimpleQuad(
        skin,
        (ul, new Vector2(0, 0)),
        (ur, new Vector2(repeat?.Item1 ?? 1, 0)),
        (lr, new Vector2(repeat?.Item1 ?? 1, repeat?.Item2 ?? 1)),
        (ll, new Vector2(0, repeat?.Item2 ?? 1)),
        material,
        bone);

  public static void AddSimpleQuad<TVertex>(
      this IMesh mesh,
      ISkin<TVertex> skin,
      (Vector3, Vector2) ulP,
      (Vector3, Vector2) urP,
      (Vector3, Vector2) lrP,
      (Vector3, Vector2) llP,
      IReadOnlyMaterial? material = null,
      IReadOnlyBone? bone = null)
      where TVertex : INormalVertex, ISingleUvVertex {
    var (ul, ulUv) = ulP;
    var (ur, urUv) = urP;
    var (lr, lrUv) = lrP;
    var (ll, llUv) = llP;

    var a = ul;
    var b = ur;
    var c = lr;

    var normal = Vector3.Cross(b - a, c - a);
    normal = Vector3.Normalize(normal);

    var vUl = skin.AddVertex(ul);
    vUl.SetUv(ulUv);
    vUl.SetLocalNormal(normal);

    var vUr = skin.AddVertex(ur);
    vUr.SetUv(urUv);
    vUr.SetLocalNormal(normal);

    var vLr = skin.AddVertex(lr);
    vLr.SetUv(lrUv);
    vLr.SetLocalNormal(normal);

    var vLl = skin.AddVertex(ll);
    vLl.SetUv(llUv);
    vLl.SetLocalNormal(normal);

    if (bone != null) {
      var boneWeights
          = skin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE, bone);
      vUl.SetBoneWeights(boneWeights);
      vUr.SetBoneWeights(boneWeights);
      vLr.SetBoneWeights(boneWeights);
      vLl.SetBoneWeights(boneWeights);
    }

    mesh.AddQuads(vUl, vUr, vLr, vLl).SetMaterial(material);
  }

  public static void AddSimpleWall<TVertex>(
      this IMesh mesh,
      ISkin<TVertex> skin,
      Vector3 point1,
      Vector3 point2,
      IReadOnlyMaterial? material = null,
      IReadOnlyBone? bone = null,
      (float, float)? repeat = null)
      where TVertex : INormalVertex, ISingleUvVertex {
    var ul = (point1, new Vector2(0, 0));
    var ur = (point1 with { X = point2.X, Y = point2.Y },
              new Vector2(repeat?.Item1 ?? 1, 0));
    var lr = (point2, new Vector2(repeat?.Item1 ?? 1, repeat?.Item2 ?? 1));
    var ll = (point2 with { X = point1.X, Y = point1.Y },
              new Vector2(0, repeat?.Item2 ?? 1));

    mesh.AddSimpleQuad(skin, ul, ur, lr, ll, material, bone);
    mesh.AddSimpleQuad(skin, ur, ul, ll, lr, material, bone);
  }

  public static void AddSimpleFloor<TVertex>(
      this IMesh mesh,
      ISkin<TVertex> skin,
      Vector3 point1,
      Vector3 point2,
      IReadOnlyMaterial? material = null,
      IReadOnlyBone? bone = null,
      (float, float)? repeat = null)
      where TVertex : INormalVertex, ISingleUvVertex {
    var ul = (point1, new Vector2(0, 0));
    var ur = (point1 with { X = point2.X, Z = point2.Z },
              new Vector2(repeat?.Item1 ?? 1, 0));
    var lr = (point2, new Vector2(repeat?.Item1 ?? 1, repeat?.Item2 ?? 1));
    var ll = (point2 with { X = point1.X, Z = point1.Z },
              new Vector2(0, repeat?.Item2 ?? 1));

    mesh.AddSimpleQuad(skin, ul, ur, lr, ll, material, bone);
    mesh.AddSimpleQuad(skin, ur, ul, ll, lr, material, bone);
  }

  public static void AddSimpleCube<TVertex>(
      this IMesh mesh,
      ISkin<TVertex> skin,
      Vector3 point1,
      Vector3 point2,
      IReadOnlyMaterial? material = null,
      IReadOnlyBone? bone = null,
      (float, float, float)? repeat = null)
      where TVertex : INormalVertex, ISingleUvVertex {
    var tUl = point1;
    var tUr = point1 with { X = point2.X };
    var tLr = point1 with { X = point2.X, Y = point2.Y };
    var tLl = point1 with { Y = point2.Y };

    var bUl = point2 with { X = point1.X, Y = point1.Y };
    var bUr = point2 with { Y = point1.Y };
    var bLr = point2;
    var bLl = point2 with { X = point1.X };

    var sameX = point1.X.IsRoughly(point2.X);
    var sameY = point1.Y.IsRoughly(point2.Y);
    var sameZ = point1.Z.IsRoughly(point2.Z);

    var (xRepeat, yRepeat, zRepeat) = repeat ?? (1, 1, 1);

    if (!sameX && !sameY) {
      var topBottomRepeat = (xRepeat, yRepeat);

      // Top
      mesh.AddSimpleQuad(skin,
                         tUl,
                         tUr,
                         tLr,
                         tLl,
                         material,
                         bone,
                         topBottomRepeat);
      // Bottom
      mesh.AddSimpleQuad(skin,
                         bUr,
                         bUl,
                         bLl,
                         bLr,
                         material,
                         bone,
                         topBottomRepeat);
    }

    if (!sameX && !sameZ) {
      var frontBackRepeat = (xRepeat, zRepeat);

      // Front
      mesh.AddSimpleQuad(skin,
                         tLl,
                         tLr,
                         bLr,
                         bLl,
                         material,
                         bone,
                         frontBackRepeat);
      // Back
      mesh.AddSimpleQuad(skin,
                         tUr,
                         tUl,
                         bUl,
                         bUr,
                         material,
                         bone,
                         frontBackRepeat);
    }

    if (!sameY && !sameZ) {
      var leftRightRepeat = (yRepeat, zRepeat);

      // Left
      mesh.AddSimpleQuad(skin,
                         tUl,
                         tLl,
                         bLl,
                         bUl,
                         material,
                         bone,
                         leftRightRepeat);
      // Right
      mesh.AddSimpleQuad(skin,
                         tLr,
                         tUr,
                         bUr,
                         bLr,
                         material,
                         bone,
                         leftRightRepeat);
    }
  }

  public static void AddSimpleCylinder<TVertex>(
      this IMesh mesh,
      ISkin<TVertex> skin,
      Vector3 point1,
      Vector3 point2,
      int steps,
      IReadOnlyMaterial? material = null,
      IReadOnlyBone? bone = null,
      (float, float)? repeat = null)
      where TVertex : INormalVertex, ISingleUvVertex {
    var center = (point1.Xy() + point2.Xy()) / 2;
    var xRadius = MathF.Abs(point2.X - point1.X) / 2;
    var yRadius = MathF.Abs(point2.Y - point1.Y) / 2;

    var z1 = point1.Z;
    var z2 = point2.Z;

    var v1 = 0;
    var v2 = repeat?.Item2 ?? 1;

    for (var i = 0; i < steps; ++i) {
      float frac1 = (1f * i / steps);
      float frac2 = (1f * (i + 1) / steps);

      var angle1 = 2 * MathF.PI * frac1;
      var angle2 = 2 * MathF.PI * frac2;

      var xy1 = center + SystemVector2Util.FromRadians(angle1) * new Vector2(xRadius, yRadius);
      var xy2 = center + SystemVector2Util.FromRadians(angle2) * new Vector2(xRadius, yRadius);

      var u1 = frac1 * (repeat?.Item1 ?? 1);
      var u2 = frac2 * (repeat?.Item1 ?? 1);

      var ul = (new Vector3(xy1, z1), new Vector2(u1, v1));
      var ur = (new Vector3(xy2, z1), new Vector2(u2, v1));
      var lr = (new Vector3(xy2, z2), new Vector2(u2, v2));
      var ll = (new Vector3(xy1, z2), new Vector2(u1, v2));

      mesh.AddSimpleQuad(skin, ul, ur, lr, ll, material, bone);
      mesh.AddSimpleQuad(skin, ur, ul, ll, lr, material, bone);
    }
  }
}