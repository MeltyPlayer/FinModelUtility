﻿using fin.math;
using fin.picross.moves;
using fin.picross.solver.methods;
using fin.picross.solver.methods.easy;
using fin.picross.solver.methods.hard;

namespace fin.picross.solver;

public class PicrossSolver {
  public IReadOnlyList<IReadOnlySet<IPicrossMove>> Solve(
      IPicrossDefinition picrossDefinition,
      out IPicrossBoardState finalBoardState) {
    var boardState = new PicrossBoardState(picrossDefinition);

    var width = picrossDefinition.Width;
    var height = picrossDefinition.Height;

    var xClueIndicesForward = new int[width];
    var xClueIndicesBackward = new int[width];
    var yClueIndicesForward = new int[height];
    var yClueIndicesBackward = new int[height];

    var columnLineStates = boardState.ColumnLineStates;
    var rowLineStates = boardState.RowLineStates;

    var moveSets = new List<IReadOnlySet<IPicrossMove>>();
    while (true) {
      var isFirstPass = moveSets.Count == 0;
      var moveSet = new HashSet<IPicrossMove>();

      for (var x = 0; x < width; ++x) {
        foreach (var picrossMove1d in CheckClues_(
                     isFirstPass,
                     boardState,
                     columnLineStates[x],
                     yClueIndicesForward,
                     yClueIndicesBackward)) {
          switch (picrossMove1d) {
            case PicrossCellMove1d picrossCellMove1d: {
              var (moveType, moveSource, y) = picrossCellMove1d;
              moveSet.Add(new PicrossCellMove(moveType, moveSource, x, y));
              break;
            }
            case PicrossClueMove picrossClueMove: {
              moveSet.Add(picrossClueMove);
              break;
            }
          }
        }
      }

      for (var y = 0; y < height; ++y) {
        foreach (var picrossMove1d in CheckClues_(
                     isFirstPass,
                     boardState,
                     rowLineStates[y],
                     xClueIndicesForward,
                     xClueIndicesBackward)) {
          switch (picrossMove1d) {
            case PicrossCellMove1d picrossCellMove1d: {
              var (moveType, moveSource, x) = picrossCellMove1d;
              moveSet.Add(new PicrossCellMove(moveType, moveSource, x, y));
              break;
            }
            case PicrossClueMove picrossClueMove: {
              moveSet.Add(picrossClueMove);
              break;
            }
          }
        }
      }

      // If no more moves, then nothing else to do... either complete or stuck.
      if (!isFirstPass && moveSet.Count == 0) {
        break;
      }

      boardState.ApplyMoves(moveSet);
      moveSets.Add(moveSet);
    }

    finalBoardState = boardState;
    return moveSets;
  }

  private static readonly IReadOnlyList<IPicrossSolverMethod>
      EASY_SOLVER_METHODS_
          = [
              new AlreadySolvedSolverMethod(),
              new AlreadySolvedUpToSolverMethod(),
              new FirstGapTooSmallSolverMethod(),
              new GapsAroundKnownCluesSolverMethod(),
          ];

  private static readonly IReadOnlyList<IPicrossSolverMethod>
      HARD_SOLVER_METHODS_
          = EASY_SOLVER_METHODS_
            .Concat([
                new SingleNextToEdgeSolverMethod(),
            ])
            .ToArray();

  private static readonly IReadOnlyList<IPicrossSolverMethod> SOLVER_METHODS_
      = HARD_SOLVER_METHODS_
        .Concat([
            new BigThenSinglesSolverMethod(),
            new ExtendFirstClueSolverMethod(),
            new ExpandFirstClueWhenPerfectFitSolverMethod(),
            new ExpandFinalUnsolvedClueSolverMethod(),
            new ExpandTwoSeparatedCluesSolverMethod(),
            new FillSmallestUnknownsBetweenEmptiesSolverMethod(),
            new GapsAroundFirstClueSolverMethod(),
            new GapsBetweenKnownCluesSolverMethod(),
            new GapsBetweenNeighboringCluesSolverMethod(),
            new GapsBetweenNeighboringShortCluesSolverMethod(),
            new MatchingBiggestOrUniqueLengthSolverMethod(),
            new OnlyClueLengthInReachSolverMethod(),
        ])
        .ToArray();

