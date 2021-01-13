using Stories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stories.VM
{
    public class HomePageViewModel
    {
        public List<Post> MostPopularPosts { get; set; }
        public List<Post> RandomCategoryPosts { get; set; }
        public List<string> HotTags { get; set; }
    }
}
