using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazingBlog.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazingBlog.Services
{
    public class UserService
    {
        private readonly BlogContext _context;
        public UserService(BlogContext context)
        {
            _context = context;
        }
        public async Task<LoggedInUser?> LoginAsync(LoginModel model)
        {
            var dbUser = await _context.Users
                                .AsNoTracking() //เพื่อไม่ให้ _context เก็บค่า object 
                                .FirstOrDefaultAsync(u => u.Email == model.Username); 
            if(dbUser is not null)
            {
                //login success
                return new LoggedInUser(dbUser.Id, $"{dbUser.FirstName} {dbUser.LastName}".Trim());
            }
            else
            {
                //login failed
                return null;
            }
        }
    }
    
}