using System.ComponentModel.DataAnnotations;

namespace Ftpb_ValidateForm
{

    public class ValidateAttachment
    {
        /// <summary>
        /// Name is attachmentType for attachment and form name for form and subforms
        /// </summary>
        [Required(ErrorMessage = "Vedlegget må ha et navn")]
        public string Name { get; set; }

        /// <summary>
        /// filename with extension
        /// </summary>
        public string Filename { get; set; }
        /// <summary>
        /// Filstørrelse i byte
        /// </summary>
        public int FileSize { get; set; }
    }
}
