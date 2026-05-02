using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;

namespace XnaApiTesting;

public class SoundData
{
    public string Name { get; private set; }
    public ListBoxItem ListBoxItem { get; private set; }

    public enum SoundType
    {
        SoundEffect,
        SoundEffectInstance,
        DynamicSoundEffectInstance
    }

    public SoundType Type { get; private set; }

    public bool PlayAppliesVolumePanPitch { get; set; }
    public bool IsLooped { get; set; }
    public AudioStopOptions? AudioStopOption { get; set; }
    public bool AutoApply3D { get; set; }
    public bool AutoSubmitBuffers { get; set; }

    public float Volume { get; set; }
    public float Pitch { get; set; }
    public float Pan { get; set; }

    public AudioListener AudioListener { get; set; }
    public Vector3 AudioListenerOrientation { get; set; }
    public AudioEmitter AudioEmitter { get; set; }
    public Vector3 AudioEmitterOrientation { get; set; }

    private object _soundObject;
    private SoundEffect _soundEffect => _soundObject as SoundEffect;
    private SoundEffectInstance _soundEffectInstance => _soundObject as SoundEffectInstance;
    private DynamicSoundEffectInstance _dynamicSoundEffectInstance => _soundObject as DynamicSoundEffectInstance;

    private bool _initialApply3DDone;
    private byte[] _data;
    private int _dataPosition;

