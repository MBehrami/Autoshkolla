namespace AdminApi.ViewModels.Candidate
{
    public class UpdateDrivingSessionRequest
    {
        public string? Status { get; set; }         // Kaloi / Deshtoi / Anuloi
        public string? Examiner { get; set; }       // Free text
        public decimal? PaymentAmount { get; set; }  // Updated payment
        public string? PaymentDate { get; set; }     // Updated payment date dd.MM.yyyy
    }
}
