namespace hoistmt.Models
{
    public class DbTenant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }

        public string Password { get; set; }
        public string DatabaseName { get; set; }
       
    }
}