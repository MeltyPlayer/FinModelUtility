﻿using System;

using Avalonia;
using Avalonia.OpenGL;
using Avalonia.ReactiveUI;
using Avalonia.Win32;

using fin.services;

using uni.cli;

namespace uni.ui.avalonia.desktop;

class Program {
  // Initialization code. Don't use any Avalonia, third-party APIs or any
  // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
  // yet and stuff might break.
  [STAThread]
  public static void Main(string[] args) {
    Cli.Run(args,
            () => {
              try {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
              } catch (Exception e) {
                ExceptionService.HandleException(e, null);
              }
            });
  }

  // Avalonia configuration, don't remove; also used by visual designer.
  public static AppBuilder BuildAvaloniaApp()
    => AppBuilder.Configure<App>()
                 .UsePlatformDetect()
                 .With(new AngleOptions {
                     GlProfiles = [
                         new GlVersion(GlProfileType.OpenGLES, 3, 1, true)
                     ],
                 })
                 .With(new Win32PlatformOptions {
                     RenderingMode = [Win32RenderingMode.AngleEgl]
                 })
                 .WithInterFont()
                 .UseReactiveUI();
}