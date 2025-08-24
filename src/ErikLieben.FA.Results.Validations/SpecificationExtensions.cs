using ErikLieben.FA.Results;

namespace ErikLieben.FA.Specifications;

// <summary>
/// Extension methods for specifications
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>
    /// Validates an entity and returns a Result
    /// </summary>
    /// <param name="specification"></param>
    /// <param name="entity">The entity to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <param name="propertyName">Optional property name for the error</param>
    /// <returns>A Result indicating success or failure</returns>
    public static Result<T> Validate<T>(this Specification<T> specification, T entity, string errorMessage, string? propertyName = null)
    {
        return specification.IsSatisfiedBy(entity)
            ? Result<T>.Success(entity)
            : Result<T>.Failure(errorMessage, propertyName ?? string.Empty);
    }


    /// <summary>
    /// Creates a specification from a predicate function
    /// </summary>
    /// <typeparam name="T">The type being validated</typeparam>
    /// <param name="predicate">The predicate function</param>
    /// <returns>A new specification</returns>
    public static Specification<T> ToSpecification<T>(this Func<T, bool> predicate)
    {
        return new DelegateSpecification<T>(predicate);
    }

    /// <summary>
    /// Validates multiple entities against a specification
    /// </summary>
    /// <typeparam name="T">The type being validated</typeparam>
    /// <param name="specification">The specification to validate against</param>
    /// <param name="entities">The entities to validate</param>
    /// <returns>A collection of entities that satisfy the specification</returns>
    public static IEnumerable<T> Filter<T>(this Specification<T> specification, IEnumerable<T> entities)
    {
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(entities);

        return entities.Where(specification.IsSatisfiedBy);
    }

    /// <summary>
    /// Validates all entities in a collection and returns Results
    /// </summary>
    /// <typeparam name="T">The type being validated</typeparam>
    /// <param name="specification">The specification to validate against</param>
    /// <param name="entities">The entities to validate</param>
    /// <param name="errorMessage">The error message template</param>
    /// <returns>Results for each entity</returns>
    public static IEnumerable<Result<T>> ValidateAll<T>(this Specification<T> specification, IEnumerable<T> entities,
        string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(entities);
        ArgumentNullException.ThrowIfNull(errorMessage);

        return entities.Select(entity => specification.Validate<T>(entity, errorMessage));
    }
}
