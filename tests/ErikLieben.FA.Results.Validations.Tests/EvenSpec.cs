using ErikLieben.FA.Specifications;

namespace ErikLieben.FA.Results.Validations.Tests;

/// <summary>
/// Specification that checks whether an integer value is even.
/// </summary>
public sealed class EvenSpec : Specification<int>
{
    public override bool IsSatisfiedBy(int entity) => entity % 2 == 0;
}
