using Microsoft.AspNetCore.Identity;

namespace BlazorApp.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public List<Post> PublishedPosts { get; set; }
    }
}
