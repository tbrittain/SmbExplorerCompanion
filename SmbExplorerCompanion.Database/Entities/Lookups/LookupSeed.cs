using System.ComponentModel.DataAnnotations.Schema;

namespace SmbExplorerCompanion.Database.Entities.Lookups;

public class LookupSeed
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }
    public DateTime SeededAt { get; set; } = DateTime.UtcNow;
}