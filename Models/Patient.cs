using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using QSProject.Data.Validators;

namespace QSProject.Data.Models
{
    public class Patient
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }


        [Range(18, 99)]
        public int Age { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        // Custom Validator
        [UrlResource]
        public string PhotoUrl { get; set; }

        // 1-N relationship
        public IList<Medicine> Medicines { get; set; } = new List<Medicine>();


    }
}
