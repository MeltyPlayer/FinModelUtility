﻿using bmd.exporter;

using fin.io;
using fin.model;
using fin.util.asserts;

using uni.platforms;
using uni.platforms.gcn;


namespace uni.games.super_mario_sunshine {
  public class
      SuperMarioSunshineModelFileGatherer : IModelFileGatherer<
          BmdModelFileBundle> {
    public IModelDirectory<BmdModelFileBundle>? GatherModelFileBundles(
        bool assert) {
      var superMarioSunshineRom =
          DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFile(
              "super_mario_sunshine.gcm");
      if (superMarioSunshineRom == null) {
        return null;
      }

      var options = GcnFileHierarchyExtractor.Options.Standard()
                                             .PruneRarcDumpNames("scene");
      var fileHierarchy =
          new GcnFileHierarchyExtractor()
              .ExtractFromRom(options, superMarioSunshineRom);

      var rootModelDirectory =
          new ModelDirectory<BmdModelFileBundle>("super_mario_sunshine");

      this.ExtractMario_(rootModelDirectory, fileHierarchy);
      this.ExtractFludd_(rootModelDirectory, fileHierarchy);
      this.ExtractYoshi_(rootModelDirectory, fileHierarchy);
      this.ExtractScenes_(rootModelDirectory, fileHierarchy);

      return rootModelDirectory;
    }

    private void ExtractMario_(
        ModelDirectory<BmdModelFileBundle> rootModelDirectory,
        IFileHierarchy fileHierarchy) {
      var marioSubdir =
          fileHierarchy.Root.TryToGetSubdir(@"data\mario");
      var bcxFiles = marioSubdir.TryToGetSubdir("bck")
                                .Files.Where(
                                    file => file.Name.StartsWith("ma_"))
                                .Select(file => file.Impl)
                                .ToArray();
      var bmdFile = marioSubdir.TryToGetSubdir("bmd")
                               .Files.Single(
                                   file => file.Name == "ma_mdl1.bmd");

      this.ExtractModels_(rootModelDirectory.AddSubdir("mario"),
                          new[] {bmdFile}, bcxFiles);
    }

    private void ExtractFludd_(
        ModelDirectory<BmdModelFileBundle> rootModelDirectory,
        IFileHierarchy fileHierarchy) {
      var fluddSubdir =
          fileHierarchy.Root.TryToGetSubdir(@"data\mario\watergun2");
      foreach (var subdir in fluddSubdir.Subdirs) {
        this.ExtractPrimaryAndSecondaryModels_(
            rootModelDirectory.AddSubdir("fludd"),
            subdir,
            file => file.Name.Contains("wg"));
      }
    }

    private void ExtractYoshi_(
        ModelDirectory<BmdModelFileBundle> rootModelDirectory,
        IFileHierarchy fileHierarchy) {
      var yoshiSubdir =
          fileHierarchy.Root.TryToGetSubdir(@"data\yoshi");
      var bcxFiles = yoshiSubdir
                     .Files.Where(
                         file => file.Extension == ".bck")
                     // TODO: Look into this, this animation seems to need extra bone(s)?
                     .Where(file => !file.Name.StartsWith("yoshi_tongue"))
                     .Select(file => file.Impl)
                     .ToArray();
      var bmdFile = yoshiSubdir.Files.Single(
          file => file.Name == "yoshi_model.bmd");

      this.ExtractModels_(rootModelDirectory.AddSubdir("yoshi"),
                          new[] {bmdFile}, bcxFiles);
    }

    private void ExtractScenes_(
        ModelDirectory<BmdModelFileBundle> rootModelDirectory,
        IFileHierarchy fileHierarchy) {
      var sceneSubdir =
          fileHierarchy.Root.TryToGetSubdir(@"data\scene");

      foreach (var subdir in sceneSubdir.Subdirs) {
        var sceneNode = rootModelDirectory.AddSubdir(subdir.Name);

        var mapSubdir = subdir.TryToGetSubdir("map");
        var bmdFiles = mapSubdir.TryToGetSubdir("map")
                                .Files.Where(file => file.Extension == ".bmd")
                                .ToArray();
        this.ExtractModels_(sceneNode.AddSubdir("map"), bmdFiles);

        var montemcommon =
            subdir.Subdirs.SingleOrDefault(
                subdir => subdir.Name == "montemcommon");
        var montewcommon =
            subdir.Subdirs.SingleOrDefault(
                subdir => subdir.Name == "montewcommon");
        var hamukurianm =
            subdir.Subdirs.SingleOrDefault(
                subdir => subdir.Name == "hamukurianm");

        foreach (var objectSubdir in subdir.Subdirs) {
          var objName = objectSubdir.Name;
          var objectNode = sceneNode.AddSubdir(objName);

          if (objName.StartsWith("montem") && !objName.Contains("common")) {
            this.ExtractFromSeparateDirectories_(
                objectNode, objectSubdir, Asserts.CastNonnull(montemcommon));
          } else if (objName.StartsWith("montew") &&
                     !objName.Contains("common")) {
            this.ExtractFromSeparateDirectories_(
                objectNode, objectSubdir, Asserts.CastNonnull(montewcommon));
          } else if (objName.StartsWith("hamukuri")) {
            if (!objName.Contains("anm")) {
              this.ExtractFromSeparateDirectories_(
                  objectNode, objectSubdir, Asserts.CastNonnull(hamukurianm));
            }
          } else {
            this.ExtractModelsAndAnimationsFromSceneObject_(
                objectNode, objectSubdir);
          }
        }
      }
    }

    private void ExtractFromSeparateDirectories_(
        IModelDirectory<BmdModelFileBundle> node,
        IFileHierarchyDirectory directory,
        IFileHierarchyDirectory common) {
      Asserts.Nonnull(common);

      var bmdFiles = directory.FilesWithExtension(".bmd")
                              .ToArray();
      var commonBcxFiles = common.FilesWithExtensions(".bca", ".bck")
                                 .Select(file => file.Impl)
                                 .ToArray();
      var commonBtiFiles = common.FilesWithExtension(".bti")
                                 .Select(file => file.Impl)
                                 .ToArray();

      var localBcxFiles = directory.FilesWithExtensions(".bca", ".bck")
                                   .Select(file => file.Impl)
                                   .ToArray();
      if (bmdFiles.Length == 1) {
        this.ExtractModels_(node,
                            bmdFiles,
                            commonBcxFiles.Concat(localBcxFiles).ToArray(),
                            commonBtiFiles);
        return;
      }

      try {
        Asserts.True(localBcxFiles.Length == 0);

        this.ExtractPrimaryAndSecondaryModels_(
            node,
            bmdFile => bmdFile.Name.StartsWith(
                "default"),
            bmdFiles,
            commonBcxFiles);
      } catch {
        ;
      }
    }

    private void ExtractModelsAndAnimationsFromSceneObject_(
        IModelDirectory<BmdModelFileBundle> node,
        IFileHierarchyDirectory directory) {
      var bmdFiles = directory.Files.Where(
                                  file => file.Extension == ".bmd")
                              .OrderByDescending(file => file.Name.Length)
                              .ToArray();

      var allBcxFiles = directory
                        .Files.Where(
                            file => file.Extension == ".bck" ||
                                    file.Extension == ".bca")
                        .Select(file => file.Impl)
                        .ToArray();

      var specialCase = false;
      if (allBcxFiles.Length == 1 &&
          allBcxFiles[0].Name == "fish_swim.bck" &&
          bmdFiles.All(file => file.Name.StartsWith("fish"))) {
        specialCase = true;
      }
      if (allBcxFiles.Length == 1 &&
          allBcxFiles[0].Name == "butterfly_fly.bck" &&
          bmdFiles.All(file => file.Name.StartsWith("butterfly"))) {
        specialCase = true;
      }
      if (allBcxFiles.All(file => file.Name.StartsWith("popo_")) &&
          bmdFiles.All(file => file.Name.StartsWith("popo"))) {
        specialCase = true;
      }

      // If there is only one model or 0 animations, it's easy to tell which
      // animations go with which model.
      if (bmdFiles.Length == 1 || allBcxFiles.Length == 0 || specialCase) {
        foreach (var bmdFile in bmdFiles) {
          this.ExtractModels_(node,
                              new[] {
                                  bmdFile
                              },
                              allBcxFiles);
        }
        return;
      }

      if (directory.Name == "montemcommon" ||
          directory.Name == "montewcommon") {
        foreach (var bmdFile in bmdFiles) {
          this.ExtractModels_(node,
                              new[] {
                                  bmdFile
                              });
        }
        return;
      }

      var unclaimedBcxFiles = allBcxFiles.ToHashSet();
      var bmdAndBcxFiles = new Dictionary<IFileHierarchyFile, IFile[]>();
      foreach (var bmdFile in bmdFiles) {
        var prefix = bmdFile.Name;
        prefix = prefix.Substring(0, prefix.Length - ".bmd".Length);

        // Blegh. These special cases are gross.
        {
          var modelIndex = prefix.IndexOf("_model");
          if (modelIndex != -1) {
            prefix = prefix.Substring(0, modelIndex);
          }

          var bodyIndex = prefix.IndexOf("_body");
          if (bodyIndex != -1) {
            prefix = prefix.Substring(0, bodyIndex);
          }

          prefix = prefix.Replace("peach_hair_ponytail", "peach_hair_pony");
          prefix = prefix.Replace("eggyoshi_normal", "eggyoshi");
        }

        var claimedBcxFiles = unclaimedBcxFiles
                              .Where(bcxFile => bcxFile.Name.StartsWith(prefix))
                              .ToArray();

        foreach (var claimedBcxFile in claimedBcxFiles) {
          unclaimedBcxFiles.Remove(claimedBcxFile);
        }

        bmdAndBcxFiles[bmdFile] = claimedBcxFiles;
      }
      //Asserts.True(unclaimedBcxFiles.Count == 0);
      foreach (var (bmdFile, bcxFiles) in bmdAndBcxFiles) {
        this.ExtractModels_(node, new[] {bmdFile}, bcxFiles);
      }
    }

    private void ExtractPrimaryAndSecondaryModels_(
        IModelDirectory<BmdModelFileBundle> node,
        IFileHierarchyDirectory directory,
        Func<IFileHierarchyFile, bool> primaryIdentifier
    ) {
      var bmdFiles = directory.Files.Where(
                                  file => file.Extension == ".bmd")
                              .ToArray();
      var bcxFiles = directory
                     .Files.Where(
                         file => file.Extension == ".bck" ||
                                 file.Extension == ".bca")
                     .Select(file => file.Impl)
                     .ToArray();

      this.ExtractPrimaryAndSecondaryModels_(node,
                                             primaryIdentifier,
                                             bmdFiles,
                                             bcxFiles);
    }

    private void ExtractPrimaryAndSecondaryModels_(
        IModelDirectory<BmdModelFileBundle> node,
        Func<IFileHierarchyFile, bool> primaryIdentifier,
        IReadOnlyList<IFileHierarchyFile> bmdFiles,
        IReadOnlyList<IFile>? bcxFiles = null
    ) {
      var primaryBmdFile =
          bmdFiles.Single(bmdFile => primaryIdentifier(bmdFile));
      this.ExtractModels_(node, new[] {primaryBmdFile}, bcxFiles);

      var secondaryBmdFiles = bmdFiles
                              .Where(bmdFile => !primaryIdentifier(bmdFile))
                              .ToArray();
      if (secondaryBmdFiles.Length > 0) {
        this.ExtractModels_(node, secondaryBmdFiles);
      }
    }

    private void ExtractModels_(
        IModelDirectory<BmdModelFileBundle> node,
        IReadOnlyList<IFileHierarchyFile> bmdFiles,
        IReadOnlyList<IFile>? bcxFiles = null,
        IReadOnlyList<IFile>? btiFiles = null
    ) {
      Asserts.True(bmdFiles.Count > 0);

      foreach (var bmdFile in bmdFiles) {
        node.AddFileBundle(new BmdModelFileBundle {
            BmdFile = bmdFile.Impl,
            BcxFiles = bcxFiles,
            BtiFiles = btiFiles,
            FrameRate = 60
        });
      }
    }
  }
}