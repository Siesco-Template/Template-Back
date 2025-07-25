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
        Sent = 3
    }
}