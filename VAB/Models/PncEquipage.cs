namespace VAB.Models
{
    public class PncEquipage
    {
        public int PncId { get; set; }
        public Pnc Pnc { get; set; }

        public int EquipageId { get; set; }
        public Equipage Equipage { get; set; }
    }
}
