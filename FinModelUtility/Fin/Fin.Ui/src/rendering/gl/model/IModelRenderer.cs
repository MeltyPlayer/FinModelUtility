﻿using fin.model;

namespace fin.ui.rendering.gl.model;

public interface IModelRenderer : IRenderable, IDisposable {
  IReadOnlyModel Model { get; }
  IReadOnlySet<IReadOnlyMesh>? HiddenMeshes { get; set; }
}

public interface IDynamicModelRenderer : IModelRenderer {
  void UpdateBuffer();
}