using GestorInventario.Application.Common.Messaging;

namespace GestorInventario.Application.Usuarios.Queries.SugerirNombreUsuario;

public sealed record SugerirNombreUsuarioQuery(
    string Nombre,
    string Apellidos) : IQuery<string>;
