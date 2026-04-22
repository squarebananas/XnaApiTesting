using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Gum.Forms;
using Gum.Forms.Controls;
using Gum.Wireframe;
using MonoGameGum;

namespace XnaApiTesting;

public class XnaApiTestingGame : Game
{
    private GraphicsDeviceManager _graphics;

    private StackPanel _stackPanel;
    private StackPanel _soundsListStackPanel;
    private StackPanel _optionsPanel;
    private StackPanel _optionsTopPanel;
    private StackPanel _optionsTopLeftPanel;
    private StackPanel _optionsTopCenterPanel;
    private StackPanel _optionsTopRightPanel;
    private StackPanel _optionsBottomListenerPanel;
    private StackPanel _optionsBottomEmitterPanel;
    private StackPanel _optionsBottomDopplerPanel;

    private ListBox _soundsList;

    private Button _playButton;
    private Button _pauseButton;
    private Button _resumeButton;
    private Button _stopButton;
    private Button _apply3DButton;
    private Button _submitBufferButton;
    private Button _disposeButton;

    private CheckBox _playAppliesVolumePanPitchCheckBox;
    private CheckBox _isLoopedCheckBox;
    private Label _stopOptionsLabel;
    private RadioButton _stopDefaultRadioButton;
    private RadioButton _stopAsAuthoredRadioButton;
    private RadioButton _stopImmediateRadioButton;
    private CheckBox _autoApply3DCheckBox;
    private CheckBox _autoSubmitBuffersCheckBox;

    private ValueAdjuster _volumeAdjuster;
    private ValueAdjuster _panAdjuster;
    private ValueAdjuster _pitchAdjuster;
    private ValueAdjuster _masterVolumeAdjuster;
    private ValueAdjuster _distanceScaleAdjuster;
    private ValueAdjuster _dopplerScaleAdjuster;
    private ValueAdjuster _speedOfSoundAdjuster;

    private Vector3Adjuster _listenerPositionAdjuster;
    private Vector3Adjuster _listenerVelocityAdjuster;
    private Vector3Adjuster _listenerOrientationAdjuster;

    private Vector3Adjuster _emitterPositionAdjuster;
    private Vector3Adjuster _emitterVelocityAdjuster;
    private Vector3Adjuster _emitterOrientationAdjuster;
    private Label _emitterDopplerScaleLabel;
    private ValueAdjuster _emitterDopplerScaleAdjuster;

    private SoundData _selectedSoundData;

    public XnaApiTestingGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;

        _graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
#if (ANDROID || iOS)
            _graphics.IsFullScreen = true;
#endif

        Content.RootDirectory = "Content";

        Window.AllowUserResizing = true;
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        GumService.Default.Initialize(this, DefaultVisualsVersion.V3);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        SoundEffect.DistanceScale = 1000;

        _stackPanel = new();
        _stackPanel.Orientation = Orientation.Horizontal;
        _stackPanel.Dock(Dock.FillVertically);
        _stackPanel.Spacing = 10;
        _stackPanel.AddToRoot();

        _soundsListStackPanel = new();
        _soundsListStackPanel.Orientation = Orientation.Vertical;
        _soundsListStackPanel.Dock(Dock.FillVertically);
        _soundsListStackPanel.Spacing = 10;
        _stackPanel.AddChild(_soundsListStackPanel);

        _soundsList = new();
        _soundsList.Dock(Dock.FillVertically);
        _soundsList.Anchor(Anchor.TopLeft);
        _soundsList.X = 10;
        _soundsList.Y = 10;
        _soundsList.Width = 350;
        _soundsList.Height = -100;
        _soundsListStackPanel.AddChild(_soundsList);

        AddSound("rock_loop_mono.wav", 44100, AudioChannels.Mono);
#if !BLAZORGL
        AddSound("rock_loop_stereo_44hz_8bit.wav", 44100, AudioChannels.Stereo); // TODO
#endif
        AddSound("bark_mono_44hz_16bit.wav", 44100, AudioChannels.Mono);
        AddSound("tone_mono_44khz_16bit.xnb");
        AddSound("tone_stereo_44khz_msadpcm.xnb", 44100, AudioChannels.Stereo);

        _soundsList.SelectedIndex = 0;

