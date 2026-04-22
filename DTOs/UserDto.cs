namespace MyApi.DTOs
{
    public class CreateUserDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
    }

    public class UpdateUserDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
    }
}