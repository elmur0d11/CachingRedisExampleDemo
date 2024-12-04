using StackExchange.Redis;
using System.Text.Json;

namespace CachingRedis.Services
{
    public class CacheService : ICacheService
    {
        #region DIContainer
        private IDatabase _cacheDb;

        public CacheService()
        {
            var redis = ConnectionMultiplexer.Connect("localhost:6379");
            _cacheDb = redis.GetDatabase();
        }
        #endregion

        #region Get
        public T GetData<T>(string key)
        {
            //Get data from redis db
            var value = _cacheDb.StringGet(key);

            //Check data from db
            if (!string.IsNullOrEmpty(value))
                return JsonSerializer.Deserialize<T>(value);

            //return T
            return default;
        }
        #endregion

        #region RemoveData
        public object RemoveData(string key)
        {
            /*
              KeyExist:
            Build-In Redis method for cheking data from cache
            if data ain't exist returns false else true.
             */
            var _exist = _cacheDb.KeyExists(key);

            /*
             if _exist returns true KeyDelete()
            will work.
            KeyDelete: is Build-In method
            for delete data using key
             */
            if (_exist)
                return _cacheDb.KeyDelete(key);

            //else return false
            return false;
        }
        #endregion

        #region SetData
        public bool SetData<T>(string key, T value, DateTimeOffset expirationTime)
        {
            //Get expiraetion Time and put this to variable expiryTime
            var expiryTime = expirationTime.DateTime.Subtract(DateTime.Now);

            /*
             Save data to redis 
            key: for find data from Redis
            JsonSerializer.Serialize(value): Converting object to JSON and saves in Redis
            expiryTime: gives Expiration time for data
            _cacheDb.StringSet: Redis Build-In method for save data in to cache
             */
            return _cacheDb.StringSet(key, JsonSerializer.Serialize(value), expiryTime);
        }
        #endregion
    }
}
