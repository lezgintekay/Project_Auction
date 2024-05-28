﻿using Project_Auction_Business.Dtos;
using Project_Auction_Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Auction_Business.Abstraction
{
    public interface IUserService
    {
        Task<ApiResponse> Register(RegisterRequestDTO model);
        Task<ApiResponse> Login(LoginRequestDTO model);
    }
}
