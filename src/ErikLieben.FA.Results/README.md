# ErikLieben.FA.Results

[![NuGet](https://img.shields.io/nuget/v/ErikLieben.FA.Results?style=flat-square)](https://www.nuget.org/packages/ErikLieben.FA.Results)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](https://opensource.org/licenses/MIT)
[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-blue?style=flat-square)](https://dotnet.microsoft.com/download/dotnet/9.0)

> **A lightweight, allocation-friendly Result type for .NET that makes success/failure flow explicit, composable, and ergonomic.**

## 👋 A Friendly Note


This is an **opinionated library** built primarily for my own projects and coding style. You're absolutely free to use it (it's MIT licensed!), but please don't expect free support or feature requests. If it works for you, great! If not, there are many other excellent libraries in the .NET ecosystem.

That said, I do welcome bug reports and thoughtful contributions. If you're thinking about a feature or change, please open an issue first to discuss it - this helps avoid disappointment if it doesn't align with the library's direction. 😊

## 🚀 Why This Library?

Instead of throwing exceptions for expected conditions, this library helps you:

- ✅ **Make outcomes explicit** - success or failure, never ambiguous
- 🔗 **Chain operations safely** - no more try/catch pyramids
- 🎯 **Build great APIs** - rich helpers for ASP.NET Core and validation
- ⚡ **Stay performant** - uses spans to minimize allocations

Perfect for **domain modeling**, collecting validation errors, and consistent API responses.

## 📦 Installation

```bash
dotnet add package ErikLieben.FA.Results
```

**Requirements:** .NET 9.0+

## 🗂️ Core Concepts

### Types

| Type | Purpose | Example Use Case |
|------|---------|------------------|
| `Result` | Non-generic success/failure | Commands with no return value |
| `Result<T>` | Generic success/failure with value | Queries that produce data |
| `ValidationError` | Error with message + optional property name | Field-level validation |

### Creating Results

```csharp
using ErikLieben.FA.Results;

// Success cases
var success = Result.Success();
var successWithValue = Result<string>.Success("Hello");

// Failure cases  
var failure = Result.Failure("Something went wrong");
var fieldError = Result<int>.Failure("Invalid age", "Age");
var multipleErrors = Result<User>.Failure(new[] {
    ValidationError.Create("Name required", "Name"),
    ValidationError.Create("Email invalid", "Email")
});
```

## ⚡ Quick Start

```csharp
using ErikLieben.FA.Results;

// Domain function that can fail predictably
Result<string> ValidateEmail(string? email)
    => !string.IsNullOrWhiteSpace(email) && email.Contains("@")
        ? Result<string>.Success(email!)
        : Result<string>.Failure("Invalid email", "Email");

// Chain operations safely
var result = ValidateEmail("user@example.com")
    .Map(email => email.ToLowerInvariant())
    .Tap(email => Console.WriteLine($"Processing: {email}"));

// Handle the result
if (result.IsSuccess)
    Console.WriteLine($"Valid email: {result.Value}");
else
    Console.WriteLine($"Error: {result.GetErrorMessages()}");
```

## ✅ When to Use Result

**Use Result for domain errors** - expected business failures that are part of your domain model:

- ✅ **Validation failures** - user input that doesn't meet business rules
- ✅ **Business rule violations** - domain constraints that prevent an operation
- ✅ **Expected workflow failures** - e.g., order rejection, insufficient inventory
- ✅ **Parsing user input** - converting strings to domain types

```csharp
// Good: Expected domain scenarios
Result<Order> ValidateOrder(OrderRequest request) { /* ... */ }
Result<Customer> FindCustomerByEmail(string email) { /* ... */ }
Result<decimal> CalculateDiscount(Customer customer, Order order) { /* ... */ }
```

## ❌ When NOT to Use Result

**Don't use Result for infrastructure concerns or unexpected failures:**

- ❌ **Infrastructure failures** - database timeouts, network errors, disk failures
- ❌ **Programming errors** - null reference exceptions, index out of bounds
- ❌ **Configuration errors** - missing config files, invalid connection strings
- ❌ **System resource issues** - out of memory, disk full

```csharp
// Bad: These should throw exceptions
Result<string> ReadConfigFile(string path) { /* Use exceptions instead */ }
Result<User> GetUserFromDatabase(int id) { /* Let DB exceptions bubble up */ }
```

**Why exceptions are better for infrastructure:**
- You get stack traces for debugging
- The calling code doesn't need to handle every possible infrastructure failure
- You can fail fast when the system is in an unrecoverable state

## 🔧 Basic Operations

### Map - Transform Success Values

```csharp
// Domain entity with internal details
public record User(
    Guid Id, 
    string Name, 
    string Email, 
    string PasswordHash,  // Internal - don't expose
    DateTime CreatedAt, 
    DateTime LastLoginAt,
    bool IsActive
);

// Public contract - only expose what clients need
public record UserSummary(Guid Id, string Name, string Email);

// Transform domain object to contract
Result<User> userResult = GetUserById(userId);
Result<UserSummary> summary = userResult.Map(user => 
    new UserSummary(user.Id, user.Name, user.Email));
// Result: Success(UserSummary) without internal details
```

### Bind - Chain Operations That Can Fail

```csharp
Result<string> ValidateNonEmpty(string? s) =>
    !string.IsNullOrWhiteSpace(s)
        ? Result<string>.Success(s!)
        : Result<string>.Failure("Required");

Result<int> ParseInt(string s) =>
    int.TryParse(s, out var n) 
        ? Result<int>.Success(n)
        : Result<int>.Failure("Not a number");

Result<int> result = ValidateNonEmpty("42")
    .Bind(ParseInt); // Success(42)
```

### Tap - Side Effects Without Changing Result

```csharp
var result = Result<int>.Success(10)
    .Tap(n => Console.WriteLine($"Processing: {n}"))
    .TapError(errors => LogErrors(errors));
```

### Match - Pattern Matching

```csharp
string message = result.Match(
    onSuccess: value => $"Got: {value}",
    onFailure: errors => $"Failed: {errors.Length} errors"
);
```

## 🛠️ Error Handling Patterns

### Accessing Results Safely

```csharp
Result<string> result = GetSomeResult();

// Check state
bool success = result.IsSuccess;
bool failed = result.IsFailure;

// Access errors (ReadOnlySpan<ValidationError>)
ReadOnlySpan<ValidationError> errors = result.Errors;

// ⚠️ Unsafe: throws on failure
string value = result.Value; 

// ✅ Safe access patterns
string safeValue = result.ValueOrDefault("fallback");
string computed = result.ValueOr(errors => $"Failed: {errors.Length} errors");
```

### Working with Multiple Errors

```csharp
var failed = Result<int>.Failure(new[]
{
    ValidationError.Create("Invalid name", "Name"),
    ValidationError.Create("Too young", "Age"),
    ValidationError.Create("Missing email", "Email")
});

// Get all error messages as single string
string allErrors = failed.GetErrorMessages();
// Result: "Invalid name; Too young; Missing email"

// Transform errors (add context, prefixes, etc.)
var prefixedErrors = failed.MapErrors(e => 
    ValidationError.Create($"[User] {e.Message}", e.PropertyName ?? string.Empty));

// Filter specific errors
var nameErrors = failed.FilterErrors(e => e.PropertyName == "Name");

// ⚠️ Important: FilterErrors returns Success(default) when no errors match
if (nameErrors.IsSuccess)
{
    // This means NO name errors were found, not that validation passed!
    Console.WriteLine("No name-specific errors");
}
```

### ReadOnlySpan<ValidationError> Considerations

```csharp
Result<int> failed = Result<int>.Failure("Error 1", "Error 2");

// ❌ Don't do this - span can't be enumerated multiple times
foreach (var error in failed.Errors) { /* first enumeration */ }
foreach (var error in failed.Errors) { /* this might fail! */ }

// ✅ Convert to array if you need multiple enumerations
ValidationError[] errorArray = failed.Errors.ToArray();
foreach (var error in errorArray) { /* safe */ }
foreach (var error in errorArray) { /* safe */ }
```

## ✅ Validation Helpers

```csharp
using ErikLieben.FA.Results.Validations;

// Simple validations
var name = ValidationBuilder.ValidateNotNullOrWhiteSpace(
    dto.Name, "Name required", "Name");

var age = ValidationBuilder.ValidateRange(
    dto.Age, 18, 100, "Age must be between 18-100", "Age");

// Specification-based validation
class EmailSpec : Specification<string>
{
    public override bool IsSatisfiedBy(string candidate) 
        => candidate.Contains("@") && candidate.Contains("."); // Simplified
}

var email = ValidationBuilder.ValidateSingle<string, EmailSpec>(
    dto.Email, "Invalid email format", "Email");
```

## 🔗 Combining Results

```csharp
var name = ValidateNotNullOrWhiteSpace(dto.Name, "Name required", "Name");
var email = ValidateNotNullOrWhiteSpace(dto.Email, "Email required", "Email");
var age = ValidateRange(dto.Age, 0, 120, "Invalid age", "Age");

// Combine into tuple - all must succeed
var combined = ResultCombinators.Combine(name, email, age);
if (combined.IsSuccess)
{
    var (validName, validEmail, validAge) = combined.Value;
    // Create user...
}
```

## 🌐 ASP.NET Core Integration

### Controller-Based APIs

```csharp
[HttpPost]
public IActionResult CreateUser(CreateUserDto dto)
{
    Result<User> result = ValidateAndCreateUser(dto);
    
    return result.ToCreatedAtActionResult(
        actionName: nameof(GetUser),
        controllerName: "Users",
        routeValues: new { id = result.IsSuccess ? result.Value.Id : null },
        successMessage: "User created successfully"
    );
    // 201 Created with user data on success
    // 400 Bad Request with validation errors on failure
}
```

### Minimal APIs

```csharp
app.MapPost("/users", CreateUser).WithName("CreateUser");

static IResult CreateUser(CreateUserDto dto)
{
    var result = ValidateAndCreateUser(dto);
    return result.ToCreatedAtRouteResult(
        routeName: "GetUser",
        routeValues: new { id = result.IsSuccess ? result.Value.Id : null },
        successMessage: "User created successfully"
    );
}
```

### API Response Format

Success response:
```json
{
  "isSuccess": true,
  "data": { "id": "123", "name": "Alice" },
  "message": "User created successfully",
  "timestamp": "2025-01-01T12:00:00Z"
}
```

Failure response:
```json
{
  "isSuccess": false,
  "errors": [
    { "message": "Name is required", "propertyName": "Name" },
    { "message": "Invalid email format", "propertyName": "Email" }
  ],
  "message": "Validation failed",
  "timestamp": "2025-01-01T12:00:00Z"
}
```

## 🧩 Advanced Functional Combinators

These helpers are inspired by Railway-Oriented Programming patterns and the F# "recipe" approach. **Use sparingly** - they're powerful but can make code harder to understand for teams not familiar with functional patterns.

**What are combinators?** Think of them as "function transformers" - they take regular functions and convert them to work with Result types. This lets you reuse existing pure functions without rewriting them.

**When to use these:**
- You have existing pure functions you want to use with Results
- You're building complex validation pipelines
- You need to handle exceptions in a functional way
- You want to compose multiple operations that can fail

**When NOT to use these:**
- Your team isn't comfortable with functional concepts
- Simple `Map` and `Bind` already solve your problem
- You're prioritizing code readability over functional purity

### Switch — lift a pure function

**What it does:** Takes a regular function (that never fails) and converts it to work with Result types.

**Why use it:** You have existing utility functions that you want to use in a Result pipeline without rewriting them.

```csharp
// You have a pure function that never fails
int CalculateStringLength(string s) => s.Length;

// But you're working with Result<string> in your pipeline
Result<string> userInput = ValidateUserInput(input);

// Switch "lifts" your pure function to work with Results
var calculateLengthR = Result.Switch<string, int>(CalculateStringLength);

// Now you can use it in your Result pipeline
Result<int> length = userInput.Bind(calculateLengthR);
// If userInput was Success("hello") -> Success(5)
// If userInput was Failure -> still Failure (function not called)

// Alternative using Map (often simpler):
Result<int> lengthSimple = userInput.Map(s => s.Length);
```

### Try — wrap exceptions into failures

**What it does:** Takes a function that might throw exceptions and converts it into a Result-returning function.

**Why use it:** You want to handle exceptions functionally instead of using try-catch blocks, especially when working with external libraries or I/O operations in a controlled way.

```csharp
// A function that might throw (like parsing, file I/O, etc.)
DateTime ParseDate(string dateStr) => DateTime.Parse(dateStr); // Throws on invalid input

// Convert exception to a ValidationError
ValidationError MapException(Exception ex) => 
    ValidationError.Create($"Invalid date: {ex.Message}", "Date");

// Wrap it to return Result instead of throwing
var safeParseDateR = Result.Try<string, DateTime>(ParseDate, MapException);

// Now you can use it safely in pipelines
Result<DateTime> result1 = safeParseDateR("2023-12-25"); // Success(DateTime)
Result<DateTime> result2 = safeParseDateR("not-a-date"); // Failure("Invalid date: ...")

// Use in a validation pipeline
Result<DateTime> validatedDate = ValidateNotEmpty(userInput)
    .Bind(safeParseDateR)
    .Bind(ValidateDateInFuture);
```

### Compose — Kleisli composition

**What it does:** Combines two functions that return Results into a single function. Think of it as "function chaining" where each step can fail.

**Why use it:** You have multiple validation or transformation steps that you want to combine into a reusable pipeline.

```csharp
// Two separate validation functions
Result<string> ValidateNotEmpty(string? input) =>
    !string.IsNullOrWhiteSpace(input)
        ? Result<string>.Success(input!)
        : Result<string>.Failure("Input cannot be empty");

Result<string> ValidateEmailFormat(string email) =>
    email.Contains("@") && email.Contains(".")
        ? Result<string>.Success(email)
        : Result<string>.Failure("Invalid email format");

// Compose them into a single reusable email validator
var validateEmail = Result.Compose<string?, string, string>(
    ValidateNotEmpty, 
    ValidateEmailFormat
);

// Now you can use the composed function
Result<string> result1 = validateEmail("user@example.com"); // Success
Result<string> result2 = validateEmail(""); // Failure("Input cannot be empty") 
Result<string> result3 = validateEmail("invalid-email"); // Failure("Invalid email format")

// This is equivalent to chaining with Bind:
Result<string> manual = ValidateNotEmpty(input).Bind(ValidateEmailFormat);
```

### Lift — lift pure multi-arg functions and accumulate errors

**What it does:** Takes a regular function that needs multiple arguments and makes it work with multiple Result values. It collects ALL errors if any inputs fail (unlike Bind which stops at the first failure).

**Why use it:** You want to validate multiple fields and show ALL validation errors at once, rather than stopping at the first error.

```csharp
// A pure function that combines multiple values
decimal CalculateTotalPrice(decimal basePrice, decimal taxRate, int quantity) =>
    basePrice * (1 + taxRate) * quantity;

// Multiple validation results - some succeed, some fail
Result<decimal> priceResult = ValidatePrice(userInput.Price); // Success(100.0m)
Result<decimal> taxResult = ValidateTaxRate(userInput.Tax);   // Failure("Invalid tax rate")
Result<int> quantityResult = ValidateQuantity(userInput.Qty); // Failure("Quantity must be positive")

// Lift the pure function to work with Results
Result<decimal> totalResult = Result.Lift(
    CalculateTotalPrice, 
    priceResult, 
    taxResult, 
    quantityResult
);

// Result: Failure with BOTH tax and quantity errors
// If all inputs were valid: Success(calculated total)

// Compare to manual approach (stops at first error):
Result<decimal> manualResult = priceResult
    .Bind(price => taxResult
        .Bind(tax => quantityResult
            .Map(qty => CalculateTotalPrice(price, tax, qty))));
// This would only show the tax error, not the quantity error
```

## 📋 Complete Example

```csharp
using ErikLieben.FA.Results;
using ErikLieben.FA.Results.Validations;

public record CreateUserDto(string? Name, string? Email, int Age);

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

// Domain validation logic
static Result<User> ValidateAndCreateUser(CreateUserDto dto)
{
    var name = ValidationBuilder.ValidateNotNullOrWhiteSpace(
        dto.Name, "Name is required", "Name");
        
    var email = ValidationBuilder.ValidateNotNullOrWhiteSpace(
        dto.Email, "Email is required", "Email")
        .Bind(ValidateEmailFormat);
        
    var age = ValidationBuilder.ValidateRange(
        dto.Age, 18, 100, "Age must be between 18 and 100", "Age");

    var validation = ResultCombinators.Combine(name, email, age);
    if (validation.IsFailure)
        return Result<User>.Failure(validation.Errors.ToArray());

    var user = new User 
    { 
        Id = Guid.NewGuid(), 
        Name = name.Value, 
        Email = email.Value,
        Age = age.Value,
        CreatedAt = DateTimeOffset.UtcNow
    };
    
    return Result<User>.Success(user);
}

static Result<string> ValidateEmailFormat(string email)
{
    return email.Contains("@") && email.Contains(".")
        ? Result<string>.Success(email)
        : Result<string>.Failure("Invalid email format", "Email");
}

// API endpoint
app.MapPost("/users", (CreateUserDto dto) =>
{
    var result = ValidateAndCreateUser(dto);
    return result.ToCreatedAtRouteResult(
        routeName: "GetUser",
        routeValues: new { id = result.IsSuccess ? result.Value.Id : null },
        successMessage: "User created successfully"
    );
});
```

## 🧪 Testing

Override the time provider for consistent timestamps in tests:

```csharp
[Test]
public void TestApiResponse()
{
    ApiResponseTimeProvider.SharedTimeProvider = 
        new FakeTimeProvider(DateTimeOffset.Parse("2025-01-01T00:00:00Z"));
    
    // Your test code here...
    
    ApiResponseTimeProvider.SharedTimeProvider = TimeProvider.System; // Restore
}
```

## 💡 Best Practices & Common Pitfalls

### Do's ✅
- **Use for domain modeling** - represent expected business failures
- **Chain with `Bind`** - when your next operation can also fail
- **Transform with `Map`** - when you want to modify success values
- **Use `Match()` or `ValueOr()`** - avoid accessing `Value` directly
- **Convert spans to arrays** - if you need multiple enumerations of errors

### Don'ts ❌
- **Don't replace all exceptions** - use for expected domain failures only
- **Don't use for infrastructure errors** - database timeouts, network failures, etc.
- **Don't access `Value` directly** - it throws on failure
- **Don't over-engineer** - simple boolean checks might be sufficient
- **Don't force functional style** - readability trumps cleverness

### Common Pitfalls
```csharp
// ❌ Wrong: Using Result for infrastructure
Result<string> content = Result.Try(() => File.ReadAllText("config.json"));

// ✅ Right: Let infrastructure exceptions bubble up, handle at boundary
string content = File.ReadAllText("config.json"); // Let it throw

// ❌ Wrong: Accessing Value unsafely  
var result = ParseAge("not-a-number");
int age = result.Value; // Throws!

// ✅ Right: Safe access
int age = result.ValueOrDefault(0);
```

## 📚 Further Reading

- [Domain Modeling Made Functional](https://fsharpforfunandprofit.com/books/) - Scott Wlaschin
- [Against Railway-Oriented Programming](https://fsharpforfunandprofit.com/posts/against-railway-oriented-programming/) - When NOT to use Result
- [Railway-Oriented Programming](https://fsharpforfunandprofit.com/rop/) - Original concept

## 📄 License

MIT License - see the repository's LICENSE file for details.