  private static IEnumerable<IPicrossMove1d> CheckClues_(
      bool isFirstPass,
      IPicrossBoardState boardState,
      IPicrossLineState lineState,
      int[] clueIndicesForward,
      int[] clueIndicesBackward) {
    var clueStates = lineState.ClueStates;
    var cellStates = lineState.CellStates;
    var length = cellStates.Count;

    if (isFirstPass) {
      // Freebie (0 or full length)
      if (clueStates.Count == 1) {
        var clueState = clueStates[0];
        var clueLength = clueState.Length;
        var isEmpty = clueLength == 0;
        if (isEmpty || clueLength == length) {
          var moveType = isEmpty
              ? PicrossCellMoveType.MARK_EMPTY
              : PicrossCellMoveType.MARK_FILLED;
          var moveSource = isEmpty
              ? PicrossCellMoveSource.FREEBIE_EMPTY
              : PicrossCellMoveSource.FREEBIE_FULL_LENGTH;
          yield return new PicrossClueMove(
              isEmpty
                  ? PicrossClueMoveSource.FREEBIE_EMPTY
                  : PicrossClueMoveSource.FREEBIE_FULL_LENGTH,
              clueState.Clue,
              0);
          for (var i = 0; i < length; ++i) {
            yield return new PicrossCellMove1d(
                moveType,
                moveSource,
                i);
          }
        }
      }

      // Freebie (adds up to full width)
      else if ((clueStates.Sum(c => c.Length) + clueStates.Count - 1) ==
               length) {
        var lineI = 0;
        for (var c = 0; c < clueStates.Count; c++) {
          if (c > 0) {
            yield return new PicrossCellMove1d(
                PicrossCellMoveType.MARK_EMPTY,
                PicrossCellMoveSource.FREEBIE_PERFECT_FIT,
                lineI++);
          }

          var clueState = clueStates[c];
          yield return new PicrossClueMove(
              PicrossClueMoveSource.FREEBIE_PERFECT_FIT,
              clueState.Clue,
              lineI);
          for (var clueI = 0; clueI < clueState.Length; ++clueI) {
            yield return new PicrossCellMove1d(
                PicrossCellMoveType.MARK_FILLED,
                PicrossCellMoveSource.FREEBIE_PERFECT_FIT,
                lineI++);
          }
        }
      }

      yield break;
    }

    // Checks line forward and backward.
    var forwardMoves1d
        = TryToFitCluesIntoGaps_(lineState, clueIndicesForward, true);
    var backwardMoves1d
        = TryToFitCluesIntoGaps_(lineState, clueIndicesBackward, false);

    foreach (var picrossMove1d in forwardMoves1d.Concat(backwardMoves1d)) {
      yield return picrossMove1d;
    }

    // Tries to fill in where clues overlap.
    for (var i = 0; i < length; ++i) {
      var clueIndexForward = clueIndicesForward[i];
      var clueIndexBackward = clueIndicesBackward[i];
      if (clueIndexForward != -1 &&
          clueIndexForward == clueIndexBackward &&
          cellStates[i].Status == PicrossCellStatus.UNKNOWN) {
        if (clueIndexForward % 2 == 0) {
          yield return new PicrossCellMove1d(
              PicrossCellMoveType.MARK_FILLED,
              PicrossCellMoveSource.FORWARD_BACKWARD_OVERLAP,
              i);
        } else {
          yield return new PicrossCellMove1d(
              PicrossCellMoveType.MARK_EMPTY,
              PicrossCellMoveSource.EMPTY_BETWEEN_CLUES,
              i);
        }
      }
    }

    foreach (var solverMethod in SOLVER_METHODS_) {
      var hadAnyOfMethod = false;
      foreach (var move in solverMethod.TryToFindMoves(
                   boardState,
                   lineState)) {
        hadAnyOfMethod = true;
        yield return move;
      }

      if (hadAnyOfMethod) {
        yield break;
      }
    }
  }

