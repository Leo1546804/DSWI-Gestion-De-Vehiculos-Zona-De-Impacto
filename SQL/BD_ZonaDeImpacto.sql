DROP DATABASE IF EXISTS ZonaDeImpacto;
GO
CREATE DATABASE ZonaDeImpacto;
GO
USE ZonaDeImpacto;
GO

/*=== TABLA USUARIO ===*/
CREATE TABLE Usuarios (
    idUsuario INT IDENTITY PRIMARY KEY,
    nombreCompleto NVARCHAR(100) NOT NULL,
    usuario NVARCHAR(50) NOT NULL UNIQUE,
    password NVARCHAR(255) NOT NULL,
    rol NVARCHAR(20) NOT NULL CHECK (rol IN ('Admin','Trabajador')),
    estado BIT NOT NULL DEFAULT 1
);

/*=== TABLA VEHICULOS ===*/
CREATE TABLE Vehiculos (
    idVehiculo INT IDENTITY PRIMARY KEY,
    placa NVARCHAR(15) NOT NULL UNIQUE,
    marca NVARCHAR(50) NOT NULL,
    modelo NVARCHAR(50) NOT NULL,
	tipo NVARCHAR(50) NULL,
    anio INT NULL,
    kilometraje INT NULL,
    estado NVARCHAR(20) NULL,  -- Ejemplo: 'Operativo', 'En Taller'
	estadoLogico BIT NOT NULL DEFAULT 1 -- 1 = Activo, 0 = Eliminado lógicamente
);

/*=== TABLA TIPOGASTO ===*/
CREATE TABLE TipoGasto (
    idTipoGasto INT IDENTITY PRIMARY KEY,
    nombre NVARCHAR(50) NOT NULL UNIQUE,   -- gasolina, llantas, aceite, etc.
	descripcion NVARCHAR(200) null,
	estadoLogico BIT NOT NULL DEFAULT 1
);

/*=== TABLA MANTENIMIENTOS ===*/
CREATE TABLE Mantenimientos (
    idMantenimiento INT IDENTITY PRIMARY KEY,
    codigoMantenimiento AS ('M' + RIGHT('0000' + CAST(idMantenimiento AS VARCHAR(5)), 5)) PERSISTED,
    idVehiculo INT NOT NULL,
    tipo NVARCHAR(20) NOT NULL CHECK (tipo IN ('Preventivo','Correctivo')),
    descripcion NVARCHAR(200),
    fecha DATE NOT NULL,
    idUsuario INT NOT NULL,  -- QUIÉN HIZO EL MANTENIMIENTO (DUEÑO DEL MANTENIMIENTO)
	estadoLogico BIT NOT NULL DEFAULT 1,
    
    FOREIGN KEY (idVehiculo) REFERENCES Vehiculos(idVehiculo),
    FOREIGN KEY (idUsuario) REFERENCES Usuarios(idUsuario)
);

/*=== TABLA GASTOS ===*/
CREATE TABLE Gastos (
    idGasto INT IDENTITY PRIMARY KEY,
    idMantenimiento INT NOT NULL,  -- RELACIÓN CON MANTENIMIENTO (y su usuario)
    idTipoGasto INT NOT NULL,
    monto DECIMAL(10,2) NOT NULL,
    fecha DATE NOT NULL,
    descripcion NVARCHAR(200),
    -- NO idUsuario aquí, se obtiene de Mantenimientos
    
    FOREIGN KEY (idMantenimiento) REFERENCES Mantenimientos(idMantenimiento),
    FOREIGN KEY (idTipoGasto) REFERENCES TipoGasto(idTipoGasto)
);
GO
/*==============================================*/
/*========== PROCEDIMIENTOS ALMACENADOS ========*/
/*==============================================*/

/*=== LOGIN ===*/
CREATE OR ALTER PROCEDURE dbo.usp_LoginUsuario
@usuario NVARCHAR(50),
@password NVARCHAR(255)
AS
BEGIN
	SELECT idUsuario, nombreCompleto, usuario, rol, estado
	FROM Usuarios
	WHERE usuario =@usuario 
	AND password = @password
	AND estado = 1
END 
GO

/*=================================*/
/*======== PROC DE USUARIO ========*/
/*=================================*/

--INSERTAR
CREATE OR ALTER PROCEDURE dbo.usp_InsertarUsuario
@nombreCompleto NVARCHAR(100),
    @usuario NVARCHAR(50),
    @password NVARCHAR(255),
    @rol NVARCHAR(20)
AS
BEGIN
	INSERT INTO Usuarios (nombreCompleto, usuario, password, rol)
    VALUES (@nombreCompleto, @usuario, @password, @rol);
END
GO

--LISTAR
CREATE OR ALTER PROCEDURE dbo.usp_ListarUsuarios
    @filtroNombre NVARCHAR(50) = NULL,
    @filtroRol NVARCHAR(20) = NULL,
    @filtroEstado INT = NULL  -- 1=Activos, 0=Inactivos, NULL=Todos
AS
BEGIN
    SELECT idUsuario, nombreCompleto, usuario, rol, estado 
    FROM Usuarios
    WHERE 
        -- Filtro por nombre (búsqueda parcial)
        (@filtroNombre IS NULL OR usuario LIKE @filtroNombre + '%')
        -- Filtro por rol
        AND (@filtroRol IS NULL OR rol = @filtroRol)
        -- Filtro por estado
        AND (
            @filtroEstado IS NULL 
            OR estado = CASE 
                WHEN @filtroEstado = 1 THEN 1 
                WHEN @filtroEstado = 0 THEN 0 
            END
        )
    ORDER BY nombreCompleto;
END
GO

--BUSCAR POR ID
CREATE OR ALTER PROCEDURE dbo.usp_ObtenerUsuario
    @idUsuario INT
AS
BEGIN
    SELECT * FROM Usuarios WHERE idUsuario = @idUsuario;
END
GO

--EDITAR O ACTUALIZAR
CREATE OR ALTER PROCEDURE dbo.usp_EditarUsuario
    @idUsuario INT,
    @nombreCompleto NVARCHAR(100),
    @usuario NVARCHAR(50),
    @password NVARCHAR(255),
    @rol NVARCHAR(20),
    @estado BIT
AS
BEGIN
    UPDATE Usuarios
    SET nombreCompleto = @nombreCompleto,
        usuario = @usuario,
        password = @password,
        rol = @rol,
        estado = @estado
    WHERE idUsuario = @idUsuario;
