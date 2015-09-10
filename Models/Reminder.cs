using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace MvcApplication2.Models
{
    public class Reminder
    {
        [Required(ErrorMessage = "Falta a indicação da repetição semanal dos lembretes")]
        public IEnumerable<Boolean> repeatingDays { get; set; }
        public int id { get; set; }

        [Required]
        [RegularExpression(@"\d{4,4}-\d{1,2}-\d{1,2}", ErrorMessage = "Formato de data inválido")]
        public string date { get; set; }

        [Required]
        [RegularExpression(@"\d{1,2}:\d{1,2}", ErrorMessage = "Formato de tempo inválido")]
        public string time { get; set; }

        
        public string urls { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Contacto inválido")]
        public string contact { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Título inválido, demasiados caracteres")]
        public string title { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Descrição inválida, demasiados caracteres")]
        public string description { get; set; }
        public int daysofweek { get; set; }

    }
}