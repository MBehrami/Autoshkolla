namespace AdminApi.ViewModels.Vehicle
{
    public class AddVehicleRequest
    {
        public string? PlateNumber { get; set; }
        public string? ChassisNumber { get; set; }
        public string? Color { get; set; }
        public string? Type { get; set; }
        public string? Brand { get; set; }
        public string? RegistrationDate { get; set; }
        public string? ExpiryDate { get; set; }
        public string? CertificateNumber { get; set; }
    }
}
