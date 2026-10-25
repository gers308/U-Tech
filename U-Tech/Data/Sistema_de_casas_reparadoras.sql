Create database CasaMatriz_Sistema;

go

use CasaMatriz_Sistema;

go



--Tablas que guardaran las informacion basica de la casa administradora de las sucursales
--y las informacion de las sucursales.
Create table Casa_Matriz(
	Casa_Matriz_ID int primary key, 
	Nombre varchar(200) not null,
	Direccion varchar(500) not null
);



go
--Tabla de sucursales
--
Create table Sucursal(
	Sucursal_ID int primary key identity(1,1),
	Nombre_Sucursal varchar(100) not null,
	Direccion varchar(500) not null,
	--llave foranea a la casa matriz
	Casa_matriz int,

constraint FK_Casa_Matriz_ID foreign key (Casa_Matriz) references casa_Matriz(Casa_Matriz_ID)
);





go
--tabla de clientes
--
create table Cliente(
	Cliente_ID int primary key identity(1,1),
	Nombre varchar(100) not null,
	Telefono varchar(12) not null,
	Email varchar(250) unique not null
);



go
--tabla con la informacion de la computadora del cliente
--
create table Computadora(
	PC_ID int primary key identity(1,1),
	Marca varchar(100) null,
	Tipo_computadora varchar(100) null,
	Descripcion varchar(500) not null,

	--llave foranea para identificar que cliente entro la pc
	Cliente_ID int,

constraint FK_cliente_ID foreign key (Cliente_ID) references Cliente(Cliente_ID)
);



go
--Creando la tabla para guardas al personal de la sucursal
--
create table Personal(
	Personal_ID int primary key identity(1,1),
	Nombre varchar(100) not null,
	Apellido varchar(100) not null,
	rol varchar(50) not null,

	--llave foranea para identificar a que sucursal pertenece el trabajado
	Sucursal_ID int,

constraint FK_sucursal_ID foreign key (Sucursal_ID) references Sucursal(Sucursal_id)
);



go
--Creando la tabla para la solicitud del cliente
--
create table Solicitud(
    Solicitud_ID int primary key identity(1,1),
    fecha_recibida datetime,
    Estado varchar(100),

	--llaves foraneas para identificar y manejar mejor la solicitud, a que sucursal pertenece, que computadora es y quien le recibio la pc
    PC_ID int not null,
    Cliente_ID int not null,
    Sucursal_ID int not null,
    Personal_ID int not null,

constraint FK_pc_ID foreign key (PC_ID) references Computadora(PC_ID),
constraint FK_cliente2_ID foreign key (Cliente_ID) references Cliente(Cliente_ID),
constraint FK_sucursal2_ID foreign key (Sucursal_ID) references Sucursal(Sucursal_id),
constraint FK_personal_ID foreign key (Personal_ID) references Personal(Personal_ID)
);


go
--tabla del diagnostico
--
create table Diagnostico(
	Diagnostico_ID int primary key identity(1,1),
	Descripcion_Diagnostico Text null,
	Costo_Estimacion decimal(10,2) check(Costo_Estimacion>0),
	Decision_Cliente varchar(10), --decision del cliente
	Fecha datetime,

	--llaves foranea para identificar que pc paso a diagnostico y que tecnico la realiza
	Personal_ID int,
	Solicitud_ID int,

constraint FK_personal2_ID foreign key (Personal_ID) references Personal(Personal_ID),
constraint FK_solicitud_ID foreign key (Solicitud_ID) references Solicitud(Solicitud_ID)
);


go
--tabla reparacion
--
create table Reparacion(
	Reparacion_ID int primary key identity(1,1),
	Fecha_Inicio datetime null,
	Fecha_Final datetime null,
	Detalle_trabajo text,
	Costo_Final decimal(10,2),
	Estado_Final varchar(100),

	--llave para identificar que pc se esta reparando
	Diagnostico_ID int,

constraint FK_Diagnostico_ID foreign key (Diagnostico_ID) references Diagnostico(Diagnostico_ID)
);



go
--tabla expediente
--
create table expediente(
	Expediente_ID int primary key identity(1,1),
	Descripcion text,	--Aca el tecnico escribira lo que se le ha hecho a la pc del cliente

	Reparacion_ID int,
	Tecnico_ID int,
	PC_ID int,
	Cliente_ID int,

constraint FK_reparacion_ID foreign key (Reparacion_ID) references Reparacion(reparacion_ID),
constraint FK_tecnico_ID foreign key (Tecnico_ID) references personal(Personal_ID),
constraint FK_pc2_ID foreign key (PC_ID) references computadora(PC_ID),
constraint FK_cliente3_ID foreign key (Cliente_ID) references Cliente(Cliente_ID)
);







