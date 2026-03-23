using System.ComponentModel.DataAnnotations;

namespace ShopImport.Service.Hangfire.Infrastructure.JobManagement;

internal class ChildJobEntry
{
    [Key]
    public required string JobId { get; set; }
    public required string ParentJobId { get; set; }
    public int Status { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public required DateTime ParentCreatedAt { get; set; }
}