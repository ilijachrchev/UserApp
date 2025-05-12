using System.Threading.Tasks;

namespace UserApp.Services
{
    public interface IEmailSender
    {
        Task<bool> SendEmailAsync(
            string email,
            string subject,
            string htmlMessage);
    }
}
