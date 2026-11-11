using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FurBot.Database.Entities;

[Table("GelbooruPostTable")]
[PrimaryKey(nameof(InstanceUri), nameof(Id), nameof(Md5))]
internal sealed class PostEntity
{
    [StringLength(128)] public string InstanceUri { get; set; } = string.Empty;
    public long Id { get; set; }
    [MaxLength(16)] public byte[] Md5 { get; set; } = [];
}
