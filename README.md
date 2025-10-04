```markdown
# 🎓 Portal Académico - Examen Parcial

## 📋 Descripción
Sistema web interno para gestión de cursos, estudiantes y matrículas universitarias. Desarrollado como examen parcial de Programación I.

**Stack Tecnológico:** ASP.NET Core MVC (.NET 8) + Identity, EF Core, Redis, Docker

## 🚀 Despliegue en Render
**URL de producción:** [Agregar URL después del despliegue]

### Variables de entorno en producción:
```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:8080
ConnectionStrings__DefaultConnection=Data Source=PortalAcademico.db
Redis__ConnectionString=rediss://...
```

## 🛠️ Configuración Local

### Prerrequisitos
- .NET 8 SDK
- SQLite (incluido en .NET)
- Redis (opcional para desarrollo)

### Pasos de instalación
1. **Clonar el repositorio**
```bash
git clone https://github.com/German-RT/PortalAcademico.git
cd PortalAcademico
```

2. **Restaurar paquetes NuGet**
```bash
dotnet restore
```

3. **Ejecutar migraciones de la base de datos**
```bash
dotnet ef database update
```

4. **Ejecutar la aplicación**
```bash
dotnet run
```

### Ejecutar con Docker
```bash
# Construir imagen
docker build -t portalacademico .

# Ejecutar contenedor
docker run -p 8080:8080 portalacademico
```

## 👤 Usuarios de Prueba

### Coordinador (Administrador)
- **Email:** coordinador@universidad.edu
- **Password:** Password123!
- **Funciones:** CRUD cursos, gestión de matrículas

### Estudiante
- **Email:** estudiante@universidad.edu  
- **Password:** Password123!
- **Funciones:** Ver catálogo, inscribirse en cursos

## 📚 Funcionalidades Implementadas

### ✅ Pregunta 1: Bootstrap + Modelo de datos
- Modelos Curso y Matricula con validaciones
- Configuración EF Core + SQLite
- Restricciones: créditos > 0, horario inicio < fin, cupo máximo
- Data seeding con 3 cursos y usuario coordinador

### ✅ Pregunta 2: Catálogo de cursos y filtros
- Vista catálogo con cursos activos
- Filtros por nombre, créditos y horario
- Vista detalle con botón "Inscribirse"
- Validaciones server-side

### ✅ Pregunta 3: Inscripción y validaciones
- Sistema de matrículas con estados (Pendiente/Confirmada/Cancelada)
- Validaciones: cupo máximo, no duplicados, horarios no solapados
- Vista "Mis Matrículas" para usuarios
- Feedback de operaciones

### ✅ Pregunta 4: Sesiones y Redis
- Session con último curso visitado
- Cache de cursos activos (60 segundos)
- Enlace "Volver al curso {Nombre}" en layout
- Integración Redis/Memory Cache

### ✅ Pregunta 5: Panel de Coordinador
- Rol Coordinador con `[Authorize(Roles="Coordinador")]`
- CRUD completo de cursos (crear, editar, desactivar)
- Gestión de matrículas (confirmar/cancelar)
- Panel exclusivo con estadísticas

## 🗂️ Estructura del Proyecto
```
PortalAcademico/
├── Controllers/         # Controladores MVC
│   ├── CursosController.cs
│   ├── MatriculasController.cs
│   └── CoordinadorController.cs
├── Models/             # Modelos de datos y ViewModels
│   ├── Curso.cs
│   ├── Matricula.cs
│   ├── CatalogoViewModel.cs
│   └── CoordinadorViewModel.cs
├── Views/              # Vistas Razor
│   ├── Cursos/
│   ├── Matriculas/
│   └── Coordinador/
├── Data/               # DbContext y configuración EF
│   ├── ApplicationDbContext.cs
│   └── SeedData.cs
├── Services/           # Servicios de aplicación
│   └── CacheService.cs
└── Program.cs          # Configuración principal
```

## 🔧 Tecnologías Utilizadas
- **Backend:** ASP.NET Core MVC 8.0, Entity Framework Core
- **Frontend:** Razor Views, Bootstrap 5, JavaScript
- **Base de datos:** SQLite (Development), SQLite (Production)
- **Cache y Session:** Redis + Memory Cache
- **Autenticación:** ASP.NET Core Identity
- **Contenedores:** Docker
- **Despliegue:** Render.com

## 📊 Control de Versiones
Cada pregunta se desarrolló en una rama independiente:

| Rama | Descripción | PR |
|------|-------------|----|
| `feature/bootstrap-dominio` | Pregunta 1: Modelos y configuración | ✅ |
| `feature/catalogo-cursos` | Pregunta 2: Catálogo y filtros | ✅ |
| `feature/matriculas` | Pregunta 3: Sistema de matrículas | ✅ |
| `feature/sesion-redis` | Pregunta 4: Session y Cache | ✅ |
| `feature/panel-coordinador` | Pregunta 5: Panel coordinador | ✅ |

## 🐛 Solución de Problemas Comunes

### Error: "Unable to resolve service for type ICacheService"
**Solución:** Verificar que en `Program.cs` esté registrado:
```csharp
builder.Services.AddScoped<ICacheService, CacheService>();
```

### Error: No aparece Panel de Coordinador
**Solución:** Iniciar sesión con `coordinador@universidad.edu`

### Error: Redis no disponible
**Solución:** La aplicación usa Memory Cache como fallback

## 👨‍💻 Autor
**Germán** - Examen Parcial Programación I

## 📄 Licencia
Este proyecto es para fines educativos como parte de la evaluación académica.
```
