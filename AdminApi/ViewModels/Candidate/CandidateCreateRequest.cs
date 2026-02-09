using System.Collections.Generic;

namespace AdminApi.ViewModels.Candidate
{
    public class CandidateCreateRequest
    {
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
        public int? InstructorId { get; set; }
        public string? VehicleType { get; set; }
        public string? PaymentMethod { get; set; }
        public int? PracticalHours { get; set; }
        public int TotalServiceAmount { get; set; }
        public int? DocWithdrawalAmount { get; set; }
        public string? DocWithdrawalDate { get; set; }
        public int? DrivingPaymentAmount { get; set; }
        public string? DrivingPaymentDate { get; set; }
        public List<InstallmentInfo> Installments { get; set; } = new List<InstallmentInfo>();
    }

    public class InstallmentInfo
    {
        public int InstallmentNumber { get; set; }
        public int Amount { get; set; }
        public string? InstallmentDate { get; set; }
    }
}
