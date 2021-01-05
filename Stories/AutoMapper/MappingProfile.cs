using AutoMapper;
using Stories.Models;
using Stories.VM.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stories.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreatePostRequest, Post>();
        }
    }
}