        _optionsPanel = new();
        _optionsPanel.Orientation = Orientation.Vertical;
        _optionsPanel.X = 10;
        _optionsPanel.Y = 10;
        _optionsPanel.Spacing = 5;
        _stackPanel.AddChild(_optionsPanel);

        _optionsTopPanel = new();
        _optionsTopPanel.Orientation = Orientation.Horizontal;
        _optionsPanel.AddChild(_optionsTopPanel);

        _optionsTopLeftPanel = new();
        _optionsTopLeftPanel.Orientation = Orientation.Vertical;
        _optionsTopLeftPanel.Spacing = 5;
        _optionsTopPanel.AddChild(_optionsTopLeftPanel);

        _playButton = new() { Text = "Play" };
        _playButton.Click += (s, e) => _selectedSoundData.Play();
        _optionsTopLeftPanel.AddChild(_playButton);

        _pauseButton = new() { Text = "Pause" };
        _pauseButton.Click += (s, e) => _selectedSoundData.Pause();
        _optionsTopLeftPanel.AddChild(_pauseButton);

        _resumeButton = new() { Text = "Resume" };
        _resumeButton.Click += (s, e) => _selectedSoundData.Resume();
        _optionsTopLeftPanel.AddChild(_resumeButton);

        _stopButton = new() { Text = "Stop" };
        _stopButton.Click += (s, e) => _selectedSoundData.Stop();
        _optionsTopLeftPanel.AddChild(_stopButton);

        _apply3DButton = new() { Text = "Apply 3D" };
        _apply3DButton.Click += (s, e) => _selectedSoundData.Apply3D();
        _optionsTopLeftPanel.AddChild(_apply3DButton);

        _submitBufferButton = new() { Text = "Submit Buffer" };
        _submitBufferButton.Click += (s, e) => _selectedSoundData.SubmitBuffer();
        _optionsTopLeftPanel.AddChild(_submitBufferButton);

        _disposeButton = new() { Text = "Dispose" };
        _disposeButton.Click += (s, e) => _selectedSoundData.Dispose();
        _optionsTopLeftPanel.AddChild(_disposeButton);

        _optionsTopCenterPanel = new();
        _optionsTopCenterPanel.Orientation = Orientation.Vertical;
        _optionsTopCenterPanel.X = 90;
        _optionsTopCenterPanel.Y = 10;
        _optionsTopCenterPanel.Spacing = 10;
        _optionsTopPanel.AddChild(_optionsTopCenterPanel);

        _playAppliesVolumePanPitchCheckBox = new() { Text = "Play Applies Volume/Pan/Pitch" };
        _playAppliesVolumePanPitchCheckBox.Width = 200;
        _playAppliesVolumePanPitchCheckBox.Click += (s, e) => _selectedSoundData.PlayAppliesVolumePanPitch = _playAppliesVolumePanPitchCheckBox.IsChecked.Value;
        _optionsTopCenterPanel.AddChild(_playAppliesVolumePanPitchCheckBox);

        _isLoopedCheckBox = new() { Text = "IsLooped" };
        _isLoopedCheckBox.Click += (s, e) => _selectedSoundData.IsLooped = _isLoopedCheckBox.IsChecked.Value;
        _optionsTopCenterPanel.AddChild(_isLoopedCheckBox);

        _stopOptionsLabel = new() { Text = "Stop Options" };
        _optionsTopCenterPanel.AddChild(_stopOptionsLabel);

        _stopDefaultRadioButton = new() { Text = "Default" };
        _stopDefaultRadioButton.Y = -10;
        _stopDefaultRadioButton.Click += (s, e) => _selectedSoundData.AudioStopOption = null;
        _optionsTopCenterPanel.AddChild(_stopDefaultRadioButton);

        _stopAsAuthoredRadioButton = new() { Text = "As Authored" };
        _stopAsAuthoredRadioButton.Y = -7;
        _stopAsAuthoredRadioButton.Click += (s, e) => _selectedSoundData.AudioStopOption = AudioStopOptions.AsAuthored;
        _optionsTopCenterPanel.AddChild(_stopAsAuthoredRadioButton);

