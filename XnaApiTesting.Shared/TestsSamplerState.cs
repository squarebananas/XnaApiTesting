using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Gum.Forms.Controls;
using MonoGameGum;

namespace XnaApiTesting;

public class TestsSamplerState
{
    private StackPanel _stackPanel;
    private StackPanel _leftColumn;
    private ComboBox _textureComboBox;
    private EnumRadioSelector _filterSelector;
    private EnumRadioSelector _addressUSelector;
    private EnumRadioSelector _addressVSelector;
    private EnumRadioSelector _addressWSelector;
    private StackPanel adjustersColumn;
    private ValueAdjuster _minMipLevelAdjuster;
    private ValueAdjuster _maxMipLevelAdjuster;
    private ValueAdjuster _mipMapLodBiasAdjuster;
    private ValueAdjuster _maxAnisotropyAdjuster;

    private BasicEffect _effect;
    private VertexPositionTexture[] _vertices;
    private Texture2D _textureGrate;
    private Texture2D _textureColorMap;

    private SamplerState _samplerState;
    private SamplerState _boundSamplerState;

    public TestsSamplerState(GraphicsDevice graphicsDevice, ContentManager contentManager)
    {
        _stackPanel = new();
        _stackPanel.Orientation = Orientation.Horizontal;
        _stackPanel.X = 10;
        _stackPanel.Y = 10;
        _stackPanel.Spacing = 10;
        _stackPanel.AddToRoot();

        _leftColumn = new StackPanel { Orientation = Orientation.Vertical, Spacing = 15 };
        _stackPanel.AddChild(_leftColumn);

        _filterSelector = new EnumRadioSelector("Filter:", 300, typeof(TextureFilter), TextureFilter.Linear,
            (value) => { _samplerState.Filter = (TextureFilter)value; RecreateState(); });
        _leftColumn.AddChild(_filterSelector.StackPanel);

        _textureComboBox = new() { Text = "Texture" };
        _textureComboBox.Width = 250;
        _textureComboBox.Items = new string[] { "Grate", "Color Map" };
        _textureComboBox.SelectedIndex = 0;
        _leftColumn.AddChild(_textureComboBox);

        _addressUSelector = new EnumRadioSelector("AddressU:", 100, typeof(TextureAddressMode), TextureAddressMode.Wrap,
            (value) => { _samplerState.AddressU = (TextureAddressMode)value; RecreateState(); });
        _stackPanel.AddChild(_addressUSelector.StackPanel);
        _addressVSelector = new EnumRadioSelector("AddressV:", 100, typeof(TextureAddressMode), TextureAddressMode.Wrap,
            (value) => { _samplerState.AddressV = (TextureAddressMode)value; RecreateState(); });
        _stackPanel.AddChild(_addressVSelector.StackPanel);
        _addressWSelector = new EnumRadioSelector("AddressW:", 100, typeof(TextureAddressMode), TextureAddressMode.Wrap,
            (value) => { _samplerState.AddressW = (TextureAddressMode)value; RecreateState(); });
        _stackPanel.AddChild(_addressWSelector.StackPanel);

        adjustersColumn = new StackPanel { Orientation = Orientation.Vertical, Spacing = 3 };
        _stackPanel.AddChild(adjustersColumn);

        _minMipLevelAdjuster = new ValueAdjuster("Min Mip Level:", 250, 0, 16, 1, integerValue: true);
        _minMipLevelAdjuster.Value = 16;
        _minMipLevelAdjuster.ValueChanged += (value) =>
        {
            typeof(SamplerState).GetProperty("MinMipLevel")?.SetValue(_samplerState, (int)value);
            RecreateState();
        };
        adjustersColumn.AddChild(_minMipLevelAdjuster.StackPanel);

        _maxMipLevelAdjuster = new ValueAdjuster("Max Mip Level:", 250, 0, 16, 1, integerValue: true);
        _maxMipLevelAdjuster.Value = 0;
        _maxMipLevelAdjuster.ValueChanged += (value) => { _samplerState.MaxMipLevel = (int)value; RecreateState(); };
        adjustersColumn.AddChild(_maxMipLevelAdjuster.StackPanel);

        _mipMapLodBiasAdjuster = new ValueAdjuster("Mip Map Level Of Detail Bias:", 250, -16, 16, 0.1);
        _mipMapLodBiasAdjuster.Value = 0;
        _mipMapLodBiasAdjuster.ValueChanged += (value) => { _samplerState.MipMapLevelOfDetailBias = (float)value; RecreateState(); };
        adjustersColumn.AddChild(_mipMapLodBiasAdjuster.StackPanel);

        _maxAnisotropyAdjuster = new ValueAdjuster("Max Anisotropy:", 250, 1, 16, 1, integerValue: true);
        _maxAnisotropyAdjuster.Value = 4;
        _maxAnisotropyAdjuster.ValueChanged += (value) => { _samplerState.MaxAnisotropy = (int)value; RecreateState(); };
        adjustersColumn.AddChild(_maxAnisotropyAdjuster.StackPanel);

        _effect = new(graphicsDevice);

        _vertices = [
            new (new Vector3(1000, 0, 500), new Vector2(10, -9)),
            new (new Vector3(-1000, 0, 500), new Vector2(-9, -9)),
            new (new Vector3(1000, 0, -1500), new Vector2(10, 10)),
            new (new Vector3(-1000, 0, -1500), new Vector2(-9, 10)) ];

        _textureGrate = contentManager.Load<Texture2D>("grate_color_mipmaps");

        _textureColorMap = new Texture2D(graphicsDevice, 512, 512, true, SurfaceFormat.Color);
        for (int i = 0; i < _textureColorMap.LevelCount; i++)
        {
            Color color = i switch
            {
                0 => Color.Black,
                1 => Color.Brown,
                2 => Color.Red,
                3 => Color.Orange,
                4 => Color.Yellow,
                5 => Color.Green,
                6 => Color.Blue,
                7 => Color.Purple,
                8 => Color.Gray,
                _ => Color.White
            };
            Color[] data = new Color[Math.Max(1, _textureColorMap.Width >> i) * Math.Max(1, _textureColorMap.Height >> i)];
            Array.Fill(data, color);
            _textureColorMap.SetData(i, null, data, 0, data.Length);
        }

        _samplerState = new()
        {
            Filter = TextureFilter.Linear,
            AddressU = TextureAddressMode.Wrap,
            AddressV = TextureAddressMode.Wrap,
            AddressW = TextureAddressMode.Wrap,
            MaxAnisotropy = 4,
            MaxMipLevel = 0,
            MipMapLevelOfDetailBias = 0,
            FilterMode = TextureFilterMode.Default,
            ComparisonFunction = CompareFunction.Never
        };

        RecreateState();
    }