    public SoundData(ContentManager contentManager, string name, SoundType soundType, int? sampleRate = null, AudioChannels? audioChannels = null)
    {
        Name = name;
        Type = soundType;

        Stream stream = null;
        if (name.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            stream = TitleContainer.OpenStream($"Content/{name}");

        switch (Type)
        {
            case SoundType.SoundEffect:
                if (stream != null)
                    _soundObject = SoundEffect.FromStream(stream);
                else
                    _soundObject = contentManager.Load<SoundEffect>($"{Path.GetFileNameWithoutExtension(name)}");
                PlayAppliesVolumePanPitch = true;
                break;

            case SoundType.SoundEffectInstance:
                if (stream != null)
                    _soundObject = SoundEffect.FromStream(stream).CreateInstance();
                else
                    _soundObject = contentManager.Load<SoundEffect>($"{Path.GetFileNameWithoutExtension(name)}").CreateInstance();
                AutoApply3D = true;
                break;

            case SoundType.DynamicSoundEffectInstance:
                _soundObject = new DynamicSoundEffectInstance(sampleRate.Value, audioChannels.Value);
                int headerSize = 44;
                _data = new byte[stream.Length - headerSize];
                stream.Seek(headerSize, SeekOrigin.Begin);
                stream.Read(_data);
                _dynamicSoundEffectInstance.BufferNeeded += (s, e) => SubmitBuffer();
                AutoSubmitBuffers = true;
                AutoApply3D = true;
                break;
        }

        Volume = 1f;
        Pitch = 0f;
        Pan = 0f;

        AudioListener = new();
        AudioEmitter = new();

        ListBoxItem = new();
        ListBoxItem.Visual.Height = 80;
        ListBoxItem.Visual.HeightUnits = Gum.DataTypes.DimensionUnitType.Absolute;
        (ListBoxItem.Visual as ListBoxItemVisual).TextInstance.Y = Type == SoundType.SoundEffect ? 20 : 0;
        ListBoxItem.BindingContext = this;
    }

    public void Update()
    {
        if ((Type == SoundType.SoundEffectInstance || Type == SoundType.DynamicSoundEffectInstance) &&
            (_soundEffectInstance.IsDisposed == false))
        {
            _soundEffectInstance.IsLooped = IsLooped;
            if (_initialApply3DDone && AutoApply3D)
                    _soundEffectInstance.Apply3D(AudioListener, AudioEmitter);

            if (_soundEffectInstance.Volume != Volume)
                _soundEffectInstance.Volume = Volume;
            if (_soundEffectInstance.Pitch != Pitch)
                _soundEffectInstance.Pitch = Pitch;
            if (_soundEffectInstance.Pan != Pan)
                _soundEffectInstance.Pan = Pan;

            Matrix listenerOrientation = Matrix.CreateFromYawPitchRoll(
                MathHelper.ToRadians(AudioListenerOrientation.Y),
                MathHelper.ToRadians(AudioListenerOrientation.X),
                MathHelper.ToRadians(AudioListenerOrientation.Z));
            AudioListener.Forward = listenerOrientation.Forward;
            AudioListener.Up = listenerOrientation.Up;

            Matrix emitterOrientation = Matrix.CreateFromYawPitchRoll(
                MathHelper.ToRadians(AudioEmitterOrientation.Y),
                MathHelper.ToRadians(AudioEmitterOrientation.X),
                MathHelper.ToRadians(AudioEmitterOrientation.Z));
            AudioEmitter.Forward = emitterOrientation.Forward;
            AudioEmitter.Up = emitterOrientation.Up;
        }

        string typeText = Type.ToString();
        string stateText = "";
        string pendingBufferCountText = "";
        switch (Type)
        {
            case SoundType.SoundEffect:
                (ListBoxItem.Visual as ListBoxItemVisual).TextInstance.Y = 10;
                break;
            case SoundType.SoundEffectInstance:
                stateText = _soundEffectInstance.IsDisposed ? "Disposed" : _soundEffectInstance.State.ToString();
                break;
            case SoundType.DynamicSoundEffectInstance:
                stateText = _dynamicSoundEffectInstance.IsDisposed ? "Disposed" : _dynamicSoundEffectInstance.State.ToString();
                pendingBufferCountText = $"PendingBufferCount: {_dynamicSoundEffectInstance.PendingBufferCount}";
                break;
        }

        string text = $"{typeText}\n{Name}\n{stateText} {pendingBufferCountText}";
        (ListBoxItem.Visual as ListBoxItemVisual).TextInstance.Text = text;
    }

    public void Play()
    {
        switch (Type)
        {
            case SoundType.SoundEffect:
                if (PlayAppliesVolumePanPitch)
                    _soundEffect.Play(Volume, Pitch, Pan);
                else
                    _soundEffect.Play();
                break;
            case SoundType.SoundEffectInstance:
            case SoundType.DynamicSoundEffectInstance:
                _soundEffectInstance.Play();
                break;
        }
    }

    public void Pause()
    {
        if (Type == SoundType.SoundEffect)
            return;
        _soundEffectInstance.Pause();
    }

    public void Resume()
    {
        if (Type == SoundType.SoundEffect)
            return;
        _soundEffectInstance.Resume();
    }

    public void Stop()
    {
        if (Type == SoundType.SoundEffect)
            return;
        if (AudioStopOption != null)
            _soundEffectInstance.Stop(AudioStopOption.Value == AudioStopOptions.Immediate);
        else
            _soundEffectInstance.Stop();
    }

    public void Apply3D()
    {
        if (Type == SoundType.SoundEffect)
            return;
        _initialApply3DDone = true;
        _soundEffectInstance.Apply3D(AudioListener, AudioEmitter);
    }

    public void SubmitBuffer()
    {
        if (Type != SoundType.DynamicSoundEffectInstance || !AutoSubmitBuffers)
            return;

        int sampleSizeInBytes = _dynamicSoundEffectInstance.GetSampleSizeInBytes(TimeSpan.FromSeconds(1));
        int lengthToCopy = Math.Min(sampleSizeInBytes, _data.Length - _dataPosition);
        byte[] buffer = _data[_dataPosition..(_dataPosition + lengthToCopy)];
        _dynamicSoundEffectInstance.SubmitBuffer(buffer);
        _dataPosition += sampleSizeInBytes;
        if (_dataPosition >= _data.Length)
            _dataPosition = 0;
    }

    public void Dispose()
    {
        if (Type == SoundType.SoundEffect)
            return;
        _soundEffectInstance.Dispose();
    }
}
