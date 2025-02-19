namespace Exercise5.Models;

public class Animal
{
    public int IdAnimal { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Category { get; set; } = null!;
    public string Area { get; set; } = null!;
}
