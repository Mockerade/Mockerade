using System.Text;
using Mockerade.SourceGenerators.Entities;

namespace Mockerade.SourceGenerators.Internals;

internal static partial class SourceGeneration
{
	public static string GetMethodSetups(HashSet<(int, bool)> methodSetups)
	{
		var sb = new StringBuilder();
		foreach (var item in methodSetups)
		{
			sb.AppendLine();
			if (item.Item2)
			{
				AppendVoidMethod(sb, item.Item1);
			}
			else
			{
				AppendReturnMethod(sb, item.Item1);
			}
		}
		return sb.ToString();
	}

	private static void AppendVoidMethod(StringBuilder sb, int numberOfParameters)
	{
		sb.Append("public void SetupMethod(");
		for (int i = 0; i < numberOfParameters; i++)
		{
			if (i > 0)
			{
				sb.Append(", ");
			}
			sb.Append($"T{i} param{i}");
		}
		sb.Append(") { /* Implementation */ }");
	}

	private static void AppendReturnMethod(StringBuilder sb, int numberOfParameters)
	{
		sb.Append("public void SetupMethod(");
		for (int i = 0; i < numberOfParameters; i++)
		{
			if (i > 0)
			{
				sb.Append(", ");
			}
			sb.Append($"T{i} param{i}");
		}
		sb.Append(") { /* Implementation */ }");
	}
}
