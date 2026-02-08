namespace AdminApi.ViewModels.Schedule
{
    public class AddScheduleEventRequest
    {
        public string? EventDate { get; set; }      // dd.MM.yyyy
        public string? StartTime { get; set; }      // HH:mm
        public string? EndTime { get; set; }        // HH:mm
        public int InstructorUserId { get; set; }   // Admin sets; Instructor auto-assigned
        public int CandidateId { get; set; }
        public int VehicleId { get; set; }
        public string? Notes { get; set; }
    }
}
