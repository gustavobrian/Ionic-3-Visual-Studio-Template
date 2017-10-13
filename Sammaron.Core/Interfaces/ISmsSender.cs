using System.Threading.Tasks;

namespace Sammaron.Core.Interfaces
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}