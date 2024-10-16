﻿using fin.math;
using fin.model;
using fin.model.util;
using fin.util.image;


namespace fin.ui.rendering.gl.model;

public class MergedMaterialAcrossMeshesRenderer : IModelRenderer {
  private GlBufferManager? bufferManager_;
  private readonly IReadOnlyTextureTransformManager? textureTransformManager_;
  private IReadOnlyMesh selectedMesh_;

  private (IReadOnlyMesh, MergedMaterialPrimitivesAcrossMeshesRenderer)[]
      materialMeshRenderers_ = [];

  public MergedMaterialAcrossMeshesRenderer(
      IReadOnlyModel model,
      IReadOnlyTextureTransformManager? textureTransformManager = null) {
    this.Model = model;
    this.textureTransformManager_ = textureTransformManager;

    SelectedMeshService.OnMeshSelected += selectedMesh
        => this.selectedMesh_ = selectedMesh;
  }

  // Generates buffer manager and model within the current GL context.
  private void GenerateModelIfNull_() {
    if (this.bufferManager_ != null) {
      return;
    }

    this.bufferManager_ = new GlBufferManager(this.Model);

    var primitiveQueue = new PrimitiveRenderPriorityList();
    foreach (var mesh in this.Model.Skin.Meshes) {
      foreach (var primitive in mesh.Primitives) {
        primitiveQueue.Add(mesh,
                           primitive,
                           primitive.InversePriority,
                           primitive.Material?.TransparencyType ??
                           TransparencyType.OPAQUE);
      }
    }

    var primitiveMerger = new PrimitiveMerger();
    var allMaterialMeshRenderers
        = new List<(IReadOnlyMesh, MergedMaterialPrimitivesAcrossMeshesRenderer
            )>();
    Action<IReadOnlyMesh, IReadOnlyMaterial, IList<IReadOnlyPrimitive>>
        addMergedPrimitiveMesh = (mesh, material, primitives) => {
          if (primitives.Count == 0) {
            return;
          }

          if (!primitiveMerger.TryToMergePrimitives(
                  primitives,
                  out var mergedPrimitives)) {
            return;
          }

          allMaterialMeshRenderers.Add(
              (mesh,
               new MergedMaterialPrimitivesAcrossMeshesRenderer(
                   this.textureTransformManager_,
                   this.bufferManager_,
                   this.Model,
                   material,
                   mergedPrimitives)));
        };

    IReadOnlyMesh? currentMesh = null;
    IReadOnlyMaterial? currentMaterial = null;
    var currentPrimitives = new List<IReadOnlyPrimitive>();
    foreach (var (mesh, primitive) in primitiveQueue) {
      if (currentMesh != mesh || currentMaterial != primitive.Material) {
        addMergedPrimitiveMesh(currentMesh!,
                               currentMaterial!,
                               currentPrimitives);

        currentMesh = mesh;
        currentMaterial = primitive.Material;
        currentPrimitives.Clear();
      }

      currentPrimitives.Add(primitive);
    }

    addMergedPrimitiveMesh(currentMesh!, currentMaterial!, currentPrimitives);

    this.materialMeshRenderers_ = allMaterialMeshRenderers.ToArray();
  }

  ~MergedMaterialAcrossMeshesRenderer() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    foreach (var (_, materialMeshRenderer) in this.materialMeshRenderers_) {
      materialMeshRenderer.Dispose();
    }

    this.bufferManager_?.Dispose();
  }

  public IReadOnlyModel Model { get; }

  public IReadOnlySet<IReadOnlyMesh>? HiddenMeshes { get; set; }

  public void Render() {
    this.GenerateModelIfNull_();

    foreach (var (mesh, materialMeshRenderer) in this.materialMeshRenderers_) {
      if (this.HiddenMeshes?.Contains(mesh) ?? false) {
        continue;
      }

      var isSelected = this.selectedMesh_ == mesh;

      if (isSelected) {
        GlUtil.RenderOutline(materialMeshRenderer.Render);
      }

      materialMeshRenderer.Render();

      if (isSelected) {
        GlUtil.RenderHighlight(materialMeshRenderer.Render);
      }
    }
  }
}