using DAL.Models;

namespace ProjectAPI.DTO.AccountDTOs
{
    public class UserDTO
    {
        public string Id { get; set; } 
        public string Name { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Address { get; set; }
    }

    public static class UserExtensions
    {
        public static UserDTO ToUserDTO(this User user)
        {
            return new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address
            };
        }
    }
}
