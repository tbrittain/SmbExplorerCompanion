﻿namespace SmbExplorerCompanion.Core.Entities.Search;

public class SearchResultDto
{
    public SearchResultType Type { get; init; }
    public string Name { get; init; } = default!;
    public string Description { get; init; } = default!;
    public int Id { get; init; }
}