        _stopImmediateRadioButton = new() { Text = "Immediate" };
        _stopImmediateRadioButton.Y = -7;
        _stopImmediateRadioButton.Click += (s, e) => _selectedSoundData.AudioStopOption = AudioStopOptions.Immediate;
        _optionsTopCenterPanel.AddChild(_stopImmediateRadioButton);

        _autoApply3DCheckBox = new() { Text = "Auto Apply 3D\nEach Frame" };
        _autoApply3DCheckBox.Y = 2;
        _autoApply3DCheckBox.Width = 200;
        _autoApply3DCheckBox.Click += (s, e) => _selectedSoundData.AutoApply3D = _autoApply3DCheckBox.IsChecked.Value;
        _optionsTopCenterPanel.AddChild(_autoApply3DCheckBox);

        _autoSubmitBuffersCheckBox = new();
        _autoSubmitBuffersCheckBox.Text = "Auto Submit Buffers";
        _autoSubmitBuffersCheckBox.Width = 200;
        _autoSubmitBuffersCheckBox.Click += (s, e) => _selectedSoundData.AutoSubmitBuffers = _autoSubmitBuffersCheckBox.IsChecked.Value;
        _optionsTopCenterPanel.AddChild(_autoSubmitBuffersCheckBox);

        _optionsTopRightPanel = new();
        _optionsTopRightPanel.Orientation = Orientation.Vertical;
        _optionsTopRightPanel.X = 25;
        _optionsTopRightPanel.Spacing = 5;
        _optionsTopPanel.AddChild(_optionsTopRightPanel);

        _volumeAdjuster = new("Volume", 150, 0d, 1d, 0.01d);
        _volumeAdjuster.ValueChanged += (value) => _selectedSoundData.Volume = (float)value;
        _optionsTopRightPanel.AddChild(_volumeAdjuster.StackPanel);
        _panAdjuster = new("Pan", 150, -1d, 1d, 0.01d);
        _panAdjuster.ValueChanged += (value) => _selectedSoundData.Pan = (float)value;
        _optionsTopRightPanel.AddChild(_panAdjuster.StackPanel);
        _pitchAdjuster = new("Pitch", 150, -1d, 1d, 0.01d);
        _pitchAdjuster.ValueChanged += (value) => _selectedSoundData.Pitch = (float)value;
        _optionsTopRightPanel.AddChild(_pitchAdjuster.StackPanel);

        _masterVolumeAdjuster = new("Master Volume", 150, 0d, 1d, 0.01d);
        _masterVolumeAdjuster.StackPanel.Y = 30;
        _masterVolumeAdjuster.ValueChanged += (value) => SoundEffect.MasterVolume = (float)value;
        _optionsTopRightPanel.AddChild(_masterVolumeAdjuster.StackPanel);
        _distanceScaleAdjuster = new("Distance Scale", 150, 0.01d, 10000d, 1d);
        _distanceScaleAdjuster.ValueChanged += (value) => SoundEffect.DistanceScale = (float)value;
        _optionsTopRightPanel.AddChild(_distanceScaleAdjuster.StackPanel);
        _dopplerScaleAdjuster = new("Doppler Scale", 150, 0.01d, 2d, 0.01d);
        _dopplerScaleAdjuster.ValueChanged += (value) => SoundEffect.DopplerScale = (float)value;
        _optionsTopRightPanel.AddChild(_dopplerScaleAdjuster.StackPanel);
        _speedOfSoundAdjuster = new("Speed Of Sound", 150, 0.5d, 1000d, 0.5d);
        _speedOfSoundAdjuster.ValueChanged += (value) => SoundEffect.SpeedOfSound = (float)value;
        _optionsTopRightPanel.AddChild(_speedOfSoundAdjuster.StackPanel);

        _optionsBottomListenerPanel = new();
        _optionsBottomListenerPanel.Orientation = Orientation.Horizontal;
        _optionsBottomListenerPanel.Y = 30;
        _optionsBottomListenerPanel.Spacing = 5;
        _optionsPanel.AddChild(_optionsBottomListenerPanel);

