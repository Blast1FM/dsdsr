using System;

namespace Server;

public class RandomIntCacheManager
{
    Mutex mut = new();
    Random random = new();
    int? _cachedInt = null;
    TimeSpan _cacheTimer = TimeSpan.FromMinutes(5);
    DateTime _lastChanged;

    void UpdateCachedNumber()
    {
        mut.WaitOne();
        _cachedInt = random.Next();
        _lastChanged = DateTime.Now;
        mut.ReleaseMutex();
    }

    public int RetrieveRandomNumber(DateTime requestTimestamp)
    {
        if(_cachedInt == null)
        {
            UpdateCachedNumber();
            return (int)_cachedInt!;
        } 

        if(requestTimestamp - _lastChanged > _cacheTimer)
        {
            UpdateCachedNumber();
            return (int)_cachedInt!;
        }

        if (requestTimestamp - _lastChanged < _cacheTimer)
        {
            return (int)_cachedInt!;
        }

        throw new Exception("idfk what happened lmao");
    }
    
}
