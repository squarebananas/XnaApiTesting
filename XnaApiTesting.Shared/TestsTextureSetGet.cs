using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Platform.Graphics.Utilities;
using Gum.Forms.Controls;
using MonoGameGum;

namespace XnaApiTesting;

public class TestsTextureSetGet
{
    private StackPanel _stackPanel;

    private StackPanel _typeColumn;
    private EnumRadioSelector _typeSelector;
    private EnumRadioSelector _cubeFaceSelector;

    private StackPanel _createColumn;
    private Label _createLabel;
    private ComboBox _loadComboBox;
    private ValueAdjuster _newWidthAdjuster;
    private ValueAdjuster _newHeightAdjuster;
    private ValueAdjuster _newDepthAdjuster;
    private CheckBox _newMipmapsCheckBox;
    private ComboBox _newSurfaceFormatComboBox;
    private Button _createButton;
    private Button _loadButton;

    private StackPanel _setDataColumn;
    private Label _setDataLabel;
    private ValueAdjuster _setDataLevelAdjuster;
    private ValueAdjuster _setDataXAdjuster;
    private ValueAdjuster _setDataYAdjuster;
    private ValueAdjuster _setDataZAdjuster;
    private ValueAdjuster _setDataWidthAdjuster;
    private ValueAdjuster _setDataHeightAdjuster;
    private ValueAdjuster _setDataDepthAdjuster;
    private ValueAdjuster _setDataOffsetAdjuster;
    private ValueAdjuster _setDataCountAdjuster;
    private ComboBox _setDataFillTypeComboBox;
    private Button _setDataFixInvalidButton;
    private Button _setDataButton;

    private StackPanel _getDataColumn;
    private Label _getDataLabel;
    private ValueAdjuster _getDataLevelAdjuster;
    private ValueAdjuster _getDataXAdjuster;
    private ValueAdjuster _getDataYAdjuster;
    private ValueAdjuster _getDataZAdjuster;
    private ValueAdjuster _getDataWidthAdjuster;
    private ValueAdjuster _getDataHeightAdjuster;
    private ValueAdjuster _getDataDepthAdjuster;
    private ValueAdjuster _getDataOffsetAdjuster;
    private ValueAdjuster _getDataCountAdjuster;
    private Button _getDataFixInvalidButton;
    private Button _getDataButton;

    public enum TextureType
    {
        Texture2D,
        Texture3D,
        TextureCube,
        RenderTarget2D,
        RenderTarget3D,
        RenderTargetCube
    }
    private TextureType _selectedTextureType;

    private CubeMapFace _selectedCubeMapFace;
    private Dictionary<string, string> _textureNameToAssetName;

    public enum FillType
    {
        RgbGradient,
        Checkers,
        Random,
        Red,
        Green,
        Blue,
        White,
        Black,
        Transparent,
        RedAlpha50
    }
    private FillType _selectedFillType;

    private Texture2D _texture2D;
    private Texture3D _texture3D;
    private TextureCube _textureCube;
    private RenderTarget2D _renderTarget2D;
    private RenderTarget3D _renderTarget3D;
    private RenderTargetCube _renderTargetCube;

    private Texture2D _getDataTexture2D;
    private Texture3D _getDataTexture3D;
    private Texture2D _getDataTextureCubeFace;

    private Texture _currentTexture => _selectedTextureType switch
    {
        TextureType.Texture2D => _texture2D,
        TextureType.Texture3D => _texture3D,
        TextureType.TextureCube => _textureCube,
        TextureType.RenderTarget2D => _renderTarget2D,
        TextureType.RenderTarget3D => _renderTarget3D,
        TextureType.RenderTargetCube => _renderTargetCube
    };

    private ContentManager[] _assetContentManagers;
    private SpriteBatch _spriteBatch;
    private Effect _effectTexture3D;
    private Effect _effectTextureCube;
    private SamplerState[] _mipLevelSamplerStates;
    private VertexPosition[] _cubeVertices;
    private int[] _cubeIndices;

