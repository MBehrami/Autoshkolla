using System;

namespace AdminApi.ViewModels.Candidate
{
    public class AdditionalLessonInfo
    {
        public int AdditionalLessonId { get; set; }
        public int? LinkedCandidateId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PersonalNumber { get; set; }
        public string? ContactNumber { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? InstructorId { get; set; }
        public string? InstructorName { get; set; }
        public string? VehicleType { get; set; }
        public string? PaymentMethod { get; set; }
        public int? PracticalHours { get; set; }
        public decimal ServicePayment { get; set; }
        public int TotalPaidAmount { get; set; }
        public string? AdditionalNotes { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
