using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Models.Entities
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
    }
}
