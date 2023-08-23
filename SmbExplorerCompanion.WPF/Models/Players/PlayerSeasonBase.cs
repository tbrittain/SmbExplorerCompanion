﻿namespace SmbExplorerCompanion.WPF.Models.Players;

public class PlayerSeasonBase : PlayerDetailBase
{
    public int SeasonNumber { get; set; }
    public string TeamNames { get; set; } = string.Empty;
    public int Age { get; set; }
}