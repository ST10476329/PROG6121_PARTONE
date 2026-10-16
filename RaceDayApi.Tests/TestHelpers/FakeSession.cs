using Microsoft.AspNetCore.Http;

namespace RaceDayApi.Tests
{// start of namespace
    public class FakeSession : ISession
    {// start of class

        // the pretend session keeps its values here
        private Dictionary<string, byte[]> data = new Dictionary<string, byte[]>();

        public bool IsAvailable { get { return true; } }
        public string Id { get { return "test-session"; } }
        public IEnumerable<string> Keys { get { return data.Keys; } }

        public void Clear() { data.Clear(); }
        public void Remove(string key) { data.Remove(key); }
        public void Set(string key, byte[] value) { data[key] = value; }
        public bool TryGetValue(string key, out byte[] value) { return data.TryGetValue(key, out value); }

        public Task LoadAsync(CancellationToken cancellationToken = default) { return Task.CompletedTask; }
        public Task CommitAsync(CancellationToken cancellationToken = default) { return Task.CompletedTask; }

    }// end of class
}// end of namespace