END
GO

--ELIMINAR/ DESABILITAR AL USUARIO
CREATE OR ALTER PROCEDURE dbo.usp_EliminarUsuario
    @idUsuario INT
AS
BEGIN
    -- hacemos UPDATE del estado
    UPDATE Usuarios 
    SET estado = 0 
    WHERE idUsuario = @idUsuario;
END
GO
--VOLVEMOS A HABILITAR AL USUARIO
CREATE OR ALTER PROCEDURE dbo.usp_HabilitarUsuario
    @idUsuario INT
AS
BEGIN
    UPDATE Usuarios 
    SET estado = 1 
    WHERE idUsuario = @idUsuario;
END
GO

/*===================================*/
/*======== PROC DE VEHICULOS ========*/
/*===================================*/

--INSERTAMOS
CREATE OR ALTER PROCEDURE dbo.usp_InsertarVehiculo
	@placa nvarchar(15),
	@marca nvarchar(50),
	@modelo nvarchar(50),
    @tipo nvarchar(50),
	@anio int,
	@kilometraje int,
	@estado nvarchar(20)
AS
BEGIN
	INSERT INTO Vehiculos(placa, marca, modelo, tipo, anio, kilometraje, estado)
	VALUES(@placa, @marca, @modelo, @tipo, @anio, @kilometraje, @estado);
END
GO

--LISTAMOS
CREATE OR ALTER PROCEDURE dbo.usp_ListarVehiculos
    @filtroPlaca NVARCHAR(15) = NULL,
    @filtroMarca NVARCHAR(50) = NULL,
    @filtroAnio INT = NULL,
    @filtroEstado NVARCHAR(20) = NULL,
    @soloActivos BIT = NULL
AS
BEGIN
    SELECT idVehiculo, placa, marca, modelo, tipo, anio, kilometraje, estado, estadoLogico
    FROM Vehiculos
    WHERE 
        (@filtroPlaca IS NULL OR placa LIKE @filtroPlaca + '%')
        AND (@filtroMarca IS NULL OR marca = @filtroMarca)
        AND (@filtroAnio IS NULL OR anio = @filtroAnio)
        AND (@filtroEstado IS NULL OR estado = @filtroEstado)
        AND (
            @soloActivos IS NULL 
            OR estadoLogico = CASE 
                WHEN @soloActivos = 1 THEN 1 
                WHEN @soloActivos = 0 THEN 0 
            END
        )
    ORDER BY placa;
END
GO

--EDITAMOS O ACTUALIZAMOS
CREATE OR ALTER PROCEDURE dbo.usp_EditarVehiculo
	@idVehiculo int,
	@placa nvarchar(15),
	@marca nvarchar(50),
	@modelo nvarchar(50),
    @tipo nvarchar(50),
	@anio int,
	@kilometraje int,
	@estado nvarchar(20)
AS
BEGIN
	UPDATE Vehiculos
	SET placa = @placa,
		marca = @marca,
		modelo = @modelo,
        tipo = @tipo,
		anio = @anio,
		kilometraje = @kilometraje,
		estado = @estado
	WHERE idVehiculo = @idVehiculo;
END
GO

--OBTENEMOS POR ID 
CREATE OR ALTER PROCEDURE dbo.usp_ObtenerVehiculo
	@idVehiculo int
AS
BEGIN 
	SELECT idVehiculo, placa, marca, modelo, tipo, anio, kilometraje, estado, estadoLogico
	FROM Vehiculos 
	WHERE idVehiculo = @idVehiculo;
END
GO

--ELIMINAMOS 
CREATE OR ALTER PROCEDURE dbo.usp_EliminarVehiculo
	@idVehiculo INT
AS
BEGIN
    -- hacemos UPDATE
    UPDATE Vehiculos 
    SET estadoLogico = 0 
    WHERE idVehiculo = @idVehiculo;
END
GO

-- HABILITAMOS VEHÍCULO
CREATE OR ALTER PROCEDURE dbo.usp_HabilitarVehiculo
    @idVehiculo INT
AS
BEGIN
    UPDATE Vehiculos 
    SET estadoLogico = 1 
    WHERE idVehiculo = @idVehiculo;
END
GO

/*========================================*/
/*======== PROC DE MANTENIMIENTOS ========*/
/*========================================*/

--INSERTAMOS
CREATE OR ALTER PROCEDURE dbo.usp_InsertarMantenimiento
	@idVehiculo INT,
	@tipo NVARCHAR(20),
	@descripcion NVARCHAR(200) = NULL,
	@fecha DATE,
	@idUsuario INT
AS
BEGIN
	BEGIN TRY
		INSERT INTO Mantenimientos (idVehiculo, tipo, descripcion, fecha, idUsuario)
		VALUES (@idVehiculo, @tipo, @descripcion, @fecha, @idUsuario);
		
		-- Retornar el ID insertado (opcional)
		SELECT SCOPE_IDENTITY() AS idMantenimiento;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH
END
GO

--LISTAMOS
CREATE OR ALTER PROCEDURE dbo.usp_ListarMantenimientos
	@filtroCodigo INT = NULL,       --Buscar por numero(1=M00001)
	@filtroVehiculo INT = NULL,		--Filtrar por vehiculo
	@filtroTipo NVARCHAR(20) = NULL,--Preventivo o correctivo
	@filtroFechaDesde DATE = NULL,	--rango de fechas
	@filtroFechaHasta DATE = NULL,
	@filtroEstado BIT =1,			--1=activos, 0=anulados, NULL=todos
	@idUsuario INT = NULL			--para filtro por usuario(trabajador)
AS
BEGIN
    SELECT 
        m.idMantenimiento,
        m.codigoMantenimiento,
        v.placa,
        v.marca,
        v.modelo,
        m.tipo,
        m.descripcion,
        m.fecha,
        u.nombreCompleto as usuarioNombre,
        u.idUsuario as idUsuario,
        m.estadoLogico  -- NUEVO
    FROM Mantenimientos m 
    INNER JOIN Vehiculos v ON m.idVehiculo = v.idVehiculo
    INNER JOIN Usuarios u ON m.idUsuario = u.idUsuario
    WHERE (@filtroCodigo IS NULL OR m.idMantenimiento = @filtroCodigo)
      AND (@filtroVehiculo IS NULL OR m.idVehiculo = @filtroVehiculo)
      AND (@filtroTipo IS NULL OR m.tipo = @filtroTipo)
      AND (@filtroFechaDesde IS NULL OR m.fecha >= @filtroFechaDesde)
      AND (@filtroFechaHasta IS NULL OR m.fecha <= @filtroFechaHasta)
      AND (@filtroEstado IS NULL OR m.estadoLogico = @filtroEstado)  -- NUEVO
      AND (@idUsuario IS NULL OR m.idUsuario = @idUsuario)
    ORDER BY m.fecha DESC, m.idMantenimiento DESC;
