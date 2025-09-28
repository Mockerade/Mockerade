﻿using System.Text;
using Mockerade.SourceGenerators.Internals;
using Microsoft.CodeAnalysis;

namespace Mockerade.SourceGenerators.Entities;

internal readonly record struct Property
{
	public Property(IPropertySymbol propertySymbol)
	{
		Accessibility = propertySymbol.DeclaredAccessibility;
		IsVirtual = propertySymbol.IsVirtual;
		Name = propertySymbol.Name;
		Type = new Type(propertySymbol.Type);
		Getter = propertySymbol.GetMethod is null ? null : new Method(propertySymbol.GetMethod);
		Setter = propertySymbol.SetMethod is null ? null : new Method(propertySymbol.SetMethod);
	}

	public Type Type { get; }

	public Method? Setter { get; }

	public Method? Getter { get; }

	public bool IsVirtual { get; }

	public Accessibility Accessibility { get; }
	public string Name { get; }
}
