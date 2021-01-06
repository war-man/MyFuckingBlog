using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Stories.Models;
using Stories.VM.Request;
using Stories.VM.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TEK.Core.UoW;

namespace Stories.Services
{
    public interface IBlogService
    {
        Task<List<Post>> GetPosts();
        Task<List<PostResponse>> GetLatestPosts(int pageNumber);
        Task<Post> CreatePost(CreatePostRequest request);
        Task<List<Category>> GetCategories();
        Task<Category> GetCategory(string categoryId);
    }

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

        public async Task<List<PostResponse>> GetLatestPosts(int pageNumber)
        {
            var postEach = 4;

            var skip = (pageNumber - 1) * postEach;

            var postR = new List<PostResponse>();

            var posts = await _unitOfWork.GetRepository<Post>().GetAll().OrderByDescending(x => x.CreatedDate).Skip(skip).Take(postEach).ToListAsync();

            var cats = await _unitOfWork.GetRepository<Category>().GetAll().ToListAsync();

            foreach (var post in posts)
            {
                var pr =  _mapper.Map<PostResponse>(post);
                pr.Category = cats.Find(x => x.Id == post.CategoryId).Name;
                postR.Add(pr);
            }

            return postR;
        }

        public async Task<Post> CreatePost(CreatePostRequest request)
        {
            var post = new Post();
            post = _mapper.Map<Post>(request);

            post.ReadMinute = CalculateReadMinutes(post.Content);
            post.Link = await GenerateLinkAsync(post.Title);

            _unitOfWork.GetRepository<Post>().Add(post);
            await _unitOfWork.CommitAsync();
            return post;
        }

        public async Task<List<Category>> GetCategories()
        {
            var categories = await _unitOfWork.GetRepository<Category>().GetAll().ToListAsync();
            return categories;
        }

        public async Task<Category> GetCategory(string categoryId)
        {
            var category = await _unitOfWork.GetRepository<Category>().FindAsync(x => x.Id == categoryId);
            return category;
        }

        private int CalculateReadMinutes(string content)
        {
            int length = content.Length;
            return length/4/250;
        }

        private async Task<string> GenerateLinkAsync(string Title)
        {
            var unsign = convertToUnSign3(Title);
            var snakeCase = RemoveSpecialCharacters(unsign.ToLower().Replace(" ", "_"));

            if (snakeCase.Length > 250)
            {
                snakeCase = snakeCase.Substring(0, 248);
            }
            var link = await _unitOfWork.GetRepository<Post>().FindAsync(x => x.Link == snakeCase);

            if (link == null)
            {
                return snakeCase;
            }

            return snakeCase + "_2";
        }

        public static string convertToUnSign3(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
