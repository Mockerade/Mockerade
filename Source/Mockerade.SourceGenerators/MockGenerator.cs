﻿using System.Collections.Immutable;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Mockerade.SourceGenerators.Entities;
using Mockerade.SourceGenerators.Internals;

namespace Mockerade.SourceGenerators;

/// <summary>
///     The <see cref="IIncrementalGenerator" /> for the registration of mocks.
/// </summary>
[Generator]
public class MockGenerator : IIncrementalGenerator
{
	void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context)
	{
		context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
			"Mock.g.cs",
			SourceText.From(SourceGeneration.Mock, Encoding.UTF8)));

		IncrementalValueProvider<ImmutableArray<MockClass?>> expectationsToRegister = context.SyntaxProvider
			.CreateSyntaxProvider(
				static (s, _) => s.IsMockForInvocationExpressionSyntax(),
				(ctx, _) => GetSemanticTargetForGeneration(ctx))
			.Where(static m => m is not null)
			.Collect();

		context.RegisterSourceOutput(expectationsToRegister,
			(spc, source) => Execute([..source.Where(t => t != null).Distinct().Cast<MockClass>(),], spc));
	}

	private static MockClass? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
	{
		if (context.Node.TryExtractGenericNameSyntax(context.SemanticModel, out GenericNameSyntax? genericNameSyntax))
		{
			SemanticModel semanticModel = context.SemanticModel;

			ITypeSymbol[] types = genericNameSyntax.TypeArgumentList.Arguments
				.Select(t => semanticModel.GetTypeInfo(t).Type)
				.Where(t => t is not null)
				.Cast<ITypeSymbol>()
				.ToArray();
			MockClass mockClassClass = new(types);
			return mockClassClass;
		}

		return null;
	}

	private static void Execute(ImmutableArray<MockClass> mocksToGenerate, SourceProductionContext context)
	{
		var namedMocksToGenerate = CreateNames(mocksToGenerate);
		foreach (var mockToGenerate in namedMocksToGenerate)
		{
			string result = SourceGeneration.GetMockClass(mockToGenerate.Name, mockToGenerate.MockClass);
			// Create a separate class file for each mock
			var fileName = $"For{mockToGenerate.Name}.g.cs";
			context.AddSource(fileName, SourceText.From(result, Encoding.UTF8));
		}
		
		foreach (var (name, extensionToGenerate) in GetDistinctExtensions(mocksToGenerate))
		{
			string result = SourceGeneration.GetExtensionClass(extensionToGenerate);
			// Create a separate class file for each mock
			var fileName = $"ExtensionsFor{name}.g.cs";
			context.AddSource(fileName, SourceText.From(result, Encoding.UTF8));
		}

		context.AddSource("MockRegistration.g.cs",
			SourceText.From(SourceGeneration.RegisterMocks(namedMocksToGenerate), Encoding.UTF8));
	}

	private static string GetFileName(Class @class)
		=> $"{@class.Namespace}_{@class.ClassName}".Replace(".", "_");

	private static List<(string Name, Class Class)> GetDistinctExtensions(ImmutableArray<MockClass> mocksToGenerate)
	{
		HashSet<(string, string)> classNames = new();
		var result = new List<(string Name, Class MockClass)>();
		foreach (var mockToGenerate in mocksToGenerate)
		{
			if (classNames.Add((mockToGenerate.Namespace, mockToGenerate.ClassName)))
			{
				int suffix = 1;
				var actualName = mockToGenerate.ClassName;
				while (result.Any(r => r.Name == actualName))
				{
					actualName = $"{mockToGenerate.ClassName}_{suffix++}";
				}
				result.Add((actualName, mockToGenerate));
			}
			foreach (var item in mockToGenerate.AdditionalImplementations)
			{
				if (classNames.Add((item.Namespace, item.ClassName)))
				{
					int suffix = 1;
					var actualName = item.ClassName;
					while (result.Any(r => r.Name == actualName))
					{
						actualName = $"{item.ClassName}_{suffix++}";
					}
					result.Add((actualName, item));
				}
			}
		}

		return result;
	}
	private static (string Name, MockClass MockClass)[] CreateNames(ImmutableArray<MockClass> mocksToGenerate)
	{
		var result = new (string Name, MockClass MockClass)[mocksToGenerate.Length];
		for(int i=0;i< mocksToGenerate.Length; i++)
		{
			MockClass mockClass = mocksToGenerate[i];
			string name = mockClass.ClassName;
			if (mockClass.AdditionalImplementations.Any())
			{
				name += "_" + string.Join("_", mockClass.AdditionalImplementations.Select(t => t.ClassName));
			}
			int suffix = 1;
			var actualName = name;
			while (result.Any(r => r.Name == actualName))
			{
				actualName = $"{name}_{suffix++}";
			}
			result[i] = (actualName, mockClass);
		}
		return result;
	}
}
