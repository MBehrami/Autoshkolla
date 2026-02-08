namespace AdminApi.ViewModels.Candidate
{
    public class UpdateDrivingSessionRequest
    {
        public string? Status { get; set; }     // Kaloi / Deshtoi / Anuloi
        public string? Examiner { get; set; }   // Free text
    }
}
