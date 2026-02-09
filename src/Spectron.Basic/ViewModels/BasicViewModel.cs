using System.Text;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.ComponentModel;
using OldBit.Spectron.Basic.Reader;
using OldBit.Spectron.Emulation.Devices.Memory;

namespace OldBit.Spectron.Basic.ViewModels;

public partial class BasicViewModel(IEmulatorMemory memory) : ObservableObject
{
    [ObservableProperty]
    private TextEditor? _basicViewer;

    public void Loaded()
    {
        var reader = new BasicMemoryReader(memory);
        var lines = reader.ReadAllLines();

        var text = new StringBuilder();

        foreach (var line in lines)
        {
            text.Append(line.LineNumber.ToString().PadLeft(4, ' '));
            text.Append(' ');
            text.Append("TODO");
            text.AppendLine();
        }

        BasicViewer?.Text = text.ToString();
    }

    partial void OnBasicViewerChanged(TextEditor? value) =>
        value?.TextArea.TextView.LineTransformers.Add(new BasicLineTransformer());
}