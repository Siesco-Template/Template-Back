using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Enums
{
    public enum Term : byte
    {
        [Display(Name = "I rüb")]
        First = 1,

        [Display(Name = "II rüb")]
        Second = 2,

        [Display(Name = "III rüb")]
        Third = 3,

        [Display(Name = "IV rüb")]
        Fourth = 4,
    }
}