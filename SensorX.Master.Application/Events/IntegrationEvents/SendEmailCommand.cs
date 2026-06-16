using MassTransit;

namespace SensorX.Master.Application.Events.IntegrationEvents;

[MessageUrn("send-email-command")]
[EntityName("send-email-command")]
public class SendEmailCommand
{
    public string? To { get; set; }
    public string? ToName { get; set; }
    public string? Role { get; set; }
    public string Subject { get; set; } = default!;
    public string HtmlBody { get; set; } = default!;
}
