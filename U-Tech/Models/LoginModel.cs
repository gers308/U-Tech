using System.ComponentModel.DataAnnotations;

namespace U_Tech.Models
{
    public class LoginModel
    {
        // Validación para el login
        [Required(ErrorMessage = "El usuario es obligatorio")]
        [StringLength(50, ErrorMessage = "El usuario no puede tener más de 50 caracteres")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "La contraseña debe tener entre 4 y 100 caracteres")]
        public string Contrasena { get; set; }

        // 🔥 PASO 1: Nueva propiedad para el checkbox "Recordarme"
        [Display(Name = "Recordarme")]
        public bool Recordarme { get; set; }
    }
}