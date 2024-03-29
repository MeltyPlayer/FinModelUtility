﻿using cmb.api;

using fin.io;
using fin.io.bundles;

using uni.platforms.threeDs;
using uni.util.bundles;
using uni.util.io;

namespace uni.games.majoras_mask_3d {
  using IAnnotatedCmbBundle = IAnnotatedFileBundle<CmbModelFileBundle>;

  public class MajorasMask3dFileBundleGatherer
      : IAnnotatedFileBundleGatherer<CmbModelFileBundle> {
    private readonly IModelSeparator separator_
        = new ModelSeparator(directory => directory.Name)
          .Register(new AllAnimationsModelSeparatorMethod(),
                    "zelda_cow",
                    "zelda2_jso")
          .Register(new SameNameSeparatorMethod(), "zelda2_zoraband")
          .Register(new PrimaryModelSeparatorMethod("eyegoal.cmb"),
                    "zelda2_eg");


    public IEnumerable<IAnnotatedCmbBundle> GatherFileBundles() {
      if (!new ThreeDsFileHierarchyExtractor().TryToExtractFromGame(
              "majoras_mask_3d",
              out var fileHierarchy)) {
        return Enumerable.Empty<IAnnotatedCmbBundle>();
      }

      return new AnnotatedFileBundleGathererAccumulatorWithInput<
                 CmbModelFileBundle,
                 IFileHierarchy>(fileHierarchy)
             .Add(this.GetAutomaticModels_)
             .Add(this.GetModelsViaSeparator_)
             .Add(this.GetLinkModels_)
             .GatherFileBundles();
    }

    private IEnumerable<IAnnotatedCmbBundle> GetAutomaticModels_(
        IFileHierarchy fileHierarchy) {
      var actorsDir = fileHierarchy.Root.AssertGetExistingSubdir("actors");

      foreach (var actorDir in actorsDir.GetExistingSubdirs()) {
        if (actorDir.Name.StartsWith("zelda2_link_")) {
          continue;
        }

        var animations =
            actorDir.FilesWithExtensionRecursive(".csab").ToArray();
        var models = actorDir.FilesWithExtensionRecursive(".cmb").ToArray();

        if (models.Length == 1 || animations.Length == 0) {
          foreach (var model in models) {
            yield return new CmbModelFileBundle(
                "majoras_mask_3d",
                model,
                animations,
                null,
                null).Annotate(model);
          }
        }
      }
    }


    private IEnumerable<IAnnotatedCmbBundle> GetModelsViaSeparator_(
        IFileHierarchy fileHierarchy)
      => new FileHierarchyAssetBundleSeparator<CmbModelFileBundle>(
          fileHierarchy,
          subdir => {
            if (!separator_.Contains(subdir)) {
              return Enumerable.Empty<IAnnotatedCmbBundle>();
            }

            var cmbFiles =
                subdir.FilesWithExtensionsRecursive(".cmb").ToArray();
            if (cmbFiles.Length == 0) {
              return Enumerable.Empty<IAnnotatedCmbBundle>();
            }

            var csabFiles =
                subdir.FilesWithExtensionsRecursive(".csab").ToArray();
            var ctxbFiles =
                subdir.FilesWithExtensionsRecursive(".ctxb").ToArray();

            try {
              var bundles =
                  this.separator_.Separate(subdir, cmbFiles, csabFiles);
              return bundles.Select(bundle => new CmbModelFileBundle(
                                        "majoras_mask_3d",
                                        bundle.ModelFile,
                                        bundle.AnimationFiles.ToArray(),
                                        ctxbFiles,
                                        null
                                    ).Annotate(bundle.ModelFile));
            } catch {
              return Enumerable.Empty<IAnnotatedCmbBundle>();
            }
          }
      ).GatherFileBundles();

    private IEnumerable<IAnnotatedCmbBundle> GetLinkModels_(
        IFileHierarchy fileHierarchy) {
      var actorsDir = fileHierarchy.Root.AssertGetExistingSubdir("actors");

      var modelsAndAnimations = new[] {
          ("zelda2_link_boy_new/boy/model/link_demon.cmb", "boy"),
          ("zelda2_link_child_new/child/model/link_child.cmb", "child"),
          ("zelda2_link_goron_new/goron/model/link_goron.cmb", "goron"),
          ("zelda2_link_nuts_new/nuts/model/link_deknuts.cmb", "nuts"),
          ("zelda2_link_zora_new/zora/model/link_zora.cmb", "zora"),
      };

      return modelsAndAnimations.Select(modelPathAndAnimationDir => {
        var (modelPath, animationDir) = modelPathAndAnimationDir;

        var cmbFile = actorsDir.AssertGetExistingFile(modelPath);
        var csabFiles = fileHierarchy
                        .Root.AssertGetExistingSubdir(
                            $"actors/zelda2_link_new/{animationDir}/anim")
                        .FilesWithExtension(".csab")
                        .ToArray();

        return new CmbModelFileBundle(
            "majoras_mask_3d",
            cmbFile,
            csabFiles,
            null,
            null).Annotate(cmbFile);
      });
    }
  }
}