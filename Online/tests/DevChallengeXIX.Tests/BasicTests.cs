using DevChallengeXIX.Requests;
using System.Net;
using Xunit;

namespace DevChallengeXIX.Tests
{
    public class BasicTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private static string GetNewId() => Guid.NewGuid().ToString("n");
        private readonly CustomWebApplicationFactory<Program> _factory;

        public BasicTests(CustomWebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public async Task Post_Api_People()
        {
            var request = new PersonRequest(GetNewId(), new[] { "A", "B", "C" });
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/people", request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Post_Api_People_Twice_Will_Throw()
        {
            var request = new PersonRequest(GetNewId(), new[] { "A", "B" });
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/people", request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            response.EnsureSuccessStatusCode();

            response = await client.PostAsJsonAsync("/api/people", request);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task Post_Api_Trust_Connections_Should_Return_201()
        {
            var client = _factory.CreateClient();
            var url = $"api/people/{GetNewId()}/trust_connections";

            var map = new Dictionary<string, int>
            {
                { Guid.NewGuid().ToString(), 1 },
                { Guid.NewGuid().ToString(), 2 }
            };

            var response = await client.PostAsJsonAsync(url, map);
            response.EnsureSuccessStatusCode();

            map.Clear();
            map.Add(Guid.NewGuid().ToString(), 3);
            map.Add(Guid.NewGuid().ToString(), 4);

            response = await client.PostAsJsonAsync(url, map);
            response.EnsureSuccessStatusCode();

            response = await client.PostAsJsonAsync(url, map);
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Post_Api_Messages_Should_Return_Correct_Nodes()
        {
            var client = _factory.CreateClient();

            var t = new[] { "A", "B" };
            var a = GetNewId();
            var b = GetNewId();
            var c = GetNewId();
            var d = GetNewId();

            await AddPerson(client, a, t);
            await AddPerson(client, b, t);
            await AddPerson(client, c, t);
            await AddPerson(client, d, t);

            await AddTrust(client, a, b, 1);
            await AddTrust(client, a, c, 2);
            await AddTrust(client, c, b, 3);
            await AddTrust(client, c, d, 4);

            var request = new MessageRequest("Hello world!", new[] { "A" }, a, 1);

            var response = await client.PostAsJsonAsync("/api/messages", request);
            response.EnsureSuccessStatusCode();

            var map = await response.Content.ReadFromJsonAsync<Dictionary<string, List<string>>>();

            Assert.NotNull(map);

            Assert.True(map!.ContainsKey(a));
            Assert.True(map.ContainsKey(c));

            Assert.True(map[a].Count == 2);
            Assert.Contains(b, map[a]);
            Assert.Contains(c, map[a]);

            Assert.True(map[c].Count == 1);
            Assert.Contains(d, map[c]);
        }

        [Fact]
        public async Task Post_Api_Path_Should_Return_Correct_Path()
        {
            var client = _factory.CreateClient();

            var t = new[] { "A", "B" };
            var tt = new[] { "A", "B", "C" };

            var a = GetNewId();
            var b = GetNewId();
            var c = GetNewId();
            var d = GetNewId();

            await AddPerson(client, a, t);
            await AddPerson(client, b, t);
            await AddPerson(client, c, t);
            await AddPerson(client, d, tt);

            await AddTrust(client, a, b, 1);
            await AddTrust(client, a, c, 2);
            await AddTrust(client, b, c, 3);
            await AddTrust(client, c, d, 4);

            var request = new MessageRequest("Hello world!", tt, a, 1);

            var response = await client.PostAsJsonAsync("/api/path", request);
            response.EnsureSuccessStatusCode();

            var map = await response.Content.ReadFromJsonAsync<PathResponse>();

            Assert.NotNull(map);
            Assert.NotNull(map!.From);
            Assert.NotEmpty(map.Path);

            Assert.Equal(a, map.From);
            Assert.Equal(2, map.Path.Length);
            Assert.Equal(c, map.Path[0]);
            Assert.Equal(d, map.Path[1]);
        }

        static async Task AddPerson(HttpClient client, string id, string[] topics)
        {
            var request = new PersonRequest(id, topics);
            var response = await client.PostAsJsonAsync("/api/people", request);

            response.EnsureSuccessStatusCode();
        }

        static async Task AddTrust(HttpClient client, string from, string to, int level)
        {
            var url = $"api/people/{from}/trust_connections";
            var map = new Dictionary<string, int>
            {
                { to, level }
            };
            var response = await client.PostAsJsonAsync(url, map);
            response.EnsureSuccessStatusCode();
        }
    }
}