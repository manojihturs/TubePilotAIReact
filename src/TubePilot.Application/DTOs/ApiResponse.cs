namespace TubePilot.Application.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
        public T? Data { get; set; }
        public PaginationMeta? Meta { get; set; }
    }
}
