using System.Collections.Concurrent;

namespace EtherscanApi.Internal;

/// <summary>
/// Client-side throttling: a plan-wide limiter plus per-endpoint limiters for endpoints that Etherscan
/// documents with their own limit (for example "throttled to 2 calls/second regardless of API Pro tier").
/// </summary>
/// <remarks>
/// One instance is shared per client (or per DI container). It has no external dependencies.
/// </remarks>
internal sealed class EtherscanRateLimiter : IDisposable
{
    private readonly SlidingWindowLimiter _global;
    private readonly ConcurrentDictionary<string, SlidingWindowLimiter> _endpointLimiters = new(StringComparer.Ordinal);
    private readonly int _queueLimit;
    private readonly TimeProvider _timeProvider;

    public EtherscanRateLimiter(int requestsPerSecond, int queueLimit, TimeProvider? timeProvider = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(requestsPerSecond, 1);
        _queueLimit = queueLimit;
        _timeProvider = timeProvider ?? TimeProvider.System;
        RequestsPerSecond = requestsPerSecond;
        _global = new SlidingWindowLimiter(requestsPerSecond, queueLimit, _timeProvider);
    }

    public int RequestsPerSecond { get; }

    public async ValueTask AcquireAsync(EtherscanRequest request, CancellationToken cancellationToken)
    {
        if (request.EndpointRequestsPerSecond is { } endpointLimit && endpointLimit < RequestsPerSecond)
        {
            var limiter = _endpointLimiters.GetOrAdd(
                request.Module + "/" + request.Action,
                static (_, state) => new SlidingWindowLimiter(state.Limit, state.Queue, state.Time),
                (Limit: endpointLimit, Queue: _queueLimit, Time: _timeProvider));
            await AcquireCoreAsync(limiter, request, cancellationToken).ConfigureAwait(false);
        }

        await AcquireCoreAsync(_global, request, cancellationToken).ConfigureAwait(false);
    }

    private static async ValueTask AcquireCoreAsync(SlidingWindowLimiter limiter, EtherscanRequest request, CancellationToken cancellationToken)
    {
        if (!await limiter.TryAcquireAsync(cancellationToken).ConfigureAwait(false))
        {
            throw ErrorClassifier.Decorate(
                new EtherscanRateLimitException(
                    "The client-side rate limiter queue is full. Increase MaxQueuedRequests or reduce concurrency.")
                {
                    IsClientSide = true,
                },
                request);
        }
    }

    public void Dispose()
    {
        _global.Dispose();
        foreach (var limiter in _endpointLimiters.Values)
        {
            limiter.Dispose();
        }

        _endpointLimiters.Clear();
    }
}

/// <summary>
/// Allows at most <c>permits</c> acquisitions in any window of slightly more than one second.
/// Keeps the timestamps of the last <c>permits</c> acquisitions in a ring buffer; a caller waits until the
/// oldest of them leaves the window. Waiters are served one at a time.
/// </summary>
internal sealed class SlidingWindowLimiter : IDisposable
{
    // A small margin over one second absorbs clock and network jitter between client and server.
    private static readonly TimeSpan Window = TimeSpan.FromMilliseconds(1100);

    private readonly long[] _timestamps;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly TimeProvider _timeProvider;
    private readonly int _queueLimit;
    private int _next;
    private int _pending;

    public SlidingWindowLimiter(int permits, int queueLimit, TimeProvider timeProvider)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(permits, 1);
        _timestamps = new long[permits];
        _queueLimit = queueLimit;
        _timeProvider = timeProvider;
    }

    /// <summary>Waits for a permit. Returns <see langword="false"/> when the wait queue is full.</summary>
    public async ValueTask<bool> TryAcquireAsync(CancellationToken cancellationToken)
    {
        // One caller may hold the gate; up to _queueLimit more may wait for it.
        if (Interlocked.Increment(ref _pending) > _queueLimit + 1)
        {
            Interlocked.Decrement(ref _pending);
            return false;
        }

        try
        {
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var oldest = _timestamps[_next];
                if (oldest != 0)
                {
                    var elapsed = _timeProvider.GetElapsedTime(oldest);
                    if (elapsed < Window)
                    {
                        await Task.Delay(Window - elapsed, _timeProvider, cancellationToken).ConfigureAwait(false);
                    }
                }

                _timestamps[_next] = _timeProvider.GetTimestamp();
                _next = (_next + 1) % _timestamps.Length;
                return true;
            }
            finally
            {
                _gate.Release();
            }
        }
        finally
        {
            Interlocked.Decrement(ref _pending);
        }
    }

    public void Dispose() => _gate.Dispose();
}
