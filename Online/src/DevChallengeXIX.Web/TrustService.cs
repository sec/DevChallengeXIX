using DevChallengeXIX.Database;
using DevChallengeXIX.Requests;
using Microsoft.EntityFrameworkCore;

namespace DevChallengeXIX.Web;

public class TrustService
{
    private readonly ITrustContext _db;

    public TrustService(ITrustContext db) => _db = db;

    public async Task<IResult> CreatePerson(PersonRequest request)
    {
        var person = await _db.PersonTopics.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (person is not null)
        {
            return Results.UnprocessableEntity();
        }

        person = PersonTopic.Create(request.Id, request.Topics);
        await _db.PersonTopics.AddAsync(person);
        await _db.SaveChangesAsync();

        return Results.Created(string.Empty, request);
    }

    public async Task<IResult> AddOrUpdateTrust(string id, IDictionary<string, int> request)
    {
        foreach (var kv in request)
        {
            var old = await _db.Persons
                .Where(x => (x.A == id && x.B == kv.Key) || (x.B == id && x.A == kv.Key))
                .SingleOrDefaultAsync();
            if (old is null)
            {
                old = Person.Create(id, kv.Key, kv.Value);
                await _db.Persons.AddAsync(old);
            }
            else
            {
                old.Level = kv.Value;
            }
        }

        await _db.SaveChangesAsync();

        return Results.Created(string.Empty, null);
    }

    public Task<IResult> SendMessage(MessageRequest request)
    {
        var ret = new Dictionary<string, HashSet<string>>();
        var fringe = new Queue<string>();
        var visited = new HashSet<string>();

        fringe.Enqueue(request.FromPersonId);
        visited.Add(request.FromPersonId);

        while (fringe.TryDequeue(out var current))
        {
            foreach (var n in GetNextNodes(current, request.MinTrustLevel))
            {
                if (!visited.Contains(n))
                {
                    visited.Add(n);

                    var topics = GetTopics(n);
                    if (request.Topics.All(needed => topics.Any(e => e == needed)))
                    {
                        fringe.Enqueue(n);

                        if (!ret.ContainsKey(current))
                        {
                            ret[current] = new();
                        }
                        ret[current].Add(n);
                    }
                }
            }
        }

        return Task.FromResult(ret.Count == 0 ? Results.NotFound() : Results.Created(string.Empty, ret));
    }

    public Task<IResult> SendPath(MessageRequest request)
    {
        var fringe = new Queue<string>();
        var visited = new HashSet<string>();
        var prev = new Dictionary<string, string>();

        fringe.Enqueue(request.FromPersonId);
        visited.Add(request.FromPersonId);

        while (fringe.TryDequeue(out var current))
        {
            if (current != request.FromPersonId)
            {
                var topics = GetTopics(current);
                if (request.Topics.All(needed => topics.Any(e => e == needed)))
                {
                    var path = new List<string>();
                    while (true)
                    {
                        path.Add(current);
                        if (!prev.ContainsKey(current))
                        {
                            break;
                        }
                        current = prev[current];
                    }
                    path.Reverse();

                    return Task.FromResult(Results.Created(string.Empty, new PathResponse(path.First(), path.Skip(1).ToArray())));
                }
            }

            foreach (var n in GetNextNodes(current, request.MinTrustLevel))
            {
                if (!visited.Contains(n))
                {
                    visited.Add(n);
                    fringe.Enqueue(n);
                    prev[n] = current;
                }
            }
        }

        return Task.FromResult(Results.NotFound());
    }

    List<string> GetNextNodes(string start, int minLevel)
    {
        var nodes = _db.Persons
            .AsNoTracking()
            .Where(x => x.A == start || x.B == start)
            .Where(x => x.Level >= minLevel)
            .ToList();

        return nodes.Select(x => x.A).Concat(nodes.Select(x => x.B)).Where(x => x != start).ToList();
    }

    string[] GetTopics(string node)
    {
        return _db.PersonTopics
                .AsNoTracking()
                .Where(x => x.Id == node)
                .SingleOrDefault()?.AllTopics ?? Array.Empty<string>();
    }
}