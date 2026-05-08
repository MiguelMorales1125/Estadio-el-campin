using StadiumSystem.Domain.Enums;

namespace StadiumSystem.Domain.Entities;

public class StadiumState
{
    public int Id { get; set; }
    public StadiumStates Mode { get; set; } = StadiumStates.MANTENIMIENTO;
}