    public TestsTextureSetGet(GraphicsDevice graphicsDevice, ContentManager contentManager)
    {
        _stackPanel = new();
        _stackPanel.Orientation = Orientation.Horizontal;
        _stackPanel.X = 10;
        _stackPanel.Y = 10;
        _stackPanel.Spacing = 10;
        _stackPanel.AddToRoot();

        _typeColumn = new StackPanel { Orientation = Orientation.Vertical, Spacing = 3 };
        _stackPanel.AddChild(_typeColumn);

        _typeSelector = new EnumRadioSelector("Type:", 200, typeof(TextureType), TextureType.Texture2D,
            (value) => ChangeTextureType((TextureType)value));
        _typeColumn.AddChild(_typeSelector.StackPanel);

        _cubeFaceSelector = new EnumRadioSelector("CubeMapFace:", 200, typeof(CubeMapFace), CubeMapFace.PositiveX,
            (value) => _selectedCubeMapFace = (CubeMapFace)value);
        _cubeFaceSelector.StackPanel.Y = 10;
        _typeColumn.AddChild(_cubeFaceSelector.StackPanel);

        _createColumn = new StackPanel { Orientation = Orientation.Vertical, Spacing = 3 };
        _stackPanel.AddChild(_createColumn);
        _createLabel = new() { Text = "Create New / Load" };
        _createColumn.AddChild(_createLabel);

        _loadComboBox = new();
        _loadComboBox.Width = 321;
        _loadComboBox.SelectionChanged += (s, e) => ShowCreateOrLoad();
        _createColumn.AddChild(_loadComboBox);

        _newWidthAdjuster = new("Width:", 60, 1, 2048, 1, integerValue: true);
        _newWidthAdjuster.Value = 32;
        _newWidthAdjuster.ValueChanged += (value) => { };
        _createColumn.AddChild(_newWidthAdjuster.StackPanel);
        _newHeightAdjuster = new("Height:", 60, 1, 2048, 1, integerValue: true);
        _newHeightAdjuster.Value = 32;
        _newHeightAdjuster.ValueChanged += (value) => { };
        _createColumn.AddChild(_newHeightAdjuster.StackPanel);
        _newDepthAdjuster = new("Depth:", 60, 1, 2048, 1, integerValue: true);
        _newDepthAdjuster.Value = 32;
        _newDepthAdjuster.ValueChanged += (value) => { };
        _createColumn.AddChild(_newDepthAdjuster.StackPanel);

        _newMipmapsCheckBox = new() { Text = "Mipmaps" };
        _newMipmapsCheckBox.X = 221;
        _newMipmapsCheckBox.Y = 2;
        _newMipmapsCheckBox.IsChecked = true;
        _createColumn.AddChild(_newMipmapsCheckBox);

        SurfaceFormat[] surfaceFormats = [
            SurfaceFormat.Color,
            SurfaceFormat.Vector4,
            SurfaceFormat.Dxt1];

        _newSurfaceFormatComboBox = new() { Text = "SurfaceFormat" };
        _newSurfaceFormatComboBox.Width = 321;
        _newSurfaceFormatComboBox.Items = surfaceFormats;
        _newSurfaceFormatComboBox.SelectedIndex = 0;
        _createColumn.AddChild(_newSurfaceFormatComboBox);

        _createButton = new() { Text = "Create" };
        _createButton.Width = 321;
        _createButton.Click += (s, e) => CreateTexture();
        _createColumn.AddChild(_createButton);

        _loadButton = new() { Text = "Load" };
        _loadButton.IsVisible = false;
        _loadButton.Width = 321;
        _loadButton.Click += (s, e) => LoadTexture(contentManager);
        _createColumn.AddChild(_loadButton);

        _setDataColumn = new StackPanel { Orientation = Orientation.Vertical, Spacing = 3 };
        _stackPanel.AddChild(_setDataColumn);
        _setDataLabel = new() { Text = "Set Data" };
        _setDataColumn.AddChild(_setDataLabel);

        _setDataLevelAdjuster = new("Level:", 60, 0, 15, 1, integerValue: true);
        _setDataLevelAdjuster.Value = 0;
        _setDataLevelAdjuster.ValueChanged += (value) => { };
        _setDataColumn.AddChild(_setDataLevelAdjuster.StackPanel);
        _setDataXAdjuster = new("X:", 60, 0, 2048, 1, integerValue: true);
        _setDataXAdjuster.Value = 0;
        _setDataColumn.AddChild(_setDataXAdjuster.StackPanel);
        _setDataYAdjuster = new("Y:", 60, 0, 2048, 1, integerValue: true);
        _setDataYAdjuster.Value = 0;
        _setDataColumn.AddChild(_setDataYAdjuster.StackPanel);
        _setDataZAdjuster = new("Z:", 60, 0, 2048, 1, integerValue: true);
        _setDataZAdjuster.Value = 0;
        _setDataColumn.AddChild(_setDataZAdjuster.StackPanel);

        _setDataWidthAdjuster = new("Width:", 60, 1, 2048, 1, integerValue: true);
        _setDataWidthAdjuster.Value = 256;
        _setDataWidthAdjuster.ValueChanged += (value) => { };
        _setDataColumn.AddChild(_setDataWidthAdjuster.StackPanel);
        _setDataHeightAdjuster = new("Height:", 60, 1, 2048, 1, integerValue: true);
        _setDataHeightAdjuster.Value = 256;
        _setDataColumn.AddChild(_setDataHeightAdjuster.StackPanel);
        _setDataDepthAdjuster = new("Depth:", 60, 1, 2048, 1, integerValue: true);
        _setDataDepthAdjuster.Value = 256;
        _setDataColumn.AddChild(_setDataDepthAdjuster.StackPanel);

        _setDataOffsetAdjuster = new("Offset:", 60, 0, 2048, 1, integerValue: true);
        _setDataOffsetAdjuster.Value = 0;
        _setDataColumn.AddChild(_setDataOffsetAdjuster.StackPanel);
        _setDataCountAdjuster = new("Count:", 60, 0, 1000000000, 1, integerValue: true);
        _setDataCountAdjuster.Value = 256;
        _setDataColumn.AddChild(_setDataCountAdjuster.StackPanel);

        List<FillType> fillTypes = new();
        foreach (object value in Enum.GetValues(typeof(FillType)))
            fillTypes.Add((FillType)value);

        _setDataFillTypeComboBox = new();
        _setDataFillTypeComboBox.Width = 321;
        _setDataFillTypeComboBox.Items = fillTypes;
        _setDataFillTypeComboBox.SelectedIndex = 0;
        _setDataFillTypeComboBox.SelectionChanged += (s, e) => _selectedFillType = (FillType)_setDataFillTypeComboBox.SelectedObject;
        _setDataColumn.AddChild(_setDataFillTypeComboBox);

        _setDataFixInvalidButton = new() { Text = "Fix Invalid Values" };
        _setDataFixInvalidButton.Width = 321;
        _setDataFixInvalidButton.Click += (s, e) => CheckInvalidValues(fixInvalidSetValues: true);
        _setDataColumn.AddChild(_setDataFixInvalidButton);

        _setDataButton = new() { Text = "Set Data" };
        _setDataButton.Width = 321;
        _setDataButton.Click += (s, e) => SetData();
        _setDataColumn.AddChild(_setDataButton);

        _getDataColumn = new StackPanel { Orientation = Orientation.Vertical, X = 10, Spacing = 3 };
        _stackPanel.AddChild(_getDataColumn);
        _getDataLabel = new() { Text = "Get Data" };
        _getDataColumn.AddChild(_getDataLabel);

        _getDataLevelAdjuster = new("Level:", 60, 0, 15, 1, integerValue: true);
        _getDataLevelAdjuster.Value = 0;
        _getDataColumn.AddChild(_getDataLevelAdjuster.StackPanel);
        _getDataXAdjuster = new("X:", 60, 0, 2048, 1, integerValue: true);
        _getDataXAdjuster.Value = 0;
        _getDataColumn.AddChild(_getDataXAdjuster.StackPanel);
        _getDataYAdjuster = new("Y:", 60, 0, 2048, 1, integerValue: true);
        _getDataYAdjuster.Value = 0;
        _getDataColumn.AddChild(_getDataYAdjuster.StackPanel);
        _getDataZAdjuster = new("Z:", 60, 0, 2048, 1, integerValue: true);
        _getDataZAdjuster.Value = 0;
        _getDataColumn.AddChild(_getDataZAdjuster.StackPanel);

        _getDataWidthAdjuster = new("Width:", 60, 1, 2048, 1, integerValue: true);
        _getDataWidthAdjuster.Value = 256;
        _getDataColumn.AddChild(_getDataWidthAdjuster.StackPanel);
        _getDataHeightAdjuster = new("Height:", 60, 1, 2048, 1, integerValue: true);
        _getDataHeightAdjuster.Value = 256;
        _getDataColumn.AddChild(_getDataHeightAdjuster.StackPanel);
        _getDataDepthAdjuster = new("Depth:", 60, 1, 2048, 1, integerValue: true);
        _getDataDepthAdjuster.Value = 256;
        _getDataColumn.AddChild(_getDataDepthAdjuster.StackPanel);

        _getDataOffsetAdjuster = new("Offset:", 60, 0, 2048, 1, integerValue: true);
        _getDataOffsetAdjuster.Value = 0;
        _getDataColumn.AddChild(_getDataOffsetAdjuster.StackPanel);
        _getDataCountAdjuster = new("Count:", 60, 0, 1000000000, 1, integerValue: true);
        _getDataCountAdjuster.Value = 256;
        _getDataColumn.AddChild(_getDataCountAdjuster.StackPanel);

        _getDataFixInvalidButton = new() { Text = "Fix Invalid Values" };
        _getDataFixInvalidButton.Width = 321;
        _getDataFixInvalidButton.Click += (s, e) => CheckInvalidValues(fixInvalidGetValues: true);
        _getDataColumn.AddChild(_getDataFixInvalidButton);

        _getDataButton = new() { Text = "Get Data" };
        _getDataButton.Width = 321;
        _getDataButton.Click += (s, e) => GetData();
        _getDataColumn.AddChild(_getDataButton);

        ChangeTextureType(TextureType.Texture2D);

        _assetContentManagers = new ContentManager[6];
        _spriteBatch = new SpriteBatch(graphicsDevice);
        _effectTexture3D = contentManager.Load<Effect>("EffectTexture3D");
        _effectTextureCube = contentManager.Load<Effect>("EffectTextureCube");

        _cubeVertices =
        [
            new VertexPosition(new Vector3(-1, -1, -1)),
            new VertexPosition(new Vector3(1, -1, -1)),
            new VertexPosition(new Vector3(1, 1, -1)),
            new VertexPosition(new Vector3(-1, 1, -1)),
            new VertexPosition(new Vector3(-1, -1, 1)),
            new VertexPosition(new Vector3(1, -1, 1)),
            new VertexPosition(new Vector3(1, 1, 1)),
            new VertexPosition(new Vector3(-1, 1, 1))
        ];

        _cubeIndices = [
            0, 1, 2, 0, 2, 3,  // Back face
            1, 5, 6, 1, 6, 2,  // Right face
            5, 4, 7, 5, 7, 6,  // Front face
            4, 0, 3, 4, 3, 7,  // Left face
            3, 2, 6, 3, 6, 7,  // Top face
            4, 5, 1, 4, 1, 0]; // Bottom face

        _mipLevelSamplerStates = new SamplerState[16];
        for (int i = 0; i < _mipLevelSamplerStates.Length; i++)
        {
            _mipLevelSamplerStates[i] = new SamplerState
            {
                Filter = TextureFilter.Point,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                MaxMipLevel = i
            };
            typeof(SamplerState).GetProperty("MinMipLevel")?.SetValue(_mipLevelSamplerStates[i], i);
        }
    }

