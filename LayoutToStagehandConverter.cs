using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using Stagehand.Definitions;
using Stagehand.Definitions.Objects;

namespace HoToSta;

public sealed class ConversionOptions
{
    public bool IncludeInterior { get; set; } = true;
    public bool IncludeExterior { get; set; } = true;
    public bool ApplyDyeColors { get; set; } = true;
    public Vector3 PositionOffset { get; set; } = Vector3.Zero;
}

public sealed class ConversionResult
{
    public StageDefinition Stage { get; } = new();
    public int PlacedCount { get; set; }
    public int SkippedUnknownItemCount { get; set; }
    public List<string> SkippedItemNames { get; } = new();
}

public static class LayoutToStagehandConverter
{
    public static ConversionResult Convert(
        LayoutFile layout,
        FurnitureModelResolver resolver,
        ConversionOptions options,
        string stageName,
        string description = "",
        int intendedTerritoryType = 0)
    {
        var result = new ConversionResult();

        result.Stage.Info = new StageInfo
        {
            Name = stageName,
            AuthorName = "HoToSta",
            VersionString = "1.0",
            Description = string.IsNullOrWhiteSpace(description) ? $"Converted from MakePlace layout ({layout.HouseSize})" : description,
            IntendedTerritoryType = intendedTerritoryType,
        };

        if (options.IncludeInterior)
        {
            ConvertList(layout.InteriorFurniture, indoors: true, layout.InteriorScale, resolver, options, result);
        }

        if (options.IncludeExterior)
        {
            ConvertList(layout.ExteriorFurniture, indoors: false, layout.ExteriorScale, resolver, options, result);
        }

        return result;
    }

    private static void ConvertList(
        List<FurnitureEntry> entries,
        bool indoors,
        float scaleField,
        FurnitureModelResolver resolver,
        ConversionOptions options,
        ConversionResult result)
    {
        var divisor = scaleField == 0f ? 100f : scaleField;

        foreach (var entry in entries)
        {
            ConvertEntry(entry, indoors, divisor, resolver, options, result);
        }
    }

    private static void ConvertEntry(
        FurnitureEntry entry,
        bool indoors,
        float divisor,
        FurnitureModelResolver resolver,
        ConversionOptions options,
        ConversionResult result)
    {
        if (resolver.TryResolveMdlPath(entry.ItemId, indoors, out var mdlPath))
        {
            var loc = entry.Transform.Location;
            var rot = entry.Transform.Rotation;
            var scl = entry.Transform.Scale;

            // MakePlace coordinate conversion:
            // Y/Z swap, divide by 100 to get game yalms
            var rawPosition = new Vector3(
                SafeGet(loc, 0) / divisor,
                SafeGet(loc, 2) / divisor,
                SafeGet(loc, 1) / divisor);

            var position = rawPosition + options.PositionOffset;

            // Rotation conversion:
            // Swapping Y/Z flips coordinate handedness, so X, Y, Z must be negated with W preserved
            var rotation = new Quaternion(
                -SafeGet(rot, 0),
                -SafeGet(rot, 2),
                -SafeGet(rot, 1),
                SafeGet(rot, 3, 1f));

            var scale = new Vector3(
                SafeGet(scl, 0, 1f),
                SafeGet(scl, 1, 1f),
                SafeGet(scl, 2, 1f));

            Vector4 dyeColor = Vector4.One;
            if (options.ApplyDyeColors && entry.TryGetColorHex(out var hex) && TryParseColor(hex, out var parsedColor))
            {
                dyeColor = parsedColor;
            }

            var bgObject = new BgObjectDefinition
            {
                DisplayName = string.IsNullOrEmpty(entry.Name) ? $"Item #{entry.ItemId}" : entry.Name,
                ModelGamePath = mdlPath,
                Position = position,
                RotationQuaternion = rotation, // Automatically converts to RotationPitchYawRollDegrees
                Scale = scale,
                Opacity = 1.0f,
                DyeColor = dyeColor,
                ModpackId = string.Empty,
            };

            var uniqueKey = Guid.NewGuid().ToString("N");
            result.Stage.Objects[uniqueKey] = bgObject;
            result.PlacedCount++;
        }
        else
        {
            result.SkippedUnknownItemCount++;
            if (result.SkippedItemNames.Count < 30)
            {
                result.SkippedItemNames.Add(string.IsNullOrEmpty(entry.Name) ? $"Item #{entry.ItemId}" : entry.Name);
            }
        }

        // Recursively handle any attachments (tabletop decor, etc.)
        if (entry.Attachments is not null)
        {
            foreach (var child in entry.Attachments)
            {
                ConvertEntry(child, indoors, divisor, resolver, options, result);
            }
        }
    }

    private static float SafeGet(float[] array, int index, float fallback = 0f)
        => array.Length > index ? array[index] : fallback;

    private static bool TryParseColor(string hex, out Vector4 color)
    {
        color = Vector4.One;
        var clean = hex.TrimStart('#');
        if (clean.Length < 6)
            return false;

        if (!byte.TryParse(clean.AsSpan(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r)) return false;
        if (!byte.TryParse(clean.AsSpan(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g)) return false;
        if (!byte.TryParse(clean.AsSpan(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b)) return false;

        color = new Vector4(r / 255f, g / 255f, b / 255f, 1f);
        return true;
    }
}
