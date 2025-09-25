using System.Security.Claims;

namespace EducationPortal.Presentation.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool IsAuthenticated(this ClaimsPrincipal? user)
    {
        return user?.Identity?.IsAuthenticated == true;
    }

    public static Guid GetUserIdOrThrow(this ClaimsPrincipal user)
    {
        var idString = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idString, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        return userId;
    }
}