go
-- Tabla para definir los Roles de acceso
create table Roles (
	Rol_ID int primary key identity(1,1),
	Nombre_Rol varchar(50) not null UNIQUE --'Administrador' 'Tecnico' 'Personal xd'
);

go
-- Tabla para gestionar los Usuarios y sus credenciales
create table Usuarios (
	Usuario_ID int primary key identity(1,1),
	Usuario varchar(50) not null UNIQUE,
	Contrasena varchar(255) not null,

	-- llave foranea al Personal si el usuario es un empleado de sucursal
	Personal_ID int null, 
	constraint FK_Personal_Usuario foreign key (Personal_ID) references Personal(Personal_ID),

	-- llave foránea al Rol
	Rol_ID int not null,
	constraint FK_Rol_Usuario foreign key (Rol_ID) references Roles(Rol_ID)
);
go






/*
SELECT * FROM Casa_Matriz;
SELECT * FROM Sucursal;
SELECT * FROM Cliente;
SELECT * FROM Computadora;
SELECT * FROM Personal;
SELECT * FROM Solicitud;
SELECT * FROM Diagnostico;
SELECT * FROM Reparacion;
SELECT * FROM Expediente;
SELECT * FROM Roles;
SELECT * FROM Usuarios;
*/





--===vistas===

--vista 1
--vista de clientes y sus computadoras
--esta vista la debe de ver la persona que recibe las computadoras porque es quien registra clientes y computadoras
--el administrador ya que debe supervisar los registros
CREATE VIEW vw_Clientes_Computadoras AS
SELECT 
    C.Cliente_ID,
    C.Nombre AS Nombre_Cliente,
    C.Telefono,
    C.Email,
    PC.PC_ID,
    PC.Marca,
    PC.Tipo_computadora,
    PC.Descripcion
FROM Cliente C
INNER JOIN Computadora PC ON C.Cliente_ID = PC.Cliente_ID;

go
--prueba de la vista: vw_Clientes_Computadoras
--select * from vw_Clientes_Computadoras;
go


--vista 2
--Vista hecha para la solicitud:
--la debera de ver administrador porque supervisa las solicutes y el flujo
--y el que la recibe debe verla para dar seguimiento a la solicitudes
CREATE VIEW vw_Solicitud_Detalle AS
SELECT 
    S.Solicitud_ID,
    S.fecha_recibida,
    S.Estado,
    C.Nombre AS Nombre_Cliente,
    C.Email,
    P.Marca AS Marca_PC,
    P.Tipo_computadora,
    PR.Nombre + ' ' + PR.Apellido AS Personal_que_recibio,
    SU.Nombre_Sucursal
FROM Solicitud S
INNER JOIN Cliente C ON S.Cliente_ID = C.Cliente_ID
INNER JOIN Computadora P ON S.PC_ID = P.PC_ID
INNER JOIN Personal PR ON S.Personal_ID = PR.Personal_ID
INNER JOIN Sucursal SU ON S.Sucursal_ID = SU.Sucursal_ID;

go
--prueba de la vista: vw_Solicitud_Detalle
--select * from vw_Solicitud_Detalle;
go


--vista 3
--vista diagnostico detallados
--la debe ver el administrador para revisar diagnosticos y aprobaciones
--el tecnico la vera ya que el debe realizar o consultar el trabajo previo
--el que recibe la pc la puede ver pero solo de forma de lectura para informar al cliente
CREATE VIEW vw_Diagnosticos_Detalle AS
SELECT 
    D.Diagnostico_ID,
    D.Descripcion_Diagnostico,
    D.Costo_Estimacion,
    D.Decision_Cliente,
    D.Fecha,
    P.Nombre + ' ' + P.Apellido AS Tecnico_Diagnostico,
    C.Nombre AS Cliente,
    PC.Marca AS Marca_PC,
    PC.Tipo_computadora
FROM Diagnostico D
INNER JOIN Personal P ON D.Personal_ID = P.Personal_ID
INNER JOIN Solicitud S ON D.Solicitud_ID = S.Solicitud_ID
INNER JOIN Cliente C ON S.Cliente_ID = C.Cliente_ID
INNER JOIN Computadora PC ON S.PC_ID = PC.PC_ID;

