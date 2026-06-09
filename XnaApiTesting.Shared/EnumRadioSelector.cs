using System;
using System.Collections.Generic;
using Gum.Forms.Controls;

namespace XnaApiTesting;

public class EnumRadioSelector
{
    public string Name { get; private set; }

    public StackPanel StackPanel { get; private set; }

    public bool IsVisible
    {
        get => StackPanel.IsVisible;
        set => StackPanel.IsVisible = value;
    }

    private Label _label;
    private StackPanel _buttonsPanel;
    private List<RadioButton> _radioButtons;

    public EnumRadioSelector(string name, int width, Type enumType, object defaultValue, Action<object> onChanged)
    {
        Name = name;

        StackPanel = new() { Orientation = Orientation.Vertical };
        StackPanel.Spacing = 1;

        _label = new Label();
        _label.Text = name;
        StackPanel.AddChild(_label);

        _buttonsPanel = new() { Orientation = Orientation.Vertical };
        _buttonsPanel.Spacing = 1;
        _buttonsPanel.X = 5;

        _radioButtons = [];
        foreach (object value in Enum.GetValues(enumType))
        {
            RadioButton radioButton = new RadioButton { Text = value.ToString() };
            radioButton.Width = width;
            radioButton.IsChecked = value.Equals(defaultValue);
            radioButton.Click += (s, e) => onChanged(value);
            _radioButtons.Add(radioButton);
            _buttonsPanel.AddChild(radioButton);
        }

        StackPanel.AddChild(_buttonsPanel);
    }
}
