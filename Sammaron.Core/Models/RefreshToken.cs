using Sammaron.Core.Interfaces;

namespace Sammaron.Core.Models
{
    public class RefreshToken : IEntity<string>
    {
        public string Id { get; set; }
        public string Token { get; set; }
        public string Subject { get; set; }
    }
}