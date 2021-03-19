using System.ComponentModel.DataAnnotations;

namespace Ftpb_ValidateForm
{
    public class ValidateFormv2
    {
        /// <summary>
        /// DataFormatId of the Form. https://www.altinn.no/api/help
        /// </summary>
        [Required]
        public string DataFormatId { get; set; }
        /// <summary>
        /// DataFormatVersion of the Form. https://www.altinn.no/api/help
        /// </summary>
        [Required]
        public string DataFormatVersion { get; set; }
        /// <summary>
        /// The Form data. https://www.altinn.no/api/help
        /// </summary>
        [Required]
        public string FormData { get; set; }

        /// <summary>
        /// List of all attachment types and forms/subforms name used to validate required documentation. V2 enables more detailed validation of altinn rules for each attachement. Size, file extension and count of attachmenttypes.
        /// </summary>
        [Required]
        public ValidateAttachment[] AttachmentTypesAndForms { get; set; }
    }
}
