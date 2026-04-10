namespace AdminApi.ViewModels.Vehicle
{
    public class AddVehicleServiceRequest
    {
        public int VehicleId { get; set; }
        public string? ServiceCompany { get; set; }
        public string? ServiceDate { get; set; }    // dd.MM.yyyy
        public decimal Cost { get; set; }
        public string? Description { get; set; }
        public int StaffUserId { get; set; }
    }
}
