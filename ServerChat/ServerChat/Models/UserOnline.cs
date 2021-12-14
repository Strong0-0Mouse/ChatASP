using System.ComponentModel.DataAnnotations;

namespace ServerChat.Models
{
    public class UserOnline
    {
        [Key] public int Id { get; set; }
        public string Name { get; set; }
        public string ConnectionId { get; set; }
    }
}