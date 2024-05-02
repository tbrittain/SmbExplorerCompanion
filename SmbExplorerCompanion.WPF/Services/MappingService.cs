namespace SmbExplorerCompanion.WPF.Services;

public partial class MappingService
{
    private readonly LookupSearchService _lookupSearchService;

    public MappingService(LookupSearchService lookupSearchService)
    {
        _lookupSearchService = lookupSearchService;
    }
}