namespace ABCRetail2.Models
{

    public class ContractEntity : TableBase
    {
        public string FileName { get; set; } = "";

        public string FileUrl { get; set; } = "";

        // Timestamp 
        public DateTime UploadedOn { get; set; }
    }
}