using System.ComponentModel.DataAnnotations;

namespace ApiElTiempo.Models
{
    public class TbUser
    {
        public int IdUser { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string PasswordUser { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
