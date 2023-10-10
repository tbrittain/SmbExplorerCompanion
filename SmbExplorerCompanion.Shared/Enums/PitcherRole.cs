using System.ComponentModel;

namespace SmbExplorerCompanion.Shared.Enums;

public enum PitcherRole
{
    [Description("SP")]
    Starter = 1,
    
    [Description("SP/RP")]
    StarterReliever = 2,
    
    [Description("RP")]
    Reliever = 3,
    
    [Description("CL")]
    Closer = 4
}