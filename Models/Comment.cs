using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BlogProjectMVC2.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        public string CommentContent { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CommentPublishDate { get; set; }
        public bool IsApproved { get; set; } = false; //admin onayı

        [ForeignKey("PostId")]
        public int PostId { get; set; }
        public Post post { get; set; }

        [ForeignKey("UserId")]
        public int UserId { get; set; }
        public Users User { get; set; }

    }
}