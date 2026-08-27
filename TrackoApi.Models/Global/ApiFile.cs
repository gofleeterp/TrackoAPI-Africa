using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.Global
{
    [Table("tApiFile")]
    public class ApiFile:AuditableEntity
    {
        public ApiFile()
        {
            //Stream = null;
        }

        public long RecordId { get; set; }
        public long NatureId { get; set; }
        /// <summary>
        /// Gets or sets the FK_ nature.
        /// e.g. CN/Driver/Due/Employee/Agreement
        /// </summary>
        /// <value>The FK_ nature.</value>
        [ForeignKey("NatureId")]
        public virtual FileUploadNature fk_Nature { get; set; }

        public long RelatedId { get; set; }
        [ForeignKey("RelatedId")]
        public virtual ConstantValue fk_Related { get; set; }


        /// <summary>
        /// Gets or sets the name of the image.
        /// </summary>
        /// <value>The image name.</value>
        [MaxLength(500)]
        public string Name{get;set;}
        [MaxLength(1000)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the image media type.
        /// </summary>
        /// <value>The MIME type associated with the image.</value>
        public string MediaType{get;set;}
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets the image stream.
        /// </summary>
        /// <value>The <see cref="Stream">stream</see> containing the image content.</value>
        //public Stream Stream{get;set;}
        
        [MaxLength(2000)]
        public string ServerFilePath { get; set; }
        [MaxLength(2000,ErrorMessage = "Url Length can't be more than 2000 characters")]
        public string UrlPath { get; set; }
        [MaxLength(2000)]
        public string UserLocalPath { get; set; }
        [MaxLength(200)]
        public string UserMachine { get; set; }
        public bool IsUploadCompleted { get; set; } = false;
        public string ImageUrl { get; set; }
    }
}
