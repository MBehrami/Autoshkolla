namespace AdminApi.ViewModels.Candidate
{
    public class AddPracticalLessonRequest
    {
        public int CandidateId { get; set; }
        public string? LessonDate { get; set; }  // dd.MM.yyyy
        public string? Time { get; set; }
        public string? Vehicle { get; set; }
    }
}
