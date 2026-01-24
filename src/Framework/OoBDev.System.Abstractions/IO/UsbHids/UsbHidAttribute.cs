using System;

namespace OoBDev.System.IO.UsbHids;

/// <summary>
/// Attribute used to mark classes with USB HID device identification information.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class UsbHidAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UsbHidAttribute"/> class with default values.
    /// </summary>
    public UsbHidAttribute() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UsbHidAttribute"/> class with the specified vendor and product IDs.
    /// </summary>
    /// <param name="vendorId">The USB vendor ID.</param>
    /// <param name="productId">The USB product ID.</param>
    public UsbHidAttribute(
        ushort vendorId,
        ushort productId
        )
    {
        VendorId = vendorId;
        ProductId = productId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UsbHidAttribute"/> class with the specified vendor ID, product ID, and product mask.
    /// </summary>
    /// <param name="vendorId">The USB vendor ID.</param>
    /// <param name="productId">The USB product ID.</param>
    /// <param name="productMask">The bitmask to apply when matching the product ID.</param>
    public UsbHidAttribute(
        ushort vendorId,
        ushort productId,
        ushort productMask
        ) : this(vendorId, productId) => ProductMask = productMask;

    /// <summary>
    /// Gets or sets the USB vendor ID.
    /// </summary>
    public ushort VendorId { get; set; }

    /// <summary>
    /// Gets or sets the USB product ID.
    /// </summary>
    public ushort ProductId { get; set; }

    /// <summary>
    /// Gets or sets the bitmask applied when matching the product ID. Default is 0xffff (exact match).
    /// </summary>
    public ushort ProductMask { get; set; } = 0xffff;
}
