using Abp.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Stories.Models;
using Stories.VM;
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
        Task<BlogSingleViewModel> GetPost(string link);
        Task<List<Post>> GetMostPopularPosts();
        Task<int> CountPost(int type, string param);
        Task<List<PostResponse>> GetPostByAuthor(string username, int pageNumber);
        Task<List<PostResponse>> GetPostByCategory(string categoryName, int pageNumber);
        Task<HomePageViewModel> GetHomePagePosts(int year, int take);
        Task<SearchResultViewModel> GetSearchResultPosts(string keyword, int year, int take);
        Task<List<PostResponse>> GetLatestPosts(int pageNumber);
        Task<Post> CreatePost(CreatePostRequest request);
        Task<LayoutResponse> GetLayoutResponse();
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

        public async Task<List<Post>> GetMostPopularPosts()
        {
            var mostPopularPosts = await _unitOfWork.GetRepository<Post>().GetAll().Where(x => x.CreatedDate.Year == DateTime.Now.Year).OrderByDescending(x => x.Views).Take(4).ToListAsync();
            return mostPopularPosts;
        }

        public async Task<int> CountPost(int type, string param)
        {
            // 1: Count by Author Username
            // 2: Count by Category ID
            if (type == 1)
            {
                var user = await _unitOfWork.GetRepository<User>().FindAsync(x => x.Username == param);
                var count1 = await _unitOfWork.GetRepository<Post>().GetAll().Where(x => x.AuthorId == user.Id).CountAsync();
                return count1;
            }

            var count2 = await _unitOfWork.GetRepository<Post>().GetAll().Where(x => x.CategoryId == param).CountAsync();
            return count2;
        }

        public async Task<List<PostResponse>> GetPostByAuthor(string username, int pageNumber)
        {
            var take = 8;
            var skip = (pageNumber - 1) * take;

            var user = await _unitOfWork.GetRepository<User>().FindAsync(x => x.Username == username);
            var posts = await _unitOfWork.GetRepository<Post>().GetAll().Where(x => x.AuthorId == user.Id).OrderByDescending(x => x.CreatedDate).Skip(skip).Take(take).ToListAsync();

            var cats = await _unitOfWork.GetRepository<Category>().GetAll().ToListAsync();

            var postR = new List<PostResponse>();

            foreach (var post in posts)
            {
                var pr = _mapper.Map<PostResponse>(post);
                pr.Category = cats.Find(x => x.Id == post.CategoryId).Name;
                pr.CategoryColor = cats.Find(x => x.Id == post.CategoryId).Color;
                postR.Add(pr);
            }

            return postR;
        }

        public async Task<List<PostResponse>> GetPostByCategory(string categoryId, int pageNumber)
        {
            var take = 8;
            var skip = (pageNumber - 1) * take;

            var posts = await _unitOfWork.GetRepository<Post>().GetAll().Where(x => x.CategoryId == categoryId).OrderByDescending(x => x.CreatedDate).Skip(skip).Take(take).ToListAsync();

            var postR = new List<PostResponse>();

            var cats = await _unitOfWork.GetRepository<Category>().GetAll().ToListAsync();
            foreach (var post in posts)
            {
                var pr = _mapper.Map<PostResponse>(post);
                pr.Category = cats.Find(x => x.Id == post.CategoryId).Name;
                pr.CategoryColor = cats.Find(x => x.Id == post.CategoryId).Color;
                postR.Add(pr);
            }

            return postR;
        }

        public async Task<BlogSingleViewModel> GetPost(string link)
        {
            var post = await _unitOfWork.GetRepository<Post>().FindAsync(x => x.Link == link);
            var user = await _unitOfWork.GetRepository<User>().FindAsync(x => x.Id == post.AuthorId);
            var morePosts = await _unitOfWork.GetRepository<Post>().GetAll().Where(x => x.Id != post.Id).OrderBy(r => Guid.NewGuid()).Take(2).ToListAsync();

            var vm = _mapper.Map<BlogSingleViewModel>(post);
            vm.AuthorName = user.Name;
            vm.AuthorUsername = user.Username;
            vm.AuthorAvatar = user.Avatar;
            vm.AuthorDescription = user.Description;
            vm.MorePosts = morePosts;

            post.Views += 1;
            await _unitOfWork.CommitAsync();

            return vm;
        }

        public async Task<HomePageViewModel> GetHomePagePosts(int year, int take)
        {
            //var randomCategoryPosts = await _unitOfWork.GetRepository<Post>().GetAll().Where(x => x.CategoryId == "").OrderByDescending(x => x.CreatedDate).ToListAsync();

            var mostPopularPosts = await _unitOfWork.GetRepository<Post>().GetAll().Where(x => x.CreatedDate.Year == year).OrderByDescending(x => x.Views).Take(take).ToListAsync();

            return new HomePageViewModel
            {
                MostPopularPosts = mostPopularPosts
            };
        }

        public async Task<SearchResultViewModel> GetSearchResultPosts(string keyword, int year, int take)
        {
            var totalPosts = new List<Post>();
            var postR = new List<PostResponse>();

            if (!string.IsNullOrEmpty(keyword))
            {
                var lkw = keyword.Split(" ");

                var predicate = PredicateBuilder.New<Post>();
                foreach (string searchTerm in lkw)
                    predicate = predicate.Or(x => x.Title.ToLower().Contains(searchTerm.ToLower()));

                totalPosts = await _unitOfWork.GetRepository<Post>().GetAll().Where(predicate).OrderByDescending(x => x.CreatedDate).ToListAsync();

                // lấy 8 bài viết đầu tiên
                var searchResultPosts = totalPosts.Take(8);

                var cats = await _unitOfWork.GetRepository<Category>().GetAll().ToListAsync();

                foreach (var post in searchResultPosts)
                {
                    var pr = _mapper.Map<PostResponse>(post);
                    pr.Category = cats.Find(x => x.Id == post.CategoryId).Name;
                    pr.CategoryColor = cats.Find(x => x.Id == post.CategoryId).Color;
                    postR.Add(pr);
                }
            }

            var mostPopularPosts = await _unitOfWork.GetRepository<Post>().GetAll().Where(x => x.CreatedDate.Year == year).OrderByDescending(x => x.Views).Take(take).ToListAsync();
            
            return new SearchResultViewModel
            {
                KeyWord = keyword,
                Total = totalPosts.Count,
                MostPopularPosts = mostPopularPosts,
                SearchResultPosts = postR
            };
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
                var pr = _mapper.Map<PostResponse>(post);
                pr.Category = cats.Find(x => x.Id == post.CategoryId).Name;
                pr.CategoryColor = cats.Find(x => x.Id == post.CategoryId).Color;
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

        public async Task<LayoutResponse> GetLayoutResponse()
        {
            // get Categories
            var categories = await _unitOfWork.GetRepository<Category>().GetAll().ToListAsync();
            var posts = await _unitOfWork.GetRepository<Post>().GetAll().ToListAsync();

            // get Hot Topics
            var hotTopics = new List<HotTopic>();
            foreach(var cat in categories)
            {
                var ht = _mapper.Map<HotTopic>(cat);
                ht.PostCount = posts.Where(x => x.CategoryId == cat.Id).Count();
                hotTopics.Add(ht);
            }
            hotTopics = hotTopics.OrderByDescending(x => x.PostCount).Take(5).ToList();

            // get Don't Miss
            var dontMiss = posts.OrderBy(r => Guid.NewGuid()).Take(3).ToList();

            // get Footer Posts
            var footerPosts = new List<FooterPost>();
            var catFooters = categories.OrderBy(r => Guid.NewGuid()).Take(3).ToList();
            foreach (var cat in catFooters)
            {
                var footerPost = new FooterPost
                {
                    CategoryName = cat.Name,
                    Posts = posts.Where(x => x.CategoryId == cat.Id).OrderBy(r => Guid.NewGuid()).Take(3).ToList()
                };

                footerPosts.Add(footerPost);
            }

            // return
            return new LayoutResponse
            {
                Categories = categories,
                HotTopics = hotTopics,
                DontMiss = dontMiss,
                FooterPosts = footerPosts
            };
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

        #region Helper
        private int CalculateReadMinutes(string content)
        {
            int length = content.Length;
            return length / 4 / 225;
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
        #endregion
    }
}
