namespace Application.DTOs
{
    public class OcrResultDto
    {
        public bool? Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? RawText { get; set; }
        public PrescriptionDto? Prescription {  get; set; }

    }
}
  