END
GO

--LISTAMOS CON PAGINACIÓN (NUEVO PROCEDIMIENTO)
CREATE OR ALTER PROCEDURE dbo.usp_ObtenerMantenimientosPaginado
    @Pagina INT = 1,
    @TamanoPagina INT = 6,
    @TotalRegistros INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Contar total de registros
    SELECT @TotalRegistros = COUNT(*)
    FROM Mantenimientos m 
    INNER JOIN Vehiculos v ON m.idVehiculo = v.idVehiculo
    INNER JOIN Usuarios u ON m.idUsuario = u.idUsuario;
    
    -- Obtener registros paginados
    SELECT 
        m.idMantenimiento,
        m.codigoMantenimiento,
        v.placa,
        v.marca,
        v.modelo,
        m.tipo,
        m.descripcion,
        m.fecha,
        u.nombreCompleto as usuarioNombre,
        u.idUsuario as idUsuario,
        m.estadoLogico
    FROM Mantenimientos m 
    INNER JOIN Vehiculos v ON m.idVehiculo = v.idVehiculo
    INNER JOIN Usuarios u ON m.idUsuario = u.idUsuario
    ORDER BY m.estadoLogico DESC, m.fecha DESC, m.idMantenimiento DESC
    OFFSET (@Pagina - 1) * @TamanoPagina ROWS
    FETCH NEXT @TamanoPagina ROWS ONLY;
END
GO

-- LISTAR MANTENIMIENTOS CON PAGINACIÓN Y FILTROS (Nuevo Procedimiento)
CREATE OR ALTER PROCEDURE dbo.usp_ListarMantenimientosPaginado
    @Pagina INT = 1,
    @TamanoPagina INT = 6,
    @filtroCodigo INT = NULL,
    @filtroVehiculo INT = NULL,
    @filtroTipo NVARCHAR(20) = NULL,
    @filtroFechaDesde DATE = NULL,
    @filtroFechaHasta DATE = NULL,
    @filtroEstado BIT = NULL,
    @idUsuario INT = NULL,
    @TotalRegistros INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Contar total de registros con filtros
    SELECT @TotalRegistros = COUNT(*)
    FROM Mantenimientos m 
    INNER JOIN Vehiculos v ON m.idVehiculo = v.idVehiculo
    INNER JOIN Usuarios u ON m.idUsuario = u.idUsuario
    WHERE (@filtroCodigo IS NULL OR m.idMantenimiento = @filtroCodigo)
      AND (@filtroVehiculo IS NULL OR m.idVehiculo = @filtroVehiculo)
      AND (@filtroTipo IS NULL OR m.tipo = @filtroTipo)
      AND (@filtroFechaDesde IS NULL OR m.fecha >= @filtroFechaDesde)
      AND (@filtroFechaHasta IS NULL OR m.fecha <= @filtroFechaHasta)
      AND (@filtroEstado IS NULL OR m.estadoLogico = @filtroEstado)
      AND (@idUsuario IS NULL OR m.idUsuario = @idUsuario);
    
    -- Obtener registros paginados
    SELECT 
        m.idMantenimiento,
        m.codigoMantenimiento,
        v.placa,
        v.marca,
        v.modelo,
        m.tipo,
        m.descripcion,
        m.fecha,
        u.nombreCompleto as usuarioNombre,
        u.idUsuario as idUsuario,
        m.estadoLogico
    FROM Mantenimientos m 
    INNER JOIN Vehiculos v ON m.idVehiculo = v.idVehiculo
    INNER JOIN Usuarios u ON m.idUsuario = u.idUsuario
    WHERE (@filtroCodigo IS NULL OR m.idMantenimiento = @filtroCodigo)
      AND (@filtroVehiculo IS NULL OR m.idVehiculo = @filtroVehiculo)
      AND (@filtroTipo IS NULL OR m.tipo = @filtroTipo)
      AND (@filtroFechaDesde IS NULL OR m.fecha >= @filtroFechaDesde)
      AND (@filtroFechaHasta IS NULL OR m.fecha <= @filtroFechaHasta)
      AND (@filtroEstado IS NULL OR m.estadoLogico = @filtroEstado)
      AND (@idUsuario IS NULL OR m.idUsuario = @idUsuario)
    ORDER BY m.estadoLogico DESC, m.fecha DESC, m.idMantenimiento DESC
    OFFSET (@Pagina - 1) * @TamanoPagina ROWS
    FETCH NEXT @TamanoPagina ROWS ONLY;
