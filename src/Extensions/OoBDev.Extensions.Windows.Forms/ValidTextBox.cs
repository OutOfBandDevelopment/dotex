using System;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using OoBDev.System.Windows.Forms;

namespace OoBDev.Extensions.Windows.Forms;

/// <summary>A <see cref="TextBox"/> whose background shows whether its text matches the regular expression <see cref="Filter"/> (no filter: always valid).</summary>
public class ValidTextBox : TextBox
{
    private static readonly TimeSpan _matchTimeout = TimeSpan.FromSeconds(1);
    private string? _filter;
    private bool _lastValidate;

    /// <summary>Creates the box with a light green valid color and a light pink invalid color, and validates the empty text.</summary>
    public ValidTextBox()
    {
        ValidColor = Color.LightGreen;
        InvalidColor = Color.LightPink;
        Validate();
    }

    /// <summary>Raised when the text moves between valid and invalid.</summary>
    public event EventHandler<ValidationEventArgs>? ValidateChanged;

    /// <summary>Regular expression (Regex) for validation patterns.</summary>
    [Category("Validation Rules")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [Description("Regular Expression (Regex) for validation patterns")]
    public string? Filter
    {
        get => _filter;
        set
        {
            _filter = value;
            OnFilterChanged(EventArgs.Empty);
        }
    }

    /// <summary>Background color while the text is valid.</summary>
    [Category("Validation Rules")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [Description("Background Color if input is Valid")]
    public Color ValidColor { get; set; }

    /// <summary>Background color while the text is invalid.</summary>
    [Category("Validation Rules")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [Description("Background Color if input is Invalid")]
    public Color InvalidColor { get; set; }

    /// <summary>Whether the text matches <see cref="Filter"/>.</summary>
    public bool IsValid => string.IsNullOrWhiteSpace(Filter) || Regex.IsMatch(Text, Filter, RegexOptions.None, _matchTimeout);

    /// <summary>Applies the valid or invalid color and raises <see cref="ValidateChanged"/> when the state changed.</summary>
    public void Validate()
    {
        var validated = IsValid;
        BackColor = validated ? ValidColor : InvalidColor;
        if (_lastValidate != validated)
        {
            _lastValidate = validated;
            ValidateChanged?.Invoke(this, new ValidationEventArgs { IsValid = validated, Value = Text });
        }
    }

    /// <inheritdoc />
    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Validate();
    }

    /// <summary>Revalidates after <see cref="Filter"/> changed.</summary>
    /// <param name="e">The event data.</param>
    protected virtual void OnFilterChanged(EventArgs e) => Validate();
}
