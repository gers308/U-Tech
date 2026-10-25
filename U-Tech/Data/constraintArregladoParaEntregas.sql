EXEC sp_helpconstraint 'Reparacion';

ALTER TABLE Reparacion DROP CONSTRAINT CHK_Reparacion_Estado;


ALTER TABLE Reparacion
ADD CONSTRAINT CHK_Reparacion_Estado 
CHECK (Estado_Final IN ('Pendiente', 'En Reparacion', 'Finalizado', 'Entregado'));


SELECT Reparacion_ID, Estado_Final, Diagnostico_ID
FROM Reparacion

SELECT OBJECT_DEFINITION(OBJECT_ID('CHK_Reparacion_Estado'))

go
ALTER TABLE Reparacion DROP CONSTRAINT CHK_Reparacion_Estado;

ALTER TABLE Reparacion
ADD CONSTRAINT CHK_Reparacion_Estado CHECK (Estado_Final IN ('Finalizado', 'Entregado'));


