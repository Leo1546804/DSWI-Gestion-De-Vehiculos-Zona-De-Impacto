DROP DATABASE IF EXISTS ZonaDeImpacto;
GO
CREATE DATABASE ZonaDeImpacto;
GO
USE ZonaDeImpacto;
GO

-- TABLA USUARIO
CREATE TABLE Usuarios (
    idUsuario INT IDENTITY PRIMARY KEY,
    nombreCompleto NVARCHAR(100) NOT NULL,
    usuario NVARCHAR(50) NOT NULL UNIQUE,
    password NVARCHAR(255) NOT NULL,
    rol NVARCHAR(20) NOT NULL CHECK (rol IN ('Admin','Trabajador')),
    estado BIT NOT NULL DEFAULT 1
);
select * from Usuarios

-- TABLA VEHICULOS
CREATE TABLE Vehiculos (
    idVehiculo INT IDENTITY PRIMARY KEY,
    placa NVARCHAR(15) NOT NULL UNIQUE,
    marca NVARCHAR(50) NOT NULL,
    modelo NVARCHAR(50) NOT NULL,
	tipo NVARCHAR(50) NULL,
    anio INT NULL,
    kilometraje INT NULL,
    estado NVARCHAR(20) NULL  -- Ejemplo: 'Operativo', 'En Taller'
);

-- TABLA TIPOGASTO
CREATE TABLE TipoGasto (
    idTipoGasto INT IDENTITY PRIMARY KEY,
    nombre NVARCHAR(50) NOT NULL UNIQUE   -- gasolina, llantas, aceite, etc.
);

-- TABLA GASTOS
CREATE TABLE Gastos (
    idGasto INT IDENTITY PRIMARY KEY,
    idVehiculo INT NOT NULL,
    idTipoGasto INT NOT NULL,
    monto DECIMAL(10,2) NOT NULL,
    fecha DATE NOT NULL,
    descripcion NVARCHAR(200),
    evidencia NVARCHAR(255),  -- ruta o nombre de archivo
    idUsuario INT NOT NULL,

    FOREIGN KEY (idVehiculo) REFERENCES Vehiculos(idVehiculo),
    FOREIGN KEY (idTipoGasto) REFERENCES TipoGasto(idTipoGasto),
    FOREIGN KEY (idUsuario) REFERENCES Usuarios(idUsuario)
);

-- TABLA MANTENIMIENTOS
CREATE TABLE Mantenimientos (
    idMantenimiento INT IDENTITY PRIMARY KEY,
    idVehiculo INT NOT NULL,
    tipo NVARCHAR(20) NOT NULL CHECK (tipo IN ('Preventivo','Correctivo')),
    descripcion NVARCHAR(200),
    costo DECIMAL(10,2) NOT NULL,
    fecha DATE NOT NULL,
    evidencia NVARCHAR(255),
    idUsuario INT NOT NULL,

    FOREIGN KEY (idVehiculo) REFERENCES Vehiculos(idVehiculo),
    FOREIGN KEY (idUsuario) REFERENCES Usuarios(idUsuario)
);
go

-----------------PROCEDIMIENTOS ALMACENADOS----------------------

--LOGIN
CREATE OR ALTER PROCEDURE sp_LoginUsuario
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

--USUARIO

--insertar
CREATE OR ALTER PROCEDURE sp_InsertarUsuario
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

--listar
CREATE OR ALTER PROCEDURE sp_ListarUsuarios
AS
BEGIN
    
    SELECT idUsuario, nombreCompleto, usuario, rol, estado FROM Usuarios;
    
END
GO

--Buscarr usuario por id
CREATE OR ALTER PROCEDURE sp_ObtenerUsuario
    @idUsuario INT
AS
BEGIN
    SELECT * FROM Usuarios WHERE idUsuario = @idUsuario;
END
GO

--actualizar
CREATE OR ALTER PROCEDURE sp_EditarUsuario
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

--eliminars
CREATE OR ALTER PROCEDURE sp_EliminarUsuario
    @idUsuario INT
AS
BEGIN
    DELETE FROM Usuarios WHERE idUsuario = @idUsuario;
END
GO
EXEC sp_ListarUsuarios;
GO
select * from Usuarios

--Creo un usuario administrador
INSERT INTO Usuarios (nombreCompleto, usuario, password, rol, estado)
VALUES ('AdminK', 'admin', '12345', 'Admin', 1);

-- Creo un usuario trabajador
INSERT INTO Usuarios (nombreCompleto, usuario, password, rol, estado)
VALUES ('UserK', 'K', '12345', 'Trabajador', 1);

