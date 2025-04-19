# Dewback

A collection of Visual Studio Extensions (vsix) I work on to learn and improve productivity.

---

## EntityToModel

**EntityToModel** is a Roslyn-based analyzer and code fix provider that helps you **automatically generate clean model classes** from your EF Core entity classes.

---

### Prerequisites

Your solution must include a **project named** `Logic.Models` (a standard C# class library). This is where your model classes will be generated.

---

### How It Works

1. In your **entity project** (e.g. `Logic.Entities`), annotate a class with the `[Table]` attribute (from `System.ComponentModel.DataAnnotations.Schema`).
2. A lightbulb/code fix suggestion will appear.
3. Select **"Generate model class"**.
4. A new model class will be created in the `Logic.Models` project, mirroring the original entity's properties — **but without EF Core-specific attributes** like `[Table]`, `[Key]`, etc.

---

### Example

```csharp
// Logic.Entities/Person.cs
/// <summary>
/// Represents a person.
/// </summary>
[Table("People")]
public class Person
{
    /// <summary>
    /// The unique identifier of the person.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the person.
    /// </summary>
    public string Name { get; set; }
}

becomes:

// Logic.Models/PersonModel.cs
/// <summary>
/// Represents a person.
/// </summary>
public class PersonModel
{
    /// <summary>
    /// The unique identifier of the person.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the person.
    /// </summary>
    public string Name { get; set; }
}
```