    private void RecreateState()
    {
        _boundSamplerState?.Dispose();

        _boundSamplerState = new SamplerState
        {
            Filter = _samplerState.Filter,
            AddressU = _samplerState.AddressU,
            AddressV = _samplerState.AddressV,
            AddressW = _samplerState.AddressW,
            MaxAnisotropy = _samplerState.MaxAnisotropy,
            MaxMipLevel = _samplerState.MaxMipLevel,
            MipMapLevelOfDetailBias = _samplerState.MipMapLevelOfDetailBias,
            FilterMode = _samplerState.FilterMode,
            ComparisonFunction = _samplerState.ComparisonFunction
        };

        PropertyInfo minMipLevelProperty = typeof(SamplerState).GetProperty("MinMipLevel");
        if (minMipLevelProperty != null)
        {
            int value = (int)minMipLevelProperty.GetValue(_samplerState);
            minMipLevelProperty.SetValue(_boundSamplerState, value);
        }
    }

    public void Update()
    {
    }

    public void Draw(GraphicsDevice graphicsDevice)
    {
        graphicsDevice.Clear(Color.CornflowerBlue);

        _effect.World = Matrix.CreateTranslation(Vector3.Zero);
        _effect.View = Matrix.CreateLookAt(new Vector3(0, 50, 100), new Vector3(0, 0, -1000), Vector3.Up);
        _effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90),
            graphicsDevice.Viewport.AspectRatio, 1f, 10000f);

        _effect.TextureEnabled = true;
        _effect.Texture = (_textureComboBox.SelectedIndex == 0) ? _textureGrate : _textureColorMap;

        foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.SamplerStates[0] = _boundSamplerState;
            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _vertices, 0, 2);
        }
    }

    public void Close()
    {
        _stackPanel.RemoveFromRoot();

        _effect.Dispose();
        _samplerState.Dispose();
        _boundSamplerState.Dispose();
    }
}
