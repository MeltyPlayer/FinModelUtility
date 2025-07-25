﻿using fin.common;
using fin.image;
using fin.io;
using fin.io.bundles;
using fin.picross;
using fin.util.progress;

using MariosPicross;

namespace uni.games.marios_picross_1;

public class MariosPicross1FileBundleGatherer : INamedAnnotatedFileBundleGatherer {
  public string Name => "marios_picross_1";

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFile(
            "marios_picross_1.gb",
            out var romFile)) {
      return;
    }

    var extractedDirectory
        = ExtractorUtil.GetOrCreateExtractedDirectory(romFile);

    if (extractedDirectory.IsEmpty) {
      var puzzleDefinitionImageConverter = new PicrossDefinitionImageConverter();
      var puzzleDefinitions = new GameBoyPicrossDefinitionReader().Read(romFile);
      if (puzzleDefinitions == null) {
        return;
      }

      foreach (var puzzleDefinition in puzzleDefinitions) {
        var name = puzzleDefinition.Name;
        if (name == "") {
          continue;
        }

        var puzzleImage
            = puzzleDefinitionImageConverter.ConvertToImage(puzzleDefinition);
        var imageFile = new FinFile(Path.Join(extractedDirectory.FullPath,
                                              $"{name}.png"));
        using var imageStream = imageFile.OpenWrite();
        puzzleImage.ExportToStream(imageStream, LocalImageFormat.PNG);
      }
    }
  }
}