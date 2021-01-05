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
        public Task<List<Post>> GetPosts();
        public Task<Post> CreatePost(CreatePostRequest request);
        public Task<List<Category>> GetCategories();
    }
}
