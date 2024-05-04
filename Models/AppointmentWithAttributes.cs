using System.Diagnostics.Tracing;

namespace hoistmt.Models;

public class eventAttribute
{
    //id, title, start_time, end_time, description, notes, backgroundColor, borderColor, textColor, editable, startEditable, durationEditable, resourceEditable, display, overlap, constraint, allDay, classNames, url, extendedProps
    public int id { get; set; }
    public string title { get; set; }
    public DateTime start_time { get; set; }
    public DateTime end_time { get; set; }
    public string? description { get; set; }
    public string? notes { get; set; }
    public string? backgroundColor { get; set; }
    public string? borderColor { get; set; }
    public string? textColor { get; set; }
    public bool editable { get; set; }
    public bool startEditable { get; set; }
    public bool durationEditable { get; set; }
    public bool resourceEditable { get; set; }
    public string? display { get; set; }
    public string? overlap { get; set; }
    public string? constraint { get; set; }
    public bool allDay { get; set; }
    public string? classNames { get; set; }
}