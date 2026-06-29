using System.Linq;
using Content.IntegrationTests.Tests.Interaction;
using Content.Shared._HL.Rooms;
using Content.Shared.Mind;
using Content.Shared.Verbs;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;

namespace Content.IntegrationTests.Tests.GridSaving;

[TestFixture]
public sealed class ApartmentTests : InteractionTest
{
    private NetEntity _apartmentConsole = default;
    private RoomGridSpawnerConsoleComponent _consoleComponent = default;

    [Test]
    public async Task SaveAndLoadApartment()
    {
        await SetupTest();

        await LoadApartment();
        await SpawnEntityInApartment("Protolathe");
        Console.WriteLine(SEntMan.EntityCount);
        //Assert.That(await FindEntity("Protolathe", shouldSucceed: false) != default, "Lathe did not spawn.");
        await StashApartment();
        Console.WriteLine(SEntMan.EntityCount);
        //Assert.That(await FindEntity("Protolathe", shouldSucceed: false) == default, "Lathe was not saved and removed.");
        await LoadApartment();
        Console.WriteLine(SEntMan.EntityCount);
        //Assert.That(await FindEntity("Protolathe", shouldSucceed: false) != default, "Lathe was not re-loaded");
    }

    private async Task SetupTest()
    {
        // Give the user a visiting mind
        var mindSystem = SEntMan.System<SharedMindSystem>();
        await Server.WaitPost(() =>
        {
            var playerId = Client.Session.Data.UserId;
            var mind = mindSystem.CreateMind(playerId);
            mindSystem.Visit(mind, ToServer(Player), mind: mind.Comp);
        });

        //Spawn Room Console
        var coords = new EntityCoordinates(MapData.MapUid, 0, 0);
        TargetCoords = SEntMan.GetNetCoordinates(coords);
        await SpawnTarget("ComputerRoomGridSpawner");
        await UpdateConsole(false);
        var grid = MapData.Grid;
        ToggleNeedPower();

        // Spawn tiles in 20x20 square so we have a constant grid to work with
        for (var x = 0; x < 19; x++)
        {
            for (var y = 0; y < 19; y++)
            {
                var tileCoords = new NetCoordinates(FromServer(grid.Owner), new System.Numerics.Vector2(x - 10, y - 10));
                await SetTile(Plating, tileCoords, MapData.Grid);
            }
        }

        // Spawn Area Marker
        await SpawnEntity("RoomGridSpawnAreaMarker", new EntityCoordinates(grid.Owner, 8, 0));
        Assert.That(_consoleComponent.InUse, Is.False, "Apartment Console spawned in use.");
    }

    private async Task LoadApartment()
    {
        Assert.That(_consoleComponent.InUse, Is.False, "Tried to load an apartment into a non-empty console.");
        await Interact();
        await Server.WaitRunTicks(30);
        // Interacting re-creates the console with a new ID for some reason, so just make sure we get the right one
        await UpdateConsole();
    }

    private async Task StashApartment()
    {
        Assert.That(_consoleComponent.InUse, Is.True, "Tried to save an apartment without one loaded.");
        var verbSystem = SEntMan.System<SharedVerbSystem>();
        await Server.WaitPost(() =>
        {
            var verbs = verbSystem.GetLocalVerbs(ToServer(Target.Value), ToServer(Player), typeof(AlternativeVerb));
            var saveVerb = verbs.First(v => v.Text == "Stash room");
            verbSystem.ExecuteVerb(saveVerb, ToServer(Player), ToServer(Target.Value));
        });
        await Server.WaitRunTicks(30);
        await UpdateConsole();
    }

    private async Task SpawnEntityInApartment(string entProto)
    {
        await SpawnEntity(entProto, new EntityCoordinates(MapData.Grid.Owner, 7, 0));
    }

    private async Task UpdateConsole(bool updateTarget = true)
    {
        await Server.WaitPost(() =>
        {
            var consoles = SEntMan.AllEntities<RoomGridSpawnerConsoleComponent>();
            Assert.That(consoles.Count, Is.EqualTo(1), "Too many Consoles in existance.");
            _apartmentConsole = FromServer(SEntMan.AllEntities<RoomGridSpawnerConsoleComponent>().First());
            if (updateTarget)
                Target = _apartmentConsole;
            _consoleComponent = SEntMan.GetComponent<RoomGridSpawnerConsoleComponent>(ToServer(_apartmentConsole));
        });
    }
}
