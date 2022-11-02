using Microsoft.EntityFrameworkCore;

namespace DevChallengeXIX.Database;

public interface ITrustContext
{
    DbSet<Person> Persons { get; }

    DbSet<PersonTopic> PersonTopics { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}