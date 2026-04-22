using System;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using RenderingLibrary.Graphics;

namespace XnaApiTesting;

public class ValueAdjuster
{
    public string Name { get; private set; }

    public StackPanel StackPanel { get; private set; }

    public double Value
    {
        get => _slider.Value;
        set
        {
            _slider.Value = value;
            if (!_enteringText)
            {
                _settingText = true;
                _textBox.Text = value.ToString("0.00");
                _settingText = false;
            }
            ValueChanged?.Invoke(value);
        }
    }

    public Action<double> ValueChanged;

    public bool IsEnabled
    {
        get => _slider.IsEnabled;
        set => _slider.IsEnabled = _textBox.IsEnabled = _upButton.IsEnabled = _downButton.IsEnabled = value;
    }

    private StackPanel _buttonStackPanel;
    private Label _label;
    private Slider _slider;
    private TextBox _textBox;
    private Button _upButton;
    private Button _downButton;

    private bool _settingText;
    private bool _enteringText;

    public ValueAdjuster(string name, int labelWidth, double minValue, double maxValue, double stepChange)
    {
        StackPanel = new() { Orientation = Orientation.Horizontal };

        _label = new();
        _label.Text = name;
        _label.Width = labelWidth;
        _label.WidthUnits = Gum.DataTypes.DimensionUnitType.Absolute;
        (_label.Visual as LabelVisual).HorizontalAlignment = HorizontalAlignment.Right;
        StackPanel.AddChild(_label);

        _slider = new();
        _slider.X = 5;
        _slider.Width = 150;
        _slider.WidthUnits = Gum.DataTypes.DimensionUnitType.Absolute;
        _slider.Minimum = minValue;
        _slider.Maximum = maxValue;
        _slider.SmallChange = stepChange;
        _slider.ValueChanged += (s, e) => Value = _slider.Value;
        StackPanel.AddChild(_slider);

        _textBox = new();
        _textBox.Text = "";
        _textBox.Placeholder = "";
        (_textBox.Visual as TextBoxBaseVisual).TextInstance.HorizontalAlignment = HorizontalAlignment.Right;
        _textBox.X = 2;
        _textBox.Width = 80;
        _textBox.WidthUnits = Gum.DataTypes.DimensionUnitType.Absolute;
        _textBox.TextChanged += (s, e) =>
        {
            if (_settingText)
                return;
            _enteringText = true;
            if (double.TryParse(_textBox.Text, out double result))
                Value = Math.Clamp(result, _slider.Minimum, _slider.Maximum);
        };
        _textBox.LostFocus += (s, e) =>
        {
            _enteringText = false;
            _settingText = true;
            _textBox.Text = Value.ToString("0.00");
            _settingText = false;
        };
        StackPanel.AddChild(_textBox);

        _buttonStackPanel = new() { Orientation = Orientation.Vertical };
        StackPanel.AddChild(_buttonStackPanel);

        _upButton = new();
        _upButton.Text = "+";
        (_upButton.Visual as ButtonVisual).TextInstance.Y = -1;
        _upButton.Click += (s, e) => Value = Math.Min(Value + _slider.SmallChange, _slider.Maximum);
        _buttonStackPanel.AddChild(_upButton);

        _downButton = new();
        _downButton.Text = "-";
        (_downButton.Visual as ButtonVisual).TextInstance.Y = -2;
        _downButton.Click += (s, e) => Value = Math.Max(Value - _slider.SmallChange, _slider.Minimum);
        _buttonStackPanel.AddChild(_downButton);

        _upButton.Height = _downButton.Height = 12;
        _upButton.HeightUnits = _downButton.HeightUnits = Gum.DataTypes.DimensionUnitType.Absolute;
        _upButton.Width = _downButton.Width = 24;
        _upButton.WidthUnits = _downButton.WidthUnits = Gum.DataTypes.DimensionUnitType.Absolute;
    }
}
