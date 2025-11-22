namespace Nop.Plugin.Baramjk.Framework.Models.Customers
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Gender { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AvatarUrl { get; set; }
        public int? DateOfBirthDay { get; set; }
        public int? DateOfBirthMonth { get; set; }
        public int? DateOfBirthYear { get; set; }
        public string Phone { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}