  private static IEnumerable<IPicrossMove1d> TryToFitCluesIntoGaps_(
      IPicrossLineState lineState,
      int[] clueIndices,
      bool forward) {
    var clueStates = lineState.ClueStates;
    var cellStates = lineState.CellStates;
    var length = cellStates.Count;

    Array.Fill(clueIndices, -1);

    GetStepValues_(forward,
                   length,
                   out var iStart,
                   out var iEnd,
                   out var increment);
    GetStepValues_(forward,
                   clueStates.Count,
                   out var clueStart,
                   out var clueEnd,
                   out _);

    var totalUnknownCount = 0;

    PicrossClueState? currentClue = null;

    var isOnlyClue = clueStates.Count == 1;

    var i = iStart;
    for (var clueI = clueStart; clueI != clueEnd; clueI += increment) {
      var clueState = clueStates[clueI];

      // If already solved, skip to the location of this clue.
      if (clueState.Solved) {
        var clueStartI = clueState.StartIndex.Value;

        var targetI
            = forward ? clueStartI : (clueStartI + clueState.Length - 1);
        while (i != targetI) {
          clueIndices[i] = 2 * (clueI - 1) + increment;
          i += increment;
        }
      }

      var isFirstClue = clueI == clueStart;

      RetryClue:
      var clueUnknownCount = 0;
      int? hitKnownFilledI = null;
      var clueLength = clueState.Length;
      for (var clueCellI = 0; clueCellI < clueLength; ++clueCellI) {
        var cellState = cellStates[i + increment * clueCellI].Status;

        // Uh oh, not expecting to reach a known empty cell. Can't start here.
        if (cellState == PicrossCellStatus.KNOWN_EMPTY) {
          // Skip ahead.
          i += increment * (clueCellI + 1);
          goto RetryClue;
        }

        // If find one that's marked, we can fill the ones after.
        if (cellState == PicrossCellStatus.KNOWN_FILLED) {
          hitKnownFilledI ??= clueCellI;
        } else {
          ++clueUnknownCount;
        }
      }

      // If cell after clue is filled, we need to start at least one later.
      var afterClueI = i + increment * clueLength;
      var isAfterClueIOnBoard = afterClueI.IsInRange(0, length - 1);
      if (clueI == clueStart && isAfterClueIOnBoard) {
        if (cellStates[afterClueI].Status == PicrossCellStatus.KNOWN_FILLED) {
          yield return new PicrossCellMove1d(
              PicrossCellMoveType.MARK_EMPTY,
              PicrossCellMoveSource.TOO_FAR_FROM_KNOWN,
              i);
          i += increment;
          goto RetryClue;
        }
      }

      // Otherwise, valid clue location! Let's mark this down.

      // If we hit a known cell, we can fill in the cells after!
      if (totalUnknownCount == 0) {
        if (hitKnownFilledI != null) {
          for (var ii = hitKnownFilledI.Value; ii < clueLength; ++ii) {
            var cellI = i + increment * ii;
            if (cellStates[cellI].Status == PicrossCellStatus.UNKNOWN) {
              yield return new PicrossCellMove1d(
                  PicrossCellMoveType.MARK_FILLED,
                  PicrossCellMoveSource.NOWHERE_ELSE_TO_GO,
                  cellI);
            }
          }

          // We can mark some cells as empty after if this is the only clue.
          if (isOnlyClue) {
            var unaccountedLength = hitKnownFilledI.Value;
            for (var ii = afterClueI + increment * unaccountedLength;
                 ii != iEnd && ii.IsInRange(0, length - 1);
                 ii += increment) {
              if (cellStates[ii].Status == PicrossCellStatus.UNKNOWN) {
                yield return new PicrossCellMove1d(
                    PicrossCellMoveType.MARK_EMPTY,
                    PicrossCellMoveSource.TOO_FAR_FROM_KNOWN,
                    ii);
              }
            }
          }
        }

        if (clueUnknownCount == 0 && isAfterClueIOnBoard) {
          if (cellStates[afterClueI].Status == PicrossCellStatus.UNKNOWN) {
            yield return new PicrossCellMove1d(
                PicrossCellMoveType.MARK_EMPTY,
                PicrossCellMoveSource.EMPTY_BETWEEN_CLUES,
                afterClueI);
          }
        }
      }

      if (isFirstClue && clueUnknownCount == 0 && !clueState.Solved) {
        var clueStartI = forward ? i : i + increment * (clueLength - 1);
        yield return new PicrossClueMove(
            PicrossClueMoveSource.FIRST_CLUE_IN_LINE,
            clueState.Clue,
            clueStartI);
      }

      totalUnknownCount += clueUnknownCount;

      for (var clueCellI = 0; clueCellI < clueLength; ++clueCellI) {
        clueIndices[i + increment * clueCellI] = 2 * clueI;
      }

      if (isAfterClueIOnBoard) {
        clueIndices[afterClueI] = 2 * clueI + increment;
      }

      i = afterClueI + increment;
    }
  }

  private static void GetStepValues_(bool forward,
                                     int length,
                                     out int start,
                                     out int end,
                                     out int increment) {
    if (forward) {
      start = 0;
      end = length;
      increment = 1;
      return;
    }

    start = length - 1;
    end = -1;
    increment = -1;
  }
}