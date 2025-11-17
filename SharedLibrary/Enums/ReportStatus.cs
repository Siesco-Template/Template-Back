using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Enums
{
    public enum ReportStatus : byte
    {
        [Display(Name = "Tərtib olunub")]
        Compiled = 1,

        [Display(Name = "Baxılıb")]
        Seen = 2,

        [Display(Name = "Göndərilib")]
        Sent = 3,

        [Display(Name = "Daxil edildi")]
        DaxilEdildi = 4,

        [Display(Name = "Təsdiq edildi")]
        TesdiqEdildi = 5,

        [Display(Name = "Qaralama")]
        Qaralama = 6,

        [Display(Name = "Təqdim edilmiş")]
        TeqdimEdilmis = 7,

        [Display(Name = "Vaxtında")]
        Vaxtinda = 8,

        [Display(Name = "Gecikmiş")]
        Gecikmis = 9
    }
}