using System.Collections.Generic;

namespace AdminApi.ViewModels.Candidate
{
    public class AdditionalLessonRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PersonalNumber { get; set; }
        public string? ContactNumber { get; set; }
        public int CategoryId { get; set; }
        public int? InstructorId { get; set; }
        public string? VehicleType { get; set; }
        public string? PaymentMethod { get; set; }
        public int? PracticalHours { get; set; }
        public decimal ServicePayment { get; set; }
        public string? AdditionalNotes { get; set; }
        public List<InstallmentInfo> Installments { get; set; } = new List<InstallmentInfo>();
    }
}
