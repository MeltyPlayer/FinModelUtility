﻿using Avalonia;

using fin.audio;
using fin.ui.rendering.gl;
using fin.ui.rendering.gl.material;

using OpenTK.Graphics.OpenGL;

using uni.ui.avalonia.common.gl;

namespace uni.ui.avalonia.resources.audio;

public class AudioWaveformGlPanel : BOpenTkControl {
  private readonly AotWaveformRenderer waveformRenderer_ = new();

  public static readonly DirectProperty<AudioWaveformGlPanel,
          IAotAudioPlayback<short>?>
      ActivePlaybackProperty
          = AvaloniaProperty
              .RegisterDirect<AudioWaveformGlPanel, IAotAudioPlayback<short>?>(
                  nameof(ActivePlayback),
                  o => o.waveformRenderer_.ActivePlayback,
                  (o, value) => o.waveformRenderer_.ActivePlayback = value);

  public IAotAudioPlayback<short>? ActivePlayback {
    get => this.GetValue(ActivePlaybackProperty);
    set => this.SetValue(ActivePlaybackProperty, value);
  }

  protected override void InitGl() => GlUtil.ResetGl();
  protected override void TeardownGl() { }

  protected override void RenderGl() {
    // No idea why this scaling is necessary, it just is.
    var width = (int) (this.Bounds.Width * 1.25);
    var height = (int) (this.Bounds.Height * 1.25);
    GL.Viewport(0, 0, width, height);

    GL.ClearColor(0, 0, 0, 0);
    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

    {
      GlTransform.MatrixMode(TransformMatrixMode.PROJECTION);
      GlTransform.LoadIdentity();
      GlTransform.Ortho2d(0, width, height, 0);

      GlTransform.MatrixMode(TransformMatrixMode.VIEW);
      GlTransform.LoadIdentity();

      GlTransform.MatrixMode(TransformMatrixMode.MODEL);
      GlTransform.LoadIdentity();
    }

    CommonShaderPrograms.TEXTURELESS_SHADER_PROGRAM.Use();

    var amplitude = height * .45f;
    this.waveformRenderer_.Width = width;
    this.waveformRenderer_.Amplitude = amplitude;
    this.waveformRenderer_.MiddleY = height / 2f;
    this.waveformRenderer_.Render();
  }
}