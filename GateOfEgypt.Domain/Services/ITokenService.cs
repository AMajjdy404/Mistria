using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Mistria.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mistria.Domain.Services
{
    public interface ITokenService
    {
        Task<string> CreateTokenAsync(AppUser appUser, UserManager<AppUser> userManager, bool rememberMe = false);
        void StoreTokenInCookie(string token, DateTime expiration, HttpContext context);
    }
}
