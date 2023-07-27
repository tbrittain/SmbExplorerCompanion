﻿using Riok.Mapperly.Abstractions;
using SmbExplorerCompanion.Core.Entities.Teams;
using SmbExplorerCompanion.WPF.Models.Teams;

namespace SmbExplorerCompanion.WPF.Mappings.Teams;

[Mapper]
public partial class TeamTopPlayerHistoryMapping
{
    public partial TeamTopPlayerHistory FromDto(TeamTopPlayerHistoryDto teamTopPlayerHistoryDto);
}