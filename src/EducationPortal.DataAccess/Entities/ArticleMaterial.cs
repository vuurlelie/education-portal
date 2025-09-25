namespace EducationPortal.DataAccess.Entities;

public class ArticleMaterial : Material
{
    public ArticleMaterial() { }
    public required DateOnly PublishedAt { get; set; }  
    public required string SourceUrl { get; set; }
}