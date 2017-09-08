using System.Threading.Tasks;

namespace IntroBot {
    public interface IKeyValueDatabase {
        Task SetValue<T>(string key, T value);
        Task<T> GetRefValue<T>(string key) where T : class;
        Task<T?> GetValue<T>(string key) where T : struct;
        Task RemoveKey(string dbIntroKey);
    }
}