go
--prueba de vista: vw_Diagnosticos_Detalle
--select * from vw_Diagnosticos_Detalle;
go


--vista 4
--vista repaciones de forma mas detallada.
--la vera el administrador para ver las reparaciones hecha y costos
--el tecnico podra ver las reparaciones hechas por el o que estan asignadas a el
CREATE VIEW vw_Reparaciones_Detalle AS
SELECT 
    R.Reparacion_ID,
    D.Diagnostico_ID,
    C.Nombre AS Cliente,
    PC.Marca,
    PC.Tipo_computadora,
    T.Nombre + ' ' + T.Apellido AS Tecnico,
    R.Fecha_Inicio,
    R.Fecha_Final,
    R.Detalle_trabajo,
    R.Costo_Final,
    R.Estado_Final
FROM Reparacion R
INNER JOIN Diagnostico D ON R.Diagnostico_ID = D.Diagnostico_ID
INNER JOIN Solicitud S ON D.Solicitud_ID = S.Solicitud_ID
INNER JOIN Cliente C ON S.Cliente_ID = C.Cliente_ID
INNER JOIN Computadora PC ON S.PC_ID = PC.PC_ID
INNER JOIN Personal T ON D.Personal_ID = T.Personal_ID;

go
--prueba de vista: vw_Reparaciones_Detalle.
--select * from vw_Reparaciones_Detalle;
go


--vista 5
--vista para ver el expediente detallado o completo
--muestra todo todo el historial cliente, técnico, diagnóstico, reparación, observaciones
--el administrador tiene acceso total supervisa el historial
--el tecnico solo los expedientes de sus propias reparaciones (se filtra por Tecnico_ID)
CREATE VIEW vw_Expediente_Completo AS
SELECT 
    E.Expediente_ID,
    C.Nombre AS Cliente,
    PC.Marca,
    PC.Tipo_computadora,
    T.Nombre + ' ' + T.Apellido AS Tecnico,
    D.Descripcion_Diagnostico,
    R.Detalle_trabajo,
    R.Costo_Final,
    R.Estado_Final,
    E.Descripcion AS Observaciones
FROM Expediente E
INNER JOIN Cliente C ON E.Cliente_ID = C.Cliente_ID
INNER JOIN Computadora PC ON E.PC_ID = PC.PC_ID
INNER JOIN Personal T ON E.Tecnico_ID = T.Personal_ID
INNER JOIN Reparacion R ON E.Reparacion_ID = R.Reparacion_ID
INNER JOIN Diagnostico D ON R.Diagnostico_ID = D.Diagnostico_ID;

go
--prueba de vista: vw_Expediente_Completo
--select * from vw_Expediente_Completo;
go



/*
--prueba para las 5 vistas hechas
--1
select * from vw_Clientes_Computadoras;
--2
select * from vw_Solicitud_Detalle;
--3
select * from vw_Diagnosticos_Detalle;
--4
select * from vw_Reparaciones_Detalle;
--5
select * from vw_Expediente_Completo;
*/








go
--procedimiento almacenado para manejar mejor la informacion del login
--y que se pueda iniciar sesion mejor los diferentes roles y sus respectivas sucursales
CREATE PROCEDURE sp_Login
    @Usuario VARCHAR(50),
    @Contrasena VARCHAR(255)
AS
BEGIN
    SELECT 
        U.Usuario_ID,
        U.Usuario,
        U.Personal_ID,
        P.Sucursal_ID,
        R.Nombre_Rol
    FROM Usuarios U
    INNER JOIN Roles R ON U.Rol_ID = R.Rol_ID
    LEFT JOIN Personal P ON U.Personal_ID = P.Personal_ID
    WHERE U.Usuario = @Usuario
      AND U.Contrasena = @Contrasena;
END
go







--casa matriz info basica
INSERT INTO Casa_Matriz (Casa_Matriz_ID, Nombre, Direccion)
VALUES (1, 'U-Tech S.A. de C.V.', 'Av. Las Ciencias #123, San Salvador');
go



--inyectando datos
--sucursal
INSERT INTO Sucursal (Nombre_Sucursal, Direccion, Casa_matriz)
VALUES ('Sucursal San Salvador', 'Colonia Escalón, Calle El Mirador #123', 1);
go




