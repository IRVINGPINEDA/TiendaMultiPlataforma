namespace ProductHub.Infrastructure.Entities;

public static class OrderStatus
{
    public const string Pendiente = "Pendiente";
    public const string Confirmada = "Confirmada";
    public const string Enviada = "Enviada";
    public const string Completada = "Completada";
    public const string Cancelada = "Cancelada";

    public static readonly IReadOnlyList<string> All =
    [
        Pendiente,
        Confirmada,
        Enviada,
        Completada,
        Cancelada
    ];

    public static string? Normalize(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        return All.FirstOrDefault(s => s.Equals(status.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
