﻿using fin.io;
using fin.io.bundles;

using uni.platforms;

using UoT.api;
using UoT.memory;


namespace uni.games.ocarina_of_time {
  public class OcarinaOfTimeFileBundleGatherer
      : IFileBundleGatherer<OotModelFileBundle> {
    public IEnumerable<OotModelFileBundle> GatherFileBundles(
        bool assert) {
      if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFile(
              "ocarina_of_time.z64",
              out var ocarinaOfTimeRom)) {
        yield break;
      }

      var ocarinaOfTimeDirectory =
          DirectoryConstants.ROMS_DIRECTORY.GetSubdir("ocarina_of_time", true);
      var fileHierarchy = new FileHierarchy(ocarinaOfTimeDirectory);
      var root = fileHierarchy.Root;

      var zSegments = ZSegments.InitializeFromFile(ocarinaOfTimeRom);
      foreach (var zObject in zSegments.Objects) {
        yield return new OotModelFileBundle(root,
                                            ocarinaOfTimeRom,
                                            zObject);
      }
    }
  }
}