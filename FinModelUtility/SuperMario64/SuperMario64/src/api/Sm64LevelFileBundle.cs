﻿using fin.io;


namespace SuperMario64.api {
  public class Sm64LevelFileBundle : BSm64FileBundle, IUiFile {
    public Sm64LevelFileBundle(
        IFileHierarchyDirectory directory,
        ISystemFile sm64Rom,
        LevelId levelId) {
      this.Directory = directory;
      this.Sm64Rom = sm64Rom;
      this.LevelId = levelId;
    }

    public override IFileHierarchyFile? MainFile => null;
    public IFileHierarchyDirectory Directory { get; }

    public ISystemFile Sm64Rom { get; }
    public LevelId LevelId { get; }
    string IUiFile.BetterName => $"{LevelId}".ToLower();
    public string TrueFullName => this.Sm64Rom.FullName;
  }
}