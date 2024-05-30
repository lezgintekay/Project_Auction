﻿using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Project_Auction_Business.Abstraction;
using Project_Auction_Business.Dtos;
using Project_Auction_Core.Models;
using Project_Auction_Data_Access.Context;
using Project_Auction_Data_Access.Enums;
using Project_Auction_Data_Access.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Project_Auction_Business.Concrete
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; 
        private readonly ApiResponse _response;
        private readonly UserManager<ApplicationUser> _userManager; 
        private readonly RoleManager<IdentityRole> _roleManager;
        private string secretkey;
        public UserService(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ApiResponse response, IMapper mapper, IConfiguration _configuration, ApplicationDbContext context )
        {
            _userManager = userManager;
            _response = response;
            _mapper = mapper;
            _context = context;
            _roleManager = roleManager;

            secretkey = _configuration.GetValue<string>("SecretKey:jwtKey");


        }


        public async Task<ApiResponse> Login(LoginRequestDTO model)
        {
            ApplicationUser userFromDb = _context.ApplicationUsers.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
            if (userFromDb != null)
            {
                bool isValid = await _userManager.CheckPasswordAsync(userFromDb, model.Password);
                if (!isValid)
                {
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("Your entry information is not correct");
                    _response.IsSuccess = false;
                    return _response;
                }
                var role = await _userManager.GetRolesAsync(userFromDb);
                JwtSecurityTokenHandler tokenHandler = new();
                byte[] key = Encoding.ASCII.GetBytes(secretkey);

                SecurityTokenDescriptor tokenDescriptor = new()
                {
                    Subject = new System.Security.Claims.ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userFromDb.Id),
                        new Claim(ClaimTypes.Email, userFromDb.Email),
                        new Claim(ClaimTypes.Role, role.FirstOrDefault() == null ? "NormalUser" : role.FirstOrDefault()),
                        new Claim("fullName", userFromDb.FullName)
                    }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)

                };

                SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

                LoginResponseModel _model = new()
                {
                    Email = userFromDb.Email,
                    Token = tokenHandler.WriteToken(token),
                };
                _response.Result = _model;
                _response.IsSuccess = true;
                _response.StatusCode = System.Net.HttpStatusCode.OK;
                return _response;

            }
            _response.IsSuccess = false;
            _response.ErrorMessages.Add("Ooops! something went wrong");
            return _response;

        }
        public async Task<ApiResponse> Register(RegisterRequestDTO model)
        {
            var userFromDb = _context.ApplicationUsers.FirstOrDefault(x=>x.UserName.ToLower() == model.UserName.ToLower());
            if(userFromDb!=null)
            {
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Username already exist");
                return _response; 
            }
            //var newUser = _mapper.Map<ApplicationUser>(model);

            ApplicationUser newUser = new()
            {
                FullName = model.FullName,
                UserName = model.UserName,
                NormalizedEmail = model.UserName.ToUpper(),
                Email = model.UserName
            };

            var result = await _userManager.CreateAsync(newUser, model.Password);
            if(result.Succeeded)
            {
                var isTrue = _roleManager.RoleExistsAsync(UserType.Administrator.ToString()).GetAwaiter().GetResult();
                if(!_roleManager.RoleExistsAsync(UserType.Administrator.ToString()).GetAwaiter().GetResult())
                {
                    await _roleManager.CreateAsync(new IdentityRole(UserType.Administrator.ToString()));
                    await _roleManager.CreateAsync(new IdentityRole(UserType.Seller.ToString()));
                    await _roleManager.CreateAsync(new IdentityRole(UserType.NormalUser.ToString()));
                }
                if(model.UserType.ToString().ToLower() == UserType.Administrator.ToString().ToLower())
                {
                    await _userManager.AddToRoleAsync(newUser, UserType.Administrator.ToString());
                }
                if (model.UserType.ToString().ToLower() == UserType.Seller.ToString().ToLower())
                {
                    await _userManager.AddToRoleAsync(newUser, UserType.Seller.ToString());
                }
                else
                {
                    await _userManager.AddToRoleAsync(newUser, UserType.NormalUser.ToString());
                }
                _response.StatusCode = System.Net.HttpStatusCode.Created;
                _response.IsSuccess = true;
                return _response;
            }
            foreach(var error in result.Errors)
            {
                _response.ErrorMessages.Add(error.Code.ToString());
            }
            return _response;
        }
    }
}
