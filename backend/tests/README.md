# Tests

Suites automatizadas activas:

- `GestorInventario.Domain.Tests`: invariantes y operaciones de `Producto`.
- `GestorInventario.Application.Tests`: validadores, handlers y errores
  contractuales.
- `GestorInventario.Api.Tests`: endpoints OpenAPI y respuestas
  `ProblemDetails`.

La suite `GestorInventario.Architecture.Tests` permanece como mejora futura.
Las reglas de dependencias se verifican adicionalmente mediante inspeccion de
referencias entre proyectos y busquedas estaticas.
