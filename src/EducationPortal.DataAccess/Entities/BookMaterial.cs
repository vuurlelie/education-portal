namespace EducationPortal.DataAccess.Entities;

public class BookMaterial : Material
{
    public required string Authors { get; set; } 
    public int Pages { get; set; }

    public int FormatId { get; set; }
    public BookFormat? Format { get; set; }

    public int PublicationYear { get; set; }
}