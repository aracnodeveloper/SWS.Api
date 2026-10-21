namespace App.Modules.Catalog;

public class CategoryListDTO
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public int ProductCount { get; set; }
}
