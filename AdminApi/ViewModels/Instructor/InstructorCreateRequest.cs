namespace AdminApi.ViewModels.Instructor
{
    public class InstructorCreateRequest
    {
        public string? FirstName { get; set; }
        public string? ParentName { get; set; }
        public string? LastName { get; set; }
        public string? DateOfBirth { get; set; }  // dd.MM.yyyy
        public string? PersonalNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? InitialPassword { get; set; }
        public string? ScheduleType { get; set; }  // Full-time / Part-time
        public string? LicenseNumber { get; set; }
        public string? LicenseValidityDate { get; set; }  // dd.MM.yyyy
        public string? LicensePhotoPath { get; set; }
    }
}
