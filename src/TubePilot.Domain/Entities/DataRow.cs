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
        // True when ConfirmedImagePath actually points at a real stock-footage video clip
        // (Pexels/Pixabay) rather than a still photo — the render pipeline needs to know
        // whether to trim+overlay a video or Ken-Burns-animate a static image.
        public bool IsVideoClip { get; set; }
    }
}
