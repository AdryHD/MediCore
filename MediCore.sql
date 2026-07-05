USE [master]
GO

CREATE DATABASE [MediCore]

USE [MediCore]
GO

GO
CREATE TABLE [dbo].[tbUsuario](
	[Consecutivo] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](250) NOT NULL,
	[Cedula] [varchar](15) NOT NULL,
	[FechaNacimiento] [datetime] NOT NULL,
	[Telefono] [varchar](20) NOT NULL,
	[Correo] [varchar](100) NOT NULL,
	[Contrasenna] [varchar](10) NOT NULL,
	[Estado] [bit] NOT NULL,
 CONSTRAINT [PK_tbUsuario] PRIMARY KEY CLUSTERED 
(
	[Consecutivo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE PROCEDURE [dbo].[sp_RegistrarUsuario]
	   @Nombre varchar(250),
       @Cedula varchar(15),
       @FechaNacimiento datetime,
       @Telefono varchar(20),
       @Correo varchar(100),
       @Contrasenna varchar(10)
AS
BEGIN

Declare @vEstado BIT= 1

INSERT INTO dbo.tbUsuario
           (Nombre,
           Cedula,
           FechaNacimiento,
           Telefono,
           Correo,
           Contrasenna,
           Estado)
     VALUES
           (@Nombre,
           @Cedula,
           @FechaNacimiento,
           @Telefono,
           @Correo,
           @Contrasenna,
           @vEstado);

END
GO
USE [master]
GO
ALTER DATABASE [MediCore] SET  READ_WRITE 
GO
