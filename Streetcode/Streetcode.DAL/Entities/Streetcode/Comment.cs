using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Streetcode.DAL.Entities.Streetcode;

[Table("comments", Schema = "streetcode")]
public class Comment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Text { get; set; } = null!;

    [Required]
    public int UserId { get; set; }

    [Required]
    public int StreetcodeId { get; set; }

    public int? ParentCommentId { get; set; }

    [Required]
    public bool IsApproved { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(StreetcodeId))]
    public StreetcodeContent Streetcode { get; set; } = null!;

    [ForeignKey(nameof(ParentCommentId))]
    public Comment? ParentComment { get; set; }

    [ForeignKey(nameof(UserId))]
    public Users.User User { get; set; } = null!;

    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
