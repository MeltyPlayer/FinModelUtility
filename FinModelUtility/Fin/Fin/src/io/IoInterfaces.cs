﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.CompilerServices;

using fin.util.asserts;
using fin.util.json;

using schema.binary;


namespace fin.io {
  public interface IReadOnlyGenericFile {
    string DisplayPath { get; }

    FileSystemStream OpenRead();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    StreamReader OpenReadAsText() => new(this.OpenRead());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    T ReadNew<T>() where T : IBinaryDeserializable, new() {
      using var er = new EndianBinaryReader(this.OpenRead());
      return er.ReadNew<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    T ReadNew<T>(Endianness endianness)
        where T : IBinaryDeserializable, new() {
      using var er = new EndianBinaryReader(this.OpenRead(), endianness);
      return er.ReadNew<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    byte[] ReadAllBytes() {
      using var s = this.OpenRead();
      using var ms = new MemoryStream();
      s.CopyTo(ms);
      return ms.ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    string ReadAllText() {
      using var sr = this.OpenReadAsText();
      return sr.ReadToEnd();
    }
  }

  public interface IGenericFile : IReadOnlyGenericFile {
    FileSystemStream OpenWrite();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    StreamWriter OpenWriteAsText() => new(this.OpenWrite());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void WriteAllBytes(byte[] bytes) {
      using var s = this.OpenWrite();
      using var ms = new MemoryStream(bytes);
      ms.CopyTo(s);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void WriteAllText(string text) {
      using var sw = this.OpenWriteAsText();
      sw.Write(text);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    T Deserialize<T>() => JsonUtil.Deserialize<T>(this.ReadAllText());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void Serialize<T>(T instance) where T : notnull
      => this.WriteAllText(JsonUtil.Serialize(instance));
  }

  public interface ISystemIoObject : IEquatable<ISystemIoObject> {
    string Name => FinIoStatic.GetName(this.FullName);
    string FullName { get; }

    bool Exists { get; }

    string? GetParentFullName() => FinIoStatic.GetParentFullName(this.FullName);

    ISystemDirectory GetParent() {
      if (this.TryGetParent(out var parent)) {
        return parent;
      }

      throw new Exception("Expected parent directory to exist!");
    }

    bool TryGetParent(out ISystemDirectory parent) {
      var parentName = this.GetParentFullName();
      if (parentName != null) {
        parent = new FinDirectory(parentName);
        return true;
      }

      parent = default;
      return false;
    }

    ISystemDirectory[] GetAncestry() {
      if (!this.TryGetParent(out var firstParent)) {
        return Array.Empty<ISystemDirectory>();
      }

      var parents = new LinkedList<ISystemDirectory>();
      var current = firstParent;
      while (current.TryGetParent(out var parent)) {
        parents.AddLast(parent);
        current = parent;
      }

      return parents.ToArray();
    }

    string ToString() => this.FullName;

    bool Equals(object? other) {
      if (object.ReferenceEquals(this, other)) {
        return true;
      }

      if (other is not ISystemIoObject otherSelf) {
        return false;
      }

      return this.Equals(otherSelf);
    }

    bool IEquatable<ISystemIoObject>.Equals(ISystemIoObject? other)
      => this.FullName == other?.FullName;
  }


  // Directory

  public interface ISystemDirectory : ISystemIoObject {
    bool ISystemIoObject.Exists => FinDirectoryStatic.Exists(this.FullName);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool Create() => FinDirectoryStatic.Create(this.FullName);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool Delete(bool recursive = false)
      => FinDirectoryStatic.Delete(this.FullName, recursive);

    bool DeleteContents() {
      var didDeleteAnything = false;
      foreach (var file in this.GetExistingFiles()) {
        didDeleteAnything |= file.Delete();
      }
      foreach (var directory in this.GetExistingSubdirs()) {
        didDeleteAnything |= directory.Delete(true);
      }
      return didDeleteAnything;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void MoveTo(string path) => FinDirectoryStatic.MoveTo(this.FullName, path);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerable<ISystemDirectory> GetExistingSubdirs()
      => FinDirectoryStatic
         .GetExistingSubdirs(this.FullName)
         .Select(fullName => (ISystemDirectory) new FinDirectory(fullName));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    ISystemDirectory GetSubdir(string relativePath, bool create = false)
      => new FinDirectory(
          FinDirectoryStatic.GetSubdir(this.FullName, relativePath, create));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerable<ISystemFile> GetExistingFiles()
      => FinDirectoryStatic.GetExistingFiles(this.FullName)
                           .Select(fullName => (ISystemFile) new FinFile(fullName));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerable<ISystemFile> SearchForFiles(
        string searchPattern,
        bool includeSubdirs = false)
      => FinDirectoryStatic
         .SearchForFiles(this.FullName, searchPattern, includeSubdirs)
         .Select(fullName => (ISystemFile) new FinFile(fullName));

    bool TryToGetExistingFile(string path, out ISystemFile outFile) {
      if (FinDirectoryStatic.TryToGetExistingFile(
              this.FullName,
              path,
              out var fileFullName)) {
        outFile = new FinFile(fileFullName);
        return true;
      }

      outFile = default;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    ISystemFile GetExistingFile(string path)
      => new FinFile(FinDirectoryStatic.GetExistingFile(this.FullName, path));

    bool PossiblyAssertExistingFile(string relativePath,
                                    bool assert,
                                    out ISystemFile outFile) {
      var fileFullName =
          FinDirectoryStatic.PossiblyAssertExistingFile(
              this.FullName,
              relativePath,
              assert);
      if (fileFullName != null) {
        outFile = new FinFile(fileFullName);
        return true;
      }

      outFile = default;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerable<ISystemFile> GetFilesWithExtension(
        string extension,
        bool includeSubdirs = false)
      => FinDirectoryStatic
         .GetFilesWithExtension(this.FullName, extension, includeSubdirs)
         .Select(fullName => (ISystemFile) new FinFile(fullName));
  }


  // File 
  public interface ISystemFile : ISystemIoObject, IGenericFile {
    bool ISystemIoObject.Exists => FinFileStatic.Exists(this.FullName);

    string IReadOnlyGenericFile.DisplayPath => this.FullName;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool Delete() => FinFileStatic.Delete(FullName);

    string Extension => FinFileStatic.GetExtension(FullName);

    string FullNameWithoutExtension
      => FinFileStatic.GetNameWithoutExtension(this.FullName);

    string NameWithoutExtension
      => FinFileStatic.GetNameWithoutExtension(this.Name);

    ISystemFile CloneWithExtension(string newExtension) {
      Asserts.True(newExtension.StartsWith("."),
                   $"'{newExtension}' is not a valid extension!");
      return new FinFile(this.FullNameWithoutExtension + newExtension);
    }
  }
}