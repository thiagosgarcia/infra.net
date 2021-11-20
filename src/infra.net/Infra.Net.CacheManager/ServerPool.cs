namespace Infra.Net.CacheManager;

public class ServerPool
{
    public readonly SynchronizedCollection<Uri> _endpoints = new();

    private int _currentIndex;
    // ReSharper disable FieldCanBeMadeReadOnly.Local
    private static object _locker = new ();
    // ReSharper restore FieldCanBeMadeReadOnly.Local
    public int CurrentIndex
    {
        get
        {
            Task.Run(() =>
            {
                lock (_locker)
                {
                    if (++_currentIndex >= _endpoints.Count)
                        _currentIndex = 0;
                }
            });
            return _currentIndex;
        }
        set
        {
            lock (_locker)
            {
                if (value >= 0 || value < _endpoints.Count)
                    _currentIndex = value;
                else
                    _currentIndex = 0;
            }
        }
    }

    public void Add(Uri url)
    {
        _endpoints.Add(url);
    }

    public void Remove(Uri uri)
    {
        _endpoints.Remove(uri);
    }

    public Uri Next()
    {
        return _endpoints[CurrentIndex];
    }
}