using System;

namespace Helpdesk.Api.Incidents;

public record Created()
{
  public Source Source { get; init; } = Source.Unknown;
};

public record Updated(string Description);

public enum SourceType
{
  Unknown = 0,
  Something = 1
}

public record Source()
{
  public required string Id { get; init; }
  public required SourceType Type { get; init; }
  public static Source Unknown = new Source() { Id = "", Type = SourceType.Unknown };
}

public class Incident
{
  public Incident()
  {
  }

  public Source Source { get; set; } = Source.Unknown;
  public string Id { get; set; }
  public string ServiceId { get; set; }
  public string ProviderId { get; set; }
  public string IdNumber { get; set; }

  public string Description { get; set; }

  public void Apply(Created evt)
  {
    Source = evt.Source;
    ServiceId = $"Service {evt.Source.Id}";
    ProviderId = $"Provider {evt.Source.Id}";
  }

  public void Apply(Updated evt)
  {
    Description = evt.Description;
  }
}