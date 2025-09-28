using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mockerade.Setup;

namespace Mockerade.Checks;

/// <summary>
///     The invocations of the <see cref="Mock{T}" />
/// </summary>
public class MockAccessed<T> : IMockAccessed
{
	/// <summary>
	/// A proxy implementation of <see cref="IMockSetup"/> that forwards all calls to the provided <paramref name="inner"/> instance.
	/// </summary>
	public class Proxy(IMockAccessed inner) : MockAccessed<T>(), IMockAccessed
	{
		/// <inheritdoc cref="IMockInvoked.IsAlreadyInvoked" />
		bool IMockAccessed.IsAlreadyInvoked
			=> inner.IsAlreadyInvoked;

		/// <inheritdoc cref="IMockAccessed.PropertyGetter(string)" />
		Invocation[] IMockAccessed.PropertyGetter(string propertyName)
			=> inner.PropertyGetter(propertyName);

		/// <inheritdoc cref="IMockAccessed.PropertySetter(string, With.Parameter)" />
		Invocation[] IMockAccessed.PropertySetter(string propertyName, With.Parameter value)
			=> inner.PropertySetter(propertyName, value);
	}

	/// <inheritdoc cref="IMockInvoked.IsAlreadyInvoked" />
	bool IMockAccessed.IsAlreadyInvoked => _invocations.Count > 0;

	private readonly List<Invocation> _invocations = [];

	/// <summary>
	///     The registered invocations of the mock.
	/// </summary>
	public IReadOnlyList<Invocation> Invocations => _invocations.AsReadOnly();

	internal Invocation RegisterInvocation(Invocation invocation)
	{
		_invocations.Add(invocation);
		return invocation;
	}

	/// <inheritdoc cref="IMockAccessed.PropertyGetter(string)"/>
	Invocation[] IMockAccessed.PropertyGetter(string propertyName)
	{
		return _invocations
			.OfType<PropertyGetterInvocation>()
			.Where(property => property.Name.Equals(propertyName))
			.ToArray();
	}

	/// <inheritdoc cref="IMockAccessed.PropertySetter(string, With.Parameter)"/>
	Invocation[] IMockAccessed.PropertySetter(string propertyName, With.Parameter value)
	{
		return _invocations
			.OfType<PropertySetterInvocation>()
			.Where(property => property.Name.Equals(propertyName) && value.Matches(property.Value))
			.ToArray();
	}
}
