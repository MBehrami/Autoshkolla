namespace AdminApi.ViewModels.Schedule
{
    public class UpdateScheduleEventRequest
    {
        public string? EventDate { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public int InstructorUserId { get; set; }
        public int CandidateId { get; set; }
        public int VehicleId { get; set; }
        public string? Notes { get; set; }
    }
}
