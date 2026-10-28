using Loddlepelago.CopyPastas;
using UnityEngine;
using static Loddlepelago.Archipelago.LoddleClient;

namespace Loddlepelago.Archipelago;

public class APGui : GuiBuilder
{
    public string State = "";
    public Window NotConnectedBox;
    public Window ConnectedBox;

    protected override void InitGUI()
    {
        SetterGetter<string> addPortSetterGetter = new(() => Data.AddressPort, s => Data.AddressPort = s);
        SetterGetter<string> passwordSetterGetter = new(() => Data.Password, s => Data.Password = s);
        SetterGetter<string> slotNameSetterGetter = new(() => Data.SlotName, s => Data.SlotName = s);
        SetterGetter<string> recycleBotCostSetterGetter = new(
            () => $"{Data.RecycleCost}",
            s => Data.RecycleCost = Math.Clamp(int.TryParse(s, out var num) ? num : Data.RecycleCost, 1, 100)
        );

        NotConnectedBox = new Window(0, 100, 100, 200, "AP Client")
           .AddChildren(
                new Label("Address:port", 30), new TextField(addPortSetterGetter, 25),
                new Label("Password", 30), new TextField(passwordSetterGetter, 25),
                new Label("Slot Name", 30), new TextField(slotNameSetterGetter, 25),
                new Label("Recycle Bot Ingot Cost (1-100)", 30), new TextField(recycleBotCostSetterGetter, 25),
                new Spacer(30), new UiButton("Connect", 30, TryConnectClient),
                new Label(
                    new TextRef(() => State is not "" ? State : "Not Connected"), 20, Color.red,
                    alignment: TextAnchor.UpperLeft
                )
            );

        ConnectedBox = new Window(1, 100, 100, 200, "AP Client")
           .AddChildren(
                new Label("Recycle Bot Ingot Cost (1-100)", 30), new TextField(recycleBotCostSetterGetter, 25),
                new Spacer(30), new UiButton("Disconnect", 30, () => Client.TryDisconnect()),
                new Label("Connected", 20, Color.green, alignment: TextAnchor.UpperLeft)
            );
    }

    protected override void OnGUI()
    {
        (!Client.IsConnected ? NotConnectedBox : ConnectedBox).Render();
        Core.PlayButton?.SetActive(Client.IsConnected);
    }

    public void TryConnectClient()
    {
        var error = TryConnect(Data.AddressPort, Data.Password, Data.SlotName);
        State = error is not null ? string.Join("\n", error) : "";
        if (error is not null) return;
        SaveFile();
    }
}