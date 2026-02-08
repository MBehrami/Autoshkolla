using System;

namespace AdminApi.ViewModels.Candidate
{
    public class CandidateInfo
    {
        public int CandidateId { get; set; }
        public string? SerialNumber { get; set; }
        public string? FirstName { get; set; }
        public string? ParentName { get; set; }
        public string? LastName { get; set; }
        public string? DateOfBirth { get; set; }
        public string? PersonalNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PlaceOfBirth { get; set; }
        public string? Address { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? InstructorId { get; set; }
        public string? InstructorName { get; set; }
        public string? VehicleType { get; set; }
        public string? PaymentMethod { get; set; }
        public int? PracticalHours { get; set; }
        public int TotalServiceAmount { get; set; }
        public DateTime DateAdded { get; set; }
        public int? Year { get; set; } // For filtering by year

        // Practical-lesson counts (populated after the main query)
        public int PracticalLessonCount { get; set; }
        public int CompletedHours { get; set; }
        public int RemainingHours { get; set; }
    }
}
