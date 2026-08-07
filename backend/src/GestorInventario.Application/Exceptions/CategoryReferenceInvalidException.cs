namespace GestorInventario.Application.Exceptions;

public sealed class CategoryReferenceInvalidException : ApplicationExceptionBase
{
    public CategoryReferenceInvalidException(int categoriaId)
        : base(
            "category_reference_invalid",
            $"No existe una categoria con id {categoriaId}.",
            new Dictionary<string, string[]>
            {
                ["categoriaId"] = ["La categoria indicada no existe."]
            })
    {
    }
}
