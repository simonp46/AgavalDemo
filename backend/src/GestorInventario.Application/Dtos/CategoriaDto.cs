namespace GestorInventario.Application.Dtos;

public sealed record CategoriaDto(
    int Id,
    string Nombre,
    bool Activo);
