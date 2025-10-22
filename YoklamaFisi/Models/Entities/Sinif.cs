namespace YoklamaFisi.Models.Entities
{
    public class Sinif
    {
        public int SinifId { get; set; }
        public string SinifAd { get; set; }
        public ICollection<Ders> Dersler { get; set; } = new List<Ders>();
    }
}
