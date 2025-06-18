using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.Kafka.Utils;

public class ProductCheckResponseAwaiter
{
    private readonly ConcurrentDictionary<int, TaskCompletionSource<bool>> _pendingChecks = new();

    public Task<bool> WaitForResponseAsync(int productId, TimeSpan timeout)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingChecks[productId] = tcs;

        Task.Delay(timeout).ContinueWith(_ =>
        {
            _pendingChecks.TryRemove(productId, out var _);
            tcs.TrySetResult(false); // timeout = nie istnieje
        });

        return tcs.Task;
    }

    public void SetResponse(int productId, bool exists)
    {
        if (_pendingChecks.TryRemove(productId, out var tcs))
        {
            tcs.TrySetResult(exists);
        }
    }
}
