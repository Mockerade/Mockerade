using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Mockerade.Checks;

/// <summary>
///     The invocations of the <see cref="Mock{T}" />
/// </summary>
public class MockInvocations
{
	/// <summary>
	/// Indicates whether (at least) one invocation was already triggered.
	/// </summary>
	public bool IsAlreadyInvoked => _invocations.Count > 0;

	private readonly List<(int Index, Invocation Invocation)> _invocations = [];
	private int _index = 0;

	/// <summary>
	///     The registered invocations of the mock.
	/// </summary>
	public IEnumerable<Invocation> Invocations => _invocations.Select(x => x.Invocation);

	internal Invocation RegisterInvocation(Invocation invocation)
	{
		var index = Interlocked.Increment(ref _index);
		_invocations.Add((index, invocation));
		return invocation;
	}
}
