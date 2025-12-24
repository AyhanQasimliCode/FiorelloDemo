using System.ComponentModel.DataAnnotations;

namespace Fiorello.Areas.Admin.ViewModels.SliderVM
{
    public class CreateSliderVM
    {
        [Required(ErrorMessage = "Bos ola bilmez")]

        public IFormFile Photo { get; set; }
    }
}
