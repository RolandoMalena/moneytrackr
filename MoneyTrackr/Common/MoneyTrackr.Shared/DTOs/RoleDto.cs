namespace MoneyTrackr.Shared.DTOs
{
    public record RoleDto(string Id, string Name)
    {
        public RoleClaimDto[] Claims { get; set; } = new RoleClaimDto[0];
    }
}
