using System.ComponentModel.DataAnnotations;

namespace ServerChat.Models
{
    public class User
    {
        [Key] public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
    }
}