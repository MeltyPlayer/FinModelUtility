﻿using fin.data;
using fin.gl;
using fin.math;
using fin.model;
using fin.model.impl;
using fin.model.util;

using Tao.OpenGl;


namespace uni.ui.gl {
  /// <summary>
  ///   A renderer for a Fin model.
  /// </summary>
  public class ModelRenderer : IDisposable {
    private readonly BoneTransformManager boneTransformManager_;
    private readonly List<MaterialMeshRenderer> materialMeshRenderers_ = new();

    public ModelRenderer(IModel model,
                         BoneTransformManager boneTransformManager) {
      this.Model = model;
      this.boneTransformManager_ = boneTransformManager;

      var primitivesByMaterial = new ListDictionary<IMaterial, IPrimitive>();
      foreach (var mesh in model.Skin.Meshes) {
        foreach (var primitive in mesh.Primitives) {
          primitivesByMaterial.Add(primitive.Material, primitive);
        }
      }

      foreach (var (material, primitives) in primitivesByMaterial) {
        materialMeshRenderers_.Add(
            new MaterialMeshRenderer(
                this.boneTransformManager_,
                material,
                primitives));
      }
    }

    ~ModelRenderer() => ReleaseUnmanagedResources_();

    public void Dispose() {
      ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      foreach (var materialMeshRenderer in this.materialMeshRenderers_) {
        materialMeshRenderer.Dispose();
      }
      materialMeshRenderers_.Clear();
    }

    public IModel Model { get; }

    public void Render() {
      foreach (var materialMeshRenderer in this.materialMeshRenderers_) {
        materialMeshRenderer.Render();
      }
    }
  }

  /// <summary>
  ///   A renderer for all of the primitives of a Fin model with a common material.
  /// </summary>
  public class MaterialMeshRenderer : IDisposable {
    // TODO: Set up shader for material
    // TODO: Use material's textures

    private readonly BoneTransformManager boneTransformManager_;

    private readonly IMaterial material_;
    private readonly GlTexture? texture_;

    private readonly IList<IPrimitive> primitives_;

    public MaterialMeshRenderer(BoneTransformManager boneTransformManager,
                                IMaterial material,
                                IList<IPrimitive> primitives) {
      this.boneTransformManager_ = boneTransformManager;

      this.material_ = material;

      var finTexture = material.Textures.FirstOrDefault();
      this.texture_ = finTexture != null ? new GlTexture(finTexture) : null;

      this.primitives_ = primitives;
    }

    ~MaterialMeshRenderer() => ReleaseUnmanagedResources_();

    public void Dispose() {
      ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      this.texture_?.Dispose();
    }

    public void Render() {
      GlUtil.SetCulling(this.material_.CullingMode);
      this.texture_?.Bind();

      Gl.glBegin(Gl.GL_TRIANGLES);

      foreach (var primitive in this.primitives_) {
        var vertices = primitive.Vertices;
        var pointsCount = vertices.Count;

        switch (primitive.Type) {
          case PrimitiveType.TRIANGLES: {
            for (var v = 0; v < pointsCount; v += 3) {
              this.RenderVertex_(vertices[v + 0]);
              this.RenderVertex_(vertices[v + 2]);
              this.RenderVertex_(vertices[v + 1]);
            }
            break;
          }
          case PrimitiveType.TRIANGLE_STRIP: {
            for (var v = 0; v < pointsCount - 2; ++v) {
              IVertex v1, v2, v3;
              if (v % 2 == 0) {
                v1 = vertices[v + 0];
                v2 = vertices[v + 1];
                v3 = vertices[v + 2];
              } else {
                // Switches drawing order to maintain proper winding:
                // https://www.khronos.org/opengl/wiki/Primitive
                v1 = vertices[v + 1];
                v2 = vertices[v + 0];
                v3 = vertices[v + 2];
              }

              // Intentionally flipped to fix bug where faces were backwards.
              this.RenderVertex_(v1);
              this.RenderVertex_(v3);
              this.RenderVertex_(v2);
            }
            break;
          }
          case PrimitiveType.TRIANGLE_FAN: {
            // https://stackoverflow.com/a/8044252
            var firstVertex = vertices[0];
            for (var v = 2; v < pointsCount; ++v) {
              var v1 = firstVertex;
              var v2 = vertices[v - 1];
              var v3 = vertices[v];

              // Intentionally flipped to fix bug where faces were backwards.
              this.RenderVertex_(v1);
              this.RenderVertex_(v3);
              this.RenderVertex_(v2);
            }
            break;
          }
          case PrimitiveType.QUADS: {
            for (var v = 0; v < pointsCount; v += 4) {
              this.RenderVertex_(vertices[v + 0]);
              this.RenderVertex_(vertices[v + 1]);
              this.RenderVertex_(vertices[v + 2]);
              this.RenderVertex_(vertices[v + 3]);
            }
            break;
          }
        }
      }

      Gl.glEnd();

      this.texture_?.Unbind();
    }

    private readonly IPosition position_ = new ModelImpl.PositionImpl();
    private readonly INormal normal_ = new ModelImpl.NormalImpl();

    private void RenderVertex_(IVertex vertex) {
      // TODO: Load in the matrix instead, so we can perform projection on the GPU.
      this.boneTransformManager_.ProjectVertex(
          vertex, position_, normal_, true);

      var color = vertex.GetColor();
      if (color != null) {
        Gl.glColor4f(color.Rf, color.Gf, color.Bf, color.Af);
      }

      var uv = vertex.GetUv();
      if (uv != null) {
        Gl.glTexCoord2f(uv.U, uv.V);
      }

      Gl.glNormal3f(normal_.X, normal_.Y, normal_.Z);
      Gl.glVertex3f(position_.X, position_.Y, position_.Z);
    }
  }
}