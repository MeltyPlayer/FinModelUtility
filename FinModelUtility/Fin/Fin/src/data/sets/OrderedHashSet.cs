﻿using System.Collections;
using System.Collections.Generic;

namespace fin.data.sets {
  public class OrderedHashSet<T> : ISet<T> {
    private readonly LinkedList<T> list_ = new();
    private readonly HashSet<T> set_ = new();

    public int Count => this.list_.Count;

    public void Clear() {
      this.list_.Clear();
      this.set_.Clear();
    }

    public bool Add(T value) {
      if (this.set_.Add(value)) {
        this.list_.AddLast(value);
        return true;
      }

      return false;
    }

    public bool Remove(T value) {
      if (this.set_.Remove(value)) {
        this.list_.Remove(value);
        return true;
      }

      return false;
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    public IEnumerator<T> GetEnumerator() => this.list_.GetEnumerator();
  }
}