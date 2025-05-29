using DAL.Models;
using System.ComponentModel.DataAnnotations;

namespace ProjectAPI.DTO.EstablishmentDTOs
{
    public class EstablishmentDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Location { get; set; }
        [Required]
        [Display(Name = "Opening Hours ")]
        public string OpeningHours { get; set; }
        [Required]
        [Display(Name = "Contact Number ")]
        public string ContactNumber { get; set; }
        [Required]
        [Display(Name = "Establishment Category")]
        public EsbCategoryType EsbCategory { get; set; }
        [Required]
        [Display(Name = "Category id")]
        public int CategoryId { get; set; }
    }
    public static class EstablishmentExtensions
    {
        public static Establishment ToEstablishmentModel(this EstablishmentDTO establishmentDTO, Establishment establishment = null)
        {
            if (establishment == null)
                establishment = new Establishment();

            establishment.Name = establishmentDTO.Name;
            establishment.Location = establishmentDTO.Location;
            establishment.ContactNumber = establishmentDTO.ContactNumber;
            establishment.EsbCategory = establishmentDTO.EsbCategory;
            establishment.CategoryId = establishmentDTO.CategoryId;
            establishment.OpeningHours = establishmentDTO.OpeningHours;
            
            return establishment;
        }
    }
}
