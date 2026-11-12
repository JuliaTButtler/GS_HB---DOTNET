using HidroBairro.API.Extensions;
using HidroBairro.Application.Services;

namespace HidroBairro.API.Endpoints;

public static class CategoriaEndpoints
{
    public static RouteGroupBuilder MapCategoriaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categorias")
            .WithTags("Categorias");

        group.MapGet("/", async (
            ICategoriaService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.ListAtivasAsync(cancellationToken);
            return result.ToHttpResult();
        })
        .WithName("ListCategorias")
        .WithSummary("Lista categorias ativas de solicitação");

        return group;
    }
}
