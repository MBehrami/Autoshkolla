namespace AdminApi.ViewModels.Report
{
    public class AddDailyReportEntryRequest
    {
        public string? EntryDate { get; set; }    // dd.MM.yyyy
        public string? EntryType { get; set; }    // "Income" or "Expense"
        public string? FullName { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }

    public class ReverseDailyReportEntryRequest
    {
        public int OriginalEntryId { get; set; }
        public string? Reason { get; set; }
    }
}
