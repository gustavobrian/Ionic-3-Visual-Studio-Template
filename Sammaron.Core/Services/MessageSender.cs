using System.Threading.Tasks;
using Sammaron.Core.Interfaces;

namespace Sammaron.Core.Services
{
    public class MessageSender : IEmailSender, ISmsSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            // Plug in your email service here to send an email.
            return Task.FromResult(0);
        }

        public Task SendSmsAsync(string number, string message)
        {
            // Plug in your sms service here to send an sms to user.
            return Task.FromResult(0);
        }
    }
}