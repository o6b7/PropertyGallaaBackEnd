using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PropertyGalla.Models
{
    public class PropertyImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string PropertyId { get; set; }

        [Required]
        public byte[] ImageData { get; set; }  // 🟢 Store image binary

        [Required]
        public string ContentType { get; set; }  // 🟢 Store image MIME type (e.g., image/jpeg)

        [JsonIgnore]
        [ForeignKey("PropertyId")]
        public Property Property { get; set; }
    }

}
