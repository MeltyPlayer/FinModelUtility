﻿using System.Diagnostics;
using System.Numerics;

using fin.util.hash;

namespace vrml.util;

// Implementation of Triangulation by Ear Clipping
// by David Eberly
public class EarClipping {
  private Polygon mainPointList_;
  private Vector3 normal_;
  public List<int> Result { get; private set; }

  public void SetPoints(List<Vector3> points) {
    if (points == null || points.Count < 3) {
      throw new ArgumentException("No list or an empty list passed");
    }

    this.CalcNormal_(points);

    this.mainPointList_ = new Polygon();
    this.LinkAndAddToList_(this.mainPointList_, points);

    this.Result = new List<int>();
  }

  // calculating normal using Newell's method
  private void CalcNormal_(List<Vector3> points) {
    Vector3 normal = Vector3.Zero;
    for (var i = 0; i < points.Count; i++) {
      var j = (i + 1) % (points.Count);
      normal.X += (points[i].Y - points[j].Y) * (points[i].Z + points[j].Z);
      normal.Y += (points[i].Z - points[j].Z) * (points[i].X + points[j].X);
      normal.Z += (points[i].X - points[j].X) * (points[i].Y + points[j].Y);
    }

    this.normal_ = normal;
  }

  private void LinkAndAddToList_(Polygon polygon, List<Vector3> points) {
    ConnectionEdge prev = null, first = null;
    Dictionary<Vector3, (Vector3, int, List<ConnectionEdge>)> pointsHashSet
        = new();
    int pointCount = 0;
    for (int i = 0; i < points.Count; i++) {
      // we don't wanna have duplicates
      Vector3 p0;
      int p0Index;
      List<ConnectionEdge> incidentEdges;
      if (pointsHashSet.ContainsKey(points[i])) {
        (p0, p0Index, incidentEdges) = pointsHashSet[points[i]];
      } else {
        p0 = points[i];
        p0Index = i;
        incidentEdges = new List<ConnectionEdge>();
        pointsHashSet.Add(p0, (p0, p0Index, incidentEdges));
        pointCount++;
      }

      ConnectionEdge current = new(p0, p0Index, polygon, incidentEdges);

      first = (i == 0) ? current : first; // remember first

      if (prev != null) {
        prev.next_ = current;
      }

      current.prev_ = prev;
      prev = current;
    }

    first.prev_ = prev;
    prev.next_ = first;
    polygon.start_ = first;
    polygon.pointCount_ = pointCount;
  }

  public void Triangulate() {
    if (this.normal_ == Vector3.Zero)
      throw new Exception("The input is not a valid polygon");

    List<ConnectionEdge> nonConvexPoints
        = this.FindNonConvexPoints_(this.mainPointList_);

    if (nonConvexPoints.Count == this.mainPointList_.pointCount_)
      throw new ArgumentException("The triangle input is not valid");

    while (this.mainPointList_.pointCount_ > 2) {
      bool guard = false;
      foreach (var cur in this.mainPointList_.GetPolygonCirculator()) {
        if (!this.IsConvex_(cur))
          continue;

        if (!this.IsPointInTriangle_(cur.prev_.Origin,
                                     cur.Origin,
                                     cur.next_.Origin,
                                     nonConvexPoints)) {
          // cut off ear
          guard = true;
          this.Result.Add(cur.prev_.OriginIndex);
          this.Result.Add(cur.OriginIndex);
          this.Result.Add(cur.next_.OriginIndex);

          // Check if prev and next are still nonconvex. If not, then remove from non convex list
          if (this.IsConvex_(cur.prev_)) {
            int index = nonConvexPoints.FindIndex(x => x == cur.prev_);
            if (index >= 0)
              nonConvexPoints.RemoveAt(index);
          }

          if (this.IsConvex_(cur.next_)) {
            int index = nonConvexPoints.FindIndex(x => x == cur.next_);
            if (index >= 0)
              nonConvexPoints.RemoveAt(index);
          }

          this.mainPointList_.Remove(cur);
          break;
        }
      }

      if (this.PointsOnLine(this.mainPointList_))
        break;
      if (!guard) {
        throw new Exception("No progression. The input must be wrong");
      }
    }
  }

