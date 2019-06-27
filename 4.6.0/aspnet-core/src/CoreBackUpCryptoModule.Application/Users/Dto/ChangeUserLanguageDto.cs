using System.ComponentModel.DataAnnotations;

namespace CoreBackUpCryptoModule.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}