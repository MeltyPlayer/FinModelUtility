﻿using fin.io;

namespace fin.model.io.importer.assimp {
  public class AssimpModelFileBundle : IModelFileBundle {
    public string? GameName { get; }
    public required IReadOnlyTreeFile MainFile { get; init; }
  }
}