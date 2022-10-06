﻿using fin.util.asserts;
using OpenTK.Audio.OpenAL;
using System;


namespace fin.audio.impl.al {
  public partial class AlAudioManager {
    public IAudioSource<short> CreateAudioSource() => new AlAudioSource(this);

    private class AlAudioSource : IAudioSource<short> {
      private readonly AlAudioManager manager_;

      public AlAudioSource(AlAudioManager manager) {
        this.manager_ = manager;
      }

      public IActiveSound<short> Create(IAudioBuffer<short> buffer)
        => Create(this.manager_.CreateBufferAudioStream(buffer));

      public IActiveSound<short> Play(IAudioBuffer<short> buffer) {
        var activeSound = this.Create(buffer);
        activeSound.Play();
        return activeSound;
      }

      public IActiveSound<short> Create(IAudioStream<short> stream)
        => new AlActiveSound(stream);

      public IActiveSound<short> Play(IAudioStream<short> stream) {
        var activeSound = this.Create(stream);
        activeSound.Play();
        return activeSound;
      }
    }

    private class AlActiveSound : IActiveSound<short> {
      private readonly IAudioStream<short> stream_;
      private uint alBufferId_;
      private uint alSourceId_;

      public AlActiveSound(IAudioStream<short> stream) {
        this.stream_ = stream;

        AL.GenBuffer(out this.alBufferId_);

        ALFormat bufferFormat = default;
        short[] shortBufferData = default!;
        switch (stream.AudioChannelsType) {
          case AudioChannelsType.MONO: {
            bufferFormat = ALFormat.Mono16;
            shortBufferData = new short[1 * stream.SampleCount];

            for (var i = 0; i < stream.SampleCount; ++i) {
              shortBufferData[i] = stream.GetPcm(AudioChannelType.MONO, i);
            }

            break;
          }
          case AudioChannelsType.STEREO: {
            bufferFormat = ALFormat.Stereo16;
            shortBufferData = new short[2 * stream.SampleCount];

            // TODO: Is this correct, are they interleaved?
            for (var i = 0; i < stream.SampleCount; ++i) {
              shortBufferData[2 * i] =
                  stream.GetPcm(AudioChannelType.STEREO_LEFT, i);
              shortBufferData[2 * i + 1] =
                  stream.GetPcm(AudioChannelType.STEREO_RIGHT, i);
            }

            break;
          }
        }

        var byteCount = 2 * shortBufferData.Length;
        var byteBufferData = new byte[byteCount];
        Buffer.BlockCopy(shortBufferData, 0, byteBufferData, 0,
                         byteCount);

        AL.BufferData((int) this.alBufferId_, 
                      bufferFormat, 
                      byteBufferData,
                      byteCount,
                      stream.Frequency);

        AL.GenSource(out this.alSourceId_);
        AL.BindBufferToSource(this.alSourceId_, this.alBufferId_);
      }

      ~AlActiveSound() => this.ReleaseUnmanagedResources_();

      public void Dispose() {
        this.AssertNotDisposed_();

        this.ReleaseUnmanagedResources_();
        GC.SuppressFinalize(this);
      }

      private void ReleaseUnmanagedResources_() {
        this.State = SoundState.DISPOSED;
        AL.DeleteBuffer(ref this.alBufferId_);
        AL.DeleteSource(ref this.alSourceId_);
      }

      private void AssertNotDisposed_()
        => Asserts.False(this.State == SoundState.DISPOSED);

      public AudioChannelsType AudioChannelsType
        => this.stream_.AudioChannelsType;

      public int Frequency => this.stream_.Frequency;
      public int SampleCount => this.stream_.SampleCount;

      public SoundState State { get; private set; }

      public void Play() {
        this.AssertNotDisposed_();

        this.State = SoundState.PLAYING;
        AL.SourcePlay(this.alSourceId_);
      }

      public void Stop() {
        this.AssertNotDisposed_();

        this.State = SoundState.STOPPED;
        AL.SourceStop(this.alSourceId_);
      }

      public void Pause() {
        this.AssertNotDisposed_();

        this.State = SoundState.PAUSED;
        AL.SourcePause(this.alSourceId_);
      }

      public int SampleOffset {
        get {
          this.AssertNotDisposed_();

          AL.GetSource(this.alSourceId_,
                       ALGetSourcei.SampleOffset,
                       out var sampleOffset);
          return sampleOffset;
        }
        set {
          this.AssertNotDisposed_();

          AL.Source(this.alSourceId_, ALSourcei.SampleOffset, (int)value);
        }
      }

      public short GetPcm(AudioChannelType channelType)
        => this.stream_.GetPcm(channelType, this.SampleOffset);

      public bool Looping {
        get {
          this.AssertNotDisposed_();

          AL.GetSource(this.alSourceId_, ALSourceb.Looping, out var looping);
          return looping;
        }
        set {
          this.AssertNotDisposed_();
          AL.Source(this.alSourceId_, ALSourceb.Looping, value);
        }
      }
    }
  }
}