using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mockerade.Setup;

namespace Mockerade.Checks;

/// <summary>
///     The invocations of the <see cref="Mock{T}" />
/// </summary>
public class MockInvoked<T> : IMockInvoked
{
	/// <summary>
	/// A proxy implementation of <see cref="IMockSetup"/> that forwards all calls to the provided <paramref name="inner"/> instance.
	/// </summary>
	public class Proxy(IMockInvoked inner) : MockInvoked<T>(), IMockInvoked
	{
		/// <inheritdoc cref="IMockInvoked.IsAlreadyInvoked" />
		bool IMockInvoked.IsAlreadyInvoked
			=> inner.IsAlreadyInvoked;

		/// <inheritdoc cref="IMockInvoked.Method(string, With.Parameter[])" />
		Invocation[] IMockInvoked.Method(string methodName, params With.Parameter[] parameters)
			=> inner.Method(methodName, parameters);
	}

	/// <inheritdoc cref="IMockInvoked.IsAlreadyInvoked" />
	bool IMockInvoked.IsAlreadyInvoked => _invocations.Count > 0;

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

	/// <inheritdoc cref="IMockInvoked.Method(string, With.Parameter[])"/>
	Invocation[] IMockInvoked.Method(string methodName, params With.Parameter[] parameters)
	{
		return _invocations
			.OfType<MethodInvocation>()
			.Where(method =>
				method.Name.Equals(methodName) &&
				method.Parameters.Length == parameters.Length &&
				!parameters.Where((parameter, i) => !parameter.Matches(method.Parameters[i])).Any())
			.ToArray();
	}
}
