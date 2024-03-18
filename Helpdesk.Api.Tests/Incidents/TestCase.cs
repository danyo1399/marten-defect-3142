using System;
using System.Threading.Tasks;
using Helpdesk.Api.Incidents;
using Marten;
using Marten.Events;
using Marten.Events.Projections;
using Marten.Services.Json;
using Weasel.Core;
using Xunit;

namespace Helpdesk.Api.Tests.Incidents;

public class TestCase
{
  [Fact]
  public async Task Test()
  {
// docker run -itd --name marten-test-db --restart=unless-stopped -p:7432:5432 -e POSTGRES_PASSWORD=password postgres:14
    var options = new StoreOptions();

    var schemaName = Environment.GetEnvironmentVariable("SchemaName") ?? "Helpdesk";
    options.Events.DatabaseSchemaName = schemaName;
    options.DatabaseSchemaName = schemaName;
    options.Events.StreamIdentity = StreamIdentity.AsString;
    options.SourceCodeWritingEnabled = false;
    options.Connection("Host=localhost;Port=7432;Database=postgres;User Id=postgres;Password=password;");

    options.Projections.Snapshot<Incident>(SnapshotLifecycle.Inline).Identity(x => x.Id).Index(m => m.ServiceId)
      .Index(m => m.ProviderId)
      .UniqueIndex(model => model.IdNumber)
      .UniqueIndex(
        "uidx_source_idp_ids_id",
        m => m.Source.Id,
        m => m.ProviderId,
        m => m.ServiceId);
    ;
    options.UseDefaultSerialization(
      EnumStorage.AsString,
      nonPublicMembersStorage: NonPublicMembersStorage.All,
      serializerType: SerializerType.SystemTextJson
    );
    var store = new DocumentStore(options);
    await store.Advanced.ResetAllData();

    var session = store.IdentitySession();

    var id1 = Guid.NewGuid().ToString();
    var id2 = Guid.NewGuid().ToString();
    session.Events.Append(
      id1,
      new Created()
      {
        Source = new Source() { Id = "bla", Type = SourceType.Something }
      });

    session.Events.Append(
      id2,
      new Created()
      {
        Source = new Source() { Id = "bla", Type = SourceType.Something }
      });
    await session.SaveChangesAsync();

    session = store.IdentitySession();
    var a1 = await session.LoadAsync<Incident>(id1);
    var a2 = await session.LoadAsync<Incident>(id2);

    session.Events.Append(id1, new Updated("desc"));
    session.Events.Append(id2, new Updated("desc2"));
    await session.SaveChangesAsync();
  }
}