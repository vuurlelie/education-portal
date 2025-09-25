namespace EducationPortal.DataAccess.Entities;

public class BookFormat
{
    public int Id { get; set; }         
    public required string Name { get; set; }
    public ICollection<BookMaterial> Books { get; set; } = new List<BookMaterial>();
}