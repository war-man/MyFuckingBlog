using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Stories.Models;
using Stories.Services;
using Stories.VM.Request;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Stories.Controllers
{
    public class BlogController : Controller
    {
        private readonly ILogger<BlogController> _logger;
        private readonly IBlogService _blogService;

        public BlogController(ILogger<BlogController> logger,
            IBlogService blogService)
        {
            _logger = logger;
            _blogService = blogService;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _blogService.GetHomePagePosts(DateTime.Now.Year, 4);

            return View(posts);
        }

        [Authorize(Roles = "Admin")]
        [Route("/CreatePost")]
        public IActionResult CreatePost()
        {
            return View();
        }

        public async Task<IActionResult> Category(string id)
        {
            var category = await _blogService.GetCategory(id);

            ViewBag.category = category;

            return View();
        }

        [Route("/{controller}/Single/{link}")]
        public async Task<IActionResult> Single(string link)
        {
            var post = await _blogService.GetPost(link);
            return View(post);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string keyword)
        {
            var model = await _blogService.GetSearchResultPosts(keyword, DateTime.Now.Year, 4);
            return View(model);
        }

        [Route("/About")]
        public IActionResult About()
        {
            return View();
        }

        [Route("/Contact")]
        public IActionResult Contact()
        {
            return View();
        }

        #region Error
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("/404")]
        public IActionResult PageNotFound()
        {
            //https://www.infoworld.com/article/3545304/how-to-handle-404-errors-in-aspnet-core-mvc.html
            string originalPath = "unknown";
            if (HttpContext.Items.ContainsKey("originalPath"))
            {
                originalPath = HttpContext.Items["originalPath"] as string;
            }
            return View();
        }
        #endregion

        #region API
        [HttpPost("/{controller}/CreatePost")]
        public async Task<JsonResult> CreatePost(CreatePostRequest request)
        {
            var post = await _blogService.CreatePost(request);
            return Json(post);
        }

        [HttpGet("/{controller}/GetCategories")]
        public async Task<JsonResult> GetCategories()
        {
            var post = await _blogService.GetCategories();
            return Json(post);
        }

        [HttpGet("/{controller}/GetLatestPosts")]
        public async Task<JsonResult> GetLatestPosts(int pageNumber)
        {
            var post = await _blogService.GetLatestPosts(pageNumber);
            return Json(post);
        }
        #endregion
    }
}
