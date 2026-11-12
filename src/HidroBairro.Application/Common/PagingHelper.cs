namespace HidroBairro.Application.Common;

public static class PagingHelper
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    public static (int Page, int PageSize) Normalize(int? page, int? pageSize)
    {
        var p = page is null or < 1 ? DefaultPage : page.Value;
        var size = pageSize is null or < 1 or > MaxPageSize ? DefaultPageSize : pageSize.Value;
        return (p, size);
    }
}
