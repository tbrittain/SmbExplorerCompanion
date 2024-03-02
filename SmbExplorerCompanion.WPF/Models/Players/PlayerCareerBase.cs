using System.Text;
using SmbExplorerCompanion.Core.Entities.Players;

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

public static class PlayerCareerBaseExtensions
{
    public static PlayerCareerBase FromCore(this PlayerCareerBaseDto x)
    {
        return new PlayerCareerBase
        {
            PlayerId = x.PlayerId,
            PlayerName = x.PlayerName,
            IsPitcher = x.IsPitcher,
            TotalSalary = x.TotalSalary,
            BatHandednessId = x.BatHandednessId,
            ThrowHandednessId = x.ThrowHandednessId,
            PrimaryPositionId = x.PrimaryPositionId,
            PitcherRoleId = x.PitcherRoleId,
            ChemistryId = x.ChemistryId,
            WeightedOpsPlusOrEraMinus = x.WeightedOpsPlusOrEraMinus,
            StartSeasonNumber = x.StartSeasonNumber,
            EndSeasonNumber = x.EndSeasonNumber,
            IsRetired = x.IsRetired,
            Age = x.Age,
            RetiredCurrentAge = x.RetiredCurrentAge,
            IsHallOfFamer = x.IsHallOfFamer,
        };
    }
}