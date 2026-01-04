namespace WebApplication1.Models;

public static class AbonnementType
{
    // Prix mensuel de base pour chaque type d'abonnement (en DH)
    private static readonly Dictionary<string, decimal> PrixMensuels = new()
    {
        { "Étudiant", 150m },
        { "Standard", 250m },
        { "Premium", 400m },
        { "VIP", 600m },
        { "Famille", 500m },
        { "Senior", 200m }
    };

    public static Dictionary<string, decimal> GetTypes()
    {
        return PrixMensuels;
    }

    public static List<string> GetTypeNames()
    {
        return PrixMensuels.Keys.ToList();
    }

    public static decimal GetPrixMensuel(string type)
    {
        return PrixMensuels.TryGetValue(type, out var prix) ? prix : 0m;
    }

    public static decimal CalculerPrix(string type, int dureeMois)
    {
        var prixMensuel = GetPrixMensuel(type);
        return prixMensuel * dureeMois;
    }

    public static bool EstTypeValide(string type)
    {
        return PrixMensuels.ContainsKey(type);
    }
}