END
GO
-- AGREGA LISTAR COSTOS PAGINADOS (Nuevo Procedimiento
CREATE OR ALTER PROCEDURE dbo.usp_ListarGastosPaginado
    @Pagina INT = 1,
    @TamanoPagina INT = 6,
    @filtroMantenimientoCodigo NVARCHAR(20) = NULL,
    @filtroTipoGasto INT = NULL,
    @filtroUsuario INT = NULL, 
    @filtroFechaDesde DATE = NULL,
    @filtroFechaHasta DATE = NULL,
    @idUsuarioFiltro INT = NULL,
    @TotalRegistros INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Contar total de registros con filtros
    SELECT @TotalRegistros = COUNT(*)
    FROM Gastos g
    INNER JOIN Mantenimientos m ON g.idMantenimiento = m.idMantenimiento
    INNER JOIN Usuarios u ON m.idUsuario = u.idUsuario
    INNER JOIN TipoGasto tg ON g.idTipoGasto = tg.idTipoGasto
    WHERE (@filtroMantenimientoCodigo IS NULL OR m.codigoMantenimiento LIKE '%' + @filtroMantenimientoCodigo + '%')
      AND (@filtroTipoGasto IS NULL OR g.idTipoGasto = @filtroTipoGasto)
      -- CAMBIADO: Ahora comparamos por ID en lugar de nombre
      AND (@filtroUsuario IS NULL OR u.idUsuario = @filtroUsuario)
      AND (@filtroFechaDesde IS NULL OR g.fecha >= @filtroFechaDesde)
      AND (@filtroFechaHasta IS NULL OR g.fecha <= @filtroFechaHasta)
      AND (@idUsuarioFiltro IS NULL OR u.idUsuario = @idUsuarioFiltro);
    
    -- Obtener registros paginados
    SELECT 
        g.idGasto,
        m.codigoMantenimiento,
        tg.nombre AS tipoGastoNombre,
        u.nombreCompleto AS usuarioNombre,
        u.idUsuario,
        g.monto,
        g.fecha,
        g.descripcion,
        m.idMantenimiento,
        tg.idTipoGasto,
        m.estadoLogico AS mantenimientoActivo
    FROM Gastos g
    INNER JOIN Mantenimientos m ON g.idMantenimiento = m.idMantenimiento
    INNER JOIN Usuarios u ON m.idUsuario = u.idUsuario
    INNER JOIN TipoGasto tg ON g.idTipoGasto = tg.idTipoGasto
    WHERE (@filtroMantenimientoCodigo IS NULL OR m.codigoMantenimiento LIKE '%' + @filtroMantenimientoCodigo + '%')
      AND (@filtroTipoGasto IS NULL OR g.idTipoGasto = @filtroTipoGasto)
      -- CAMBIADO: Ahora comparamos por ID en lugar de nombre
      AND (@filtroUsuario IS NULL OR u.idUsuario = @filtroUsuario)
      AND (@filtroFechaDesde IS NULL OR g.fecha >= @filtroFechaDesde)
      AND (@filtroFechaHasta IS NULL OR g.fecha <= @filtroFechaHasta)
      AND (@idUsuarioFiltro IS NULL OR u.idUsuario = @idUsuarioFiltro)
    ORDER BY g.fecha DESC, g.idGasto DESC
    OFFSET (@Pagina - 1) * @TamanoPagina ROWS
    FETCH NEXT @TamanoPagina ROWS ONLY;
END
GO
-- Procedimiento para listar tipos de gasto con paginación (Nuevo Procedure)
CREATE PROCEDURE usp_ListarTiposGastoPaginado
    @filtroNombre VARCHAR(100) = NULL,
    @soloActivos BIT = NULL,
    @pagina INT = 1,
    @tamanoPagina INT = 6
AS
BEGIN
    DECLARE @offset INT = (@pagina - 1) * @tamanoPagina

    -- Primero obtenemos los datos paginados
    SELECT *
    FROM (
        SELECT 
            idTipoGasto,
            nombre,
            descripcion,
            estadoLogico,
            ROW_NUMBER() OVER (ORDER BY 
                CASE WHEN estadoLogico = 1 THEN 0 ELSE 1 END,
                nombre) AS RowNum
        FROM TipoGasto
        WHERE (@filtroNombre IS NULL OR nombre LIKE '%' + @filtroNombre + '%')
          AND (@soloActivos IS NULL OR estadoLogico = @soloActivos)
    ) AS Resultado
    WHERE RowNum > @offset AND RowNum <= @offset + @tamanoPagina
    ORDER BY RowNum

    -- Luego el total de registros
    SELECT COUNT(*)
    FROM TipoGasto
    WHERE (@filtroNombre IS NULL OR nombre LIKE '%' + @filtroNombre + '%')
      AND (@soloActivos IS NULL OR estadoLogico = @soloActivos)
END
GO
-- Procedimiento para listar gastos con paginación (nuevo procedure)
CREATE OR ALTER PROCEDURE dbo.usp_ListarVehiculosPaginado
    @Pagina INT = 1,
    @TamanoPagina INT = 6,
    @filtroPlaca NVARCHAR(15) = NULL,
    @filtroMarca NVARCHAR(50) = NULL,
    @filtroAnio INT = NULL,
    @filtroEstado NVARCHAR(20) = NULL,
    @soloActivos BIT = NULL,
    @TotalRegistros INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Crear tabla temporal con los datos filtrados
    CREATE TABLE #TempVehiculos (
        RowNumber INT IDENTITY(1,1),
        idVehiculo INT,
        placa NVARCHAR(15),
        marca NVARCHAR(50),
        modelo NVARCHAR(50),
        tipo NVARCHAR(50),
        anio INT,
        kilometraje INT,
        estado NVARCHAR(20),
        estadoLogico BIT
    );

    -- Insertar datos filtrados en tabla temporal
    INSERT INTO #TempVehiculos (
        idVehiculo, placa, marca, modelo, tipo, anio, 
        kilometraje, estado, estadoLogico
    )
    SELECT 
        v.idVehiculo,
        v.placa,
        v.marca,
        v.modelo,
        v.tipo,
        v.anio,
        v.kilometraje,
        v.estado,
        v.estadoLogico
    FROM Vehiculos v
    WHERE 
        (@filtroPlaca IS NULL OR v.placa LIKE '%' + @filtroPlaca + '%')
        AND (@filtroMarca IS NULL OR v.marca LIKE '%' + @filtroMarca + '%')
        AND (@filtroAnio IS NULL OR v.anio = @filtroAnio)
        AND (@filtroEstado IS NULL OR v.estado = @filtroEstado)
        AND (@soloActivos IS NULL OR v.estadoLogico = @soloActivos)
    ORDER BY v.estadoLogico DESC, v.placa;

    -- Obtener total de registros
    SELECT @TotalRegistros = COUNT(*) FROM #TempVehiculos;

    -- Obtener datos paginados
    SELECT 
        idVehiculo,
        placa,
        marca,
        modelo,
        tipo,
        anio,
        kilometraje,
        estado,
        estadoLogico
    FROM #TempVehiculos
    WHERE RowNumber BETWEEN ((@Pagina - 1) * @TamanoPagina + 1) 
        AND (@Pagina * @TamanoPagina)
    ORDER BY RowNumber;

    -- Limpiar tabla temporal
    DROP TABLE #TempVehiculos;
END
GO

--Estos procedures son importante implementar porque es solo utilizado en la vista
--y que al momento de exportar no haya complictos 
--ya que si no usamos los procedures sin paginar no se exportara toda la informacion 


--EDITAMOS
CREATE OR ALTER PROCEDURE dbo.usp_EditarMantenimiento
    @idMantenimiento INT,
    @idVehiculo INT,
    @tipo NVARCHAR(20),
    @descripcion NVARCHAR(200) = null,
    @fecha DATE
