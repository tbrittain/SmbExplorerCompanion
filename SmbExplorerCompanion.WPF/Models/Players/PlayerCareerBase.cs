using System.Text;

namespace SmbExplorerCompanion.WPF.Models.Players;

public abstract class PlayerCareerBase : PlayerDetailBase
{
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
}