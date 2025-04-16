using System.ComponentModel.DataAnnotations;

namespace BlogProjectMVC2.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        public string CategoryName { get; set; }

        public ICollection<Post> Posts { get; set; }


    }
}