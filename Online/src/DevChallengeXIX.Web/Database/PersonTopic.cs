using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevChallengeXIX.Database;

[Table("PersonTopic")]
public class PersonTopic
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [MaxLength(128)]
    public string Id { get; set; } = string.Empty;

    public string Topics { get; set; } = string.Empty;

    public string[] AllTopics => Topics.Split(",", StringSplitOptions.RemoveEmptyEntries);

    public static PersonTopic Create(string id, IEnumerable<string> topics)
    {
        return new PersonTopic()
        {
            Id = id,
            Topics = string.Join(",", topics)
        };
    }

    internal void UpdateTopics(string[] topics) => Topics = string.Join(",", topics);
}