  private bool PointsOnLine(Polygon pointList) {
    foreach (var connectionEdge in pointList.GetPolygonCirculator()) {
      if (Misc.GetOrientation(connectionEdge.prev_.Origin,
                              connectionEdge.Origin,
                              connectionEdge.next_.Origin,
                              this.normal_) !=
          0)
        return false;
    }

    return true;
  }

  private bool IsConvex_(ConnectionEdge curPoint) {
    int orientation = Misc.GetOrientation(curPoint.prev_.Origin,
                                          curPoint.Origin,
                                          curPoint.next_.Origin,
                                          this.normal_);
    return orientation == 1;
  }

  private bool IsPointInTriangle_(in Vector3 prevPoint,
                                  in Vector3 curPoint,
                                  in Vector3 nextPoint,
                                  List<ConnectionEdge> nonConvexPoints) {
    foreach (var nonConvexPoint in nonConvexPoints) {
      if (nonConvexPoint.Origin.Equals(prevPoint) ||
          nonConvexPoint.Origin.Equals(curPoint) ||
          nonConvexPoint.Origin.Equals(nextPoint))
        continue;
      if (Misc.PointInOrOnTriangle(prevPoint,
                                   curPoint,
                                   nextPoint,
                                   nonConvexPoint.Origin,
                                   this.normal_))
        return true;
    }

    return false;
  }

  private List<ConnectionEdge> FindNonConvexPoints_(Polygon p) {
    List<ConnectionEdge> resultList = new List<ConnectionEdge>();
    foreach (var connectionEdge in p.GetPolygonCirculator()) {
      if (Misc.GetOrientation(connectionEdge.prev_.Origin,
                              connectionEdge.Origin,
                              connectionEdge.next_.Origin,
                              this.normal_) !=
          1)
        resultList.Add(connectionEdge);
    }

    return resultList;
  }
}

internal class ConnectionEdge {
  public List<ConnectionEdge> IncidentEdges { get; } = new();

  public override bool Equals(object obj) {
    if (ReferenceEquals(null, obj)) return false;
    if (ReferenceEquals(this, obj)) return true;
    if (obj.GetType() != this.GetType()) return false;
    return this.Equals((ConnectionEdge) obj);
  }

  protected bool Equals(ConnectionEdge other)
    => this.next_.OriginIndex.Equals(other.next_.OriginIndex) &&
       this.OriginIndex.Equals(other.OriginIndex);


  public override int GetHashCode()
    => FluentHash.Start().With(this.next_.OriginIndex).With(this.OriginIndex);

  internal Vector3 Origin { get; }
  internal int OriginIndex { get; }

  internal ConnectionEdge prev_;
  internal ConnectionEdge next_;
  internal Polygon Polygon { get; set; }

  public ConnectionEdge(Vector3 p0,
                        int p0Index,
                        Polygon parentPolygon,
                        List<ConnectionEdge> incidentEdges) {
    this.Origin = p0;
    this.OriginIndex = p0Index;
    this.Polygon = parentPolygon;
    this.IncidentEdges = incidentEdges;
    this.IncidentEdges.Add(this);
  }

  public override string ToString() {
    return "Origin: " + this.Origin + " Next: " + this.next_.Origin;
  }
}

internal class Polygon {
  internal ConnectionEdge start_;
  internal int pointCount_ = 0;

  internal IEnumerable<ConnectionEdge> GetPolygonCirculator() {
    if (this.start_ == null) {
      yield break;
    }

    var h = this.start_;
    do {
      yield return h;
      h = h.next_;
    } while (h != this.start_);
  }

  internal void Remove(ConnectionEdge cur) {
    cur.prev_.next_ = cur.next_;
    cur.next_.prev_ = cur.prev_;
    var incidentEdges = cur.IncidentEdges;
    Debug.Assert(incidentEdges.Remove(cur));
    if (incidentEdges.Count == 0)
      this.pointCount_--;
    if (cur == this.start_)
      this.start_ = cur.prev_;
  }
}