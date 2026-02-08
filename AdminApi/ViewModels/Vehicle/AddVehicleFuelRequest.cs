namespace AdminApi.ViewModels.Vehicle
{
    public class AddVehicleFuelRequest
    {
        public string? FillDate { get; set; }      // dd.MM.yyyy
        public int VehicleId { get; set; }
        public decimal FuelAmount { get; set; }
        public string? FuelType { get; set; }       // Diesel or Benzin
        public int StaffUserId { get; set; }
    }
}
