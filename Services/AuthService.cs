using AutoMapper;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using OLA.Common;
using OLA.Entities;
using OLA.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace OLA.Services
{
    public interface IAuthService
    {
        Task<Response> Register(UserModel req);
        Task<Response> Login(LoginModel req);
        Task Logout();
        Task<Response> ChangePassword(ChangePasswordModel req);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserService userService;
        private readonly IConfiguration configuration;
        private readonly ILocalStorageService localStorageService;
        private readonly AuthenticationStateProvider authenticationStateProvider;
        private readonly IMapper mapper;
        private readonly IHttpContextAccessor httpContextAccessor;

        public AuthService(
            IUserService userService,
            IConfiguration configuration,
            ILocalStorageService localStorageService,
            AuthenticationStateProvider authenticationStateProvider,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor
            )
        {
            this.userService = userService;
            this.configuration = configuration;
            this.localStorageService = localStorageService;
            this.authenticationStateProvider = authenticationStateProvider;
            this.mapper = mapper;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<Response> Register(UserModel req)
        {
            try
            {
                var newUser = mapper.Map<User>(req);

                CreatePasswordHash(req.Password, out byte[] hash, out byte[] salt);
                newUser.PasswordSalt = salt;
                newUser.PasswordHash = hash;
                var response = await userService.UpsertAsync(newUser);
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Response> Login(LoginModel req)
        {
            try
            {
                var existingUser = await userService.GetByEmail(req.Email);

                if (existingUser == null)
                    return new Response { Error = true, Message = "User does not exist." };

                var verifyPasword = VerifyPasswordHash(
                    req.Password, existingUser.PasswordHash, existingUser.PasswordSalt
                    );

                if (verifyPasword)
                {
                    var token = CreateToken(existingUser);
                    await localStorageService.SetItemAsync("token", token);

                    var state = await authenticationStateProvider.GetAuthenticationStateAsync();

                    if (state != null)
                        return new Response { Success = true, Message = "Successfully logged in" };
                }

                return new Response { Error = true, Message = "Incorrect email or password" };
            }
            catch
            {
                throw;
            }
        }

        public async Task<Response> ChangePassword(ChangePasswordModel req)
        {
            try
            {
                var existingUser = await userService.GetByEmail(req.Email);
                if (existingUser == null)
                    return new Response { Error = true, Message = "User does not exist!" };

                CreatePasswordHash(req.NewPassword, out byte[] hash, out byte[] salt);

                existingUser.PasswordHash = hash;
                existingUser.PasswordSalt = salt;
                await userService.UpsertAsync(existingUser);

                return new Response { Success = true, Message = "Successfully changed password!" };
            }
            catch
            {
                throw;
            }
        }

        public async Task Logout()
        {
            await localStorageService.RemoveItemAsync("token");
            await authenticationStateProvider.GetAuthenticationStateAsync();
        }

        #region Auth Methods

        private bool VerifyPasswordHash(string password, byte[] hash, byte[] salt)
        {
            using (var hmac = new HMACSHA512(salt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(hash);
            }
        }

        public void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using (var hmac = new HMACSHA512())
            {
                salt = hmac.Key;
                hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public string CreateToken(User req)
        {
            var fullName = $"{req.FirstName} {req.LastName}";

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, req.Id),
                new Claim(ClaimTypes.Role, req.Role),
                new Claim(ClaimTypes.Name, fullName),
                new Claim(ClaimTypes.Email, req.Email)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                configuration.GetSection("AppSettings:Token").Value));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

        #endregion
    }
}
