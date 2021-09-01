using System.ComponentModel.DataAnnotations;
using System;
using System.Text.Json.Serialization;


namespace QSProject.Data.Models
{
    public class Medicine
    {
        public int Id { get; set; }

        // Foreign key relating to Patient medicine request owner
        public int PatientId { get; set; }

        // Required to stop cyclical Json parse error in web api
        [JsonIgnore]
        public Patient Patient { get; set; } // navigation property


   
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string MedicineName { get; set; }

        [StringLength(100, MinimumLength = 1)]
        public string Resolution { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime ResolvedOn { get; set; } = DateTime.MinValue;
        public bool Active { get; set; }





    }
}
