using MeetupAPI.Entities;

namespace MeetupAPI.Identity
{
    public interface IJwtProvider
    {
        string GenerateJwtToken(User user);
    }
}
