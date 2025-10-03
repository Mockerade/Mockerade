using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mockerade.Checks;
using static Mockerade.Checks.CheckResult;

namespace Mockerade.Monitor;

public class MockMonitor<T>
{
	/// <summary>
	///     Check which properties were accessed on the mocked instance for <typeparamref name="T"/>.
	/// </summary>
	public MockAccessed<T> Accessed { get; }

	/// <summary>
	///     Check which events were subscribed or unsubscribed on the mocked instance for <typeparamref name="T"/>.
	/// </summary>
	public MockEvent<T> Event { get; }

	/// <summary>
	///     Check which methods got invoked on the mocked instance for <typeparamref name="T"/>.
	/// </summary>
	public MockInvoked<T> Invoked { get; }

	private readonly MockInvocations _monitoredInvocations;

	private readonly MockInvocations _invocations = new();
	private int _monitoringStart = -1;

	internal MockMonitor(MockInvocations invocations)
	{
		_monitoredInvocations = invocations;
		Accessed = new MockAccessed<T>(_invocations);
		Event = new MockEvent<T>(_invocations);
		Invoked = new MockInvoked<T>(_invocations);
	}

	/// <summary>
	///     Begins monitoring invocations and returns a scope that ends monitoring when disposed.
	/// </summary>
	/// <remarks>
	///     Dispose the returned object to stop monitoring and finalize the session.
	/// </remarks>
	/// <returns>
	///     An <see cref="IDisposable"/> that ends the monitoring session when disposed.
	/// </returns>
	public IDisposable Run()
	{
		_monitoringStart = _monitoredInvocations.Invocations.Count();
		return new MonitorScope(() => this.Stop());
	}

	internal void Stop()
	{
		if (_monitoringStart >= 0)
		{
			foreach (var invocation in _monitoredInvocations.Invocations.Skip(_monitoringStart))
			{
				_invocations.RegisterInvocation(invocation);
			}
		}
		_monitoringStart = -1;
	}

	private sealed class MonitorScope(Action callback) : IDisposable
	{
		public void Dispose()
		{
			callback();
		}
	}
}
