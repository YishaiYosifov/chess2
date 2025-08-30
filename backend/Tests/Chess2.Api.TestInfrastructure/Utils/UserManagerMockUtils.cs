using Chess2.Api.Profile.Entities;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Chess2.Api.TestInfrastructure.Utils;

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
