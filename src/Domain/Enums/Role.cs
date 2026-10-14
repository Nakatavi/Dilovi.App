using System.ComponentModel;

namespace Domain.Enums;

public enum Role
{
    [Description("Super Admin")]
    SuperAdmin = 1,
    [Description("Admin")]
    Admin = 2,
    [Description("User")]
    User = 3
}