AS
BEGIN
    UPDATE Mantenimientos
    SET idVehiculo = @idVehiculo,
        tipo = @tipo,
        descripcion = @descripcion,
        fecha = @fecha
    WHERE idMantenimiento = @idMantenimiento;
END
GO

--ELIMINAMOS ANULAR/DESCATIVAR
CREATE OR ALTER PROCEDURE dbo.usp_EliminarMantenimiento
	@idMantenimiento INT
AS
BEGIN
	-- verificamos que no tenga gastos asociados
	IF EXISTS (SELECT 1 FROM Gastos WHERE idMantenimiento = @idMantenimiento)
	BEGIN 
		RAISERROR('No se puede anular el mantenimiento porque tiene gastos asociados.', 16, 1);
		RETURN;
		END
	--anulacion logica
	UPDATE Mantenimientos
	SET estadoLogico =0
	WHERE idMantenimiento = @idMantenimiento;
END
GO

-- HABILITAMOS NUEVAMENTE
CREATE OR ALTER PROCEDURE dbo.usp_HabilitarMantenimiento
	@idMantenimiento INT
AS
BEGIN
	UPDATE Mantenimientos
	SET estadoLogico =1
	WHERE idMantenimiento =@idMantenimiento;
END
GO

--LISTAMOS VEHICULOS ACTIVOS
CREATE OR ALTER PROCEDURE dbo.usp_ListarVehiculosActivos
AS
BEGIN
	SELECT idVehiculo, placa, marca, modelo, tipo
	FROM Vehiculos
	WHERE estadoLogico =1 --solo activos
	ORDER BY placa;
END
GO

--LISTAMOS USUARIOS ACTIVOS
CREATE OR ALTER PROCEDURE dbo.usp_ListarUsuariosActivos
AS
BEGIN
    SELECT idUsuario, nombreCompleto, rol
    FROM Usuarios
    WHERE estado = 1
    ORDER BY nombreCompleto;
END
GO

/*========================================*/
/*======== PROC DE TIPOS DE GASTO ========*/
/*========================================*/

--INSERTAMOS
CREATE OR ALTER PROCEDURE dbo.usp_InsertarTipoGasto
	@nombre NVARCHAR(50),
	@descripcion NVARCHAR(200) = NULL
AS
BEGIN 
	INSERT INTO TipoGasto (nombre,descripcion)
	VALUES (@nombre, @descripcion)
END
GO

--LISTAMOS
CREATE OR ALTER PROCEDURE dbo.usp_ListarTiposGasto
	@filtroNombre NVARCHAR(50) = NULL,
	@soloActivos BIT = NULL -- NULL?Todos, 1 =solo activos, 0=solo inactivos
AS
BEGIN
	SELECT idTipoGasto, nombre, descripcion, estadoLogico
	FROM TipoGasto
		WHERE (@filtroNombre IS NULL OR nombre LIKE @filtroNombre + '%')
		AND (@soloActivos IS NULL OR estadoLogico = @soloActivos)
	ORDER BY 
			CASE WHEN @soloActivos IS NULL THEN  estadoLogico END DESC,
			nombre;
END
GO

-- OBTENEMOS POR ID
CREATE OR ALTER PROCEDURE dbo.usp_ObtenerTipoGasto
	@idTipoGasto INT
AS 
BEGIN
	SELECT idTipoGasto, nombre, descripcion, estadoLogico
	FROM TipoGasto
	WHERE idTipoGasto = @idTipoGasto;
END
GO

--EDITAMOS
CREATE OR ALTER PROCEDURE dbo.usp_EditarTipoGasto
	@idTipoGasto INT,
	@nombre NVARCHAR(50),
	@descripcion NVARCHAR(200) = NULL
AS
BEGIN
	UPDATE TipoGasto
	SET nombre =@nombre,
		descripcion = @descripcion
	WHERE idTipoGasto = @idTipoGasto;
END
GO

--ELIMINAMOS/DESABILITAMOS
CREATE OR ALTER PROCEDURE dbo.usp_EliminarTipoGasto
    @idTipoGasto INT
AS
BEGIN
    UPDATE TipoGasto
    SET estadoLogico = 0
    WHERE idTipoGasto = @idTipoGasto;
END
GO

-- HABILITAMOS
CREATE OR ALTER PROCEDURE dbo.usp_HabilitarTipoGasto
	@idTipoGasto INT
AS
BEGIN
	UPDATE TipoGasto
	SET estadoLogico = 1
	WHERE idTipoGasto =@idTipoGasto;
END
GO

/*========================================*/
/*============ PROC DE GASTO =============*/
/*========================================*/

-- INSERTAR GASTO
CREATE OR ALTER PROCEDURE dbo.usp_InsertarGasto
    @idMantenimiento INT,
    @idTipoGasto INT,
    @monto DECIMAL(10,2),
    @fecha DATE,
    @descripcion NVARCHAR(200) = NULL
AS
BEGIN
    INSERT INTO Gastos (idMantenimiento, idTipoGasto, monto, fecha, descripcion)
    VALUES (@idMantenimiento, @idTipoGasto, @monto, @fecha, @descripcion);
END
GO

-- LISTAR GASTOS CON FILTROS
CREATE OR ALTER PROCEDURE dbo.usp_ListarGastos
    @filtroMantenimientoCodigo NVARCHAR(20) = NULL,
    @filtroTipoGasto INT = NULL,
    @filtroUsuario NVARCHAR(100) = NULL,
    @filtroFechaDesde DATE = NULL,
    @filtroFechaHasta DATE = NULL,
    @idUsuarioFiltro INT = NULL  -- Para trabajador: solo sus gastos
AS
BEGIN
    SELECT 
        g.idGasto,
        m.codigoMantenimiento,
        tg.nombre AS tipoGastoNombre,
        u.nombreCompleto AS usuarioNombre,
        u.idUsuario,
        g.monto,
        g.fecha,
        g.descripcion,
        m.idMantenimiento,
        tg.idTipoGasto,
        m.estadoLogico AS mantenimientoActivo
    FROM Gastos g
    INNER JOIN Mantenimientos m ON g.idMantenimiento = m.idMantenimiento
    INNER JOIN Usuarios u ON m.idUsuario = u.idUsuario
    INNER JOIN TipoGasto tg ON g.idTipoGasto = tg.idTipoGasto
    WHERE (@filtroMantenimientoCodigo IS NULL OR m.codigoMantenimiento LIKE '%' + @filtroMantenimientoCodigo + '%')
      AND (@filtroTipoGasto IS NULL OR g.idTipoGasto = @filtroTipoGasto)
      AND (@filtroUsuario IS NULL OR u.nombreCompleto LIKE '%' + @filtroUsuario + '%')
      AND (@filtroFechaDesde IS NULL OR g.fecha >= @filtroFechaDesde)
      AND (@filtroFechaHasta IS NULL OR g.fecha <= @filtroFechaHasta)
      AND (@idUsuarioFiltro IS NULL OR u.idUsuario = @idUsuarioFiltro)
    ORDER BY g.fecha DESC, g.idGasto DESC;
