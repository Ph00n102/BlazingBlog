using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazingBlog.Authentication
{
    public class BlogAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
    {
        private const string BlogAuthenticationType = "blog-auth"; //รับรองความถูกต้องของบล็อค
        private readonly AuthenticationService _authenticationService;
        public BlogAuthenticationStateProvider(AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
            AuthenticationStateChanged += BlogAuthenticationStateProvider_AuthenticationStateChanged; //เพื่อให้สถานะการรับรองถูกต้อง
        }
        
        private async void BlogAuthenticationStateProvider_AuthenticationStateChanged(Task<AuthenticationState> task)
        {
            var authState = await task;  
            if (authState is not null) //เช็คค่าว่าง
            {
                var userId = Convert.ToInt32(authState.User.FindFirstValue(ClaimTypes.NameIdentifier)); //เพิ่มประเภทการอ้างสิทธิ
                var displayName = authState.User.FindFirstValue(ClaimTypes.Name);
                LoggedInUser = new(userId, displayName!);
            }
        }
        public LoggedInUser LoggedInUser { get; private set; } = new(0, string.Empty);//แจ้งผู้ใช้เมื่อเรา login เปลี่ยนพอ logout สถานะเปลี่ยน 
        public override async Task<AuthenticationState> GetAuthenticationStateAsync() 
        {
            var claimsPrincipal = new ClaimsPrincipal(); //สร้าง ClaimsPrincipal เป็นคลาสใหม่
            var user = await _authenticationService.GetUserFromBrowserStorageAsync(); //ดึงผู้ใช้จากเบราว์เซอร์ให้ถูกต้อง เพื่อให้เป็น authetic institute อีกครั้ง
            if (user is not null)// เช็คหากเราได้รับผู้ใช้นั่น และเก็บค่า
            { 
                claimsPrincipal = GetClaimsPrincipalFromUser(user.Value);
            }
            // return new AuthenticationState(claimsPrincipal);//คืนสถานะการรับรองความถูกต้อง
            var authState = new AuthenticationState(claimsPrincipal);
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
            return authState;
        }
        public async Task<string?> LoginAsync(LoginModel loginModel) 
        {
            var loggedInUser = await _authenticationService.LoginUserAsync(loginModel);
             if (loggedInUser is null)
             {
                 return "Invalid credentials";
             }
             var authState = new AuthenticationState(GetClaimsPrincipalFromUser(loggedInUser.Value));
             NotifyAuthenticationStateChanged(Task.FromResult(authState));
             return null;
        }
        public async Task LogoutAsync()
        {
            await _authenticationService.RemoveUserFromBroserStorageAsync();
            var authState = new AuthenticationState(new ClaimsPrincipal());
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }

         
        private static ClaimsPrincipal GetClaimsPrincipalFromUser(LoggedInUser user)
        {
            var identity = new ClaimsIdentity( 
                new[]
                { //สร้างตัวแปล identity เท่ากับการอ้างถึงสิทธิใหม่ จะมีอาร์เรย์ใหม่
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), //id จะใช้ บันทึกภายใต้ประเภทการอ้างสิทธิ์ 
                    new Claim(ClaimTypes.Name, user.Displayname) //อ้างสิทธิ์ ชื่อไม่ต้องแปลงค่าเป็นสตริง
                }, BlogAuthenticationType); // ใช้พารามิเตอร์ตัวที่สองตรงนี้
               return new ClaimsPrincipal(identity);  //ระบุตัวตน  
        }
        public void Dispose() =>
         AuthenticationStateChanged -= BlogAuthenticationStateProvider_AuthenticationStateChanged;
         
    }
}