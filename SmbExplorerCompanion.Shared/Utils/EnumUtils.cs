using System.ComponentModel;
using System.Reflection;

namespace SmbExplorerCompanion.Shared.Utils;

public static class EnumUtils
{
    public static string GetEnumDescription<T>(this T value) where T : Enum
    {
        var type = value.GetType();
        var member = type.GetMember(value.ToString());
        var attribute = member[0].GetCustomAttribute<DescriptionAttribute>(false);

        return attribute?.Description ?? value.ToString();
    }
    
    public static T ParseEnumByDescription<T>(string value) where T : struct
    {
        foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is not DescriptionAttribute attribute) continue;
            if (attribute.Description != value) continue;
            if (Enum.TryParse<T>(field.Name, out var result)) return result;
        }

        return default;
    }
}