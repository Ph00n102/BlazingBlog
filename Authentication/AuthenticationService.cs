using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BlazingBlog.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;

namespace BlazingBlog.Authentication
{
    public class AuthenticationService 
    {
        private readonly UserService _userService;
        private readonly ProtectedLocalStorage _protectedLocalStorage;
        public AuthenticationService(UserService userService, ProtectedLocalStorage protectedLocalStorage)
        {
            _userService = userService;
            _protectedLocalStorage = protectedLocalStorage;
        }
        
        public async Task<LoggedInUser?> LoginUserAsync(LoginModel loginModel)
        {
            var loggedInUser = await _userService.LoginAsync(loginModel); //บันทึกผู้ใช้เมื่อเข้าสู่ระบบ
            if (loggedInUser is not null) //เมื่อผู้ใช้เข้าสู่ระบบ จะเช็คว่า not null ให้
            {
                await SaveUserToBrowserStorageAsync(loggedInUser.Value); //บักทึกข้อมูลลงในที่เก็บข้อมูล
            }
            return loggedInUser;
        }
         private const string UserStorageKey = "blg_user";
        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {

        };
        public async Task SaveUserToBrowserStorageAsync(LoggedInUser user)
        {
             await _protectedLocalStorage.SetAsync(UserStorageKey, JsonSerializer.Serialize(user, _jsonSerializerOptions));
        }
        public async Task<LoggedInUser?> GetUserFromBrowserStorageAsync() //อ่านข้อมูลจากในที่เก็บข้อมูล
        {
           try
           {
                var result = await _protectedLocalStorage.GetAsync<string>(UserStorageKey);//จะพิจารณา GetAsync จะให้ผลการจัดเก็บเบราว์เซอร์ที่ป้องกัน 
                if(result.Success && !string.IsNullOrWhiteSpace(result.Value)) //นำresultมาเช็คว่าผลลัพธ์จะสำเร็จหรือไม่ ถ้าสำเสร็จซึ่งไม่เป็นโมฆะหรือไม่ใช่ค่าว่าง
                    {
                        // var loggedInUser = JsonSerializer.Deserialize<LoggedInUser>(result.Value, _jsonSerializrOptions);
                        var loggedInUser = JsonSerializer.Deserialize<LoggedInUser>(result.Value, _jsonSerializerOptions);
                        return loggedInUser;
                    }
           }
           catch (InvalidOperationException) // จะส่งข้อยกเว้นที่เป็นการดำเนินการที่ไม่ถูก
           {
                // Eat out this exception
                // as we know this will occure when this method is being class from server
                // Where there is no Browser and LocalStorage
                // Don't worry about this, as this will be called from client side(Browser's side) after this
                // So we will have the data there
                // So we are good to ignore this excetion
           }
        return null; //return ค่าว่างกับขึ้นไป
        }
        public async Task RemoveUserFromBroserStorageAsync()=>
            await _protectedLocalStorage.DeleteAsync(UserStorageKey); //เก็บข้อมูลไว้ในเครื่องที่ได้รับการป้องกัน
    }
}