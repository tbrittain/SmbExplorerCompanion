using System.Text;

namespace SmbExplorerCompanion.WPF.Models.Players;

public abstract class PlayerCareerBase
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public bool IsPitcher { get; set; }
    public int TotalSalary { get; set; }
    public string BatHandedness { get; set; } = string.Empty;
    public string ThrowHandedness { get; set; } = string.Empty;
    public string PrimaryPosition { get; set; } = string.Empty;
    public string Chemistry { get; set; } = string.Empty;
    public int StartSeasonNumber { get; set; }
    public int EndSeasonNumber { get; set; }
    public bool IsRetired { get; set; }
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
    public int NumSeasons { get; set; }
    public double WeightedOpsPlusOrEraMinus { get; set; }
}