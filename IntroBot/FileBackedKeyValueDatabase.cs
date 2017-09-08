using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace IntroBot {
    public class FileBackedKeyValueDatabase : IKeyValueDatabase, IDisposable {
        private readonly FileStream _file;
        private readonly BinaryFormatter _formatter;
        private readonly Dictionary<string, object> _dbCache;

        public FileBackedKeyValueDatabase(string databasePath) {
            _file = File.Open(databasePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            _formatter =  new BinaryFormatter();
            try {
                _dbCache = (Dictionary<string, object>) _formatter.Deserialize(_file);
            } catch (SerializationException) {
                _dbCache = new Dictionary<string, object>();
            }
        }

        public async Task SetValue<T>(string key, T value) {
            _file.Seek(0, SeekOrigin.Begin);
            _dbCache[key] = value;
            await Task.Run(() => {
                _formatter.Serialize(_file, _dbCache);
            });
        }

        private Task<T> InternalGet<T>(string key) {
            var result = default(T);
            if (_dbCache.TryGetValue(key, out var value) && value != null) {
                result = (T) value;
            }
            return Task.FromResult(result);
        }

        public Task<T> GetRefValue<T>(string key) where T : class => InternalGet<T>(key);
        public Task<T?> GetValue<T>(string key) where T : struct => InternalGet<T?>(key);
        public Task RemoveKey(string key) {
            _dbCache.Remove(key);
            return Task.CompletedTask;
        }

        public void Dispose() {
            _file?.Dispose();
        }
    }
}