END
GO

-- OBTENER GASTO POR ID
CREATE OR ALTER PROCEDURE dbo.usp_ObtenerGasto
    @idGasto INT
AS
BEGIN
    SELECT 
        g.idGasto,
        g.idMantenimiento,
        g.idTipoGasto,
        g.monto,
        g.fecha,
        g.descripcion,
        m.codigoMantenimiento,
        tg.nombre AS tipoGastoNombre,
        u.nombreCompleto AS usuarioNombre,
        u.idUsuario,
        m.estadoLogico AS mantenimientoActivo
    FROM Gastos g
    INNER JOIN Mantenimientos m ON g.idMantenimiento = m.idMantenimiento
    INNER JOIN Usuarios u ON m.idUsuario = u.idUsuario
    INNER JOIN TipoGasto tg ON g.idTipoGasto = tg.idTipoGasto
    WHERE g.idGasto = @idGasto;
END
GO

-- EDITAR GASTO
CREATE OR ALTER PROCEDURE dbo.usp_EditarGasto
    @idGasto INT,
    @idTipoGasto INT,
    @monto DECIMAL(10,2),
    @fecha DATE,
    @descripcion NVARCHAR(200) = NULL
AS
BEGIN
    UPDATE Gastos
    SET idTipoGasto = @idTipoGasto,
        monto = @monto,
        fecha = @fecha,
        descripcion = @descripcion
    WHERE idGasto = @idGasto;
END
GO

-- ELIMINAR GASTO
CREATE OR ALTER PROCEDURE dbo.usp_EliminarGasto
    @idGasto INT
AS
BEGIN
    DELETE FROM Gastos WHERE idGasto = @idGasto;
END
GO

-- LISTAR MANTENIMIENTOS ACTIVOS PARA DROPDOWN
CREATE OR ALTER PROCEDURE dbo.usp_ListarMantenimientosActivos
AS
BEGIN
    SELECT 
        m.idMantenimiento,
        m.codigoMantenimiento,
        v.placa,
        u.nombreCompleto AS responsable
    FROM Mantenimientos m
    INNER JOIN Vehiculos v ON m.idVehiculo = v.idVehiculo
    INNER JOIN Usuarios u ON m.idUsuario = u.idUsuario
    WHERE m.estadoLogico = 1
    ORDER BY m.codigoMantenimiento DESC;
END
GO

-- LISTAR TIPOS DE GASTO ACTIVOS
CREATE OR ALTER PROCEDURE dbo.usp_ListarTiposGastoActivos
AS
BEGIN
    SELECT idTipoGasto, nombre
    FROM TipoGasto
    WHERE estadoLogico = 1
    ORDER BY nombre;
END
GO

-- LISTAR USUARIOS PARA FILTRO
CREATE OR ALTER PROCEDURE dbo.usp_ListarUsuariosParaFiltro
AS
BEGIN
    SELECT idUsuario, nombreCompleto
    FROM Usuarios
    WHERE estado = 1
    ORDER BY nombreCompleto;
END
GO




/*====================================================*/
/*========== REPORTE 1: Costos por Vehiculo ==========*/
/*====================================================*/
CREATE OR ALTER PROCEDURE dbo.usp_ReporteCostosPorVehiculo
    @fechaInicio DATE = NULL,
    @fechaFin DATE = NULL
AS
BEGIN
    SELECT 
        v.idVehiculo,
        v.placa,
        v.marca,
        v.modelo,
        v.tipo AS tipoVehiculo,
        COUNT(DISTINCT m.idMantenimiento) AS totalMantenimientos,
        SUM(g.monto) AS totalGastado,
        -- Desglose por tipo de mantenimiento
        SUM(CASE WHEN m.tipo = 'Preventivo' THEN g.monto ELSE 0 END) AS gastoPreventivo,
        SUM(CASE WHEN m.tipo = 'Correctivo' THEN g.monto ELSE 0 END) AS gastoCorrectivo
    FROM Vehiculos v
    LEFT JOIN Mantenimientos m ON v.idVehiculo = m.idVehiculo 
        AND m.estadoLogico = 1
        AND (@fechaInicio IS NULL OR m.fecha >= @fechaInicio)
        AND (@fechaFin IS NULL OR m.fecha <= @fechaFin)
    LEFT JOIN Gastos g ON m.idMantenimiento = g.idMantenimiento
    WHERE v.estadoLogico = 1
    GROUP BY v.idVehiculo, v.placa, v.marca, v.modelo, v.tipo
    ORDER BY totalGastado DESC, v.placa;
END
GO

/*===============================================================*/
/*========== REPORTE 2: Historial Completo de Vehiculo ==========*/
/*===============================================================*/
CREATE OR ALTER PROCEDURE dbo.usp_ReporteHistorialVehiculo
    @idVehiculo INT
AS
BEGIN
    -- Información del vehículo
    SELECT 
        v.placa,
        v.marca,
        v.modelo,
        v.tipo,
        v.anio,
        v.kilometraje,
        v.estado
    FROM Vehiculos v
    WHERE v.idVehiculo = @idVehiculo;

    -- Mantenimientos del vehículo con gastos desglosados
    SELECT 
        m.idMantenimiento,
        m.codigoMantenimiento,
        m.tipo AS tipoMantenimiento,
        m.descripcion AS descripcionMantenimiento,
        m.fecha AS fechaMantenimiento,
        u.nombreCompleto AS responsable,
        -- Total por mantenimiento
        (SELECT SUM(g2.monto) 
         FROM Gastos g2 
         WHERE g2.idMantenimiento = m.idMantenimiento) AS totalMantenimiento,
        -- Detalle de gastos
        STUFF((
            SELECT ', ' + tg.nombre + ': $' + CAST(g.monto AS VARCHAR(20))
            FROM Gastos g
            INNER JOIN TipoGasto tg ON g.idTipoGasto = tg.idTipoGasto
            WHERE g.idMantenimiento = m.idMantenimiento
            FOR XML PATH('')
        ), 1, 2, '') AS detalleGastos
    FROM Mantenimientos m
    INNER JOIN Usuarios u ON m.idUsuario = u.idUsuario
    WHERE m.idVehiculo = @idVehiculo
        AND m.estadoLogico = 1
    ORDER BY m.fecha DESC;
