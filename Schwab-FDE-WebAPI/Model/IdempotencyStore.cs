using System.Collections.Concurrent;

/// <summary>
/// Thread-safe, bounded LRU (Least Recently Used) cache for idempotency.
/// Prevents memory leaks and ensures high performance with concurrent requests.
/// </summary>
public class IdempotencyStore
{
    private readonly ConcurrentDictionary<string, CacheEntry> _store;
    private readonly int _maxSize;
    private readonly object _lock = new object();
    private readonly LinkedList<string> _lruList = new LinkedList<string>();

    public IdempotencyStore(int maxSize = 10000)
    {
        _maxSize = maxSize;
        _store = new ConcurrentDictionary<string, CacheEntry>(Environment.ProcessorCount * 2, maxSize + 100);
    }

    public bool TryGetValue(string key, out AttributionResponse value)
    {
        if (_store.TryGetValue(key, out var entry))
        {
            lock (_lock)
            {
                // Move to end (most recently used)
                _lruList.Remove(entry.Node);
                _lruList.AddLast(entry.Node);
            }
            value = entry.Response;
            return true;
        }
        value = null;
        return false;
    }

    public void Set(string key, AttributionResponse response)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));
        if (response == null)
            throw new ArgumentNullException(nameof(response));

        lock (_lock)
        {
            // Remove old entry if exists
            if (_store.TryGetValue(key, out var oldEntry))
            {
                _lruList.Remove(oldEntry.Node);
                _store.TryRemove(key, out _);
            }

            // Add new entry
            var node = _lruList.AddLast(key);
            _store[key] = new CacheEntry { Response = response, Node = node };

            // Evict oldest if over capacity
            if (_store.Count > _maxSize && _lruList.First != null)
            {
                var oldestKey = _lruList.First.Value;
                _lruList.RemoveFirst();
                _store.TryRemove(oldestKey, out _);
            }
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _store.Clear();
            _lruList.Clear();
        }
    }

    private class CacheEntry
    {
        public AttributionResponse Response { get; set; }
        public LinkedListNode<string> Node { get; set; }
    }
}
