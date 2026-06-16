using System;
using Microsoft.Xna.Framework;
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
    private Button _soundEffectButton;
    private Button _textureSetGetButton;
    private Button _samplerStateButton;
    private Button _graphicsDeviceButton;

    private TestsSoundEffect _testsSoundEffect;
    private TestsTextureSetGet _testsTextureSetGet;
    private TestsSamplerState _testsSamplerState;
    private TestsGraphicsDevice _testsGraphicsDevice;

    public XnaApiTestingGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.GraphicsProfile = GraphicsProfile.FL10_0;
        _graphics.PreferMultiSampling = false;

        _graphics.PreparingDeviceSettings += (s, e) =>
        {
            e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
            e.GraphicsDeviceInformation.PresentationParameters.UseDebugLayers = true;
        };

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
        _stackPanel = new();
        _stackPanel.Orientation = Orientation.Vertical;
        _stackPanel.Dock(Dock.FillVertically);
        _stackPanel.X = 10;
        _stackPanel.Y = 10;
        _stackPanel.Spacing = 10;
        _stackPanel.AddToRoot();

        _soundEffectButton = new();
        _soundEffectButton.Text = "SoundEffect Tests";
        _soundEffectButton.Click += (s, e) =>
        {
            _testsSoundEffect = new TestsSoundEffect(Content);
            _stackPanel.IsVisible = false;
        };
        _stackPanel.AddChild(_soundEffectButton);

        _textureSetGetButton = new();
        _textureSetGetButton.Text = "Texture\nSet/Get Tests";
        _textureSetGetButton.Click += (s, e) =>
        {
            _testsTextureSetGet = new TestsTextureSetGet(GraphicsDevice, Content);
            _stackPanel.IsVisible = false;
        };
        _stackPanel.AddChild(_textureSetGetButton);


        _samplerStateButton = new();
        _samplerStateButton.Text = "SamplerState Tests";
        _samplerStateButton.Click += (s, e) =>
        {
            _testsSamplerState = new TestsSamplerState(GraphicsDevice, Content);
            _stackPanel.IsVisible = false;
        };
        _stackPanel.AddChild(_samplerStateButton);

        _graphicsDeviceButton = new();
        _graphicsDeviceButton.Text = "GraphicsDevice Tests";
        _graphicsDeviceButton.Click += (s, e) =>
        {
            _testsGraphicsDevice = new TestsGraphicsDevice(GraphicsDevice, Content);
            _stackPanel.IsVisible = false;
        };
        _stackPanel.AddChild(_graphicsDeviceButton);
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

        _testsSoundEffect?.Update();
        _testsTextureSetGet?.Update();
        _testsSamplerState?.Update();
        _testsGraphicsDevice?.Update(GraphicsDevice);

        if (keyboardState.IsKeyDown(Keys.Escape) ||
            mouseState.XButton1 == ButtonState.Pressed)
        {
            _testsSoundEffect?.Close();
            _testsSoundEffect = null;
            _testsTextureSetGet?.Close();
            _testsTextureSetGet = null;
            _testsSamplerState?.Close();
            _testsSamplerState = null;
            _testsGraphicsDevice?.Close();
            _testsGraphicsDevice = null;

            Content.Unload();
            GC.Collect();

            _stackPanel.IsVisible = true;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _testsSoundEffect?.Draw();
        _testsTextureSetGet?.Draw(GraphicsDevice, gameTime);
        _testsSamplerState?.Draw(GraphicsDevice);
        _testsGraphicsDevice?.Draw(GraphicsDevice);

        GumService.Default.Draw();

        base.Draw(gameTime);
    }
}