END
GO

/*============================================================*/
/*========== REPORTE 3: Dashboard/Resumen Ejecutivo ==========*/
/*============================================================*/
CREATE OR ALTER PROCEDURE dbo.usp_ReporteDashboard
    @mes INT = NULL,
    @anio INT = NULL
AS
BEGIN
    -- Si no se especifica mes/año, usar el mes actual
    IF @mes IS NULL SET @mes = MONTH(GETDATE())
    IF @anio IS NULL SET @anio = YEAR(GETDATE())

    -- 1. Total gastado este mes
    SELECT 
        SUM(g.monto) AS totalGastadoMes
    FROM Gastos g
    WHERE MONTH(g.fecha) = @mes 
        AND YEAR(g.fecha) = @anio;

    -- 2. Total gastado por tipo de mantenimiento este mes
    SELECT 
        m.tipo AS tipoMantenimiento,
        COUNT(DISTINCT m.idMantenimiento) AS cantidad,
        SUM(g.monto) AS total
    FROM Mantenimientos m
    INNER JOIN Gastos g ON m.idMantenimiento = g.idMantenimiento
    WHERE MONTH(g.fecha) = @mes 
        AND YEAR(g.fecha) = @anio
        AND m.estadoLogico = 1
    GROUP BY m.tipo;

    -- 3. Top 3 vehículos con más gastos este mes
    SELECT TOP 3
        v.placa,
        v.marca,
        v.modelo,
        SUM(g.monto) AS totalGastado
    FROM Vehiculos v
    INNER JOIN Mantenimientos m ON v.idVehiculo = m.idVehiculo
    INNER JOIN Gastos g ON m.idMantenimiento = g.idMantenimiento
    WHERE MONTH(g.fecha) = @mes 
        AND YEAR(g.fecha) = @anio
        AND m.estadoLogico = 1
    GROUP BY v.idVehiculo, v.placa, v.marca, v.modelo
    ORDER BY totalGastado DESC;

    -- 4. Vehículos por estado
    SELECT 
        estado,
        COUNT(*) AS cantidad
    FROM Vehiculos
    WHERE estadoLogico = 1
        AND estado IS NOT NULL
    GROUP BY estado;
END
GO

/*==========================================*/
/*==== REPORTE 4: Actividad por Usuario ====*/
/*==========================================*/
CREATE OR ALTER PROCEDURE dbo.usp_ReporteActividadUsuario
    @fechaInicio DATE = NULL,
    @fechaFin DATE = NULL,
    @idUsuario INT = NULL  -- Opcional: filtrar por usuario específico
AS
BEGIN
    -- Reporte principal: Actividad de usuarios (solo trabajadores)
    SELECT 
        u.idUsuario,
        u.nombreCompleto,
        u.usuario,
		u.rol,
        -- Estadísticas generales
        COUNT(DISTINCT m.idVehiculo) AS vehiculosAtendidos,
        COUNT(DISTINCT m.idMantenimiento) AS totalMantenimientos,
        SUM(g.monto) AS totalGastado,
        -- Por tipo de mantenimiento
        SUM(CASE WHEN m.tipo = 'Preventivo' THEN 1 ELSE 0 END) AS mantenimientosPreventivos,
        SUM(CASE WHEN m.tipo = 'Correctivo' THEN 1 ELSE 0 END) AS mantenimientosCorrectivos,
        -- Gasto por tipo
        SUM(CASE WHEN m.tipo = 'Preventivo' THEN g.monto ELSE 0 END) AS gastoPreventivo,
        SUM(CASE WHEN m.tipo = 'Correctivo' THEN g.monto ELSE 0 END) AS gastoCorrectivo
    FROM Usuarios u
    LEFT JOIN Mantenimientos m ON u.idUsuario = m.idUsuario 
        AND m.estadoLogico = 1
        AND (@fechaInicio IS NULL OR m.fecha >= @fechaInicio)
        AND (@fechaFin IS NULL OR m.fecha <= @fechaFin)
    LEFT JOIN Gastos g ON m.idMantenimiento = g.idMantenimiento
     WHERE u.estado = 1 
        AND (@idUsuario IS NULL OR u.idUsuario = @idUsuario)
    GROUP BY u.idUsuario, u.nombreCompleto, u.usuario, u.rol
    ORDER BY totalGastado DESC, totalMantenimientos DESC;

    -- Si se especifica un usuario, mostrar detalle de sus mantenimientos
    IF @idUsuario IS NOT NULL
    BEGIN
        SELECT 
            v.placa,
            v.marca,
            v.modelo,
            m.tipo AS tipoMantenimiento,
            m.fecha,
            m.descripcion,
            (SELECT SUM(g2.monto) 
             FROM Gastos g2 
             WHERE g2.idMantenimiento = m.idMantenimiento) AS totalMantenimiento,
            STUFF((
                SELECT ', ' + tg.nombre + ': $' + CAST(g.monto AS VARCHAR(20))
                FROM Gastos g
                INNER JOIN TipoGasto tg ON g.idTipoGasto = tg.idTipoGasto
                WHERE g.idMantenimiento = m.idMantenimiento
                FOR XML PATH('')
            ), 1, 2, '') AS detalleGastos
        FROM Mantenimientos m
        INNER JOIN Vehiculos v ON m.idVehiculo = v.idVehiculo
        WHERE m.idUsuario = @idUsuario
            AND m.estadoLogico = 1
            AND (@fechaInicio IS NULL OR m.fecha >= @fechaInicio)
            AND (@fechaFin IS NULL OR m.fecha <= @fechaFin)
        ORDER BY m.fecha DESC, v.placa;
    END
END
GO
/*========== AGREGAMOS DATOS DE PRUEBA ==========*/
--Creo un usuario administrador
INSERT INTO Usuarios (nombreCompleto, usuario, password, rol, estado)
VALUES ('AdminK', 'admin', '12345', 'Admin', 1);

-- Creo un usuario trabajador
INSERT INTO Usuarios (nombreCompleto, usuario, password, rol, estado)
VALUES ('UserK', 'K', '12345', 'Trabajador', 1);

