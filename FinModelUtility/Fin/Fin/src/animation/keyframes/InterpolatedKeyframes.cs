﻿using System.Collections.Generic;

using fin.animation.interpolation;

using static fin.animation.keyframes.KeyframesUtil;

namespace fin.animation.keyframes;

public interface IInterpolatableKeyframes<TKeyframe, T>
    : IKeyframes<TKeyframe>,
      IConfiguredInterpolatable<T> where TKeyframe : IKeyframe<T>;

public class InterpolatedKeyframes<TKeyframe, T>(
    ISharedInterpolationConfig sharedConfig,
    IKeyframeInterpolator<TKeyframe, T> interpolator,
    IndividualInterpolationConfig<T> individualConfig = default)
    : IInterpolatableKeyframes<TKeyframe, T>
    where TKeyframe : IKeyframe<T> {
  private readonly List<TKeyframe>
      impl_ = new(individualConfig.InitialCapacity);

  public ISharedInterpolationConfig SharedConfig => sharedConfig;
  public IndividualInterpolationConfig<T> IndividualConfig => individualConfig;

  public IReadOnlyList<TKeyframe> Definitions => this.impl_;
  public bool HasAnyData => this.Definitions.Count > 0;

  public void Add(TKeyframe keyframe) => this.impl_.AddKeyframe(keyframe);

  public bool TryGetAtFrame(float frame, out T value) {
    switch (this.impl_.TryGetPrecedingAndFollowingKeyframes(
                frame,
                sharedConfig,
                individualConfig,
                out var precedingKeyframe,
                out var followingKeyframe,
                out var normalizedFrame)) {
      case InterpolationDataType.PRECEDING_AND_FOLLOWING:
        value = interpolator.Interpolate(precedingKeyframe,
                                         followingKeyframe,
                                         normalizedFrame,
                                         sharedConfig);
        return true;

      case InterpolationDataType.PRECEDING_ONLY:
        value = precedingKeyframe.ValueOut;
        return true;

      default:
      case InterpolationDataType.NONE:
        if (individualConfig.DefaultValue?.Try(out value) ?? false) {
          return true;
        }

        value = default;
        return false;
    }
  }

  public InterpolationDataType TryGetPrecedingAndFollowingKeyframes(
      float frame,
      out TKeyframe precedingKeyframe,
      out TKeyframe followingKeyframe,
      out float normalizedFrame)
    => this.impl_.TryGetPrecedingAndFollowingKeyframes(
        frame,
        sharedConfig,
        individualConfig,
        out precedingKeyframe,
        out followingKeyframe,
        out normalizedFrame);
}