--personal de la sucursal, solo de la sucursal!
INSERT INTO Personal (Nombre, Apellido, Rol, Sucursal_ID)
VALUES 
('María', 'García', 'Recepcionista', 1),
('Luis', 'Martínez', 'Técnico', 1),
('Ana', 'Hernández', 'Gerente', 1);
go






--Roles del sistema, esto afectara a todo el sistema xd
INSERT INTO Roles (Nombre_Rol)
VALUES ('Administrador'), ('Tecnico'), ('Personal'), ('GerenteSucursal');
go

SELECT * FROM Usuarios;
go




--ususarios del sistema
INSERT INTO Usuarios (Usuario, Contrasena, Personal_ID, Rol_ID)
VALUES
('mgarcia', '12345', 1, 3),   -- Recepcionista
('lmartinez', '12345', 2, 2), -- Técnico
('ahernandez', '12345', 3, 4), -- Gerente sucursal
('admin', '12345', NULL, 1);   -- Admin Global esta usuario esta por encima de todo y no estara en la tabla personal ya que el maneja todo
go







/*
select *from Casa_Matriz

select *from Sucursal

select *from Personal

select *from Roles

select *from Usuarios

select *from Cliente

select *from Computadora

select *from Solicitud

select *from Diagnostico

select *from Reparacion

select *from expediente
*/

go

--view arreglada
ALTER VIEW vw_Solicitud_Detalle
AS
SELECT 
    s.Solicitud_ID,
    s.fecha_recibida,
    s.Estado,
    c.Nombre AS Nombre_Cliente,
    c.Email,
    pc.Marca AS Marca_PC,
    pc.Tipo_computadora,
    p.Nombre + ' ' + p.Apellido AS Personal_que_recibio,
    su.Nombre_Sucursal,
    su.Sucursal_ID
FROM Solicitud s
INNER JOIN Cliente c ON s.Cliente_ID = c.Cliente_ID
INNER JOIN Computadora pc ON s.PC_ID = pc.PC_ID
INNER JOIN Personal p ON s.Personal_ID = p.Personal_ID
INNER JOIN Sucursal su ON s.Sucursal_ID = su.Sucursal_ID;
GO


go

ALTER TABLE Solicitud
ADD CONSTRAINT CHK_Solicitud_Estado CHECK (Estado IN ('Recibido','Diagnosticado','En Reparacion','Finalizado'));

go

UPDATE Solicitud SET Estado = 'Recibido' WHERE Estado IS NULL;

go



go

--arreglando un vista

ALTER VIEW vw_Clientes_Computadoras AS
SELECT 
    C.Cliente_ID,
    C.Nombre AS Nombre_Cliente,
    C.Telefono,
    C.Email,
    PC.PC_ID,
    PC.Marca,
    PC.Tipo_computadora,
    PC.Descripcion,
    Su.Sucursal_ID,
    Su.Nombre_Sucursal AS Nombre_Sucursal
FROM Cliente C
INNER JOIN Computadora PC ON C.Cliente_ID = PC.Cliente_ID
INNER JOIN Solicitud Sol ON PC.PC_ID = Sol.PC_ID
INNER JOIN Sucursal Su ON Sol.Sucursal_ID = Su.Sucursal_ID;

go


--arreglando vista
ALTER VIEW vw_Diagnosticos_Detalle AS
SELECT 
    D.Diagnostico_ID,
    D.Solicitud_ID,
    D.Descripcion_Diagnostico,
    D.Fecha,
    S.Estado,
    S.fecha_recibida,
    S.Sucursal_ID,
    C.Nombre AS Nombre_Cliente,
    PC.Marca,
    PC.Tipo_computadora,
    Su.Nombre_Sucursal
FROM Diagnostico D
INNER JOIN Solicitud S ON D.Solicitud_ID = S.Solicitud_ID
INNER JOIN Computadora PC ON S.PC_ID = PC.PC_ID
INNER JOIN Cliente C ON PC.Cliente_ID = C.Cliente_ID
INNER JOIN Sucursal Su ON S.Sucursal_ID = Su.Sucursal_ID;

go
--otra vista arreglada, para que se haga el pdf sin problemas

ALTER VIEW vw_Reparaciones_Detalle AS
SELECT 
    R.Reparacion_ID,
    R.Fecha_Inicio,
    R.Fecha_Final,
    R.Detalle_trabajo,
    R.Costo_Final,
    R.Estado_Final,
    D.Diagnostico_ID,
    D.Descripcion_Diagnostico,
    S.Solicitud_ID,
    S.Sucursal_ID,                          -- <- IMPORTANTE (ESTE FILTRA POR SUCURSAL)
    C.Nombre AS Nombre_Cliente,
    PC.Marca,
    PC.Tipo_computadora,
    Su.Nombre_Sucursal
