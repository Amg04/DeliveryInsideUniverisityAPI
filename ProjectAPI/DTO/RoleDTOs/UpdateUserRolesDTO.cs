namespace ProjectAPI.DTO.RoleDTOs
{
    public class UpdateUserRolesDTO
    {
        public string UserEmail { get; set; } = null!;
        public List<string> Roles { get; set; } = new();
    }
}