        _listenerPositionAdjuster = new("Listener Position", -10000d, 10000d, 1d);
        _listenerPositionAdjuster.ValueChanged += (value) => _selectedSoundData.AudioListener.Position = value;
        _optionsBottomListenerPanel.AddChild(_listenerPositionAdjuster.StackPanel);
        _listenerVelocityAdjuster = new("Listener Velocity", -1000d, 1000d, 1d);
        _listenerVelocityAdjuster.ValueChanged += (value) => _selectedSoundData.AudioListener.Velocity = value;
        _optionsBottomListenerPanel.AddChild(_listenerVelocityAdjuster.StackPanel);
        _listenerOrientationAdjuster = new("Listener Orientation (Pitch/Yaw/Roll)", -180d, 180d, 1d);
        _listenerOrientationAdjuster.ValueChanged += (value) => _selectedSoundData.AudioListenerOrientation = value;
        _optionsBottomListenerPanel.AddChild(_listenerOrientationAdjuster.StackPanel);

        _optionsBottomEmitterPanel = new();
        _optionsBottomEmitterPanel.Orientation = Orientation.Horizontal;
        _optionsBottomEmitterPanel.Y = 30;
        _optionsBottomEmitterPanel.Spacing = 5;
        _optionsPanel.AddChild(_optionsBottomEmitterPanel);

        _emitterPositionAdjuster = new("Emitter Position", -10000d, 10000d, 1d);
        _emitterPositionAdjuster.ValueChanged += (value) => _selectedSoundData.AudioEmitter.Position = value;
        _optionsBottomEmitterPanel.AddChild(_emitterPositionAdjuster.StackPanel);
        _emitterVelocityAdjuster = new("Emitter Velocity", -1000d, 1000d, 1d);
        _emitterVelocityAdjuster.ValueChanged += (value) => _selectedSoundData.AudioEmitter.Velocity = value;
        _optionsBottomEmitterPanel.AddChild(_emitterVelocityAdjuster.StackPanel);
        _emitterOrientationAdjuster = new("Emitter Orientation (Pitch/Yaw/Roll)", -180d, 180d, 1d);
        _emitterOrientationAdjuster.ValueChanged += (value) => _selectedSoundData.AudioEmitterOrientation = value;
        _optionsBottomEmitterPanel.AddChild(_emitterOrientationAdjuster.StackPanel);

        _optionsBottomDopplerPanel = new();
        _optionsBottomDopplerPanel.Orientation = Orientation.Vertical;
        _optionsBottomDopplerPanel.Y = 10;
        _optionsBottomDopplerPanel.Spacing = 5;
        _optionsPanel.AddChild(_optionsBottomDopplerPanel);

        _emitterDopplerScaleLabel = new() { Text = "Emitter Doppler Scale" };
        _optionsBottomDopplerPanel.AddChild(_emitterDopplerScaleLabel);

