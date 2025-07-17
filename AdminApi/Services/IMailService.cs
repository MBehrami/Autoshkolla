using System.Collections.Generic;
using System.Threading.Tasks;
using AdminApi.Models.Helper;

namespace AdminApi.Services
{
    public interface IMailService
    {
        Task SendWelcomeEmailAsync(WelcomeRequest request);
        Task SendPasswordEmailAsync(ForgetPassword request);
    }
}