﻿using cmb.api;

using fin.io;
using fin.io.bundles;

using uni.platforms.threeDs;
using uni.util.bundles;

namespace uni.games.luigis_mansion_3d {
  public class LuigisMansion3dModelFileGatherer
      : IFileBundleGatherer<CmbModelFileBundle> {
    private readonly IModelSeparator separator_ =
        new ModelSeparator(directory => directory.LocalPath)
            .Register(@"\effect\effect_mdl", new PrefixModelSeparatorMethod())
            .Register(@"\model\dluige01",
                      new NameModelSeparatorMethod("Luigi.cmb"))
            .Register(@"\model\dluige02",
                      new NameModelSeparatorMethod("Luigi.cmb"))
            .Register(@"\model\dluige03",
                      new NameModelSeparatorMethod("Luigi.cmb"))
            .Register(@"\model\luige",
                      new NameModelSeparatorMethod("Luigi.cmb"));

    public IEnumerable<CmbModelFileBundle> GatherFileBundles() {
      if (!new ThreeDsFileHierarchyExtractor().TryToExtractFromGame(
              "luigis_mansion_3d",
              out var fileHierarchy)) {
        return Enumerable.Empty<CmbModelFileBundle>();
      }

      return fileHierarchy.SelectMany(this.ExtractModel_);
    }

    public IEnumerable<CmbModelFileBundle> ExtractModel_(
        IFileHierarchyDirectory subdir) {
      var cmbFiles = subdir.FilesWithExtension(".cmb").ToArray();
      if (cmbFiles.Length == 0) {
        return Enumerable.Empty<CmbModelFileBundle>();
      }

      var csabFiles = subdir.FilesWithExtension(".csab").ToArray();
      var ctxbFiles = subdir.FilesWithExtension(".ctxb").ToArray();
      var shpaFiles = subdir.FilesWithExtension(".shpa").ToArray();

      try {
        return this.separator_.Separate(subdir, cmbFiles, csabFiles)
                   .Select(bundle => new CmbModelFileBundle(
                               "luigis_mansion_3d",
                               bundle.ModelFile,
                               bundle.AnimationFiles.ToArray(),
                               ctxbFiles,
                               shpaFiles
                           ));
      } catch {
        return Enumerable.Empty<CmbModelFileBundle>();
      }
    }
  }
}