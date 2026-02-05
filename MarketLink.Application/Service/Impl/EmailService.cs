using MarketLink.Application.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Service.Impl
{
    public class EmailService : IEmailService
    {
        public Task<string> CreateUser(CreateUserModel userDto)
        {
            throw new NotImplementedException();
        }
    }
}
