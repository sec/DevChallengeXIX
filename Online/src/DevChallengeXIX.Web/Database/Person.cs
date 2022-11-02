using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevChallengeXIX.Database;

[Table("Person")]
public class Person
{
    public int Id { get; set; }

    [MaxLength(128)]
    public string A { get; set; } = string.Empty;

    [MaxLength(128)]
    public string B { get; set; } = string.Empty;

    public int Level { get; set; }

    public static Person Create(string a, string b, int level)
    {
        return new Person()
        {
            A = a,
            B = b,
            Level = level
        };
    }
}
