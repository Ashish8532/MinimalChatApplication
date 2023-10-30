using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Domain.Dtos
{
    public class ServiceResponse<T>
    {
        public bool Succeeded { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
