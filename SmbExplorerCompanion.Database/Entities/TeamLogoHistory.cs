using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmbExplorerCompanion.Database.Entities;

public class TeamLogoHistory
{
    [Key]
    public int Id { get; set; }
    public byte[] Logo { get; set; } = default!;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Order { get; set; }

    public virtual ICollection<TeamNameHistory> TeamNameHistory { get; set; } = new HashSet<TeamNameHistory>();
}