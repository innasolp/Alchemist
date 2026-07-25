namespace Data.Extensions;

public interface IMaterialPathEntity
{
    public int Id { get; set;  }

    public int? ParentId { get; set; }

    public string Path { get; set; }
}