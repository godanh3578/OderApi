using System.ComponentModel.DataAnnotations;

namespace OrderApi.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = "";

        [Phone]
        public string Phone { get; set; } = "";
    }
}