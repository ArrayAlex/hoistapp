namespace hoistmt.Models
{
    public class Tenant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DatabaseName { get; set; } // Correct casing
        public string Username { get; set; } // Correct casing
        public string Password { get; set; } // Correct casin

        
    }
}