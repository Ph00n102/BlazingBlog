using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazingBlog.Models
{
    public record struct LoggedInUser(int UserId, string Displayname)
    {
        public readonly bool IsEmpty => UserId == 0;
    }
    
}