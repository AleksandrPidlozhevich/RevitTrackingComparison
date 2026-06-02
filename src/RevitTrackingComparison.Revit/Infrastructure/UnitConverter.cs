using Autodesk.Revit.DB;

namespace RevitTrackingComparison.Revit.Infrastructure;

public sealed class UnitConverter
{
    private readonly Units _units;

    public UnitConverter(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);
        _units = document.GetUnits();
    }

    public static double FromInternal(double internalValue, ForgeTypeId unitTypeId)
    {
        return UnitUtils.ConvertFromInternalUnits(internalValue, unitTypeId);
    }

    public static double ToInternal(double value, ForgeTypeId unitTypeId)
    {
        return UnitUtils.ConvertToInternalUnits(value, unitTypeId);
    }

    public static double FeetToMeters(double feet)
    {
        return FromInternal(feet, UnitTypeId.Meters);
    }

    public static double MetersToFeet(double meters)
    {
        return ToInternal(meters, UnitTypeId.Meters);
    }

    public static double FeetToMillimeters(double feet)
    {
        return FromInternal(feet, UnitTypeId.Millimeters);
    }

    public static double MillimetersToFeet(double millimeters)
    {
        return ToInternal(millimeters, UnitTypeId.Millimeters);
    }

    public double ToDisplay(double internalValue, ForgeTypeId specTypeId)
    {
        return UnitUtils.ConvertFromInternalUnits(internalValue, GetDisplayUnit(specTypeId));
    }

    public double FromDisplay(double displayValue, ForgeTypeId specTypeId)
    {
        return UnitUtils.ConvertToInternalUnits(displayValue, GetDisplayUnit(specTypeId));
    }

    public ForgeTypeId GetDisplayUnit(ForgeTypeId specTypeId)
    {
        return _units.GetFormatOptions(specTypeId).GetUnitTypeId();
    }

    public string Format(double internalValue, ForgeTypeId specTypeId, bool forEditing = false)
    {
        return UnitFormatUtils.Format(_units, specTypeId, internalValue, forEditing);
    }
}