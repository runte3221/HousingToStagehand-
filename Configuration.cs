using System;
using Dalamud.Configuration;

namespace HousingToStagehand;

[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public string LastLayoutPath { get; set; } = string.Empty;
    public string TargetStagesFolder { get; set; } = string.Empty;

    public bool IncludeInterior { get; set; } = true;
    public bool IncludeExterior { get; set; } = true;
    public bool ApplyDyeColors { get; set; } = true;
    public bool AnchorToPlayerPosition { get; set; } = false;
}
