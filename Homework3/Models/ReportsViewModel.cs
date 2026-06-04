namespace Homework3.Variant18.Models;


public sealed class ReportsViewModel
{
    
    public IReadOnlyList<TeacherReportRow> Teachers { get; init; } = Array.Empty<TeacherReportRow>();

   
    public IReadOnlyList<DepartmentCountReportRow> DepartmentCounts { get; init; } = Array.Empty<DepartmentCountReportRow>();

    
    public IReadOnlyList<DepartmentAverageReportRow> DepartmentAverages { get; init; } = Array.Empty<DepartmentAverageReportRow>();
}


public sealed class TeacherReportRow
{
    
    public string TeacherName { get; init; } = string.Empty;

    
    public string DepartmentName { get; init; } = string.Empty;

    
    public int Publications { get; init; }
}


public sealed class DepartmentCountReportRow
{
    
    public string DepartmentName { get; init; } = string.Empty;

    
    public int TeacherCount { get; init; }
}


public sealed class DepartmentAverageReportRow
{
    
    public string DepartmentName { get; init; } = string.Empty;

    
    public double AveragePublications { get; init; }
}
