using Bogus;
using System.Runtime.CompilerServices;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class RecordFaker<T> : Faker<T>
    where T : class
{
    public RecordFaker()
    {
        CustomInstantiator(_ =>
            RuntimeHelpers.GetUninitializedObject(typeof(T)) as T
            ?? throw new InvalidOperationException(
                $"Cannot get uninitialized record {typeof(T).Name} for faker"
            )
        );
    }
}
