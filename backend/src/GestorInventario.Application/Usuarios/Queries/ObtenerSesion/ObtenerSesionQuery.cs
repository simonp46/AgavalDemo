using GestorInventario.Application.Common.Messaging;
using GestorInventario.Application.Dtos;

namespace GestorInventario.Application.Usuarios.Queries.ObtenerSesion;

public sealed record ObtenerSesionQuery(int UsuarioId) : IQuery<UsuarioDto>;
