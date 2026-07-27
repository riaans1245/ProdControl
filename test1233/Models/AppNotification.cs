namespace test1233.Models;

public class AppNotification
{
    public int NotificationId { get; set; }

    public required string Notification { get; set; }

    public int UserId { get; set; }

    public required string UserName { get; set; }
}