using Stories.Models;
using Stories.VM.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stories.Services
{
    public interface IBlogService
    {
        Task<List<Post>> GetPosts();
        Task<Post> CreatePost(CreatePostRequest request);
        Task<List<Category>> GetCategories();
        Task<Category> GetCategory(string categoryId);
    }
}
