﻿using fin.data.dictionaries;
using fin.math;
using fin.model;
using fin.model.util;
using fin.shaders.glsl;
using fin.ui.rendering.gl.material;
using fin.util.image;


namespace fin.ui.rendering.gl.model;

public partial class ModelRendererV2 {
  private class MergedMaterialByMeshRenderer : IDynamicModelRenderer {
    private readonly bool dynamic_;
    private readonly IReadOnlyTextureTransformManager? textureTransformManager_;

    private IGlBufferManager? bufferManager_;
    private IDynamicGlBufferManager? dynamicBufferManager_;
    private IReadOnlyMesh selectedMesh_;

    private (IReadOnlyMesh, MergedMaterialPrimitivesByMeshRenderer[])[]
        materialMeshRenderers_ = [];

    public MergedMaterialByMeshRenderer(
        IReadOnlyModel model,
        IReadOnlyTextureTransformManager? textureTransformManager = null,
        bool dynamic = false) {
      this.Model = model;
      this.textureTransformManager_ = textureTransformManager;
      this.dynamic_ = dynamic;

      SelectedMeshService.OnMeshSelected += selectedMesh
          => this.selectedMesh_ = selectedMesh;
    }

    // Generates buffer manager and model within the current GL context.
    public void GenerateModelIfNull() {
      if (this.bufferManager_ != null) {
        return;
      }

      var modelRequirements = ModelRequirements.FromModel(this.Model);

      if (!this.dynamic_) {
        this.bufferManager_
            = GlBufferManager.CreateStatic(this.Model, modelRequirements);
      } else {
        this.bufferManager_ = this.dynamicBufferManager_
            = GlBufferManager.CreateDynamic(this.Model, modelRequirements);
      }

      var allMaterialMeshRenderers =
          new List<(IReadOnlyMesh, MergedMaterialPrimitivesByMeshRenderer[])>();

      // TODO: Optimize this with something like a "MinMap"?
      var meshQueue = new RenderPriorityOrderedSet<IReadOnlyMesh>();
      foreach (var mesh in this.Model.Skin.Meshes) {
        foreach (var primitive in mesh.Primitives) {
          meshQueue.Add(mesh,
                        primitive.InversePriority,
                        (primitive.Material?.TransparencyType ??
                         TransparencyType.OPAQUE) ==
                        TransparencyType.TRANSPARENT);
        }
      }

      var primitiveMerger = new PrimitiveMerger();
      foreach (var mesh in meshQueue) {
        var materialQueue = new RenderPriorityOrderedSet<IReadOnlyMaterial?>();
        var primitivesByMaterial
            = new ListDictionary<IReadOnlyMaterial?, IReadOnlyPrimitive>(
                new NullFriendlyDictionary<IReadOnlyMaterial?,
                    IList<IReadOnlyPrimitive>>());
        foreach (var primitive in mesh.Primitives) {
          primitivesByMaterial.Add(primitive.Material, primitive);
          materialQueue.Add(
              primitive.Material,
              primitive.InversePriority,
              (primitive.Material?.TransparencyType ??
               TransparencyType.OPAQUE) ==
              TransparencyType.TRANSPARENT);
        }

        var materialMeshRenderers =
            new ListDictionary<IReadOnlyMesh,
                MergedMaterialPrimitivesByMeshRenderer>();
        foreach (var material in materialQueue) {
          var primitives = primitivesByMaterial[material];
          if (!primitiveMerger.TryToMergePrimitives(
                  primitives,
                  out var mergedPrimitives)) {
            continue;
          }

          materialMeshRenderers.Add(
              mesh,
              new MergedMaterialPrimitivesByMeshRenderer(
                  this.textureTransformManager_,
                  this.bufferManager_,
                  this.Model,
                  modelRequirements,
                  material,
                  mergedPrimitives));
        }

        allMaterialMeshRenderers.AddRange(
            materialMeshRenderers
                .GetPairs()
                .Select(tuple => (tuple.key, tuple.value.ToArray())));
      }

      this.materialMeshRenderers_ = allMaterialMeshRenderers.ToArray();
    }

    ~MergedMaterialByMeshRenderer() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      foreach (var (_, materialMeshRenderers) in this.materialMeshRenderers_) {
        foreach (var materialMeshRenderer in materialMeshRenderers) {
          materialMeshRenderer.Dispose();
        }
      }

      this.bufferManager_?.Dispose();
    }

    public IReadOnlyModel Model { get; }

    public IReadOnlySet<IReadOnlyMesh>? HiddenMeshes { get; set; }

    public void UpdateBuffer() => this.dynamicBufferManager_?.UpdateBuffer();

    public void Render() {
      this.GenerateModelIfNull();

      foreach (var (mesh, materialMeshRenderers) in
               this.materialMeshRenderers_) {
        if (this.HiddenMeshes?.Contains(mesh) ?? false) {
          continue;
        }

        foreach (var materialMeshRenderer in materialMeshRenderers) {
          var isSelected = this.selectedMesh_ == mesh;

          materialMeshRenderer.Render();

          if (isSelected) {
            GlUtil.RenderHighlight(materialMeshRenderer.Render);
          }
        }
      }
    }

    public IEnumerable<IGlMaterialShader> GetMaterialShaders(
        IReadOnlyMaterial material)
      => this.materialMeshRenderers_.SelectMany(p => p.Item2)
             .Where(r => r.Material == material)
             .Select(r => r.MaterialShader);
  }
}