using System.Collections.Generic;

public static class ColorMapping
{
    private static Dictionary<int, string> colorMap = new Dictionary<int, string>
    {
        { 0, "y" },
        { 1, "b" },
        { 2, "g" }
    };

    public static string GetColor(int index)
    {
        return colorMap.TryGetValue(index, out var color) ? color : null;
    }
}