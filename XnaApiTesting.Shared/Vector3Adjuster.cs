using System;
using Microsoft.Xna.Framework;
using Gum.Forms.Controls;

namespace XnaApiTesting;

public class Vector3Adjuster
{
    public StackPanel StackPanel { get; private set; }

    public Action<Vector3> ValueChanged;

    private Label _label;

    private ValueAdjuster _xAdjuster;
    private ValueAdjuster _yAdjuster;
    private ValueAdjuster _zAdjuster;

    public Vector3 Value
    {
        get => new((float)_xAdjuster.Value, (float)_yAdjuster.Value, (float)_zAdjuster.Value);
        set
        {
            _xAdjuster.Value = value.X;
            _yAdjuster.Value = value.Y;
            _zAdjuster.Value = value.Z;
            ValueChanged?.Invoke(value);
        }
    }

    public bool IsEnabled
    {
        get => _xAdjuster.IsEnabled;
        set => _xAdjuster.IsEnabled = _yAdjuster.IsEnabled = _zAdjuster.IsEnabled = value;
    }

    public Vector3Adjuster(string name, double minValue, double maxValue, double stepChange)
    {
        StackPanel = new() { Orientation = Orientation.Vertical };

        _label = new();
        _label.Text = name;
        StackPanel.AddChild(_label);

        _xAdjuster = new("X", 20, minValue, maxValue, stepChange);
        _xAdjuster.StackPanel.Y = 5;
        _xAdjuster.ValueChanged += (value) => ValueChanged?.Invoke(new Vector3((float)value, (float)_yAdjuster.Value, (float)_zAdjuster.Value));
        StackPanel.AddChild(_xAdjuster.StackPanel);

        _yAdjuster = new("Y", 20, minValue, maxValue, stepChange);
        _yAdjuster.StackPanel.Y = 5;
        _yAdjuster.ValueChanged += (value) => ValueChanged?.Invoke(new Vector3((float)_xAdjuster.Value, (float)value, (float)_zAdjuster.Value));
        StackPanel.AddChild(_yAdjuster.StackPanel);

        _zAdjuster = new("Z", 20, minValue, maxValue, stepChange);
        _zAdjuster.StackPanel.Y = 5;
        _zAdjuster.ValueChanged += (value) => ValueChanged?.Invoke(new Vector3((float)_xAdjuster.Value, (float)_yAdjuster.Value, (float)value));
        StackPanel.AddChild(_zAdjuster.StackPanel);
    }
}
