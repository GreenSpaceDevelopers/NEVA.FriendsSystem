namespace Application.Common.Models;

public enum SortDirection
{
    Asc,
    Desc
}

public class SortExpression
{
    public string PropertyName { get; set; } = null!;
    
    public SortDirection Direction { get; set; }
} 