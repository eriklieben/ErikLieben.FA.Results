# ErikLieben.FA.Results.Validations

[![NuGet](https://img.shields.io/nuget/v/ErikLieben.FA.Results.Validations?style=flat-square)](https://www.nuget.org/packages/ErikLieben.FA.Results.Validations)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](https://opensource.org/licenses/MIT)
[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-blue?style=flat-square)](https://dotnet.microsoft.com/download/dotnet/9.0)

> **Powerful validation extensions for ErikLieben.FA.Results that integrate seamlessly with specifications and provide rich error aggregation.**

## 👋 A Friendly Note

This is an **opinionated library** built primarily for my own projects and coding style. You're absolutely free to use it (it's MIT licensed!), but please don't expect free support or feature requests. If it works for you, great! If not, there are many other excellent libraries in the .NET ecosystem.

That said, I do welcome bug reports and thoughtful contributions. If you're thinking about a feature or change, please open an issue first to discuss it - this helps avoid disappointment if it doesn't align with the library's direction. 😊

Additional validation extensions for [ErikLieben.FA.Results](../ErikLieben.FA.Results/README.md) that make validation composable, expressive, and type-safe.

## 🚀 What This Adds

This package extends the core Result library with:

- **✅ Specification-based validation** - Integrate with ErikLieben.FA.Specifications for reusable validation logic
- **🔗 Validation chaining** - Chain validations while aggregating all errors
- **🎯 Map and validate** - Transform and validate in one step with error accumulation
- **⚡ Exception bridging** - Optional conversion to exceptions for legacy integration
- **📋 Fluent builders** - Interface for building complex validation pipelines

## 📦 Installation

```bash
# Core Results library (required)
dotnet add package ErikLieben.FA.Results

# Specifications (optional, for specification-based validation)
dotnet add package ErikLieben.FA.Specifications

# This package
dotnet add package ErikLieben.FA.Results.Validations
```

**Requirements:** .NET 9.0+

## ⚡ Quick Start

```csharp
using ErikLieben.FA.Results;
using ErikLieben.FA.Results.Validations;
using ErikLieben.FA.Specifications;

// Define a reusable specification
public sealed class ValidEmailSpec : Specification<string>
{
    public override bool IsSatisfiedBy(string candidate) 
        => candidate.Contains('@') && candidate.Contains('.');
}

// Chain validations with error aggregation
var result = Result<string>.Success("user@example.com")
    .ValidateWith(s => !string.IsNullOrWhiteSpace(s), "Email required", "Email")
    .ValidateWith<string, ValidEmailSpec>("Invalid email format", "Email")
    .ValidateWith(s => s.Length <= 254, "Email too long", "Email");

// All validation errors are collected, not just the first failure
```

## 🛠️ Core Features

### Specification-Based Validation

Validate Result values using reusable specification classes:

```csharp
public sealed class PositiveNumberSpec : Specification<int>
{
    public override bool IsSatisfiedBy(int candidate) => candidate > 0;
}

var result = Result<int>.Success(5)
    .ValidateWith<int, PositiveNumberSpec>("Must be positive", "Amount");
// Result: Success(5)

var failed = Result<int>.Success(-3)
    .ValidateWith<int, PositiveNumberSpec>("Must be positive", "Amount");
// Result: Failure with ValidationError("Must be positive", "Amount")
```

### Predicate-Based Validation

Validate with custom predicates for ad-hoc checks:

```csharp
var result = Result<string>.Success("john@example.com")
    .ValidateWith(
        s => s.Contains('@'),
        errorMessage: "Email must contain '@'",
        propertyName: "Email");
```

### Validate and Transform

Combine validation and transformation in one step:

```csharp
Result<string> ParseAndValidateAge(string input)
{
    return Result<string>.Success(input)
        .ValidateAndMap(s =>
        {
            if (!int.TryParse(s, out var age))
                return Result<int>.Failure("Invalid age format", "Age");

            return age >= 18
                ? Result<int>.Success(age)
                : Result<int>.Failure("Must be 18 or older", "Age");
        })
        .Map(age => $"Validated age: {age}");
}
```

**Key benefit:** `ValidateAndMap` aggregates errors from both the original result AND the validation function.

### Error Aggregation

Unlike simple chaining, these extensions collect ALL validation errors:

```csharp
var result = Result<string>.Success("bad-email")
    .ValidateWith(s => s.Contains('@'), "Missing @", "Email")
    .ValidateWith(s => s.Contains('.'), "Missing domain", "Email")
    .ValidateWith(s => s.Length >= 5, "Too short", "Email");

// Result contains ALL three validation errors, not just the first
foreach (var error in result.Errors)
{
    Console.WriteLine($"{error.PropertyName}: {error.Message}");
}
// Output:
// Email: Missing @
// Email: Missing domain
// Email: Too short
```

## 🎯 Working with Specifications Directly

The `SpecificationValidationExtensions` provide direct specification-to-Result conversion:

### Single Value Validation

```csharp
public sealed class NonEmptySpec : Specification<string>
{
    public override bool IsSatisfiedBy(string candidate) 
        => !string.IsNullOrWhiteSpace(candidate);
}

var spec = new NonEmptySpec();
var result = spec.ValidateResult("hello", "Value cannot be empty", "Name");
// Result: Success("hello")

var failed = spec.ValidateResult("", "Value cannot be empty", "Name");
// Result: Failure with ValidationError("Value cannot be empty", "Name")
```

### Bulk Validation

```csharp
var items = new[] { "Valid", "", "AlsoValid" };

// Validate each item individually
var perItemResults = spec.ValidateMany(items, "Item cannot be empty");
// Returns: IEnumerable<Result<string>> with per-item results
// Property names will be "[0]", "[1]", "[2]" for array indices

// Validate all items as a group
var aggregateResult = spec.ValidateAll(items, "Item cannot be empty");
// Returns: Result<string[]> - Success only if ALL items pass
// Contains errors for all failing indices
```

## ⚠️ Exception Bridge (Optional)

Convert failed Results to exceptions for legacy code integration:

```csharp
// Use default ValidationException
try
{
    var value = Result<int>.Failure("Invalid", "Age")
        .ThrowIfFailure();
}
catch (ValidationException ex)
{
    // ex.Errors contains all ValidationError entries
    Console.WriteLine(ex.Message); // "Validation failed with 1 error(s)"
}

// Use custom exception factory
try
{
    var value = result.ThrowIfFailure(errors =>
    {
        var message = string.Join("; ", errors.ToArray().Select(e => 
            $"{e.PropertyName}: {e.Message}"));
        return new ArgumentException(message);
    });
}
catch (ArgumentException ex)
{
    // Custom exception with formatted message
}
```

## 🏗️ Fluent Validation Builder

The `IValidationBuilder<T>` interface defines a fluent API for complex validation scenarios:

```csharp
public interface IValidationBuilder<T>
{
    // Specification-based validation
    IValidationBuilder<T> ValidateWith<TSpec>(object value, string message, string? propertyName = null) 
        where TSpec : Specification<object>, new();
    
    // Common validations
    IValidationBuilder<T> ValidateNotNull<TValue>(TValue? value, string message, string? propertyName = null) 
        where TValue : class;
    IValidationBuilder<T> ValidateRange<TValue>(TValue value, TValue min, TValue max, string message, string? propertyName = null) 
        where TValue : IComparable<TValue>;
    IValidationBuilder<T> ValidateStringLength(string? value, int minLength, int maxLength, string message, string? propertyName = null);
    
    // Build final result
    Result<T> Build(T value);
    Result<TResult> Build<TResult>(TResult value);
    Result Build();
}
```

**Note:** This library defines the interface only. Implement it in your application for centralized validation logic.

## 📋 Complete Example

```csharp
using ErikLieben.FA.Results;
using ErikLieben.FA.Results.Validations;
using ErikLieben.FA.Specifications;

// Domain specifications
public sealed class ValidEmailSpec : Specification<string>
{
    public override bool IsSatisfiedBy(string candidate) 
        => candidate.Contains('@') && candidate.Contains('.') && candidate.Length <= 254;
}

public sealed class AdultAgeSpec : Specification<int>
{
    public override bool IsSatisfiedBy(int candidate) => candidate >= 18;
}

// DTO and domain model
public record CreateUserDto(string? Name, string? Email, int Age);
public record User(Guid Id, string Name, string Email, int Age);

// Validation pipeline
static Result<User> ValidateAndCreateUser(CreateUserDto dto)
{
    // Validate each field independently
    var nameResult = Result<string>.Success(dto.Name ?? "")
        .ValidateWith(n => !string.IsNullOrWhiteSpace(n), "Name is required", "Name")
        .ValidateWith(n => n.Length >= 2, "Name too short", "Name")
        .ValidateWith(n => n.Length <= 100, "Name too long", "Name");

    var emailResult = Result<string>.Success(dto.Email ?? "")
        .ValidateWith(e => !string.IsNullOrWhiteSpace(e), "Email is required", "Email")
        .ValidateWith<string, ValidEmailSpec>("Invalid email format", "Email");

    var ageResult = Result<int>.Success(dto.Age)
        .ValidateWith<int, AdultAgeSpec>("Must be 18 or older", "Age")
        .ValidateWith(a => a <= 120, "Age must be realistic", "Age");

    // Combine all validations using core Result combinators
    var combinedResult = ResultCombinators.Combine(nameResult, emailResult, ageResult);
    
    if (combinedResult.IsFailure)
        return Result<User>.Failure(combinedResult.Errors.ToArray());

    var (name, email, age) = combinedResult.Value;
    var user = new User(Guid.NewGuid(), name, email, age);
    
    return Result<User>.Success(user);
}

// Usage
var dto = new CreateUserDto("John", "john@example.com", 25);
var result = ValidateAndCreateUser(dto);

if (result.IsSuccess)
{
    Console.WriteLine($"Created user: {result.Value}");
}
else
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"{error.PropertyName}: {error.Message}");
    }
}
```

## 🔧 API Reference

### ResultValidationExtensions

| Method | Description |
|--------|-------------|
| `ValidateWith<T, TSpec>` | Validates success value using a Specification; aggregates errors |
| `ValidateWith<T>` | Validates with a predicate function; aggregates errors |
| `ValidateAndMap<T, TResult>` | Validates and transforms; aggregates errors from both operations |
| `ThrowIfFailure<T>` | Throws ValidationException (or custom) if Result is failure |

### SpecificationValidationExtensions

| Method | Description |
|--------|-------------|
| `ValidateResult<T>` | Validates single value against specification |
| `ValidateMany<T>` | Validates collection, returning per-item Results |
| `ValidateAll<T>` | Validates collection, returning single aggregated Result |

## 💡 Best Practices

### Do's ✅

- **Use for domain validation** - Business rules, input validation, data integrity checks
- **Aggregate errors** - Collect all validation failures to provide complete feedback
- **Leverage specifications** - Create reusable validation logic with clear names
- **Chain validations** - Build complex validation pipelines step by step
- **Preserve original errors** - Validation extensions preserve existing errors when adding new ones

### Don'ts ❌

- **Don't replace all validation** - Simple boolean checks might be sufficient for basic cases
- **Don't use for infrastructure errors** - Database failures, network timeouts should remain exceptions
- **Don't ignore property names** - Always provide meaningful property names for UI binding
- **Don't mix paradigms unnecessarily** - If your team prefers exceptions, use `ThrowIfFailure` sparingly

### Error Property Names

```csharp
// ✅ Good: Descriptive property names for UI binding
result.ValidateWith(x => x > 0, "Amount must be positive", "Amount");
result.ValidateWith(x => x.Contains("@"), "Invalid email", "EmailAddress");

// ❌ Poor: Generic or missing property names
result.ValidateWith(x => x > 0, "Must be positive", "Field1");
result.ValidateWith(x => x.Contains("@"), "Invalid", null);
```

## 🔗 Related Libraries

- **[ErikLieben.FA.Results](../ErikLieben.FA.Results/README.md)** - Core Result types and operations
- **[ErikLieben.FA.Specifications](../ErikLieben.FA.Specifications/README.md)** - Specification pattern implementation

## 📄 License

MIT License - see the [LICENSE](../../LICENSE) file for details.
