using System;
using Gum.Forms;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using RenderingLibrary.Graphics;

namespace XnaApiTesting;

public static class PopupWindow
{
    public static void ShowMessage(string message)
    {
        Window _window = new Window();
        _window.Anchor(Gum.Wireframe.Anchor.Center);
        _window.Width = 600;
        _window.Height = 400;
        _window.ResizeMode = ResizeMode.NoResize;

        TextBox _textBox = new TextBox();
        _textBox.Text = message;
        _textBox.Anchor(Gum.Wireframe.Anchor.Top);
        _textBox.Y = 10;
        _textBox.Width = 550;
        _textBox.Height = 330;
        _textBox.TextWrapping = TextWrapping.Wrap;
        _textBox.IsReadOnly = true;
        (_textBox.Visual as TextBoxBaseVisual).TextInstance.HorizontalAlignment = HorizontalAlignment.Left;
        (_textBox.Visual as TextBoxBaseVisual).TextInstance.VerticalAlignment = VerticalAlignment.Top;
        _window.AddChild(_textBox);

        Button _okButton = new Button();
        _okButton.Anchor(Gum.Wireframe.Anchor.Bottom);
        _okButton.Y = -10;
        _okButton.Text = "OK";
        _okButton.Click += (s, e) =>
        {
            _window.Close();
            FrameworkElement.PopupRoot.Children.Remove(_window.Visual);
        };
        _window.AddChild(_okButton);

        FrameworkElement.PopupRoot.Children.Add(_window.Visual);
    }
}
