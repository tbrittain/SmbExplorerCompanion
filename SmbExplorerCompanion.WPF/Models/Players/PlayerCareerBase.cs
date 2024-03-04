using System.Text;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.Models.Players;

public class PlayerCareerBase : PlayerDetailBase
{
    public int StartSeasonNumber { get; set; }
    public int EndSeasonNumber { get; set; }
    public bool IsRetired { get; set; }
    public int Age { get; set; }
    public int? RetiredCurrentAge { get; set; }
    public bool IsHallOfFamer { get; set; }
    public string DisplaySeasons
    {
        get
        {
            var sb = new StringBuilder($"{StartSeasonNumber}-");
            
            if (IsRetired)
            {
                sb.Append($"{EndSeasonNumber}");
            }
            else
            {
                sb.Append("present");
            }
            
            return sb.ToString();
        }
    }

    public string DisplayAge => !IsRetired ? $"{Age}" : $"{RetiredCurrentAge} ({Age} at retirement)";

    public int NumSeasons { get; set; }

    public string DisplayDescription => $"{NumSeasons} season{(NumSeasons == 1 ? "" : "s")} - {WeightedOpsPlusOrEraMinus:N1} smbWAR";
}