FROM Reparacion R
INNER JOIN Diagnostico D ON R.Diagnostico_ID = D.Diagnostico_ID
INNER JOIN Solicitud S ON D.Solicitud_ID = S.Solicitud_ID
INNER JOIN Computadora PC ON S.PC_ID = PC.PC_ID
INNER JOIN Cliente C ON PC.Cliente_ID = C.Cliente_ID
INNER JOIN Sucursal Su ON S.Sucursal_ID = Su.Sucursal_ID;

go
--expediente completo arreglada

ALTER VIEW vw_Expediente_Completo AS
SELECT 
    E.Expediente_ID,
    E.Descripcion AS Detalle_Expediente,
    R.Reparacion_ID,
    R.Fecha_Inicio,
    R.Fecha_Final,
    R.Costo_Final,
    R.Estado_Final,
    S.Solicitud_ID,
    S.Sucursal_ID,                          -- <- NECESARIO PARA FILTRAR POR SUCURSAL
    C.Nombre AS Nombre_Cliente,
    PC.Marca,
    PC.Tipo_computadora,
    Su.Nombre_Sucursal,
    P.Nombre + ' ' + P.Apellido AS Tecnico
FROM Expediente E
INNER JOIN Reparacion R ON E.Reparacion_ID = R.Reparacion_ID
INNER JOIN Diagnostico D ON R.Diagnostico_ID = D.Diagnostico_ID
INNER JOIN Solicitud S ON D.Solicitud_ID = S.Solicitud_ID
INNER JOIN Computadora PC ON S.PC_ID = PC.PC_ID
INNER JOIN Cliente C ON PC.Cliente_ID = C.Cliente_ID
INNER JOIN Personal P ON E.Tecnico_ID = P.Personal_ID
INNER JOIN Sucursal Su ON S.Sucursal_ID = Su.Sucursal_ID;
go







--test
-- 2) Normalizar estados existentes (si hay NULL o valores raros)
UPDATE Reparacion
SET Estado_Final = 'Finalizado'
WHERE Estado_Final IS NULL OR Estado_Final NOT IN ('En Reparacion','Finalizado','Entregado');
GO

-- 3) Eliminar constraint previo si existe y crear el nuevo
IF EXISTS (
    SELECT 1 FROM sys.check_constraints WHERE name = 'CHK_Reparacion_Estado'
)
BEGIN
    ALTER TABLE Reparacion DROP CONSTRAINT CHK_Reparacion_Estado;
END
GO

ALTER TABLE Reparacion
ADD CONSTRAINT CHK_Reparacion_Estado
CHECK (Estado_Final IN ('En Reparacion','Finalizado','Entregado'));
GO





go

IF OBJECT_ID('dbo.vw_Computadoras_Entregadas', 'V') IS NOT NULL
    DROP VIEW dbo.vw_Computadoras_Entregadas;
GO

CREATE VIEW dbo.vw_Computadoras_Entregadas AS
SELECT 
    R.Reparacion_ID,
    C.Cliente_ID,
    C.Nombre AS Cliente,
    PC.PC_ID,
    PC.Marca,
    PC.Tipo_computadora,
    R.Fecha_Inicio,
    R.Fecha_Final,
    R.Estado_Final,
    S.Nombre_Sucursal
FROM Reparacion R
INNER JOIN Diagnostico D ON R.Diagnostico_ID = D.Diagnostico_ID
INNER JOIN Solicitud S2 ON D.Solicitud_ID = S2.Solicitud_ID
INNER JOIN Cliente C ON S2.Cliente_ID = C.Cliente_ID
INNER JOIN Computadora PC ON S2.PC_ID = PC.PC_ID
INNER JOIN Sucursal S ON S2.Sucursal_ID = S.Sucursal_ID
WHERE R.Estado_Final = 'Entregado';
GO







go
ALTER TABLE Reparacion DROP CONSTRAINT CHK_Reparacion_Estado;
go

ALTER TABLE Reparacion
ADD CONSTRAINT CHK_Reparacion_Estado 
CHECK (Estado_Final IN ('Pendiente', 'En Reparacion', 'Finalizado', 'Entregado'));

select*from Usuarios
select*from Personal