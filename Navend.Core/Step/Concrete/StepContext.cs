namespace Navend.Core.Step.Concrete;

public abstract class StepContext
{
    public DateTime StartedAt { get; } = DateTime.UtcNow;
    // Ortak özellikler buraya eklenebilir: Logger, HataListesi, vb.
}