namespace System.Challenge.FIAP.Response;

public class PagedResponse<T>
{
    public List<T> Data { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; } = true;
    public List<string> Errors { get; set; } = new();

    public static PagedResponse<T> SuccessResponse(
        List<T> data, 
        int pageNumber, 
        int pageSize, 
        int totalRecords, 
        string message = "Dados recuperados com sucesso")
    {
        return new PagedResponse<T>
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            Message = message,
            Success = true
        };
    }

    public static PagedResponse<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new PagedResponse<T>
        {
            Message = message,
            Success = false,
            Errors = errors ?? new List<string>()
        };
    }
}
