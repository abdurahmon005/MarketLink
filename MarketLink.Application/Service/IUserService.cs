using MarketLink.Application.Models.User;
using MarketLink.Application.Models.UserOtp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Service
{
    public interface IUserService
    {
        Task<string> CreateUser(CreateUserModel userDto);
        Task<UserResponseModel> GetByIdUser(Guid id);
        Task<string> UpdateUser(UpdateUserModel userDto);
        Task<string> DeleteByUser(Guid userId);
        Task<LoginResponseModel> LoginAsync(LoginUserModel loginUser);
        Task<string> VerifyOtpAsync(OtpVerificationModel model);
        Task<string> DeleteMe();

    }
}
