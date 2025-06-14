﻿using fin.picross.moves;

namespace fin.picross.solver.methods.easy;

public class GapsAroundKnownCluesSolverMethod : IPicrossSolverMethod {
  public IEnumerable<IPicrossMove1d> TryToFindMoves(
      IPicrossBoardState _,
      IPicrossLineState lineState) {
    var clueStates = lineState.ClueStates;
    var cellStates = lineState.CellStates;
    var length = cellStates.Count;

    foreach (var clueState in clueStates) {
      if (!clueState.Solved) {
        continue;
      }

      var startI = clueState.StartIndex.Value;
      var beforeI = startI - 1;
      var afterI = startI + clueState.Length;

      if (beforeI >= 0 &&
          cellStates[beforeI].Status == PicrossCellStatus.UNKNOWN) {
        yield return new PicrossCellMove1d(
            PicrossCellMoveType.MARK_EMPTY,
            PicrossCellMoveSource.EMPTY_AROUND_KNOWN_CLUE,
            beforeI);
      }

      if (afterI < length &&
          cellStates[afterI].Status == PicrossCellStatus.UNKNOWN) {
        yield return new PicrossCellMove1d(
            PicrossCellMoveType.MARK_EMPTY,
            PicrossCellMoveSource.EMPTY_AROUND_KNOWN_CLUE,
            afterI);
      }
    }
  }
}