    private void ChangeTextureType(TextureType textureType)
    {
        _selectedTextureType = textureType;

        bool typeIs3D = _selectedTextureType == TextureType.Texture3D || _selectedTextureType == TextureType.RenderTarget3D;
        bool typeIsCube = _selectedTextureType == TextureType.TextureCube || _selectedTextureType == TextureType.RenderTargetCube;

        _cubeFaceSelector.IsVisible = (_currentTexture as TextureCube) != null;
        _newWidthAdjuster.Name = typeIsCube ? "Size:" : "Width:";
        _newDepthAdjuster.IsVisible = typeIs3D && (_loadComboBox.SelectedIndex == 0);
        _setDataDepthAdjuster.IsVisible = _setDataZAdjuster.IsVisible =
            _getDataDepthAdjuster.IsVisible = _getDataZAdjuster.IsVisible = typeIs3D;

        switch (_selectedTextureType)
        {
            case TextureType.Texture2D:
            case TextureType.RenderTarget2D:
                _textureNameToAssetName = new()
                {
                    { "Create New", null },
                    { "Grate Color No Mipmaps", "grate_color" },
                    { "Grate Color Mipmaps", "grate_color_mipmaps" },
                    { "Grate DXT1 No Mipmaps", "grate_compressed" },
                    { "Grate DXT1 Mipmaps", "grate_compressed_mipmaps" }
                };
                break;

                case TextureType.Texture3D:
                case TextureType.RenderTarget3D:
                    _textureNameToAssetName = new()
                    {
                        { "Create New", null }
                    };
                    break;

                case TextureType.TextureCube:
                case TextureType.RenderTargetCube:
                    _textureNameToAssetName = new()
                    {
                        { "Create New", null },
                        { "Cubemap Color No Mipmaps", "cubemap_color" },
                        { "Cubemap Color Mipmaps", "cubemap_color_mipmaps" },
                        { "Cubemap DXT1 No Mipmaps", "cubemap_compressed" },
                        { "Cubemap DXT1 Mipmaps", "cubemap_compressed_mipmaps" }
                    };
                    break;
        }

        string[] textureNames = [.. _textureNameToAssetName.Keys];
        _loadComboBox.Items = textureNames;
        _loadComboBox.SelectedIndex = 0;
    }

    private void ShowCreateOrLoad()
    {
        if (_loadComboBox.SelectedIndex == 0)
        {
            bool typeIs3D = _selectedTextureType == TextureType.Texture3D || _selectedTextureType == TextureType.RenderTarget3D;
            bool typeIsCube = _selectedTextureType == TextureType.TextureCube || _selectedTextureType == TextureType.RenderTargetCube;

            _newWidthAdjuster.IsVisible = true;
            _newHeightAdjuster.IsVisible = !typeIsCube;
            _newDepthAdjuster.IsVisible = typeIs3D;
            _newMipmapsCheckBox.IsVisible = true;
            _newSurfaceFormatComboBox.IsVisible = true;
            _createButton.IsVisible = true;
            _loadButton.IsVisible = false;
        }
        else
        {
            _newWidthAdjuster.IsVisible = false;
            _newHeightAdjuster.IsVisible = false;
            _newDepthAdjuster.IsVisible = false;
            _newMipmapsCheckBox.IsVisible = false;
            _newSurfaceFormatComboBox.IsVisible = false;
            _createButton.IsVisible = false;
            _loadButton.IsVisible = true;
        }
    }

