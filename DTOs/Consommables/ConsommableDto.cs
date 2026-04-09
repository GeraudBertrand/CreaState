namespace CreaState.DTOs.Consommables
{
    public class ConsommableDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public int Quantite { get; set; }
        public int Seuil { get; set; }
        public string CouleurNom { get; set; } = string.Empty;
        public string CouleurHex { get; set; } = string.Empty;
        public bool IsLowStock { get; set; }
    }
}
