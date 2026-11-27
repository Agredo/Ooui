using Aurora.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Aurora.ViewModels;

public sealed class JourneyStepViewModel : ObservableObject
{
    public JourneyStepViewModel(int number, ItineraryDay day)
    {
        Number = number.ToString("00");
        Day = day;
    }

    public ItineraryDay Day { get; }
    public string Number { get; }
}
