namespace OoBDev.UiDefinitions.Forms;

/// <summary>Which <see cref="UiControl"/> a generated form field becomes — the same vocabulary a device panel uses.</summary>
public enum FormFieldKind
{
    /// <summary>Inferred from the property: bool → toggle, enum or <see cref="FormFieldAttribute.OptionsFrom"/> → choice, read-only → indicator, <c>ICommand</c> → button, anything else → text field (with an integer/number constraint for a numeric type).</summary>
    Auto,
    TextField,
    Toggle,
    Choice,

    /// <summary>A list of items with one selected: <see cref="FormFieldAttribute.OptionsFrom"/> names the items, <see cref="FormFieldAttribute.AddCommand"/> and <see cref="FormFieldAttribute.DeleteCommand"/> the buttons.</summary>
    List,
    Numeric,
    Slider,
    Indicator,
    Button,
}
