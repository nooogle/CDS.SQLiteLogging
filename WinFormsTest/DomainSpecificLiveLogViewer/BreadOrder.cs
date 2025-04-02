using System.ComponentModel;

namespace WinFormsTest.DomainSpecificLiveLogViewer;

/// <summary>
/// Represents an order for baking bread.
/// </summary>
[TypeConverter(typeof(ExpandableObjectConverter))]
class BreadOrder
{
    /// <summary>
    /// Gets or sets the batch number for the bread order.
    /// </summary>
    public string BatchNumber { get; set; } = "Mill_01";

    /// <summary>
    /// Gets or sets the type of flour used for the bread.
    /// </summary>
    public string FlourType { get; set; } = "White";

    /// <summary>
    /// Gets or sets the number of loafs to be baked.
    /// </summary>
    public int NumberOfLoafs { get; set; } = 10;

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => $"";
}
