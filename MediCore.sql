USE [master]
GO

CREATE DATABASE [MediCore]

USE [MediCore]
GO

GO
CREATE TABLE [dbo].[tbRol](
	[id_rol] [int] IDENTITY(1,1) NOT NULL,
	[nombre_rol] [varchar](30) NOT NULL,
 CONSTRAINT [PK_tbRol] PRIMARY KEY CLUSTERED
(
	[id_rol] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[tbRol] ADD CONSTRAINT [UQ_tbRol_NombreRol] UNIQUE NONCLUSTERED ([nombre_rol] ASC)
GO

INSERT INTO dbo.tbRol (nombre_rol) VALUES ('ADMINISTRADOR')
GO
INSERT INTO dbo.tbRol (nombre_rol) VALUES ('DOCTOR')
GO
INSERT INTO dbo.tbRol (nombre_rol) VALUES ('RECEPCIONISTA')
GO

CREATE TABLE [dbo].[tbUsuario](
	[Consecutivo] [int] IDENTITY(1,1) NOT NULL,
	[id_rol] [int] NULL,
	[Nombre] [varchar](250) NOT NULL,
	[Cedula] [varchar](15) NOT NULL,
	[FechaNacimiento] [datetime] NULL,
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

ALTER TABLE [dbo].[tbUsuario]  WITH CHECK ADD  CONSTRAINT [FK_tbUsuario_tbRol] FOREIGN KEY([id_rol])
REFERENCES [dbo].[tbRol] ([id_rol])
GO
ALTER TABLE [dbo].[tbUsuario] CHECK CONSTRAINT [FK_tbUsuario_tbRol]
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
           (id_rol,
           Nombre,
           Cedula,
           FechaNacimiento,
           Telefono,
           Correo,
           Contrasenna,
           Estado)
     VALUES
		     (NULL,
           @Nombre,
           @Cedula,
           @FechaNacimiento,
           @Telefono,
           @Correo,
           @Contrasenna,
           @vEstado);

END
GO

CREATE TABLE [dbo].[Especialidades](
	[id_especialidad] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [nvarchar](80) NOT NULL,
	[descripcion] [nvarchar](255) NULL,
	[estado] [varchar](10) NOT NULL,
	[fecha_creacion] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Especialidades] PRIMARY KEY CLUSTERED
(
	[id_especialidad] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Especialidades] ADD CONSTRAINT [UQ_Especialidades_Nombre] UNIQUE NONCLUSTERED ([nombre] ASC)
GO
ALTER TABLE [dbo].[Especialidades] ADD CONSTRAINT [DF_Especialidades_Estado] DEFAULT ('ACTIVO') FOR [estado]
GO
ALTER TABLE [dbo].[Especialidades] ADD CONSTRAINT [DF_Especialidades_FechaCreacion] DEFAULT (getdate()) FOR [fecha_creacion]
GO
ALTER TABLE [dbo].[Especialidades] ADD CONSTRAINT [CK_Especialidades_Estado] CHECK ([estado] IN ('ACTIVO','INACTIVO'))
GO

CREATE TABLE [dbo].[Doctores](
	[id_doctor] [int] IDENTITY(1,1) NOT NULL,
	[id_usuario] [int] NULL,
	[id_especialidad] [int] NOT NULL,
	[nombre_completo] [nvarchar](150) NOT NULL,
	[cedula] [nvarchar](20) NOT NULL,
	[codigo_colegiado] [nvarchar](30) NOT NULL,
	[telefono] [nvarchar](20) NULL,
	[correo] [nvarchar](150) NOT NULL,
	[estado] [varchar](10) NOT NULL,
 CONSTRAINT [PK_Doctores] PRIMARY KEY CLUSTERED
(
	[id_doctor] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Doctores] ADD CONSTRAINT [UQ_Doctores_Cedula] UNIQUE NONCLUSTERED ([cedula] ASC)
GO
ALTER TABLE [dbo].[Doctores] ADD CONSTRAINT [UQ_Doctores_CodigoColegiado] UNIQUE NONCLUSTERED ([codigo_colegiado] ASC)
GO
ALTER TABLE [dbo].[Doctores] ADD CONSTRAINT [UQ_Doctores_Correo] UNIQUE NONCLUSTERED ([correo] ASC)
GO
ALTER TABLE [dbo].[Doctores] ADD CONSTRAINT [DF_Doctores_Estado] DEFAULT ('ACTIVO') FOR [estado]
GO
ALTER TABLE [dbo].[Doctores] ADD CONSTRAINT [CK_Doctores_Estado] CHECK ([estado] IN ('ACTIVO','INACTIVO'))
GO

CREATE TABLE [dbo].[Pacientes](
	[id_paciente] [int] IDENTITY(1,1) NOT NULL,
	[id_usuario] [int] NOT NULL,
	[nombre_completo] [nvarchar](150) NOT NULL,
	[cedula] [nvarchar](20) NOT NULL,
	[fecha_nacimiento] [date] NOT NULL,
	[sexo] [varchar](10) NOT NULL,
	[telefono] [nvarchar](20) NULL,
	[direccion] [nvarchar](255) NULL,
	[estado] [varchar](10) NOT NULL,
	[fecha_registro] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Pacientes] PRIMARY KEY CLUSTERED
(
	[id_paciente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Pacientes] ADD CONSTRAINT [UQ_Pacientes_IdUsuario] UNIQUE NONCLUSTERED ([id_usuario] ASC)
GO
ALTER TABLE [dbo].[Pacientes] ADD CONSTRAINT [UQ_Pacientes_Cedula] UNIQUE NONCLUSTERED ([cedula] ASC)
GO
ALTER TABLE [dbo].[Pacientes] ADD CONSTRAINT [DF_Pacientes_Estado] DEFAULT ('ACTIVO') FOR [estado]
GO
ALTER TABLE [dbo].[Pacientes] ADD CONSTRAINT [DF_Pacientes_FechaRegistro] DEFAULT (getdate()) FOR [fecha_registro]
GO
ALTER TABLE [dbo].[Pacientes] ADD CONSTRAINT [CK_Pacientes_Sexo] CHECK ([sexo] IN ('M','F','OTRO'))
GO
ALTER TABLE [dbo].[Pacientes] ADD CONSTRAINT [CK_Pacientes_Estado] CHECK ([estado] IN ('ACTIVO','INACTIVO'))
GO

CREATE TABLE [dbo].[HorariosMedicos](
	[id_horario] [int] IDENTITY(1,1) NOT NULL,
	[id_doctor] [int] NOT NULL,
	[dia_semana] [tinyint] NOT NULL,
	[hora_inicio] [time](7) NOT NULL,
	[hora_fin] [time](7) NOT NULL,
	[duracion_cita_min] [int] NOT NULL,
	[estado] [varchar](10) NOT NULL,
 CONSTRAINT [PK_HorariosMedicos] PRIMARY KEY CLUSTERED
(
	[id_horario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[HorariosMedicos] ADD CONSTRAINT [DF_HorariosMedicos_Estado] DEFAULT ('ACTIVO') FOR [estado]
GO
ALTER TABLE [dbo].[HorariosMedicos] ADD CONSTRAINT [CK_HorariosMedicos_DiaSemana] CHECK ([dia_semana] BETWEEN 1 AND 7)
GO
ALTER TABLE [dbo].[HorariosMedicos] ADD CONSTRAINT [CK_HorariosMedicos_Horas] CHECK ([hora_fin] > [hora_inicio])
GO
ALTER TABLE [dbo].[HorariosMedicos] ADD CONSTRAINT [CK_HorariosMedicos_DuracionMinima] CHECK ([duracion_cita_min] >= 15)
GO
ALTER TABLE [dbo].[HorariosMedicos] ADD CONSTRAINT [CK_HorariosMedicos_Estado] CHECK ([estado] IN ('ACTIVO','INACTIVO'))
GO

CREATE TABLE [dbo].[Citas](
	[id_cita] [int] IDENTITY(1,1) NOT NULL,
	[id_paciente] [int] NOT NULL,
	[id_doctor] [int] NOT NULL,
	[fecha_cita] [datetime2](7) NOT NULL,
	[duracion_min] [int] NOT NULL,
	[motivo] [nvarchar](255) NULL,
	[estado] [varchar](20) NOT NULL,
	[id_cita_anterior] [int] NULL,
	[fecha_creacion] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Citas] PRIMARY KEY CLUSTERED
(
	[id_cita] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Citas] ADD CONSTRAINT [DF_Citas_Estado] DEFAULT ('PENDIENTE') FOR [estado]
GO
ALTER TABLE [dbo].[Citas] ADD CONSTRAINT [DF_Citas_FechaCreacion] DEFAULT (getdate()) FOR [fecha_creacion]
GO
ALTER TABLE [dbo].[Citas] ADD CONSTRAINT [CK_Citas_Estado] CHECK ([estado] IN ('PENDIENTE','PROGRAMADA','CONFIRMADA','REPROGRAMADA','CANCELADA','ATENDIDA'))
GO

CREATE TABLE [dbo].[Expedientes](
	[id_expediente] [int] IDENTITY(1,1) NOT NULL,
	[id_paciente] [int] NOT NULL,
	[alergias] [nvarchar](max) NULL,
	[antecedentes] [nvarchar](max) NULL,
	[tipo_sangre] [varchar](5) NULL,
	[fecha_apertura] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Expedientes] PRIMARY KEY CLUSTERED
(
	[id_expediente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Expedientes] ADD CONSTRAINT [UQ_Expedientes_IdPaciente] UNIQUE NONCLUSTERED ([id_paciente] ASC)
GO
ALTER TABLE [dbo].[Expedientes] ADD CONSTRAINT [DF_Expedientes_FechaApertura] DEFAULT (getdate()) FOR [fecha_apertura]
GO

CREATE TABLE [dbo].[HistorialMedico](
	[id_historial] [int] IDENTITY(1,1) NOT NULL,
	[id_expediente] [int] NOT NULL,
	[id_cita] [int] NULL,
	[id_doctor] [int] NOT NULL,
	[fecha_consulta] [datetime2](7) NOT NULL,
	[sintomas] [nvarchar](max) NULL,
	[diagnostico] [nvarchar](max) NOT NULL,
	[tratamiento] [nvarchar](max) NULL,
	[observaciones] [nvarchar](max) NULL,
 CONSTRAINT [PK_HistorialMedico] PRIMARY KEY CLUSTERED
(
	[id_historial] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[HistorialMedico] ADD CONSTRAINT [DF_HistorialMedico_FechaConsulta] DEFAULT (getdate()) FOR [fecha_consulta]
GO

CREATE TABLE [dbo].[Archivos](
	[id_archivo] [int] IDENTITY(1,1) NOT NULL,
	[id_expediente] [int] NULL,
	[id_usuario] [int] NULL,
	[nombre] [nvarchar](255) NOT NULL,
	[tipo_mime] [varchar](100) NOT NULL,
	[tamano_bytes] [bigint] NOT NULL,
	[contenido] [varbinary](max) NOT NULL,
	[estado] [varchar](10) NOT NULL,
	[fecha_carga] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Archivos] PRIMARY KEY CLUSTERED
(
	[id_archivo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Archivos] ADD CONSTRAINT [DF_Archivos_Estado] DEFAULT ('ACTIVO') FOR [estado]
GO
ALTER TABLE [dbo].[Archivos] ADD CONSTRAINT [DF_Archivos_FechaCarga] DEFAULT (getdate()) FOR [fecha_carga]
GO
ALTER TABLE [dbo].[Archivos] ADD CONSTRAINT [CK_Archivos_Estado] CHECK ([estado] IN ('ACTIVO','INACTIVO'))
GO

CREATE TABLE [dbo].[Bitacora](
	[id_bitacora] [bigint] IDENTITY(1,1) NOT NULL,
	[fecha] [datetime2](7) NOT NULL,
	[nivel] [varchar](10) NOT NULL,
	[id_usuario] [int] NULL,
	[controlador] [varchar](100) NOT NULL,
	[accion] [varchar](100) NOT NULL,
	[mensaje] [nvarchar](max) NOT NULL,
	[stack_trace] [nvarchar](max) NULL,
	[ip_origen] [varchar](45) NULL,
 CONSTRAINT [PK_Bitacora] PRIMARY KEY CLUSTERED
(
	[id_bitacora] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Bitacora] ADD CONSTRAINT [DF_Bitacora_Fecha] DEFAULT (getdate()) FOR [fecha]
GO
ALTER TABLE [dbo].[Bitacora] ADD CONSTRAINT [CK_Bitacora_Nivel] CHECK ([nivel] IN ('INFO','WARN','ERROR'))
GO

CREATE TABLE [dbo].[Notificaciones](
	[id_notificacion] [int] IDENTITY(1,1) NOT NULL,
	[id_usuario_destino] [int] NULL,
	[correo_destino] [nvarchar](150) NOT NULL,
	[tipo] [varchar](30) NOT NULL,
	[asunto] [nvarchar](150) NOT NULL,
	[cuerpo] [nvarchar](max) NOT NULL,
	[estado] [varchar](10) NOT NULL,
	[fecha_envio] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Notificaciones] PRIMARY KEY CLUSTERED
(
	[id_notificacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Notificaciones] ADD CONSTRAINT [DF_Notificaciones_FechaEnvio] DEFAULT (getdate()) FOR [fecha_envio]
GO
ALTER TABLE [dbo].[Notificaciones] ADD CONSTRAINT [CK_Notificaciones_Tipo] CHECK ([tipo] IN ('REGISTRO','CITA_PROGRAMADA','CITA_CANCELADA','RECUPERACION'))
GO
ALTER TABLE [dbo].[Notificaciones] ADD CONSTRAINT [CK_Notificaciones_Estado] CHECK ([estado] IN ('ENVIADO','FALLIDO'))
GO

ALTER TABLE [dbo].[Doctores]  WITH CHECK ADD  CONSTRAINT [FK_Doctores_tbUsuario] FOREIGN KEY([id_usuario])
REFERENCES [dbo].[tbUsuario] ([Consecutivo])
GO
ALTER TABLE [dbo].[Doctores] CHECK CONSTRAINT [FK_Doctores_tbUsuario]
GO

ALTER TABLE [dbo].[Doctores]  WITH CHECK ADD  CONSTRAINT [FK_Doctores_Especialidades] FOREIGN KEY([id_especialidad])
REFERENCES [dbo].[Especialidades] ([id_especialidad])
GO
ALTER TABLE [dbo].[Doctores] CHECK CONSTRAINT [FK_Doctores_Especialidades]
GO

ALTER TABLE [dbo].[Pacientes]  WITH CHECK ADD  CONSTRAINT [FK_Pacientes_tbUsuario] FOREIGN KEY([id_usuario])
REFERENCES [dbo].[tbUsuario] ([Consecutivo])
GO
ALTER TABLE [dbo].[Pacientes] CHECK CONSTRAINT [FK_Pacientes_tbUsuario]
GO

ALTER TABLE [dbo].[HorariosMedicos]  WITH CHECK ADD  CONSTRAINT [FK_HorariosMedicos_Doctores] FOREIGN KEY([id_doctor])
REFERENCES [dbo].[Doctores] ([id_doctor])
GO
ALTER TABLE [dbo].[HorariosMedicos] CHECK CONSTRAINT [FK_HorariosMedicos_Doctores]
GO

ALTER TABLE [dbo].[Citas]  WITH CHECK ADD  CONSTRAINT [FK_Citas_Pacientes] FOREIGN KEY([id_paciente])
REFERENCES [dbo].[Pacientes] ([id_paciente])
GO
ALTER TABLE [dbo].[Citas] CHECK CONSTRAINT [FK_Citas_Pacientes]
GO

ALTER TABLE [dbo].[Citas]  WITH CHECK ADD  CONSTRAINT [FK_Citas_Doctores] FOREIGN KEY([id_doctor])
REFERENCES [dbo].[Doctores] ([id_doctor])
GO
ALTER TABLE [dbo].[Citas] CHECK CONSTRAINT [FK_Citas_Doctores]
GO

ALTER TABLE [dbo].[Citas]  WITH CHECK ADD  CONSTRAINT [FK_Citas_CitaAnterior] FOREIGN KEY([id_cita_anterior])
REFERENCES [dbo].[Citas] ([id_cita])
GO
ALTER TABLE [dbo].[Citas] CHECK CONSTRAINT [FK_Citas_CitaAnterior]
GO

ALTER TABLE [dbo].[Expedientes]  WITH CHECK ADD  CONSTRAINT [FK_Expedientes_Pacientes] FOREIGN KEY([id_paciente])
REFERENCES [dbo].[Pacientes] ([id_paciente])
GO
ALTER TABLE [dbo].[Expedientes] CHECK CONSTRAINT [FK_Expedientes_Pacientes]
GO

ALTER TABLE [dbo].[HistorialMedico]  WITH CHECK ADD  CONSTRAINT [FK_HistorialMedico_Expedientes] FOREIGN KEY([id_expediente])
REFERENCES [dbo].[Expedientes] ([id_expediente])
GO
ALTER TABLE [dbo].[HistorialMedico] CHECK CONSTRAINT [FK_HistorialMedico_Expedientes]
GO

ALTER TABLE [dbo].[HistorialMedico]  WITH CHECK ADD  CONSTRAINT [FK_HistorialMedico_Citas] FOREIGN KEY([id_cita])
REFERENCES [dbo].[Citas] ([id_cita])
GO
ALTER TABLE [dbo].[HistorialMedico] CHECK CONSTRAINT [FK_HistorialMedico_Citas]
GO

ALTER TABLE [dbo].[HistorialMedico]  WITH CHECK ADD  CONSTRAINT [FK_HistorialMedico_Doctores] FOREIGN KEY([id_doctor])
REFERENCES [dbo].[Doctores] ([id_doctor])
GO
ALTER TABLE [dbo].[HistorialMedico] CHECK CONSTRAINT [FK_HistorialMedico_Doctores]
GO

ALTER TABLE [dbo].[Archivos]  WITH CHECK ADD  CONSTRAINT [FK_Archivos_Expedientes] FOREIGN KEY([id_expediente])
REFERENCES [dbo].[Expedientes] ([id_expediente])
GO
ALTER TABLE [dbo].[Archivos] CHECK CONSTRAINT [FK_Archivos_Expedientes]
GO

ALTER TABLE [dbo].[Archivos]  WITH CHECK ADD  CONSTRAINT [FK_Archivos_tbUsuario] FOREIGN KEY([id_usuario])
REFERENCES [dbo].[tbUsuario] ([Consecutivo])
GO
ALTER TABLE [dbo].[Archivos] CHECK CONSTRAINT [FK_Archivos_tbUsuario]
GO

ALTER TABLE [dbo].[Bitacora]  WITH CHECK ADD  CONSTRAINT [FK_Bitacora_tbUsuario] FOREIGN KEY([id_usuario])
REFERENCES [dbo].[tbUsuario] ([Consecutivo])
GO
ALTER TABLE [dbo].[Bitacora] CHECK CONSTRAINT [FK_Bitacora_tbUsuario]
GO

ALTER TABLE [dbo].[Notificaciones]  WITH CHECK ADD  CONSTRAINT [FK_Notificaciones_tbUsuario] FOREIGN KEY([id_usuario_destino])
REFERENCES [dbo].[tbUsuario] ([Consecutivo])
GO
ALTER TABLE [dbo].[Notificaciones] CHECK CONSTRAINT [FK_Notificaciones_tbUsuario]
GO

CREATE PROCEDURE [dbo].[spRegistrarBitacora]
	@Nivel			varchar(10),
	@IdUsuario		int = NULL,
	@Controlador	varchar(100),
	@Accion			varchar(100),
	@Mensaje		nvarchar(max),
	@StackTrace		nvarchar(max) = NULL,
	@IpOrigen		varchar(45) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO dbo.Bitacora (fecha, nivel, id_usuario, controlador, accion, mensaje, stack_trace, ip_origen)
	VALUES (GETDATE(), @Nivel, @IdUsuario, @Controlador, @Accion, @Mensaje, @StackTrace, @IpOrigen);
END
GO

CREATE PROCEDURE [dbo].[spCambiarEstadoEspecialidad]
	@IdEspecialidad	int,
	@NuevoEstado	varchar(10),
	@IdUsuario		int = NULL
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Resultado int = 0;

	IF NOT EXISTS (SELECT 1 FROM dbo.Especialidades WHERE id_especialidad = @IdEspecialidad)
	BEGIN
		SET @Resultado = 2;
	END
	ELSE IF (@NuevoEstado = 'INACTIVO' AND EXISTS (
				SELECT 1 FROM dbo.Doctores
				WHERE id_especialidad = @IdEspecialidad AND estado = 'ACTIVO'))
	BEGIN
		SET @Resultado = 1;
	END
	ELSE
	BEGIN
		UPDATE dbo.Especialidades
		SET estado = @NuevoEstado
		WHERE id_especialidad = @IdEspecialidad;

		SET @Resultado = 0;
	END

	SELECT @Resultado AS Resultado;
END
GO

CREATE PROCEDURE [dbo].[spRegistrarDoctor]
	@NombreCompleto		nvarchar(150),
	@Cedula				nvarchar(20),
	@CodigoColegiado	nvarchar(30),
	@Correo				nvarchar(150),
	@Telefono			nvarchar(20) = NULL,
	@IdEspecialidad		int,
	@Contrasenna		varchar(10)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Resultado int = 0;
	DECLARE @IdRolDoctor int;
	DECLARE @IdUsuario int;

	SELECT @IdRolDoctor = id_rol FROM dbo.tbRol WHERE nombre_rol = 'DOCTOR';

	IF NOT EXISTS (SELECT 1 FROM dbo.Especialidades WHERE id_especialidad = @IdEspecialidad AND estado = 'ACTIVO')
	BEGIN
		SET @Resultado = 1;
	END
	ELSE IF EXISTS (SELECT 1 FROM dbo.Doctores WHERE cedula = @Cedula)
	BEGIN
		SET @Resultado = 2;
	END
	ELSE IF EXISTS (SELECT 1 FROM dbo.Doctores WHERE codigo_colegiado = @CodigoColegiado)
	BEGIN
		SET @Resultado = 3;
	END
	ELSE IF EXISTS (SELECT 1 FROM dbo.Doctores WHERE correo = @Correo)
			OR EXISTS (SELECT 1 FROM dbo.tbUsuario WHERE Correo = @Correo)
	BEGIN
		SET @Resultado = 4;
	END
	ELSE
	BEGIN
		BEGIN TRY
			BEGIN TRANSACTION;

			INSERT INTO dbo.tbUsuario (id_rol, Nombre, Cedula, FechaNacimiento, Telefono, Correo, Contrasenna, Estado)
			VALUES (@IdRolDoctor, @NombreCompleto, @Cedula, NULL, ISNULL(@Telefono, ''), @Correo, @Contrasenna, 1);

			SET @IdUsuario = SCOPE_IDENTITY();

			INSERT INTO dbo.Doctores (id_usuario, id_especialidad, nombre_completo, cedula, codigo_colegiado, telefono, correo, estado)
			VALUES (@IdUsuario, @IdEspecialidad, @NombreCompleto, @Cedula, @CodigoColegiado, @Telefono, @Correo, 'ACTIVO');

			COMMIT TRANSACTION;
		END TRY
		BEGIN CATCH
			IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
			SET @Resultado = 99;
		END CATCH
	END

	SELECT @Resultado AS Resultado;
END
GO

CREATE PROCEDURE [dbo].[spCambiarEstadoDoctor]
	@IdDoctor		int,
	@NuevoEstado	varchar(10),
	@IdUsuario		int = NULL
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Resultado int = 0;
	DECLARE @IdUsuarioDoctor int;

	IF NOT EXISTS (SELECT 1 FROM dbo.Doctores WHERE id_doctor = @IdDoctor)
	BEGIN
		SET @Resultado = 2;
	END
	ELSE
	BEGIN
		SELECT @IdUsuarioDoctor = id_usuario FROM dbo.Doctores WHERE id_doctor = @IdDoctor;

		UPDATE dbo.Doctores
		SET estado = @NuevoEstado
		WHERE id_doctor = @IdDoctor;

		IF @IdUsuarioDoctor IS NOT NULL
		BEGIN
			UPDATE dbo.tbUsuario
			SET Estado = CASE WHEN @NuevoEstado = 'ACTIVO' THEN 1 ELSE 0 END
			WHERE Consecutivo = @IdUsuarioDoctor;
		END

		SET @Resultado = 0;
	END

	SELECT @Resultado AS Resultado;
END
GO

CREATE PROCEDURE [dbo].[spValidarDisponibilidadCita]
	@IdDoctor		int,
	@FechaCita		datetime2,
	@DuracionMin	int,
	@IdCitaExcluir	int = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT COUNT(1) AS CantidadTraslapes
	FROM dbo.Citas
	WHERE id_doctor = @IdDoctor
		AND estado NOT IN ('CANCELADA', 'REPROGRAMADA')
		AND (@IdCitaExcluir IS NULL OR id_cita <> @IdCitaExcluir)
		AND fecha_cita < DATEADD(MINUTE, @DuracionMin, @FechaCita)
		AND DATEADD(MINUTE, duracion_min, fecha_cita) > @FechaCita;
END
GO

USE [master]
GO
ALTER DATABASE [MediCore] SET  READ_WRITE
GO
