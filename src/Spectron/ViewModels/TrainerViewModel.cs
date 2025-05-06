using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Extensions;
using OldBit.Spectron.Files.Pok;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class TrainerViewModel : ReactiveObject
{
    private readonly Emulator _emulator;
    private readonly PokeFile? _pokeFile;

    public ObservableCollection<TrainerItemViewModel> Trainers { get; } = [];

    public TrainerViewModel(Emulator emulator, PokeFile? pokeFile)
    {
        _emulator = emulator;
        _pokeFile = pokeFile;

        if (_pokeFile != null)
        {
            LoadTrainers();
        }
    }

    private void LoadTrainers()
    {
        foreach (var trainer in _pokeFile!.Trainers)
        {
            var trainerItem = new TrainerItemViewModel(trainer)
            {
                IsEnabled = IsTrainerEnabled(trainer),
            };

            foreach (var poke in trainer.Pokes.Where(x => x.Value == null))
            {
                var pokeItem = new TrainerItemPokeViewModel
                {
                    Address = poke.Address.ToString(),
                    CustomValue = ReadMemoryValue(poke),
                };

                trainerItem.CustomPokes.Add(pokeItem);
            }

            trainerItem.WhenAnyValue(x => x.IsEnabled)
                .Skip(1)
                .Subscribe(isEnabled =>
            {
                if (isEnabled)
                {
                    ApplyTrainer(trainerItem);
                }
                else
                {
                    UndoTrainer(trainerItem);
                }
            });

            Trainers.Add(trainerItem);
        }
    }

    private bool IsTrainerEnabled(Trainer trainer)
    {
        foreach (var poke in trainer.Pokes)
        {
            var value = ReadMemoryValue(poke);

            if (value != poke.Value)
            {
                return false;
            }
        }

        return true;
    }

    private void ApplyTrainer(TrainerItemViewModel trainerItem)
    {
        foreach (var poke in trainerItem.Trainer.Pokes)
        {
            var value = trainerItem.CustomPokes.Where(x => x.Address == poke.Address.ToString())
                .Select(x => x.CustomValue)
                .FirstOrDefault();

            WriteMemoryValue(poke, value ?? poke.Value ?? 0);
        }
    }

    private void UndoTrainer(TrainerItemViewModel trainerItem)
    {
        foreach (var poke in trainerItem.Trainer.Pokes)
        {
            WriteMemoryValue(poke, poke.OriginalValue);
        }
    }

    private byte? ReadMemoryValue(Poke poke) =>
        _emulator.Memory.Read(poke.Address, poke.MemoryBank > 7 ? null : poke.MemoryBank);

    private void WriteMemoryValue(Poke poke, byte value) =>
        _emulator.Memory.Write(poke.Address, value, poke.MemoryBank > 7 ? null : poke.MemoryBank);
}