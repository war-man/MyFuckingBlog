using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Stories.Models;
using Stories.VM.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TEK.Core.UoW;

namespace Stories.Services
{ 
    public class BlogService : IBlogService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IMapper _mapper;

        public BlogService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<Post>> GetPosts()
        {
            var posts = await _unitOfWork.GetRepository<Post>().GetAll().ToListAsync();
            return posts;
        }

        public async Task<Post> CreatePost(CreatePostRequest request)
        {
            var post = new Post();
            post = _mapper.Map<Post>(request);

            _unitOfWork.GetRepository<Post>().Add(post);
            await _unitOfWork.CommitAsync();
            return post;
        }

        public async Task<List<Category>> GetCategories()
        {
            var categories = await _unitOfWork.GetRepository<Category>().GetAll().ToListAsync();
            return categories;
        }
    }
}
