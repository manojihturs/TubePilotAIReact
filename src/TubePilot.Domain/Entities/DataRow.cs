namespace TubePilot.Domain.Entities
{
    public enum ImageStatus { NotRequired, Pending, CandidatesFetched, Confirmed }

    // Hangs off ProjectOutputs (not Projects directly) since a single project can have
    // more than one Table-type output.
    public class DataRow
    {
        public Guid Id { get; set; }
        public Guid ProjectOutputId { get; set; }
        public int RowIndex { get; set; }
        public string DataJson { get; set; } = null!;
        public ImageStatus ImageStatus { get; set; }
        public string? ConfirmedImagePath { get; set; }
    }
}
