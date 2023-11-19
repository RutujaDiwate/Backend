using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class EmployeeDetails
    {
        [Key]
        public string userId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}