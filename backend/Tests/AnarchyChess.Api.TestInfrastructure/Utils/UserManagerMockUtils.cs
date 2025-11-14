using AnarchyChess.Api.Profile.Entities;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace AnarchyChess.Api.TestInfrastructure.Utils;

public class UserManagerMockUtils
{
    public static UserManager<AuthedUser> CreateUserManagerMock() =>
        Substitute.ForPartsOf<UserManager<AuthedUser>>(
            Substitute.For<IUserStore<AuthedUser>>(),
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );
}
