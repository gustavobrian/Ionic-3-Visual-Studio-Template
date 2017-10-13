using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sammaron.Server.ViewModels
{
    public class ProfileModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Headline { get; set; }
        public string Summary { get; set; }
        public string ProfileImage { get; set; }

        public void CreateImage(string path, out string url)
        {
            var extention = Regex.Matches(ProfileImage, @"data:([a-zA-Z]+)\/(?<extention>[a-zA-Z]+)").Select(e => e.Groups["extention"].Value).FirstOrDefault();
            var fileName = $"{FirstName}_{LastName}{DateTime.Now:yyyyMMddhhssmm}.{extention}";
            url = $"/images/{fileName}";
            var bytes = Convert.FromBase64String(ProfileImage.Split(",")[1]);
            using (var imageFile = new FileStream($"{path}\\{fileName}", FileMode.Create))
            {
                imageFile.Write(bytes, 0, bytes.Length);
                imageFile.Flush();
            }
        }
    }
}