-- Verificamos
SELECT * FROM Usuarios;
GO

-- Ingresamos 5 Vehículos 
INSERT INTO Vehiculos (placa, marca, modelo, tipo, anio, kilometraje, estado) VALUES
('ABC-101', 'Toyota', 'Coaster', 'Bus', 2019, 120000, 'Operativo'),
('BCD-202', 'Hyundai', 'Accent', 'Sedan', 2020, 80000, 'Operativo'),
('CDE-303', 'Kia', 'Sportage', 'SUV', 2021, 60000, 'Operativo'),
('DEF-404', 'Nissan', 'Navara', 'Pick-Up', 2018, 140000, 'En Taller'),
('EFG-505', 'Ford', 'Transit', 'Furgoneta', 2017, 160000, 'Operativo'),
('FGH-606', 'Chevrolet', 'Spark', 'Compacto', 2019, 70000, 'Operativo'),
('GHI-707', 'Mazda', 'CX-5', 'CrossOver', 2022, 30000, 'Operativo'),
('HIJ-808', 'Honda', 'Civic', 'HatchBack', 2020, 65000, 'Operativo'),
('IJK-909', 'BMW', 'Z4', 'Roadster', 2021, 40000, 'Operativo'),
('JKL-111', 'Mercedes', 'S500', 'Limusina', 2018, 90000, 'Operativo'),
('KLM-222', 'Volkswagen', 'Combi', 'MicroBus', 2016, 180000, 'En Taller'),
('LMN-333', 'Audi', 'A5', 'Convertible', 2022, 25000, 'Operativo'),
('MNO-444', 'Toyota', 'Hilux', 'TodoTerreno', 2020, 85000, 'Operativo'),
('NOP-555', 'Chevrolet', 'Camaro', 'Cupe', 2021, 45000, 'Operativo');
GO

-- Ingresamos 10 tipos de gasto
INSERT INTO TipoGasto (nombre, descripcion) VALUES
('Combustible', 'Gasolina o diésel'),
('Aceite y lubricantes', 'Aceite de motor y grasas'),
('Llantas y neumáticos', 'Compra o cambio de llantas'),
('Repuestos mecánicos', 'Frenos, filtros, correas'),
('Repuestos eléctricos', 'Baterías, luces, sensores'),
('Mano de obra', 'Pago por servicio técnico'),
('Servicios de taller', 'Alineación, balanceo, escáner'),
('Lavado y limpieza', 'Lavado interior y exterior'),
('Documentación vehicular', 'SOAT y revisión técnica'),
('Otros gastos operativos', 'Peajes y gastos menores');

--Ingresamos 15 mantenimientos
INSERT INTO Mantenimientos (idVehiculo, tipo, descripcion, fecha, idUsuario, estadoLogico)
VALUES 
(1, 'Preventivo', 'Cambio de aceite y filtros', '2024-01-15', 1, 1),
(2, 'Correctivo', 'Reparación de frenos', '2024-01-20', 2, 1),
(3, 'Preventivo', 'Revisión general y afinamiento', '2024-02-05', 1, 1),
(1, 'Correctivo', 'Cambio de batería', '2024-02-10', 1, 1),
(4, 'Preventivo', 'Rotación de llantas y alineación', '2024-02-15', 1, 1),
(5, 'Correctivo', 'Reparación de sistema eléctrico', '2024-02-20', 2, 1),
(2, 'Preventivo', 'Cambio de líquidos y lubricantes', '2024-03-01', 2, 1),
(3, 'Correctivo', 'Reparación de suspensión', '2024-03-10', 2, 1),
(6, 'Preventivo', 'Revisión de sistema de aire acondicionado', '2024-03-15', 2, 1),
(4, 'Correctivo', 'Cambio de transmisión', '2024-03-20', 2, 0);

INSERT INTO Gastos (idMantenimiento, idTipoGasto, monto, fecha, descripcion) VALUES
-- Gasto 1: Relacionado con Mantenimiento 1 (Cambio aceite - Vehículo 1, Usuario 1)
(1, 1, 185.50, '2024-01-15', 'Combustible diésel para prueba después del cambio de aceite'),

-- Gasto 2: Relacionado con Mantenimiento 2 (Reparación frenos - Vehículo 2, Usuario 2)
(2, 4, 420.75, '2024-01-20', 'Compra de pastillas, discos y líquido de frenos'),

-- Gasto 3: Relacionado con Mantenimiento 3 (Afinamiento - Vehículo 3, Usuario 1)
(3, 6, 150.00, '2024-02-05', 'Mano de obra para afinamiento completo'),

-- Gasto 4: Relacionado con Mantenimiento 4 (Cambio batería - Vehículo 1, Usuario 1)
(4, 5, 295.80, '2024-02-10', 'Batería 12V 75Ah para bus Toyota Coaster'),

-- Gasto 5: Relacionado con Mantenimiento 5 (Rotación llantas - Vehículo 4, Usuario 1)
(5, 7, 85.25, '2024-02-15', 'Alineación y balanceo en taller especializado'),

-- Gasto 6: Relacionado con Mantenimiento 6 (Reparación eléctrica - Vehículo 5, Usuario 2)
(6, 5, 320.40, '2024-02-20', 'Alternador y cableado para furgoneta Ford Transit'),

-- Gasto 7: Relacionado con Mantenimiento 7 (Cambio líquidos - Vehículo 2, Usuario 2)
(7, 2, 175.90, '2024-03-01', 'Aceite sintético, anticongelante y líquido de dirección'),

-- Gasto 8: Relacionado con Mantenimiento 8 (Reparación suspensión - Vehículo 3, Usuario 2)
(8, 4, 560.25, '2024-03-10', 'Amortiguadores delanteros y bujes de suspensión'),

-- Gasto 9: Relacionado con Mantenimiento 9 (Aire acondicionado - Vehículo 6, Usuario 2)
(9, 7, 210.00, '2024-03-15', 'Recarga de gas y revisión sistema de aire acondicionado'),

-- Gasto 10: Relacionado con Mantenimiento 10 (Cambio transmisión - Vehículo 4, Usuario 2)
(10, 4, 1250.00, '2024-03-20', 'Transmisión completa para Nissan Navara 4x4');
SELECT * FROM Usuarios
GO
SELECT * FROM Vehiculos
GO
SELECT * FROM TipoGasto
GO
SELECT * FROM Mantenimientos
GO	

