using OldBit.Beep.Filters;

namespace OldBit.Spectron.Emulation.Devices.Audio;

// TODO: Implement some kind of filter to avoid clicks and pops (Gaussian filter?)
public class BeeperFilter : IAudioFilter
{
    public float Apply(float value)
    {
       // if (value != -1)
        {
          //  System.Diagnostics.Debug.WriteLine($"BeeperFilter: {value}");
        }



        return value;
    }
}