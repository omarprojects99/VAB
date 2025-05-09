using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VAB.Models
{
    public class Equipage
    {
        [Key]
        public int EquipageId { get; set; }

        [Required]
        public int Cc1Id { get; set; }

        [ForeignKey("Cc1Id")]
        public Cc Cc1 { get; set; }

        public int? Cc2Id { get; set; }

        [ForeignKey("Cc2Id")]
        public Cc? Cc2 { get; set; }

        [Required]
        public int Pnc1Id { get; set; }

        [ForeignKey("Pnc1Id")]
        public Pnc Pnc1 { get; set; }

        public int? Pnc2Id { get; set; }

        [ForeignKey("Pnc2Id")]
        public Pnc? Pnc2 { get; set; }

        public List<PncEquipage> PncEquipages { get; set; } = new List<PncEquipage>();


    }
}