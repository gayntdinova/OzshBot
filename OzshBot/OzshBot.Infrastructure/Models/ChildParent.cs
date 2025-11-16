using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OzshBot.Infrastructure.Models;

[Table("children_parents")]
public class ChildParent
{
    [Column("child_id")]
    public Guid ChildId { get; set; }
    public virtual Person Child { get; set; }
    [Column("parent_id")]
    public Guid ParentId { get; set; }
    public virtual Parent Parent { get; set; }
}
