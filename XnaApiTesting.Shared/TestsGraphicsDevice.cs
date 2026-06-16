using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;
using Gum.Forms.Controls;
using MonoGameGum;

namespace XnaApiTesting;

public class TestsGraphicsDevice
{
    private StackPanel _stackPanel;

    private StackPanel _getDataColumn;
    private Label _getDataLabel;
    private ValueAdjuster _getDataXAdjuster;
    private ValueAdjuster _getDataYAdjuster;
    private ValueAdjuster _getDataWidthAdjuster;
    private ValueAdjuster _getDataHeightAdjuster;
    private ValueAdjuster _getDataOffsetAdjuster;
    private ValueAdjuster _getDataCountAdjuster;
    private Button _getDataFixInvalidButton;
    private Button _getDataButton;

    private SpriteBatch _spriteBatch;
    private Texture2D _whitePixel;
    private Texture2D _getDataTexture;

    public TestsGraphicsDevice(GraphicsDevice graphicsDevice, ContentManager contentManager)
    {
        _stackPanel = new();
        _stackPanel.Orientation = Orientation.Horizontal;
        _stackPanel.X = 10;
        _stackPanel.Y = 10;
        _stackPanel.Spacing = 10;
        _stackPanel.AddToRoot();

        _getDataColumn = new StackPanel { Orientation = Orientation.Vertical, Spacing = 3 };
        _stackPanel.AddChild(_getDataColumn);
        _getDataLabel = new() { Text = "Get Back Buffer Data" };
        _getDataColumn.AddChild(_getDataLabel);
        _getDataXAdjuster = new("X:", 60, 0, 2048, 1, integerValue: true);
        _getDataXAdjuster.Value = 0;
        _getDataColumn.AddChild(_getDataXAdjuster.StackPanel);
        _getDataYAdjuster = new("Y:", 60, 0, 2048, 1, integerValue: true);
        _getDataYAdjuster.Value = 0;
        _getDataColumn.AddChild(_getDataYAdjuster.StackPanel);
        _getDataWidthAdjuster = new("Width:", 60, 1, 2048, 1, integerValue: true);
        _getDataWidthAdjuster.Value = 256;
        _getDataColumn.AddChild(_getDataWidthAdjuster.StackPanel);
        _getDataHeightAdjuster = new("Height:", 60, 1, 2048, 1, integerValue: true);
        _getDataHeightAdjuster.Value = 256;
        _getDataColumn.AddChild(_getDataHeightAdjuster.StackPanel);
        _getDataOffsetAdjuster = new("Offset:", 60, 0, 2048, 1, integerValue: true);
        _getDataOffsetAdjuster.Value = 0;
        _getDataColumn.AddChild(_getDataOffsetAdjuster.StackPanel);
        _getDataCountAdjuster = new("Count:", 60, 0, 1000000000, 1, integerValue: true);
        _getDataCountAdjuster.Value = 256;
        _getDataColumn.AddChild(_getDataCountAdjuster.StackPanel);
        _getDataFixInvalidButton = new() { Text = "Fix Invalid Values" };
        _getDataFixInvalidButton.Width = 321;
        _getDataFixInvalidButton.Click += (s, e) => CheckInvalidValues(graphicsDevice, fixInvalidValues: true);
        _getDataColumn.AddChild(_getDataFixInvalidButton);
        _getDataButton = new() { Text = "Get Data" };
        _getDataButton.Width = 321;
        _getDataButton.Click += (s, e) => GetBackBufferData(graphicsDevice);
        _getDataColumn.AddChild(_getDataButton);

        _spriteBatch = new SpriteBatch(graphicsDevice);
        _whitePixel = new Texture2D(graphicsDevice, 1, 1);
        _whitePixel.SetData([Color.White]);

        CheckInvalidValues(graphicsDevice, fixInvalidValues: true);
    }

    public void Update(GraphicsDevice graphicsDevice)
    {
        CheckInvalidValues(graphicsDevice);
    }

    public void GetBackBufferData(GraphicsDevice graphicsDevice)
    {
        try
        {
            int x = (int)_getDataXAdjuster.Value;
            int y = (int)_getDataYAdjuster.Value;
            int width = (int)_getDataWidthAdjuster.Value;
            int height = (int)_getDataHeightAdjuster.Value;
            int offset = (int)_getDataOffsetAdjuster.Value;
            int count = (int)_getDataCountAdjuster.Value;

            byte[] data = new byte[(count + offset)];// * graphicsDevice.PresentationParameters.BackBufferFormat.GetSize()];
            //Color[] data = new Color[(count + offset)];
            graphicsDevice.GetBackBufferData(new Rectangle(x, y, width, height), data, offset, count);

            _getDataTexture?.Dispose();
            _getDataTexture = new Texture2D(graphicsDevice, width, height, false, graphicsDevice.PresentationParameters.BackBufferFormat);
            _getDataTexture.SetData(data, offset, count);
        }
        catch (Exception ex)
        {
            PopupWindow.ShowMessage(ex.Message + '\n' + ex.StackTrace);
        }
    }

    public void CheckInvalidValues(GraphicsDevice graphicsDevice, bool fixInvalidValues = false)
    {
        Color validColor = new(74, 74, 74);
        Color invalidColor = Color.Red;

        bool valid = true;
        int widthLimit = graphicsDevice.PresentationParameters.BackBufferWidth;
        int heightLimit = graphicsDevice.PresentationParameters.BackBufferHeight;
        valid &= CheckAdjuster(_getDataXAdjuster, 0, widthLimit - 1, fixInvalidValues);
        valid &= CheckAdjuster(_getDataYAdjuster, 0, heightLimit - 1, fixInvalidValues);

        int remainingWidthLimit = widthLimit - (int)_getDataXAdjuster.Value;
        int remainingHeightLimit = heightLimit - (int)_getDataYAdjuster.Value;
        valid &= CheckAdjuster(_getDataWidthAdjuster, 0, remainingWidthLimit, fixInvalidValues);
        valid &= CheckAdjuster(_getDataHeightAdjuster, 0, remainingHeightLimit, fixInvalidValues);

        int selectedWidth = (int)_getDataWidthAdjuster.Value;
        int selectedHeight = (int)_getDataHeightAdjuster.Value;
        int expectedCount = selectedWidth * selectedHeight * graphicsDevice.PresentationParameters.BackBufferFormat.GetSize();
        //int expectedCount = selectedWidth * selectedHeight;
        valid &= CheckAdjuster(_getDataCountAdjuster, expectedCount, expectedCount, fixInvalidValues);

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

    public void Draw(GraphicsDevice graphicsDevice)
    {
        graphicsDevice.Clear(Color.CornflowerBlue);

        if (_getDataTexture != null)
        {
            float maxDimension = Math.Max(_getDataTexture.Width, _getDataTexture.Height);
            Vector2 scale = 400f * new Vector2(_getDataTexture.Width, _getDataTexture.Height) / maxDimension;
            Rectangle rectangle = new((int)_getDataButton.Visual.AbsoluteLeft, 300, (int)scale.X, (int)scale.Y);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(_whitePixel, new Rectangle(rectangle.X - 1, rectangle.Y - 1, rectangle.Width + 2, rectangle.Height + 2), Color.White);
            _spriteBatch.Draw(_getDataTexture, rectangle, Color.White);
            _spriteBatch.End();
        }
    }

    public void Close()
    {
        _stackPanel.RemoveFromRoot();

        _spriteBatch.Dispose();
        _whitePixel.Dispose();
        _getDataTexture?.Dispose();
    }
}
