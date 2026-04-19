using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

public class UserProfile
{
    public int Id { get; set; }

    public string IdentityUserId { get; set; } = null!;

    public string FullName { get; set; } = "";

    public string PhoneNumber { get; set; } = "";
}