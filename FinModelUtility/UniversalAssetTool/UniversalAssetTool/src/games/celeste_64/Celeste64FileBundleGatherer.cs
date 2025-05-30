﻿using Celeste64.api;

using fin.common;
using fin.io.bundles;
using fin.model.io.importers.gltf;
using fin.util.progress;

using fmod.api;

namespace uni.games.celeste_64;

public class Celeste64FileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingSubdir(
            Path.Join("celeste_64", ExtractorUtil.PREREQS),
            out var celeste64Dir)) {
      return;
    }

    var fileHierarchy
        = ExtractorUtil.GetFileHierarchy("celeste_64", celeste64Dir);
    var root = fileHierarchy.Root;

    foreach (var bankFile in root.FilesWithExtensionRecursive(".bank")) {
      organizer.Add(new BankAudioFileBundle(bankFile).Annotate(bankFile));
    }

    var modelDirectory = root.AssertGetExistingSubdir("Models");
    foreach (var glbFile in
             modelDirectory.FilesWithExtensionRecursive(".glb")) {
      organizer.Add(new GltfModelFileBundle(glbFile).Annotate(glbFile));
    }

    var textureDirectory = root.AssertGetExistingSubdir("Textures");
    foreach (var mapFile in root.AssertGetExistingSubdir("Maps")
                                .GetExistingFiles()) {
      organizer.Add(new Celeste64MapModelFileBundle {
          MapFile = mapFile,
          TextureDirectory = textureDirectory,
      }.Annotate(mapFile));
      organizer.Add(new Celeste64MapSceneFileBundle {
          MapFile = mapFile,
          ModelDirectory = modelDirectory,
          TextureDirectory = textureDirectory,
      }.Annotate(mapFile));
    }
  }
}