﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Domain.Dtos
{
    public class GifDataDto
    {
        public IFormFile file { get; set; }
    }
}
