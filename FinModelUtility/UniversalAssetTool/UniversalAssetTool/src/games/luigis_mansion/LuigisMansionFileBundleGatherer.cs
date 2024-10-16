﻿using fin.io.bundles;
using fin.util.progress;

using uni.platforms.gcn;

namespace uni.games.luigis_mansion;

public class LuigisMansionFileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
            "luigis_mansion",
            GcnFileHierarchyExtractor.Options
                                     .Standard()
                                     .UseRarcDumpForExtensions(
                                         // For some reason, some MDL files are compressed as RARC.
                                         ".mdl"),
            out var fileHierarchy)) {
      return;
    }
  }
}