    private void CreateTexture()
    {
        try
        {
            int width = (int)_newWidthAdjuster.Value;
            int height = (int)_newHeightAdjuster.Value;
            int depth = (int)_newDepthAdjuster.Value;
            bool mipmaps = _newMipmapsCheckBox.IsChecked.Value;
            SurfaceFormat surfaceFormat = (SurfaceFormat)_newSurfaceFormatComboBox.SelectedObject;

            switch (_selectedTextureType)
            {
                case TextureType.Texture2D:
                    _texture2D?.Dispose();
                    _texture2D = new Texture2D(_spriteBatch.GraphicsDevice, width, height, mipmaps, surfaceFormat);
                    break;

                case TextureType.Texture3D:
                    _texture3D?.Dispose();
                    _texture3D = new Texture3D(_spriteBatch.GraphicsDevice, width, height, depth, mipmaps, surfaceFormat);
                    break;

                case TextureType.TextureCube:
                    _textureCube?.Dispose();
                    _textureCube = new TextureCube(_spriteBatch.GraphicsDevice, width, mipmaps, surfaceFormat);
                    break;

                case TextureType.RenderTarget2D:
                    _renderTarget2D?.Dispose();
                    _renderTarget2D = new RenderTarget2D(_spriteBatch.GraphicsDevice, width, height, mipmaps, surfaceFormat,
                        DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
                    break;

                case TextureType.RenderTarget3D:
                    _renderTarget3D?.Dispose();
                    _renderTarget3D = new RenderTarget3D(_spriteBatch.GraphicsDevice, width, height, depth, mipmaps, surfaceFormat,
                        DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
                    break;

                case TextureType.RenderTargetCube:
                    _renderTargetCube?.Dispose();
                    _renderTargetCube = new RenderTargetCube(_spriteBatch.GraphicsDevice, width, mipmaps, surfaceFormat,
                        DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
                    break;
            }

            if (_currentTexture.Format != surfaceFormat)
                throw new Exception($"Unable to create texture with SurfaceFormat {surfaceFormat}.");

            switch (_selectedTextureType)
            {
                case TextureType.Texture2D:
                case TextureType.RenderTarget2D:
                    for (int i = 0; i < _currentTexture.LevelCount; i++)
                    {
                        int levelWidth = Math.Max(1, width >> i);
                        int levelHeight = Math.Max(1, height >> i);
                        SetDataForLevel(i, 0, 0, 0, levelWidth, levelHeight, 1, 0, null);
                    }
                    break;

                case TextureType.Texture3D:
                case TextureType.RenderTarget3D:
                    for (int i = 0; i < _currentTexture.LevelCount; i++)
                    {
                        int levelWidth = Math.Max(1, width >> i);
                        int levelHeight = Math.Max(1, height >> i);
                        int levelDepth = Math.Max(1, depth >> i);
                        SetDataForLevel(i, 0, 0, 0, levelWidth, levelHeight, levelDepth, 0, null);
                    }
                    break;

                case TextureType.TextureCube:
                case TextureType.RenderTargetCube:
                    for (int i = 0; i < _currentTexture.LevelCount; i++)
                    {
                        int levelSize = Math.Max(1, width >> i);
                        for (int j = 0; j < 6; j++)
                            SetDataForLevel(i, 0, 0, j, levelSize, levelSize, 1, 0, null);
                    }
                    break;
            }
        }
        catch(Exception ex)
        {
            PopupWindow.ShowMessage(ex.Message);
        }

        CheckInvalidValues(fixInvalidSetValues: true, fixInvalidGetValues: true);
    }

    private void LoadTexture(ContentManager contentManager)
    {
        _assetContentManagers[(int)_selectedTextureType]?.Dispose();
        ContentManager assetContentManager = new ContentManager(contentManager.ServiceProvider, contentManager.RootDirectory);
        string assetName = _textureNameToAssetName[_loadComboBox.SelectedObject.ToString()];

        try
        {
            switch (_selectedTextureType)
            {
                case TextureType.Texture2D:
                    _texture2D?.Dispose();
                    _texture2D = assetContentManager.Load<Texture2D>(assetName);
                    break;

                case TextureType.RenderTarget2D:
                    Texture2D texture2D = assetContentManager.Load<Texture2D>(assetName);
                    _renderTarget2D?.Dispose();
                    _renderTarget2D = new RenderTarget2D(_spriteBatch.GraphicsDevice, texture2D.Width, texture2D.Height,
                        texture2D.LevelCount > 1, texture2D.Format, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

                    if (_renderTarget2D.Format != texture2D.Format)
                        throw new Exception($"Unable to create texture with SurfaceFormat {texture2D.Format}.");

                    for (int level = 0; level < texture2D.LevelCount; level++)
                    {
                        int levelWidth = Math.Max(1, texture2D.Width >> level);
                        int levelHeight = Math.Max(1, texture2D.Height >> level);
                        int dataSize = levelWidth * levelHeight * 4;
                        byte[] data = new byte[dataSize];
                        texture2D.GetData(level, new Rectangle(0, 0, levelWidth, levelHeight), data, 0, data.Length);
                        _renderTarget2D.SetData(level, new Rectangle(0, 0, levelWidth, levelHeight), data, 0, data.Length);
                    }
                    break;

                case TextureType.TextureCube:
                    _textureCube?.Dispose();
                    _textureCube = assetContentManager.Load<TextureCube>(assetName);
                    break;

                case TextureType.RenderTargetCube:
                    TextureCube textureCube = assetContentManager.Load<TextureCube>(assetName);
                    _renderTargetCube?.Dispose();
                    _renderTargetCube = new RenderTargetCube(_spriteBatch.GraphicsDevice, textureCube.Size,
                        textureCube.LevelCount > 1, textureCube.Format, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

                    if (_renderTargetCube.Format != textureCube.Format)
                        throw new Exception($"Unable to create texture with SurfaceFormat {textureCube.Format}.");

                    for (int face = 0; face < 6; face++)
                    {
                        for (int level = 0; level < textureCube.LevelCount; level++)
                        {
                            int levelSize = Math.Max(1, textureCube.Size >> level);
                            int dataSize = levelSize * levelSize * 4;
                            if (textureCube.Format == SurfaceFormat.Dxt1)
                            {
                                int blockCount = (levelSize + 3) / 4;
                                dataSize = blockCount * blockCount * 8;
                            }
                            byte[] data = new byte[dataSize];
                            textureCube.GetData((CubeMapFace)face, level, new Rectangle(0, 0, levelSize, levelSize), data, 0, data.Length);
                            _renderTargetCube.SetData((CubeMapFace)face, level, new Rectangle(0, 0, levelSize, levelSize), data, 0, data.Length);
                        }
                    }
                    break;
            }

            _assetContentManagers[(int)_selectedTextureType] = assetContentManager;
            CheckInvalidValues(fixInvalidSetValues: true, fixInvalidGetValues: true);
        }
        catch (Exception ex)
        {
            PopupWindow.ShowMessage(ex.Message);
        }
    }

    public Color[] GenerateFillData(int level, int width, int height, int depth)
    {
        Color[] fillData = new Color[width * height * depth];

        float levelScale = 1f / (1 << level);
        Random random = new(0);
        Color color = default;

        for (int i = 0; i < fillData.Length; i++)
        {
            int x = i % width;
            int y = (i / width) % height;
            int z = i / (width * height);

            switch (_selectedFillType)
            {
                case FillType.RgbGradient:
                    color = new Color((float)x / width, (float)y / height, (float)z / depth);
                    break;

                case FillType.Checkers:
                    int checkerSize = 16;
                    int levelCheckerSize = Math.Max(1, (int)(checkerSize * levelScale));
                    color = (((x / levelCheckerSize) + (y / levelCheckerSize) + (z / levelCheckerSize)) % 2 == 0) ? Color.White : Color.Black;
                    break;

                case FillType.Random:
                    color = new Color(random.Next(256), random.Next(256), random.Next(256), 255);
                    break;

                case FillType.Red: color = Color.Red; break;
                case FillType.Green: color = Color.Green; break;
                case FillType.Blue: color = Color.Blue; break;
                case FillType.White: color = Color.White; break;
                case FillType.Black: color = Color.Black; break;
                case FillType.Transparent: color = Color.Transparent; break;
                case FillType.RedAlpha50: color = new Color(255, 0, 0, 128); break;
            }

            fillData[i] = color;
        }

        return fillData;
    }

    public void EncodeDxt1Block(Color[] pixels, Span<byte> output)
    {
        Vector3 minColor = new(float.MaxValue);
        Vector3 maxColor = new(float.MinValue);
        for (int i = 0; i < 16; i++)
        {
            Vector3 pixel = pixels[i].ToVector3();
            minColor = Vector3.Min(minColor, pixel);
            maxColor = Vector3.Max(maxColor, pixel);
        }

        Bgr565 color0 = new(maxColor);
        Bgr565 color1 = new(minColor);

        Vector3[] palette = new Vector3[4];
        palette[0] = color0.ToVector3();
        palette[1] = color1.ToVector3();
        if (color0.PackedValue > color1.PackedValue)
        {
            palette[2] = ((2f * palette[0]) + palette[1]) / 3f;
            palette[3] = (palette[0] + (2f * palette[1])) / 3f;
        }
        else
        {
            palette[2] = (palette[0] + palette[1]) * 0.5f;
            palette[3] = Vector3.Zero;
        }

        uint indices = 0;
        for (int i = 0; i < 16; i++)
        {
            Vector3 pixel = pixels[i].ToVector3();
            int closestIndex = 0;
            float closestError = float.MaxValue;
            for (int p = 0; p < 4; p++)
            {
                Vector3 d = pixel - palette[p];
                float error = Vector3.Dot(d, d);
                if (error < closestError)
                {
                    closestIndex = p;
                    closestError = error;
                }
            }
            indices |= (uint)(closestIndex << (i * 2));
        }

        BitConverter.TryWriteBytes(output.Slice(0, 2), color0.PackedValue);
        BitConverter.TryWriteBytes(output.Slice(2, 2), color1.PackedValue);
        BitConverter.TryWriteBytes(output.Slice(4, 4), indices);
    }

    public void SetData()
    {
        try
        {
            int level = (int)_setDataLevelAdjuster.Value;
            int x = (int)_setDataXAdjuster.Value;
            int y = (int)_setDataYAdjuster.Value;
            int z = (int)_setDataZAdjuster.Value;
            int width = (int)_setDataWidthAdjuster.Value;
            int height = (int)_setDataHeightAdjuster.Value;
            int depth = (int)_setDataDepthAdjuster.Value;
            int offset = (int)_setDataOffsetAdjuster.Value;
            int count = (int)_setDataCountAdjuster.Value;

            if (_currentTexture is Texture2D)
            {
                z = 0;
                depth = 1;
            }
            if (_currentTexture is TextureCube)
            {
                z = (int)_selectedCubeMapFace;
                depth = 1;
            }

            SetDataForLevel(level, x, y, z, width, height, depth, offset, count);
        }
        catch (Exception ex)
        {
            PopupWindow.ShowMessage(ex.Message);
        }
    }

    public void SetDataForLevel(int level, int x, int y, int z, int width, int height, int depth, int offset, int? count)
    {
        Color[] fillData = GenerateFillData(level, width, height, depth);

        switch (_currentTexture.Format)
        {
            case SurfaceFormat.Color:
                Color[] colorData = fillData;
                if (offset >= 1)
                {
                    colorData = new Color[fillData.Length + offset];
                    Array.Copy(fillData, 0, colorData, offset, fillData.Length);
                }
                SetDataForTextureType(level, x, y, z, width, height, depth, colorData, offset, count ?? colorData.Length);
                break;

            case SurfaceFormat.Vector4:
                Vector4[] vector4Data = new Vector4[fillData.Length + offset];
                for (int j = 0; j < fillData.Length; j++)
                    vector4Data[j + offset] = fillData[j].ToVector4();
                SetDataForTextureType(level, x, y, z, width, height, depth, vector4Data, offset, count ?? vector4Data.Length);
                break;

            case SurfaceFormat.Dxt1:
                int blockCountX = (width + 3) / 4;
                int blockCountY = (height + 3) / 4;
                int dxt1DataSize = blockCountX * blockCountY * depth * 8;
                byte[] dxt1Data = new byte[dxt1DataSize + offset];
                Color[] blockData = new Color[16];
                for (int sliceZ = 0; sliceZ < depth; sliceZ++)
                {
                    for (int blockY = 0; blockY < blockCountY; blockY++)
                    {
                        for (int blockX = 0; blockX < blockCountX; blockX++)
                        {
                            for (int i = 0; i < blockData.Length; i++)
                            {
                                int dataX = (blockX * 4) + (i % 4);
                                int dataY = (blockY * 4) + (i / 4);
                                if ((dataX < width) && (dataY < height))
                                    blockData[i] = fillData[dataX + (dataY * width) + (sliceZ * width * height)];
                                else
                                    blockData[i] = Color.Transparent;
                            }
                            int blockIndex = blockX + (blockY * blockCountX) + (sliceZ * blockCountX * blockCountY);
                            EncodeDxt1Block(blockData, dxt1Data.AsSpan((blockIndex * 8) + offset, 8));
                        }
                    }
                }

#if DESKTOPGL
                if (depth >= 2)
                {
                    // OpenGL has differing vendor requirements for compressed Texture3D
                    // For example Nvidia internally stores DXT in slice/4 > column > row > slice%4 order
                    // To get around this upload all slices for one single block at a time
                    byte[] blockAllSlicesData = new byte[(8 * depth) + offset];
                    for (int blockY = 0; blockY < blockCountY; blockY++)
                    {
                        for (int blockX = 0; blockX < blockCountX; blockX++)
                        {
                            for (int sliceZ = 0; sliceZ < depth; sliceZ++)
                            {
                                for (int i = 0; i < 8; i++)
                                {
                                    blockAllSlicesData[(sliceZ * 8) + i + offset] =
                                        dxt1Data[((blockX + (blockY * blockCountX) + (sliceZ * blockCountX * blockCountY)) * 8) + i + offset];
                                }
                            }
                            SetDataForTextureType(level, x + (blockX * 4), y + (blockY * 4), z,
                                Math.Min(4, width), Math.Min(4, height), depth, blockAllSlicesData, offset,
                                (count != null) ? count.Value / (blockCountX * blockCountY) : blockAllSlicesData.Length);
                        }
                    }
                }
                else
                {
                    SetDataForTextureType(level, x, y, z, width, height, depth, dxt1Data, offset, (count ?? dxt1Data.Length));
                }
#else
                SetDataForTextureType(level, x, y, z, width, height, depth, dxt1Data, offset, (count ?? dxt1Data.Length));
#endif
                break;
        }
    }

    public void SetDataForTextureType<T>(int level, int x, int y, int z, int width, int height, int depth,
        T[] data, int offset, int count) where T : struct
    {
        switch (_selectedTextureType)
        {
            case TextureType.Texture2D:
            case TextureType.RenderTarget2D:
                Texture2D texture2D = _currentTexture as Texture2D;
                texture2D.SetData(level, new Rectangle(x, y, width, height), data, offset, count);
                break;

            case TextureType.Texture3D:
            case TextureType.RenderTarget3D:
                Texture3D texture3D = _currentTexture as Texture3D;
                texture3D.SetData(level, x, y, (x + width), (y + height), z, (z + depth), data, offset, count);
                break;

            case TextureType.TextureCube:
            case TextureType.RenderTargetCube:
                TextureCube textureCube = _currentTexture as TextureCube;
                textureCube.SetData((CubeMapFace)z, level, new Rectangle(x, y, width, height), data, offset, count);
                break;
        }
    }

    public void GetData()
    {
        try
        {
            int level = (int)_getDataLevelAdjuster.Value;
            int x = (int)_getDataXAdjuster.Value;
            int y = (int)_getDataYAdjuster.Value;
            int z = (int)_getDataZAdjuster.Value;
            int width = (int)_getDataWidthAdjuster.Value;
            int height = (int)_getDataHeightAdjuster.Value;
            int depth = (int)_getDataDepthAdjuster.Value;
            int offset = (int)_getDataOffsetAdjuster.Value;
            int count = (int)_getDataCountAdjuster.Value;

            Texture2D texture2D = _currentTexture as Texture2D;
            Texture3D texture3D = _currentTexture as Texture3D;
            TextureCube textureCube = _currentTexture as TextureCube;
            Rectangle rectangle = new(x, y, width, height);
            byte[] byteData = [0];
            int offsetInBytes = offset * _currentTexture.Format.GetSize();

            switch (_currentTexture.Format)
            {
                case SurfaceFormat.Color:
                    Color[] colorData = new Color[offset + count];
                    if (_currentTexture is Texture2D)
                        texture2D.GetData(level, rectangle, colorData, offset, count);
                    if (_currentTexture is Texture3D)
                        texture3D.GetData(level, x, y, x + width, y + height, z, z + depth, colorData, offset, count);
                    if (_currentTexture is TextureCube)
                        textureCube.GetData(_selectedCubeMapFace, level, rectangle, colorData, offset, count);
                    byteData = System.Runtime.InteropServices.MemoryMarshal.AsBytes(colorData.AsSpan()).ToArray();
                    break;

                case SurfaceFormat.Vector4:
                    Vector4[] vector4Data = new Vector4[offset + count];
                    if (_currentTexture is Texture2D)
                        texture2D.GetData(level, rectangle, vector4Data, offset, count);
                    if (_currentTexture is Texture3D)
                        texture3D.GetData(level, x, y, x + width, y + height, z, z + depth, vector4Data, offset, count);
                    if (_currentTexture is TextureCube)
                        textureCube.GetData(_selectedCubeMapFace, level, rectangle, vector4Data, offset, count);
                    byteData = System.Runtime.InteropServices.MemoryMarshal.AsBytes(vector4Data.AsSpan()).ToArray();
                    break;

                case SurfaceFormat.Dxt1:
                    offsetInBytes = offset;
                    byteData = new byte[offsetInBytes + count];
                    if (_currentTexture is Texture2D)
                        texture2D.GetData(level, rectangle, byteData, offset, count);
                    if (_currentTexture is Texture3D)
                        texture3D.GetData(level, x, y, x + width, y + height, z, z + depth, byteData, offset, count);
                    if (_currentTexture is TextureCube)
                        textureCube.GetData(_selectedCubeMapFace, level, rectangle, byteData, offset, count);
                    break;
            }

            int byteCount = byteData.Length - offsetInBytes;
            switch (_selectedTextureType)
            {
                case TextureType.Texture2D:
                case TextureType.RenderTarget2D:
                    _getDataTexture2D = new Texture2D(_spriteBatch.GraphicsDevice, width, height, false, _currentTexture.Format);
                    _getDataTexture2D.SetData(byteData, offsetInBytes, byteCount);
                    break;

                case TextureType.Texture3D:
                case TextureType.RenderTarget3D:
                    _getDataTexture3D = new Texture3D(_spriteBatch.GraphicsDevice, width, height, depth, false, _currentTexture.Format);
                    _getDataTexture3D.SetData(byteData, offsetInBytes, byteCount);
                    break;

                case TextureType.TextureCube:
                case TextureType.RenderTargetCube:
                    _getDataTextureCubeFace = new Texture2D(_spriteBatch.GraphicsDevice, width, height, false, _currentTexture.Format);
                    _getDataTextureCubeFace.SetData(byteData, offsetInBytes, byteCount);
                    break;
            }
        }
        catch (Exception ex)
        {
            PopupWindow.ShowMessage(ex.Message + '\n' + ex.StackTrace);
        }
    }

    public void CheckInvalidValues(bool fixInvalidSetValues = false, bool fixInvalidGetValues = false)
    {
        Color validColor = new(74, 74, 74);
        Color invalidColor = Color.Red;

        _cubeFaceSelector.IsVisible = (_currentTexture as TextureCube) != null;
        _setDataColumn.IsVisible = _getDataColumn.IsVisible = _currentTexture != null;

        int levelLimit = int.MaxValue;
        int widthLimit = int.MaxValue;
        int heightLimit = int.MaxValue;
        int depthLimit = int.MaxValue;
        SurfaceFormat surfaceFormat = default;

        if (_currentTexture != null)
        {
            levelLimit = _currentTexture.LevelCount;
            surfaceFormat = _currentTexture.Format;

            switch (_selectedTextureType)
            {
                case TextureType.Texture2D:
                case TextureType.RenderTarget2D:
                    Texture2D texture2D = _currentTexture as Texture2D;
                    widthLimit = texture2D.Width;
                    heightLimit = texture2D.Height;
                    break;

                case TextureType.Texture3D:
                case TextureType.RenderTarget3D:
                    Texture3D texture3D = _currentTexture as Texture3D;
                    widthLimit = texture3D.Width;
                    heightLimit = texture3D.Height;
                    depthLimit = texture3D.Depth;
                    break;

                case TextureType.TextureCube:
                case TextureType.RenderTargetCube:
                    TextureCube textureCube = _currentTexture as TextureCube;
                    widthLimit = textureCube.Size;
                    heightLimit = textureCube.Size;
                    break;
            }
        }

        bool valid = true;

        valid &= CheckAdjuster(_setDataLevelAdjuster, 0, levelLimit - 1, fixInvalidSetValues);
        int levelWidthLimit = Math.Max(1, widthLimit >> (int)_setDataLevelAdjuster.Value);
        int levelHeightLimit = Math.Max(1, heightLimit >> (int)_setDataLevelAdjuster.Value);
        int levelDepthLimit = Math.Max(1, depthLimit >> (int)_setDataLevelAdjuster.Value);
        valid &= CheckAdjuster(_setDataXAdjuster, 0, levelWidthLimit - 1, fixInvalidSetValues);
        valid &= CheckAdjuster(_setDataYAdjuster, 0, levelHeightLimit - 1, fixInvalidSetValues);
        if (_currentTexture is Texture3D)
            valid &= CheckAdjuster(_setDataZAdjuster, 0, levelDepthLimit - 1, fixInvalidSetValues);

        int remainingWidthLimit = levelWidthLimit - (int)_setDataXAdjuster.Value;
        int remainingHeightLimit = levelHeightLimit - (int)_setDataYAdjuster.Value;
        int remainingDepthLimit = levelDepthLimit - (int)_setDataZAdjuster.Value;
        valid &= CheckAdjuster(_setDataWidthAdjuster, 0, remainingWidthLimit, fixInvalidSetValues);
        valid &= CheckAdjuster(_setDataHeightAdjuster, 0, remainingHeightLimit, fixInvalidSetValues);
        if (_currentTexture is Texture3D)
            valid &= CheckAdjuster(_setDataDepthAdjuster, 0, remainingDepthLimit, fixInvalidSetValues);

        int selectedWidth = (int)_setDataWidthAdjuster.Value;
        int selectedHeight = (int)_setDataHeightAdjuster.Value;
        int selectedDepth = (_currentTexture is Texture3D) ? (int)_setDataDepthAdjuster.Value : 1;

        int expectedCount = selectedWidth * selectedHeight;
        if (surfaceFormat == SurfaceFormat.Dxt1)
        {
            int blockCountX = (selectedWidth + 3) / 4;
            int blockCountY = (selectedHeight + 3) / 4;
            expectedCount = blockCountX * blockCountY * 8;
        }
        expectedCount *= selectedDepth;
        valid &= CheckAdjuster(_setDataCountAdjuster, expectedCount, expectedCount, fixInvalidSetValues);

        _setDataFixInvalidButton.IsEnabled = !valid;

        valid = true;

        valid &= CheckAdjuster(_getDataLevelAdjuster, 0, levelLimit - 1, fixInvalidGetValues);
        levelWidthLimit = Math.Max(1, widthLimit >> (int)_getDataLevelAdjuster.Value);
        levelHeightLimit = Math.Max(1, heightLimit >> (int)_getDataLevelAdjuster.Value);
        levelDepthLimit = Math.Max(1, depthLimit >> (int)_getDataLevelAdjuster.Value);
        valid &= CheckAdjuster(_getDataXAdjuster, 0, levelWidthLimit - 1, fixInvalidGetValues);
        valid &= CheckAdjuster(_getDataYAdjuster, 0, levelHeightLimit - 1, fixInvalidGetValues);
        if (_currentTexture is Texture3D)
            valid &= CheckAdjuster(_getDataZAdjuster, 0, levelDepthLimit - 1, fixInvalidGetValues);

        remainingWidthLimit = levelWidthLimit - (int)_getDataXAdjuster.Value;
        remainingHeightLimit = levelHeightLimit - (int)_getDataYAdjuster.Value;
        remainingDepthLimit = levelDepthLimit - (int)_getDataZAdjuster.Value;
        valid &= CheckAdjuster(_getDataWidthAdjuster, 0, remainingWidthLimit, fixInvalidGetValues);
        valid &= CheckAdjuster(_getDataHeightAdjuster, 0, remainingHeightLimit, fixInvalidGetValues);
        if (_currentTexture is Texture3D)
            valid &= CheckAdjuster(_getDataDepthAdjuster, 0, remainingDepthLimit, fixInvalidGetValues);

        selectedWidth = (int)_getDataWidthAdjuster.Value;
        selectedHeight = (int)_getDataHeightAdjuster.Value;
        selectedDepth = (_currentTexture is Texture3D) ? (int)_getDataDepthAdjuster.Value : 1;

        expectedCount = selectedWidth * selectedHeight;
        if (surfaceFormat == SurfaceFormat.Dxt1)
        {
            int blockCountX = (selectedWidth + 3) / 4;
            int blockCountY = (selectedHeight + 3) / 4;
            expectedCount = blockCountX * blockCountY * 8;
        }
        expectedCount *= selectedDepth;
        valid &= CheckAdjuster(_getDataCountAdjuster, expectedCount, expectedCount, fixInvalidGetValues);

        _getDataFixInvalidButton.IsEnabled = !valid;
    }

    public bool CheckAdjuster(ValueAdjuster adjuster, int? min, int? max, bool fixInvalidValue)
    {
        Color validColor = new(74, 74, 74);
        Color invalidColor = Color.Red;

        int value = (int)adjuster.Value;
        bool valueTooLow = min.HasValue && value < min.Value;
        bool valueTooHigh = max.HasValue && value > max.Value;

        if (fixInvalidValue)
        {
            if (valueTooLow)
                adjuster.Value = min.Value;
            valueTooLow = false;
            if (valueTooHigh)
                adjuster.Value = max.Value;
            valueTooHigh = false;
        }

        bool valid = !valueTooLow && !valueTooHigh;
        adjuster.TextBoxBackgroundColor = valid ? validColor : invalidColor;
        return valid;
    }

    public void Update()
    {
        CheckInvalidValues();
    }

    public void Draw(GraphicsDevice graphicsDevice, GameTime gameTime)
    {
        //if (_renderTarget2D != null)
        //{
        //    graphicsDevice.SetRenderTarget(_renderTarget2D);
        //    graphicsDevice.SetRenderTarget(null);
        //    graphicsDevice.Clear(Color.CornflowerBlue);
        //}

        if (_currentTexture is Texture2D)
        {
            Texture2D texture = _currentTexture as Texture2D;
            Vector2 position = new Vector2(10, 350);
            int maxDimension = Math.Max(texture.Width, texture.Height);
            Vector2 scale = 256f * new Vector2(texture.Width, texture.Height) / maxDimension;

            for (int i = 0; i < texture.LevelCount; i++)
            {
                float levelScale = 1f / (1 << i);
                int gap = 5;
                if (i == 1)
                    position += new Vector2(scale.X + gap, 0);
                if (i >= 2)
                    position.Y += (scale.Y * levelScale * 2f) + gap;

                _spriteBatch.Begin(samplerState: _mipLevelSamplerStates[i]);
                _spriteBatch.Draw(texture, new Rectangle((int)position.X, (int)position.Y, (int)(scale.X * levelScale), (int)(scale.Y * levelScale)),
                   new Rectangle(0, 0, texture.Width, texture.Height), Color.White);
                _spriteBatch.End();
            }

            if (_getDataTexture2D != null)
            {
                maxDimension = Math.Max(_getDataTexture2D.Width, _getDataTexture2D.Height);
                scale = 256f * new Vector2(_getDataTexture2D.Width, _getDataTexture2D.Height) / maxDimension;

                _spriteBatch.Begin(samplerState: _mipLevelSamplerStates[0]);
                _spriteBatch.Draw(_getDataTexture2D, new Rectangle((int)_getDataButton.Visual.AbsoluteLeft, 350, (int)scale.X, (int)scale.Y), Color.White);
                _spriteBatch.End();
            }
        }

        if (_currentTexture is Texture3D)
        {
            Texture3D texture = _currentTexture as Texture3D;
            int maxDimension = Math.Max(texture.Width, Math.Max(texture.Height, texture.Depth));
            Vector3 scale = 100f * new Vector3(texture.Width, texture.Height, texture.Depth) / maxDimension;
            Vector3 position = new(200f, 530f, 0f);
            Matrix rotationMatrix = Matrix.CreateRotationY(MathHelper.ToRadians(30)) * Matrix.CreateRotationX(MathHelper.ToRadians(30));
            Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 200), Vector3.Zero, Vector3.Up);
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, 0, 0.1f, 1000f);

            graphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            for (int i = 0; i < _currentTexture.LevelCount; i++)
            {
                float levelScale = 1f / (1 << i);
                if (i == 1)
                    position += new Vector3(250f, -80f, 0f);
                if (i >= 2)
                    position += new Vector3((levelScale * -120f), (levelScale * 400f) + 10f, 0f);
                Matrix world = Matrix.CreateScale(scale * levelScale) * rotationMatrix * Matrix.CreateTranslation(position);

                foreach (EffectPass pass in _effectTexture3D.CurrentTechnique.Passes)
                {
                    _effectTexture3D.Parameters["xWorldViewProjection"].SetValue(world * view * projection);
                    _effectTexture3D.Parameters["xTexture3D"].SetValue(texture);
                    _effectTexture3D.Parameters["xColor"].SetValue(Color.White.ToVector4() * 0.5f);
                    _effectTexture3D.Parameters["xOffsetZ"].SetValue(0);
                    pass.Apply();
                    graphicsDevice.SamplerStates[0] = _mipLevelSamplerStates[i];
                    graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _cubeVertices, 0, _cubeVertices.Length,
                        _cubeIndices, 0, _cubeIndices.Length / 3);

                    _effectTexture3D.Parameters["xColor"].SetValue(Color.White.ToVector4());
                    _effectTexture3D.Parameters["xOffsetZ"].SetValue(-((float)gameTime.TotalGameTime.TotalSeconds % 2f));
                    pass.Apply();
                    graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _cubeVertices, 0, _cubeVertices.Length,
                        _cubeIndices, 12, 2);
                }
            }

            if (_getDataTexture3D != null)
            {
                maxDimension = Math.Max(_getDataTexture3D.Width, Math.Max(_getDataTexture3D.Height, _getDataTexture3D.Depth));
                scale = 100f * new Vector3(_getDataTexture3D.Width, _getDataTexture3D.Height, _getDataTexture3D.Depth) / maxDimension;
                position = new(1065f, 530f, 0f);
                Matrix world = Matrix.CreateScale(scale) * rotationMatrix * Matrix.CreateTranslation(position);

                _effectTexture3D.Parameters["xWorldViewProjection"].SetValue(world * view * projection);
                _effectTexture3D.Parameters["xTexture3D"].SetValue(_getDataTexture3D);
                _effectTexture3D.Parameters["xColor"].SetValue(Color.White.ToVector4() * 0.5f);
                _effectTexture3D.Parameters["xOffsetZ"].SetValue(0);
                _effectTexture3D.CurrentTechnique.Passes[0].Apply();
                graphicsDevice.SamplerStates[0] = _mipLevelSamplerStates[0];
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _cubeVertices, 0, _cubeVertices.Length,
                    _cubeIndices, 0, _cubeIndices.Length / 3);

                _effectTexture3D.Parameters["xColor"].SetValue(Color.White.ToVector4());
                _effectTexture3D.Parameters["xOffsetZ"].SetValue(-((float)gameTime.TotalGameTime.TotalSeconds % 2f));
                _effectTexture3D.CurrentTechnique.Passes[0].Apply();
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _cubeVertices, 0, _cubeVertices.Length,
                    _cubeIndices, 12, 2);
            }
        }

        if (_currentTexture is TextureCube)
        {
            TextureCube texture = _currentTexture as TextureCube;
            Vector3 position = new(200f, 530f, 0f);
            Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 200), Vector3.Zero, Vector3.Up);
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, 0, 0.1f, 1000f);

            Matrix rotationMatrix = _selectedCubeMapFace switch
            {
                CubeMapFace.PositiveX => Matrix.CreateRotationY(MathHelper.ToRadians(90f)),
                CubeMapFace.NegativeX => Matrix.CreateRotationY(MathHelper.ToRadians(-90f)),
                CubeMapFace.PositiveY => Matrix.CreateRotationX(MathHelper.ToRadians(90f)),
                CubeMapFace.NegativeY => Matrix.CreateRotationX(MathHelper.ToRadians(-90f)),
                CubeMapFace.PositiveZ => Matrix.Identity,
                CubeMapFace.NegativeZ => Matrix.CreateRotationY(MathHelper.ToRadians(-180f))
            };
            rotationMatrix *= Matrix.CreateRotationY(MathHelper.ToRadians(25f)) * Matrix.CreateRotationX(MathHelper.ToRadians(25f));

            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            for (int i = 0; i < _currentTexture.LevelCount; i++)
            {
                float levelScale = 1f / (1 << i);
                if (i == 1)
                    position += new Vector3(250f, -80f, 0f);
                if (i >= 2)
                    position += new Vector3((levelScale * -120f), (levelScale * 400f) + 10f, 0f);
                Matrix world = Matrix.CreateScale(100f * levelScale) * rotationMatrix * Matrix.CreateTranslation(position);

                _effectTextureCube.Parameters["xWorldViewProjection"].SetValue(world * view * projection);
                _effectTextureCube.Parameters["xTextureCube"].SetValue(texture);
                _effectTextureCube.CurrentTechnique.Passes[0].Apply();
                graphicsDevice.SamplerStates[0] = _mipLevelSamplerStates[i];
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _cubeVertices, 0, _cubeVertices.Length,
                    _cubeIndices, 0, _cubeIndices.Length / 3);
            }

            if (_getDataTextureCubeFace != null)
            {
                _spriteBatch.Begin(samplerState: _mipLevelSamplerStates[0]);
                _spriteBatch.Draw(_getDataTextureCubeFace, new Rectangle((int)_getDataButton.Visual.AbsoluteLeft, 350, 256, 256), Color.White);
                _spriteBatch.End();
            }
        }
    }

    public void Close()
    {
        _stackPanel.RemoveFromRoot();

        _spriteBatch.Dispose();
        _effectTexture3D.Dispose();
        for (int i= 0; i < _mipLevelSamplerStates.Length; i++)
            _mipLevelSamplerStates[i].Dispose();

        _texture2D?.Dispose();
        _texture3D?.Dispose();
        _textureCube?.Dispose();

        _renderTarget2D?.Dispose();
        _renderTarget3D?.Dispose();
        _renderTargetCube?.Dispose();

        _getDataTexture2D?.Dispose();
        _getDataTexture3D?.Dispose();
        _getDataTextureCubeFace?.Dispose();
    }
}