        _emitterDopplerScaleAdjuster = new("", 20, 0.01d, 2d, 0.01d);
        _emitterDopplerScaleAdjuster.ValueChanged += (value) => _selectedSoundData.AudioEmitter.DopplerScale = (float)value;
        _optionsBottomDopplerPanel.AddChild(_emitterDopplerScaleAdjuster.StackPanel);
    }

    protected override void UnloadContent()
    {
    }

    protected override void Update(GameTime gameTime)
    {
        MouseState mouseState = Mouse.GetState();
        KeyboardState keyboardState = Keyboard.GetState();
        GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

        GraphicalUiElement.CanvasWidth = Window.ClientBounds.Width;
        GraphicalUiElement.CanvasHeight = Window.ClientBounds.Height;
        GumService.Default.Update(gameTime);

        for (int i = 0; i < _soundsList.Items.Count; i++)
        {
            ListBoxItem listBoxItem = _soundsList.Items[i] as ListBoxItem;
            SoundData soundData = listBoxItem.BindingContext as SoundData;
            soundData.Update();
        }

        _selectedSoundData = (_soundsList.SelectedObject as ListBoxItem)?.BindingContext as SoundData;

        bool adjustableAtPlayback = _selectedSoundData.Type != SoundData.SoundType.SoundEffect;

        _pauseButton.IsEnabled = adjustableAtPlayback;
        _resumeButton.IsEnabled = adjustableAtPlayback;
        _stopButton.IsEnabled = adjustableAtPlayback;
        _apply3DButton.IsEnabled = adjustableAtPlayback;
        _submitBufferButton.IsEnabled = _selectedSoundData.Type == SoundData.SoundType.DynamicSoundEffectInstance;
        _disposeButton.IsEnabled = adjustableAtPlayback;

        _playAppliesVolumePanPitchCheckBox.IsEnabled = _selectedSoundData.Type == SoundData.SoundType.SoundEffect;
        _playAppliesVolumePanPitchCheckBox.IsChecked = _selectedSoundData.PlayAppliesVolumePanPitch;

        _isLoopedCheckBox.IsEnabled = adjustableAtPlayback;
        _isLoopedCheckBox.IsChecked = _selectedSoundData.IsLooped;

        _stopDefaultRadioButton.IsEnabled = adjustableAtPlayback;
        _stopDefaultRadioButton.IsChecked = _selectedSoundData.AudioStopOption == null;
        _stopAsAuthoredRadioButton.IsEnabled = adjustableAtPlayback;
        _stopAsAuthoredRadioButton.IsChecked = _selectedSoundData.AudioStopOption == AudioStopOptions.AsAuthored;
        _stopImmediateRadioButton.IsEnabled = adjustableAtPlayback;
        _stopImmediateRadioButton.IsChecked = _selectedSoundData.AudioStopOption == AudioStopOptions.Immediate;

        _autoApply3DCheckBox.IsEnabled = adjustableAtPlayback;
        _autoApply3DCheckBox.IsChecked = _selectedSoundData.AutoApply3D;
        _autoSubmitBuffersCheckBox.IsEnabled = _selectedSoundData.Type == SoundData.SoundType.DynamicSoundEffectInstance;
        _autoSubmitBuffersCheckBox.IsChecked = _selectedSoundData.AutoSubmitBuffers;

        _volumeAdjuster.Value = _selectedSoundData.Volume;
        _panAdjuster.Value = _selectedSoundData.Pan;
        _pitchAdjuster.Value = _selectedSoundData.Pitch;

        _masterVolumeAdjuster.Value = SoundEffect.MasterVolume;
        _distanceScaleAdjuster.Value = SoundEffect.DistanceScale;
        _dopplerScaleAdjuster.Value = SoundEffect.DopplerScale;
        _speedOfSoundAdjuster.Value = SoundEffect.SpeedOfSound;

        _listenerPositionAdjuster.IsEnabled = adjustableAtPlayback;
        _listenerPositionAdjuster.Value = _selectedSoundData.AudioListener.Position;
        _listenerVelocityAdjuster.IsEnabled = adjustableAtPlayback;
        _listenerVelocityAdjuster.Value = _selectedSoundData.AudioListener.Velocity;
        _listenerOrientationAdjuster.IsEnabled = adjustableAtPlayback;
        _listenerOrientationAdjuster.Value = _selectedSoundData.AudioListenerOrientation;

        _emitterPositionAdjuster.IsEnabled = adjustableAtPlayback;
        _emitterPositionAdjuster.Value = _selectedSoundData.AudioEmitter.Position;
        _emitterVelocityAdjuster.IsEnabled = adjustableAtPlayback;
        _emitterVelocityAdjuster.Value = _selectedSoundData.AudioEmitter.Velocity;
        _emitterOrientationAdjuster.IsEnabled = adjustableAtPlayback;
        _emitterOrientationAdjuster.Value = _selectedSoundData.AudioEmitterOrientation;

        _emitterDopplerScaleAdjuster.IsEnabled = adjustableAtPlayback;
        _emitterDopplerScaleAdjuster.Value = _selectedSoundData.AudioEmitter.DopplerScale;

        base.Update(gameTime);
    }

    public void AddSound(string name, int? sampleRate = null, AudioChannels? audioChannels = null)
    {
        SoundData soundEffectData = new(Content, name, SoundData.SoundType.SoundEffect);
        _soundsList.Items.Add(soundEffectData.ListBoxItem);

        SoundData soundEffectInstanceData = new(Content, name, SoundData.SoundType.SoundEffectInstance);
        _soundsList.Items.Add(soundEffectInstanceData.ListBoxItem);

        if (name.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
        {
            SoundData dynamicSoundEffectInstanceData = new(Content, name, SoundData.SoundType.DynamicSoundEffectInstance, sampleRate, audioChannels);
            _soundsList.Items.Add(dynamicSoundEffectInstanceData.ListBoxItem);
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        GumService.Default.Draw();
        base.Draw(gameTime);
    }
}
