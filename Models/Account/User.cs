
namespace hoistmt.Models
{
    public class User
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Password { get; set; } // Correct casing
        public string Username { get; set; } // Correct casing
        public string email { get; set; } // Correct casing

        public int Active { get; set; }

        public int roleID { get; set; }

        public string? roleName { get; set; }
        public string? position { get; set; }

        public string? phone { get; set; }

    
    }
}