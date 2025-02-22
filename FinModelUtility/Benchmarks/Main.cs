﻿using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace benchmarks {
  public class Program {
    public static void Main(string[] args) {
      var summary = BenchmarkRunner.Run<Finite>(
          ManualConfig.Create(DefaultConfig.Instance)
                      .WithOptions(ConfigOptions
                                       .DisableOptimizationsValidator));
    }
  }
}