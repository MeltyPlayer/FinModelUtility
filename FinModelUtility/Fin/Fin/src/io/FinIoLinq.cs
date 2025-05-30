﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace fin.io;

public static class FinIoLinq {
  public static IEnumerable<T> ByName<T>(this IEnumerable<T> objects,
                                         string name)
      where T : IReadOnlyTreeIoObject
    => objects.Where(
        o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

  public static IEnumerable<T> NotByName<T>(this IEnumerable<T> objects,
                                         string name)
      where T : IReadOnlyTreeIoObject
    => objects.Where(
        o => !o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

  public static T SingleByName<T>(this IEnumerable<T> objects, string name)
      where T : IReadOnlyTreeIoObject
    => objects.ByName(name).Single();

  public static T? SingleOrDefaultByName<T>(this IEnumerable<T> objects,
                                            string name)
      where T : IReadOnlyTreeIoObject
    => objects.ByName(name).SingleOrDefault();
}