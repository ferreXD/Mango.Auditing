// ReSharper disable once CheckNamespace
namespace Mango.Auditing
{
    public interface IOperationInstrumentator
    {
        TResult Instrument<TResult>(string name, Func<TResult> func, Dictionary<string, object>? metadata = null, Action<Exception>? onFailure = null);
        Task<TResult> InstrumentAsync<TResult>(string name, Func<Task<TResult>> func, Dictionary<string, object>? metadata = null, Action<Exception>? onFailure = null);
        void Instrument(string name, Action action, Dictionary<string, object>? metadata = null, Action<Exception>? onFailure = null);
    }
}
