using CommunityToolkit.Mvvm.Messaging.Messages;
using OldBit.Spectron.Emulation.Devices.Gamepad;
using OldBit.Spectron.Settings;

namespace OldBit.Spectron.Messages;

public class ShowPreferencesViewMessage(Preferences preferences, GamepadManager gamepadManager) : AsyncRequestMessage<Preferences>
{
    public Preferences Preferences { get; } = preferences;
    public GamepadManager GamepadManager { get; } = gamepadManager;
}