using System.ComponentModel.DataAnnotations;

namespace App3.Modals
{
    public enum Criterion
    {
        [Display(Name = "track")] track,
        [Display(Name = "artist")] artist
    }
}
