using Project_Auction_Business.Abstraction;
using Project_Auction_Business.Dtos;
using Project_Auction_Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Auction_Business.Concrete
{
    public class UserService : IUserService
    {
        public Task<ApiResponse> Login(LoginRequestDTO model)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse> Register(RegisterRequestDTO model)
        {
            throw new NotImplementedException();
        }
    }
}
