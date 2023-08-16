﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

using fin.math.floats;
using fin.model;
using fin.util.asserts;
using fin.util.hash;

namespace fin.math.matrix.four {
  using SystemMatrix = Matrix4x4;


  public sealed class FinMatrix4x4 : IFinMatrix4x4 {
    public const int ROW_COUNT = 4;
    public const int COLUMN_COUNT = 4;
    public const int CELL_COUNT = ROW_COUNT * COLUMN_COUNT;

    internal SystemMatrix impl_;
    public SystemMatrix Impl => this.impl_;

    public static IReadOnlyFinMatrix4x4 IDENTITY =
        new FinMatrix4x4().SetIdentity();

    public FinMatrix4x4() {
      this.SetZero();
    }

    public FinMatrix4x4(IReadOnlyList<float> data) {
      Asserts.Equal(CELL_COUNT, data.Count);
      for (var i = 0; i < CELL_COUNT; ++i) {
        this[i] = data[i];
      }
    }

    public FinMatrix4x4(IReadOnlyList<double> data) {
      Asserts.Equal(CELL_COUNT, data.Count);
      for (var i = 0; i < CELL_COUNT; ++i) {
        this[i] = (float) data[i];
      }
    }

    public FinMatrix4x4(IReadOnlyFinMatrix4x4 other) => this.CopyFrom(other);
    public FinMatrix4x4(SystemMatrix other) => this.CopyFrom(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IFinMatrix4x4 Clone() => new FinMatrix4x4(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFrom(IReadOnlyFinMatrix4x4 other) {
      Asserts.Different(this, other, "Copying into same matrix!");

      if (other is FinMatrix4x4 otherImpl) {
        this.CopyFrom(otherImpl.Impl);
      } else {
        for (var r = 0; r < ROW_COUNT; ++r) {
          for (var c = 0; c < COLUMN_COUNT; ++c) {
            this[r, c] = other[r, c];
          }
        }
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFrom(SystemMatrix other) => impl_ = other;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IFinMatrix4x4 SetIdentity() {
      impl_ = SystemMatrix.Identity;
      return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IFinMatrix4x4 SetZero() {
      impl_ = new SystemMatrix();
      return this;
    }

    public float this[int index] {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Unsafe.Add(ref impl_.M11, index);
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => Unsafe.Add(ref impl_.M11, index) = value;
    }

    public float this[int row, int column] {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this[FinMatrix4x4.GetIndex_(row, column)];
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => this[FinMatrix4x4.GetIndex_(row, column)] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetIndex_(int row, int column)
      => COLUMN_COUNT * row + column;


    // Addition
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IFinMatrix4x4 CloneAndAdd(IReadOnlyFinMatrix4x4 other)
      => this.Clone().AddInPlace(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IFinMatrix4x4 AddInPlace(IReadOnlyFinMatrix4x4 other) {
      this.AddIntoBuffer(other, this);
      return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddIntoBuffer(
        IReadOnlyFinMatrix4x4 other,
        IFinMatrix4x4 buffer) {
      if (other is FinMatrix4x4 otherImpl &&
          buffer is FinMatrix4x4 bufferImpl) {
        bufferImpl.impl_ = SystemMatrix.Add(impl_, otherImpl.impl_);
        return;
      }

      for (var r = 0; r < ROW_COUNT; ++r) {
        for (var c = 0; c < COLUMN_COUNT; ++c) {
          buffer[r, c] = this[r, c] + other[r, c];
        }
      }
    }


    // Matrix Multiplication
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IFinMatrix4x4 CloneAndMultiply(IReadOnlyFinMatrix4x4 other)
      => this.Clone().MultiplyInPlace(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IFinMatrix4x4 MultiplyInPlace(IReadOnlyFinMatrix4x4 other) {
      this.MultiplyIntoBuffer(other, this);
      return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MultiplyIntoBuffer(
        IReadOnlyFinMatrix4x4 other,
        IFinMatrix4x4 buffer) {
      if (other is FinMatrix4x4 otherImpl &&
          buffer is FinMatrix4x4 bufferImpl) {
        bufferImpl.impl_ = SystemMatrix.Multiply(otherImpl.impl_, impl_);
        return;
      }

      for (var r = 0; r < ROW_COUNT; ++r) {
        for (var c = 0; c < COLUMN_COUNT; ++c) {
          var value = 0f;

          for (var i = 0; i < 4; ++i) {
            value += this[r, i] * other[i, c];
          }

          buffer[r, c] = value;
        }
      }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IFinMatrix4x4 CloneAndMultiply(SystemMatrix other)
      => this.Clone().MultiplyInPlace(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IFinMatrix4x4 MultiplyInPlace(SystemMatrix other) {
      this.MultiplyIntoBuffer(other, this);
      return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MultiplyIntoBuffer(
        SystemMatrix other,
        IFinMatrix4x4 buffer) {
      if (buffer is FinMatrix4x4 bufferImpl) {
        bufferImpl.impl_ = SystemMatrix.Multiply(other, impl_);
        return;
      }

      for (var r = 0; r < ROW_COUNT; ++r) {
        for (var c = 0; c < COLUMN_COUNT; ++c) {
          var value = 0f;

          for (var i = 0; i < 4; ++i) {
            value += this[r, i] * other[i, c];
          }

          buffer[r, c] = value;
        }
      }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IFinMatrix4x4 CloneAndMultiply(float other)
      => this.Clone().MultiplyInPlace(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IFinMatrix4x4 MultiplyInPlace(float other) {
      this.MultiplyIntoBuffer(other, this);
      return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MultiplyIntoBuffer(float other, IFinMatrix4x4 buffer) {
      if (buffer is FinMatrix4x4 bufferImpl) {
        bufferImpl.impl_ = SystemMatrix.Multiply(impl_, other);
        return;
      }

      for (var r = 0; r < ROW_COUNT; ++r) {
        for (var c = 0; c < COLUMN_COUNT; ++c) {
          buffer[r, c] = this[r, c] * other;
        }
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IFinMatrix4x4 CloneAndInvert()
      => this.Clone().InvertInPlace();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IFinMatrix4x4 InvertInPlace() {
      this.InvertIntoBuffer(this);
      return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void InvertIntoBuffer(IFinMatrix4x4 buffer) {
      if (buffer is FinMatrix4x4 bufferImpl) {
        SystemMatrix.Invert(impl_, out bufferImpl.impl_);
        return;
      }

      SystemMatrix.Invert(impl_, out var invertedSystemMatrix);
      Matrix4x4ConversionUtil.CopySystemIntoFin(invertedSystemMatrix, buffer);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IFinMatrix4x4 CloneAndTranspose()
      => this.Clone().TransposeInPlace();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IFinMatrix4x4 TransposeInPlace() {
      impl_ = Matrix4x4.Transpose(impl_);
      return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TransposeIntoBuffer(IFinMatrix4x4 buffer) {
      Asserts.Different(this, buffer);
      for (var r = 0; r < ROW_COUNT; ++r) {
        for (var c = 0; c < COLUMN_COUNT; ++c) {
          buffer[r, c] = this[c, r];
        }
      }
    }

    // Shamelessly copied from https://math.stackexchange.com/a/1463487
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTranslationInto(out Position dst) {
      dst = default;
      Unsafe.As<Position, Vector3>(ref dst) = this.impl_.Translation;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyRotationInto(out Quaternion dst) {
      this.Decompose(out _, out dst, out _);
      dst = -dst;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyScaleInto(out Scale dst)
      => this.Decompose(out _, out _, out dst);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Decompose(out Position translation,
                          out Quaternion rotation,
                          out Scale scale) {
      translation = default;
      scale = default;
      Asserts.True(
          Matrix4x4.Decompose(
              impl_,
              out Unsafe.As<Scale, Vector3>(ref scale),
              out rotation,
              out Unsafe.As<Position, Vector3>(ref translation)),
          "Failed to decompose matrix!");
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
      => ReferenceEquals(this, obj) || this.Equals(obj);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(IReadOnlyFinMatrix4x4? other) {
      if (other == null) {
        return false;
      }

      for (var r = 0; r < ROW_COUNT; ++r) {
        for (var c = 0; c < COLUMN_COUNT; ++c) {
          if (!this[r, c].IsRoughly(other[r, c])) {
            return false;
          }
        }
      }

      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() {
      var error = FloatsExtensions.ROUGHLY_EQUAL_ERROR;

      var hash = new FluentHash();
      for (var i = 0; i < CELL_COUNT; ++i) {
        var value = this[i];
        value = MathF.Round(value / error) * error;
        hash = hash.With(value.GetHashCode());
      }

      return hash;
    }
  }
}