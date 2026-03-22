namespace AdminApi.ViewModels.Candidate
{
    public class AddDrivingSessionRequest
    {
        public int? CandidateId { get; set; }
        /// <summary>First + Last name typed manually when the candidate is not in the system.</summary>
        public string? ManualCandidateName { get; set; }
        public int VehicleId { get; set; }
        public string? DrivingDate { get; set; }   // dd.MM.yyyy
        public string? DrivingTime { get; set; }   // HH:mm
        public decimal PaymentAmount { get; set; }
        public string? PaymentDate { get; set; }    // dd.MM.yyyy
    }
}
