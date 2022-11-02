using Microsoft.EntityFrameworkCore;

namespace DevChallengeXIX.Database;

public class TrustContext : DbContext, ITrustContext
{
    public TrustContext(DbContextOptions<TrustContext> options) : base(options)
    {
    }

    public DbSet<Person> Persons => Set<Person>();

    public DbSet<PersonTopic> PersonTopics => Set<PersonTopic>();
}