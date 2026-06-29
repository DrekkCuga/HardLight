using System.Linq;
using Content.Client._NF.Shipyard.UI;
using Content.Client.Shuttles.Save;
using Content.IntegrationTests.Tests.Interaction;
using Content.Server._NF.Bank;
using Content.Shared._NF.Bank.Components;
using Content.Shared._NF.Shipyard;
using Content.Shared._NF.Shipyard.Events;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Map;

namespace Content.IntegrationTests.Tests.GridSaving;

[TestFixture]
public sealed class ShuttleTests : InteractionTest
{
    private const string TestVesselName = "AVL Nest";
    private ShipyardConsoleMenu _window = default;
    private bool _hasShip = false;

    [Test]
    public async Task SaveAndLoadShuttle()
    {
        await SetupTest();

        await BuyVessel();
        await SaveVessel();
        await LoadVessel();
    }

    private async Task SetupTest()
    {
        //System dependencies
        var bankSys = SEntMan.EntitySysManager.GetEntitySystem<BankSystem>();

        // Give the player a bank account and some money
        await Server.WaitPost(() => { SEntMan.EnsureComponent<BankAccountComponent>(ToServer(Player)); });
        bankSys.TryBankDeposit(ToServer(Player), 10000000);

        //Spawn shipyard console
        TargetCoords = SEntMan.GetNetCoordinates(new EntityCoordinates(MapData.MapUid, 1, 0));
        await SpawnTarget("ComputerShipyard");
        ToggleNeedPower();

        // Add ID Card to the console
        await InteractUsing("UniversalIDCard");

        // Open the Computer
        await Interact();
        _window = GetWindow<ShipyardConsoleMenu>();

        // Search for the Vessel
        var searchBar = GetControlFromField<LineEdit>(nameof(_window.SearchBar), _window);
        await Client.WaitPost(() => { searchBar.SetText("Nest", true); });
    }

    private async Task BuyVessel()
    {
        Assert.That(_hasShip, Is.False, "Tried to buy a ship, but already have one.");
        var vesselEntry = (VesselRow)_window.Vessels.Children.First(c => ((VesselRow)c).VesselName.Text == TestVesselName);
        var vesselId = vesselEntry.Vessel.ID;
        await SendBui(ShipyardConsoleUiKey.Shipyard, new ShipyardConsolePurchaseMessage(vesselId));
        Assert.That(GetGridCount(), Is.GreaterThan(1), "Bought vessel, but it didn't spawn?");
        _hasShip = true;
        await Pair.RunSeconds(1);
    }

    private async Task SaveVessel()
    {
        Assert.That(_hasShip, Is.True, "Tried to save a ship, but didn't have one to begin with!");
        await SendBui(ShipyardConsoleUiKey.Shipyard, new ShipyardConsoleSaveMessage());
        Assert.That(GetGridCount(), Is.EqualTo(1), "Saved vessel but it didn't despawn.");
        _hasShip = false;
        await Pair.RunSeconds(1);
    }

    private async Task LoadVessel()
    {
        Assert.That(_hasShip, Is.False, "Tried to Load a ship, but already had one.");
        var loadEntries = _window.SavedShipsList;
        Assert.That(loadEntries.Count(), Is.EqualTo(1), "Test Client has more than one ship saved!");
        var loadVessel = loadEntries.First();

        // Load the Vessel
        var shipFileMgmtSys = CEntMan.EntitySysManager.GetEntitySystem<ShipFileManagementSystem>();
        var filePath = (string)loadVessel.Metadata!;
        var yamlData = "";
        await Client.WaitPost(async () =>
        {
            yamlData = await shipFileMgmtSys.GetShipYamlData(filePath);
        });
        await SendBui(ShipyardConsoleUiKey.Shipyard, new ShipyardConsoleLoadMessage(yamlData, filePath));
        Assert.That(GetGridCount(), Is.GreaterThan(1), "Loaded Vessel, but it didn't spawn in.");
        _hasShip = true;
        await Pair.RunSeconds(1);
    }

    private int GetGridCount()
    {
        return MapMan.GetAllGrids(MapId).Count();
    }
}
