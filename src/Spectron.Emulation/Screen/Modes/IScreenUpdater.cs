namespace OldBit.Spectron.Emulation.Screen.Modes;

public interface IScreenUpdater
{
    void Update(int frameBufferIndex, Word bitmapAddress, Word attributeAddress);

    void Invalidate();

    void SetDirty(int address);

    void ToggleFlash();
}