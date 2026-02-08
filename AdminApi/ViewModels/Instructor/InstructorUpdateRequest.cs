namespace AdminApi.ViewModels.Instructor
{
    public class InstructorUpdateRequest
    {
        public int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? ParentName { get; set; }
        public string? LastName { get; set; }
        public string? DateOfBirth { get; set; }
        public string? PersonalNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? ScheduleType { get; set; }
        public string? LicenseNumber { get; set; }
        public string? LicenseValidityDate { get; set; }
        public string? LicensePhotoPath { get; set; }
    }
}