-- Verificamos
SELECT * FROM Usuarios;
GO
/*===========PROC DE VEHICULOS=========*/

-- 1. INSERTAMOS
CREATE OR ALTER PROCEDURE sp_InsertarVehiculo
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

-- 2. LISTAMOS
CREATE OR ALTER PROCEDURE sp_ListarVehiculos
AS
BEGIN
	SELECT idVehiculo, placa, marca, modelo, tipo, anio, kilometraje, estado
	FROM Vehiculos;
END 
GO

-- 3. EDITAMOS
CREATE OR ALTER PROCEDURE sp_EditarVehiculo
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
GO

-- 4. OBTENEMOS POR SU ID 
CREATE OR ALTER PROCEDURE sp_ObtenerVehiculo
	@idVehiculo int
AS
BEGIN 
	SELECT idVehiculo, placa, marca, modelo, tipo, anio, kilometraje, estado
	FROM Vehiculos 
	WHERE idVehiculo = @idVehiculo;
END
GO

-- 5. ELIMINAMOS 
CREATE OR ALTER PROCEDURE sp_EliminarVehiculo
	@idVehiculo int
AS
BEGIN
	DELETE FROM Vehiculos WHERE idVehiculo = @idVehiculo;
END
GO
-- Registro 1: Vehículo operativo común
INSERT INTO Vehiculos (placa, marca, modelo, anio, kilometraje, estado)
VALUES ('ABC-123', 'Toyota', 'Hilux', 2020, 45000, 'Operativo'),
		('XYZ-789', 'Hyundai', 'Tucson', 2023, 12000, 'Operativo'),
		('DEF-456', 'Ford', 'Ranger', 2019, 85000, 'En Taller'),
		('GHI-101', 'Chevrolet', 'Spark', 2015, 120000, 'Operativo'),
		('JKL-202', 'Nissan', 'Versa', 2022, 25000, 'Operativo');
GO

SELECT * FROM Vehiculos
GO

/*===========Proc de Mantenimientos=========*/
--REGISTEAMOS
CREATE OR ALTER PROCEDURE sp_RegistrarMantenimiento
@idVehiculo INT,
@tipo NVARCHAR(20),
@descripcion NVARCHAR(200),
@costo DECIMAL(10,2),
@fecha DATE,
@evidencia NVARCHAR(255),
@idUsuario INT
AS
BEGIN
    INSERT INTO Mantenimientos(idVehiculo, tipo, descripcion, costo, fecha, evidencia, idUsuario)
    VALUES (@idVehiculo, @tipo, @descripcion, @costo, @fecha, @evidencia, @idUsuario);
END
GO

--LISTAMOS

CREATE OR ALTER PROCEDURE sp_ListarMantenimientos
AS
BEGIN
    SELECT 
        M.idMantenimiento,
        M.idVehiculo,
        V.placa,
        M.tipo,
        M.descripcion,
        M.costo,
        M.fecha,
        M.evidencia,
        M.idUsuario,
        U.nombreCompleto
    FROM Mantenimientos M
    INNER JOIN Vehiculos V ON V.idVehiculo = M.idVehiculo
    INNER JOIN Usuarios U ON U.idUsuario = M.idUsuario
    ORDER BY M.idMantenimiento DESC;
END
GO


--OBTEBNEMOS POR ID
CREATE OR ALTER PROCEDURE sp_ObtenerMantenimiento
@idMantenimiento INT
AS
BEGIN
    SELECT 
        idMantenimiento,
        idVehiculo,
        tipo,
        descripcion,
        costo,
        fecha,
        evidencia,
        idUsuario
    FROM Mantenimientos
    WHERE idMantenimiento = @idMantenimiento;
END
GO
--Editamos
CREATE OR ALTER PROCEDURE sp_EditarMantenimiento
@idMantenimiento INT,
@idVehiculo INT,
@tipo NVARCHAR(20),
@descripcion NVARCHAR(200),
@costo DECIMAL(10,2),
@fecha DATE,
@evidencia NVARCHAR(255),
@idUsuario INT
AS
BEGIN
    UPDATE Mantenimientos
    SET 
        idVehiculo = @idVehiculo,
        tipo = @tipo,
        descripcion = @descripcion,
        costo = @costo,
        fecha = @fecha,
        evidencia = @evidencia,
        idUsuario = @idUsuario
    WHERE idMantenimiento = @idMantenimiento;
END
GO
--ELIMININAMOS
CREATE OR ALTER PROCEDURE sp_EliminarMantenimiento
@idMantenimiento INT
AS
BEGIN
    DELETE FROM Mantenimientos
    WHERE idMantenimiento = @idMantenimiento;
END
GO