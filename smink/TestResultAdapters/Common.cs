using System.Text;

namespace smink.TestResultAdapters;

public static class Common
{
    /// <summary>
    /// Provides xUnit-ish prettifying of display names
    /// </summary>
    /// <param name="testName"></param>
    /// <returns></returns>
    public static string DisplayName(string? testName)
    {
	    return new StringBuilder(testName)
		    .Replace("_eq_", " = ")
		    .Replace("_ne_", " != ")
		    .Replace("_lt_", " < ")
		    .Replace("_le_", " <= ")
		    .Replace("_gt_", " > ")
		    .Replace("_ge_", " >= ")

		    .Replace("_", " ")
		    .Replace(".", ", ")
		    .ToString();

    }


}