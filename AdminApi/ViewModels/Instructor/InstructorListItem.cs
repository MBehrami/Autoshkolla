namespace AdminApi.ViewModels.Instructor
{
    /// <summary>List view: no Email, no DateOfBirth. FullName = FirstName + LastName only (no ParentName).</summary>
    public class InstructorListItem
    {
        public int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        /// <summary>Display: FirstName + " " + LastName (no ParentName in list).</summary>
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ScheduleType { get; set; }
        public string? LicenseNumber { get; set; }
        public string? LicenseValidityDate { get; set; }
        public bool IsActive { get; set; }
    }
}
