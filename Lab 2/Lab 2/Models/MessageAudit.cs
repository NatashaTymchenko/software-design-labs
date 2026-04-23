namespace MessengerApp.Models;

public class MessageAudit
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public string OldText { get; set; } = string.Empty;
    public DateTime EditedAt { get; set; }
}