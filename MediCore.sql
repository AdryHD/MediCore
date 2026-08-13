USE [master]
GO

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'MediCore')
BEGIN
	CREATE DATABASE [MediCore]
END
GO

USE [MediCore]
GO

IF OBJECT_ID(N'dbo.tbRol', N'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[tbRol](
		[id_rol] [int] IDENTITY(1,1) NOT NULL,
		[nombre_rol] [varchar](30) NOT NULL,
	 CONSTRAINT [PK_tbRol] PRIMARY KEY CLUSTERED
	(
		[id_rol] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]
END
GO

IF OBJECT_ID(N'dbo.UQ_tbRol_NombreRol', N'UQ') IS NULL
BEGIN
	ALTER TABLE [dbo].[tbRol] ADD CONSTRAINT [UQ_tbRol_NombreRol] UNIQUE NONCLUSTERED ([nombre_rol] ASC)
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbRol WHERE nombre_rol = 'ADMINISTRADOR')
BEGIN
	INSERT INTO dbo.tbRol (nombre_rol) VALUES ('ADMINISTRADOR')
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.tbRol WHERE nombre_rol = 'DOCTOR')
BEGIN
	INSERT INTO dbo.tbRol (nombre_rol) VALUES ('DOCTOR')
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.tbRol WHERE nombre_rol = 'RECEPCIONISTA')
BEGIN
	INSERT INTO dbo.tbRol (nombre_rol) VALUES ('RECEPCIONISTA')
END
GO

IF OBJECT_ID(N'dbo.tbUsuario', N'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[tbUsuario](
		[Consecutivo] [int] IDENTITY(1,1) NOT NULL,
		[id_rol] [int] NOT NULL,
		[Nombre] [varchar](250) NOT NULL,
		[Cedula] [varchar](15) NOT NULL,
		[FechaNacimiento] [datetime] NULL,
		[Telefono] [varchar](20) NOT NULL,
		[Correo] [varchar](100) NOT NULL,
		[Contrasenna] [varchar](10) NOT NULL,
		[Estado] [bit] NOT NULL,
		[FechaExpiracionTemp] [datetime] NULL,
	 CONSTRAINT [PK_tbUsuario] PRIMARY KEY CLUSTERED 
	(
		[Consecutivo] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]
END
GO

IF OBJECT_ID(N'dbo.UQ_tbUsuario_Correo', N'UQ') IS NULL
BEGIN
	ALTER TABLE [dbo].[tbUsuario] ADD CONSTRAINT [UQ_tbUsuario_Correo] UNIQUE NONCLUSTERED ([Correo] ASC)
END
GO
IF OBJECT_ID(N'dbo.UQ_tbUsuario_Cedula', N'UQ') IS NULL
BEGIN
	ALTER TABLE [dbo].[tbUsuario] ADD CONSTRAINT [UQ_tbUsuario_Cedula] UNIQUE NONCLUSTERED ([Cedula] ASC)
END
GO

IF OBJECT_ID(N'dbo.FK_tbUsuario_tbRol', N'F') IS NULL
BEGIN
	ALTER TABLE [dbo].[tbUsuario]  WITH CHECK ADD  CONSTRAINT [FK_tbUsuario_tbRol] FOREIGN KEY([id_rol])
	REFERENCES [dbo].[tbRol] ([id_rol])

	ALTER TABLE [dbo].[tbUsuario] CHECK CONSTRAINT [FK_tbUsuario_tbRol]
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.tbUsuario') AND name = 'id_rol' AND is_nullable = 1)
BEGIN
	DECLARE @IdRolRecepcionista INT;
	SELECT @IdRolRecepcionista = id_rol FROM dbo.tbRol WHERE nombre_rol = 'RECEPCIONISTA';

	UPDATE dbo.tbUsuario SET id_rol = @IdRolRecepcionista WHERE id_rol IS NULL;

	ALTER TABLE [dbo].[tbUsuario] ALTER COLUMN [id_rol] [int] NOT NULL
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbUsuario WHERE Correo = 'admin@medicore.com')
BEGIN
	DECLARE @IdRolAdmin INT;
	SELECT @IdRolAdmin = id_rol FROM dbo.tbRol WHERE nombre_rol = 'ADMINISTRADOR';

	INSERT INTO dbo.tbUsuario (id_rol, Nombre, Cedula, FechaNacimiento, Telefono, Correo, Contrasenna, Estado)
	VALUES (@IdRolAdmin, 'Administrador', '000000000', NULL, '00000000', 'admin@medicore.com', 'Admin123', 1)
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_RegistrarUsuario]
	   @Nombre varchar(250),
       @Cedula varchar(15),
       @FechaNacimiento datetime,
       @Telefono varchar(20),
       @Correo varchar(100),
       @Contrasenna varchar(10),
       @IdRol int
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
		     (@IdRol,
           @Nombre,
           @Cedula,
           @FechaNacimiento,
           @Telefono,
           @Correo,
           @Contrasenna,
           @vEstado);

END
GO

IF OBJECT_ID(N'dbo.Especialidades', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'dbo.UQ_Especialidades_Nombre', N'UQ') IS NULL
BEGIN
	ALTER TABLE [dbo].[Especialidades] ADD CONSTRAINT [UQ_Especialidades_Nombre] UNIQUE NONCLUSTERED ([nombre] ASC)
END
GO
IF OBJECT_ID(N'dbo.DF_Especialidades_Estado', N'D') IS NULL
BEGIN
	ALTER TABLE [dbo].[Especialidades] ADD CONSTRAINT [DF_Especialidades_Estado] DEFAULT ('ACTIVO') FOR [estado]
END
GO
IF OBJECT_ID(N'dbo.DF_Especialidades_FechaCreacion', N'D') IS NULL
BEGIN
	ALTER TABLE [dbo].[Especialidades] ADD CONSTRAINT [DF_Especialidades_FechaCreacion] DEFAULT (getdate()) FOR [fecha_creacion]
END
GO
IF OBJECT_ID(N'dbo.CK_Especialidades_Estado', N'C') IS NULL
BEGIN
	ALTER TABLE [dbo].[Especialidades] ADD CONSTRAINT [CK_Especialidades_Estado] CHECK ([estado] IN ('ACTIVO','INACTIVO'))
END
GO

IF OBJECT_ID(N'dbo.Doctores', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'dbo.UQ_Doctores_Cedula', N'UQ') IS NULL
BEGIN
	ALTER TABLE [dbo].[Doctores] ADD CONSTRAINT [UQ_Doctores_Cedula] UNIQUE NONCLUSTERED ([cedula] ASC)
END
GO
IF OBJECT_ID(N'dbo.UQ_Doctores_CodigoColegiado', N'UQ') IS NULL
BEGIN
	ALTER TABLE [dbo].[Doctores] ADD CONSTRAINT [UQ_Doctores_CodigoColegiado] UNIQUE NONCLUSTERED ([codigo_colegiado] ASC)
END
GO
IF OBJECT_ID(N'dbo.UQ_Doctores_Correo', N'UQ') IS NULL
BEGIN
	ALTER TABLE [dbo].[Doctores] ADD CONSTRAINT [UQ_Doctores_Correo] UNIQUE NONCLUSTERED ([correo] ASC)
END
GO
IF OBJECT_ID(N'dbo.DF_Doctores_Estado', N'D') IS NULL
BEGIN
	ALTER TABLE [dbo].[Doctores] ADD CONSTRAINT [DF_Doctores_Estado] DEFAULT ('ACTIVO') FOR [estado]
END
GO
IF OBJECT_ID(N'dbo.CK_Doctores_Estado', N'C') IS NULL
BEGIN
	ALTER TABLE [dbo].[Doctores] ADD CONSTRAINT [CK_Doctores_Estado] CHECK ([estado] IN ('ACTIVO','INACTIVO'))
END
GO

IF OBJECT_ID(N'dbo.Pacientes', N'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[Pacientes](
		[id_paciente] [int] IDENTITY(1,1) NOT NULL,
		[nombre_completo] [nvarchar](150) NOT NULL,
		[cedula] [nvarchar](20) NOT NULL,
		[fecha_nacimiento] [date] NOT NULL,
		[sexo] [varchar](10) NOT NULL,
		[telefono] [nvarchar](20) NULL,
		[correo] [nvarchar](150) NOT NULL,
		[direccion] [nvarchar](255) NULL,
		[estado] [varchar](10) NOT NULL,
		[fecha_registro] [datetime2](7) NOT NULL,
	 CONSTRAINT [PK_Pacientes] PRIMARY KEY CLUSTERED
	(
		[id_paciente] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]
END
GO

IF OBJECT_ID(N'dbo.UQ_Pacientes_Cedula', N'UQ') IS NULL
BEGIN
	ALTER TABLE [dbo].[Pacientes] ADD CONSTRAINT [UQ_Pacientes_Cedula] UNIQUE NONCLUSTERED ([cedula] ASC)
END
GO
IF OBJECT_ID(N'dbo.DF_Pacientes_Estado', N'D') IS NULL
BEGIN
	ALTER TABLE [dbo].[Pacientes] ADD CONSTRAINT [DF_Pacientes_Estado] DEFAULT ('ACTIVO') FOR [estado]
END
GO
IF OBJECT_ID(N'dbo.DF_Pacientes_FechaRegistro', N'D') IS NULL
BEGIN
	ALTER TABLE [dbo].[Pacientes] ADD CONSTRAINT [DF_Pacientes_FechaRegistro] DEFAULT (getdate()) FOR [fecha_registro]
END
GO
IF OBJECT_ID(N'dbo.CK_Pacientes_Sexo', N'C') IS NULL
BEGIN
	ALTER TABLE [dbo].[Pacientes] ADD CONSTRAINT [CK_Pacientes_Sexo] CHECK ([sexo] IN ('M','F','OTRO'))
END
GO
IF OBJECT_ID(N'dbo.CK_Pacientes_Estado', N'C') IS NULL
BEGIN
	ALTER TABLE [dbo].[Pacientes] ADD CONSTRAINT [CK_Pacientes_Estado] CHECK ([estado] IN ('ACTIVO','INACTIVO'))
END
GO

IF COL_LENGTH('dbo.Pacientes', 'id_usuario') IS NOT NULL
BEGIN
	IF OBJECT_ID(N'dbo.FK_Pacientes_tbUsuario', N'F') IS NOT NULL
		ALTER TABLE [dbo].[Pacientes] DROP CONSTRAINT [FK_Pacientes_tbUsuario]

	IF OBJECT_ID(N'dbo.UQ_Pacientes_IdUsuario', N'UQ') IS NOT NULL
		ALTER TABLE [dbo].[Pacientes] DROP CONSTRAINT [UQ_Pacientes_IdUsuario]

	ALTER TABLE [dbo].[Pacientes] DROP COLUMN [id_usuario]
END
GO

IF COL_LENGTH('dbo.Pacientes', 'correo') IS NULL
BEGIN
	ALTER TABLE [dbo].[Pacientes] ADD [correo] [nvarchar](150) NOT NULL CONSTRAINT [DF_Pacientes_Correo_Temp] DEFAULT ('')

	ALTER TABLE [dbo].[Pacientes] DROP CONSTRAINT [DF_Pacientes_Correo_Temp]
END
GO

IF OBJECT_ID(N'dbo.UQ_Pacientes_Correo', N'UQ') IS NULL
BEGIN
	ALTER TABLE [dbo].[Pacientes] ADD CONSTRAINT [UQ_Pacientes_Correo] UNIQUE NONCLUSTERED ([correo] ASC)
END
GO

IF OBJECT_ID(N'dbo.HorariosMedicos', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'dbo.DF_HorariosMedicos_Estado', N'D') IS NULL
BEGIN
	ALTER TABLE [dbo].[HorariosMedicos] ADD CONSTRAINT [DF_HorariosMedicos_Estado] DEFAULT ('ACTIVO') FOR [estado]
END
GO
IF OBJECT_ID(N'dbo.CK_HorariosMedicos_DiaSemana', N'C') IS NULL
BEGIN
	ALTER TABLE [dbo].[HorariosMedicos] ADD CONSTRAINT [CK_HorariosMedicos_DiaSemana] CHECK ([dia_semana] BETWEEN 1 AND 7)
END
GO
IF OBJECT_ID(N'dbo.CK_HorariosMedicos_Horas', N'C') IS NULL
BEGIN
	ALTER TABLE [dbo].[HorariosMedicos] ADD CONSTRAINT [CK_HorariosMedicos_Horas] CHECK ([hora_fin] > [hora_inicio])
END
GO
IF OBJECT_ID(N'dbo.CK_HorariosMedicos_DuracionMinima', N'C') IS NULL
BEGIN
	ALTER TABLE [dbo].[HorariosMedicos] ADD CONSTRAINT [CK_HorariosMedicos_DuracionMinima] CHECK ([duracion_cita_min] >= 15)
END
GO
IF OBJECT_ID(N'dbo.CK_HorariosMedicos_Estado', N'C') IS NULL
BEGIN
	ALTER TABLE [dbo].[HorariosMedicos] ADD CONSTRAINT [CK_HorariosMedicos_Estado] CHECK ([estado] IN ('ACTIVO','INACTIVO'))
END
GO

IF OBJECT_ID(N'dbo.Citas', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'dbo.DF_Citas_Estado', N'D') IS NULL
BEGIN
	ALTER TABLE [dbo].[Citas] ADD CONSTRAINT [DF_Citas_Estado] DEFAULT ('PENDIENTE') FOR [estado]
END
GO
IF OBJECT_ID(N'dbo.DF_Citas_FechaCreacion', N'D') IS NULL
BEGIN
	ALTER TABLE [dbo].[Citas] ADD CONSTRAINT [DF_Citas_FechaCreacion] DEFAULT (getdate()) FOR [fecha_creacion]
END
GO
IF OBJECT_ID(N'dbo.CK_Citas_Estado', N'C') IS NULL
BEGIN
	ALTER TABLE [dbo].[Citas] ADD CONSTRAINT [CK_Citas_Estado] CHECK ([estado] IN ('PENDIENTE','PROGRAMADA','CONFIRMADA','REPROGRAMADA','CANCELADA','ATENDIDA'))
END
GO

IF OBJECT_ID(N'dbo.Expedientes', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'dbo.UQ_Expedientes_IdPaciente', N'UQ') IS NULL
BEGIN
	ALTER TABLE [dbo].[Expedientes] ADD CONSTRAINT [UQ_Expedientes_IdPaciente] UNIQUE NONCLUSTERED ([id_paciente] ASC)
END
GO
IF OBJECT_ID(N'dbo.DF_Expedientes_FechaApertura', N'D') IS NULL
BEGIN
	ALTER TABLE [dbo].[Expedientes] ADD CONSTRAINT [DF_Expedientes_FechaApertura] DEFAULT (getdate()) FOR [fecha_apertura]
END
GO

IF OBJECT_ID(N'dbo.HistorialMedico', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'dbo.DF_HistorialMedico_FechaConsulta', N'D') IS NULL
BEGIN
	ALTER TABLE [dbo].[HistorialMedico] ADD CONSTRAINT [DF_HistorialMedico_FechaConsulta] DEFAULT (getdate()) FOR [fecha_consulta]
END
GO

IF OBJECT_ID(N'dbo.Archivos', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'dbo.DF_Archivos_Estado', N'D') IS NULL
BEGIN
	ALTER TABLE [dbo].[Archivos] ADD CONSTRAINT [DF_Archivos_Estado] DEFAULT ('ACTIVO') FOR [estado]
END
GO
IF OBJECT_ID(N'dbo.DF_Archivos_FechaCarga', N'D') IS NULL
BEGIN
	ALTER TABLE [dbo].[Archivos] ADD CONSTRAINT [DF_Archivos_FechaCarga] DEFAULT (getdate()) FOR [fecha_carga]
END
GO
IF OBJECT_ID(N'dbo.CK_Archivos_Estado', N'C') IS NULL
BEGIN
	ALTER TABLE [dbo].[Archivos] ADD CONSTRAINT [CK_Archivos_Estado] CHECK ([estado] IN ('ACTIVO','INACTIVO'))
END
GO

IF OBJECT_ID(N'dbo.Bitacora', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'dbo.DF_Bitacora_Fecha', N'D') IS NULL
BEGIN
	ALTER TABLE [dbo].[Bitacora] ADD CONSTRAINT [DF_Bitacora_Fecha] DEFAULT (getdate()) FOR [fecha]
END
GO
IF OBJECT_ID(N'dbo.CK_Bitacora_Nivel', N'C') IS NULL
BEGIN
	ALTER TABLE [dbo].[Bitacora] ADD CONSTRAINT [CK_Bitacora_Nivel] CHECK ([nivel] IN ('INFO','WARN','ERROR'))
END
GO

IF OBJECT_ID(N'dbo.Notificaciones', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'dbo.DF_Notificaciones_FechaEnvio', N'D') IS NULL
BEGIN
	ALTER TABLE [dbo].[Notificaciones] ADD CONSTRAINT [DF_Notificaciones_FechaEnvio] DEFAULT (getdate()) FOR [fecha_envio]
END
GO
IF OBJECT_ID(N'dbo.CK_Notificaciones_Tipo', N'C') IS NULL
BEGIN
	ALTER TABLE [dbo].[Notificaciones] ADD CONSTRAINT [CK_Notificaciones_Tipo] CHECK ([tipo] IN ('REGISTRO','CITA_PROGRAMADA','CITA_CANCELADA','RECUPERACION'))
END
GO

IF EXISTS (
	SELECT 1 FROM sys.check_constraints
	WHERE name = 'CK_Notificaciones_Tipo'
	AND definition NOT LIKE '%CITA_REPROGRAMADA%'
)
BEGIN
	ALTER TABLE [dbo].[Notificaciones] DROP CONSTRAINT [CK_Notificaciones_Tipo]
	ALTER TABLE [dbo].[Notificaciones] ADD CONSTRAINT [CK_Notificaciones_Tipo]
		CHECK ([tipo] IN ('REGISTRO','CITA_PROGRAMADA','CITA_CANCELADA','CITA_REPROGRAMADA','RECUPERACION'))
END
GO

IF OBJECT_ID(N'dbo.CK_Notificaciones_Estado', N'C') IS NULL
BEGIN
	ALTER TABLE [dbo].[Notificaciones] ADD CONSTRAINT [CK_Notificaciones_Estado] CHECK ([estado] IN ('ENVIADO','FALLIDO'))
END
GO

IF OBJECT_ID(N'dbo.FK_Doctores_tbUsuario', N'F') IS NULL
BEGIN
	ALTER TABLE [dbo].[Doctores]  WITH CHECK ADD  CONSTRAINT [FK_Doctores_tbUsuario] FOREIGN KEY([id_usuario])
	REFERENCES [dbo].[tbUsuario] ([Consecutivo])

	ALTER TABLE [dbo].[Doctores] CHECK CONSTRAINT [FK_Doctores_tbUsuario]
END
GO

IF OBJECT_ID(N'dbo.FK_Doctores_Especialidades', N'F') IS NULL
BEGIN
	ALTER TABLE [dbo].[Doctores]  WITH CHECK ADD  CONSTRAINT [FK_Doctores_Especialidades] FOREIGN KEY([id_especialidad])
	REFERENCES [dbo].[Especialidades] ([id_especialidad])

	ALTER TABLE [dbo].[Doctores] CHECK CONSTRAINT [FK_Doctores_Especialidades]
END
GO

IF OBJECT_ID(N'dbo.FK_HorariosMedicos_Doctores', N'F') IS NULL
BEGIN
	ALTER TABLE [dbo].[HorariosMedicos]  WITH CHECK ADD  CONSTRAINT [FK_HorariosMedicos_Doctores] FOREIGN KEY([id_doctor])
	REFERENCES [dbo].[Doctores] ([id_doctor])

	ALTER TABLE [dbo].[HorariosMedicos] CHECK CONSTRAINT [FK_HorariosMedicos_Doctores]
END
GO

IF OBJECT_ID(N'dbo.FK_Citas_Pacientes', N'F') IS NULL
BEGIN
	ALTER TABLE [dbo].[Citas]  WITH CHECK ADD  CONSTRAINT [FK_Citas_Pacientes] FOREIGN KEY([id_paciente])
	REFERENCES [dbo].[Pacientes] ([id_paciente])

	ALTER TABLE [dbo].[Citas] CHECK CONSTRAINT [FK_Citas_Pacientes]
END
GO

IF OBJECT_ID(N'dbo.FK_Citas_Doctores', N'F') IS NULL
BEGIN
	ALTER TABLE [dbo].[Citas]  WITH CHECK ADD  CONSTRAINT [FK_Citas_Doctores] FOREIGN KEY([id_doctor])
	REFERENCES [dbo].[Doctores] ([id_doctor])

	ALTER TABLE [dbo].[Citas] CHECK CONSTRAINT [FK_Citas_Doctores]
END
GO

IF OBJECT_ID(N'dbo.FK_Citas_CitaAnterior', N'F') IS NULL
BEGIN
	ALTER TABLE [dbo].[Citas]  WITH CHECK ADD  CONSTRAINT [FK_Citas_CitaAnterior] FOREIGN KEY([id_cita_anterior])
	REFERENCES [dbo].[Citas] ([id_cita])

	ALTER TABLE [dbo].[Citas] CHECK CONSTRAINT [FK_Citas_CitaAnterior]
END
GO

IF OBJECT_ID(N'dbo.FK_Expedientes_Pacientes', N'F') IS NULL
BEGIN
	ALTER TABLE [dbo].[Expedientes]  WITH CHECK ADD  CONSTRAINT [FK_Expedientes_Pacientes] FOREIGN KEY([id_paciente])
	REFERENCES [dbo].[Pacientes] ([id_paciente])

	ALTER TABLE [dbo].[Expedientes] CHECK CONSTRAINT [FK_Expedientes_Pacientes]
END
GO

IF OBJECT_ID(N'dbo.FK_HistorialMedico_Expedientes', N'F') IS NULL
BEGIN
	ALTER TABLE [dbo].[HistorialMedico]  WITH CHECK ADD  CONSTRAINT [FK_HistorialMedico_Expedientes] FOREIGN KEY([id_expediente])
	REFERENCES [dbo].[Expedientes] ([id_expediente])

	ALTER TABLE [dbo].[HistorialMedico] CHECK CONSTRAINT [FK_HistorialMedico_Expedientes]
END
GO

IF OBJECT_ID(N'dbo.FK_HistorialMedico_Citas', N'F') IS NULL
BEGIN
	ALTER TABLE [dbo].[HistorialMedico]  WITH CHECK ADD  CONSTRAINT [FK_HistorialMedico_Citas] FOREIGN KEY([id_cita])
	REFERENCES [dbo].[Citas] ([id_cita])

	ALTER TABLE [dbo].[HistorialMedico] CHECK CONSTRAINT [FK_HistorialMedico_Citas]
END
GO

IF OBJECT_ID(N'dbo.FK_HistorialMedico_Doctores', N'F') IS NULL
BEGIN
	ALTER TABLE [dbo].[HistorialMedico]  WITH CHECK ADD  CONSTRAINT [FK_HistorialMedico_Doctores] FOREIGN KEY([id_doctor])
	REFERENCES [dbo].[Doctores] ([id_doctor])

	ALTER TABLE [dbo].[HistorialMedico] CHECK CONSTRAINT [FK_HistorialMedico_Doctores]
END
GO

IF OBJECT_ID(N'dbo.FK_Archivos_Expedientes', N'F') IS NULL
BEGIN
	ALTER TABLE [dbo].[Archivos]  WITH CHECK ADD  CONSTRAINT [FK_Archivos_Expedientes] FOREIGN KEY([id_expediente])
	REFERENCES [dbo].[Expedientes] ([id_expediente])

	ALTER TABLE [dbo].[Archivos] CHECK CONSTRAINT [FK_Archivos_Expedientes]
END
GO

IF OBJECT_ID(N'dbo.FK_Archivos_tbUsuario', N'F') IS NULL
BEGIN
	ALTER TABLE [dbo].[Archivos]  WITH CHECK ADD  CONSTRAINT [FK_Archivos_tbUsuario] FOREIGN KEY([id_usuario])
	REFERENCES [dbo].[tbUsuario] ([Consecutivo])

	ALTER TABLE [dbo].[Archivos] CHECK CONSTRAINT [FK_Archivos_tbUsuario]
END
GO

IF OBJECT_ID(N'dbo.FK_Bitacora_tbUsuario', N'F') IS NULL
BEGIN
	ALTER TABLE [dbo].[Bitacora]  WITH CHECK ADD  CONSTRAINT [FK_Bitacora_tbUsuario] FOREIGN KEY([id_usuario])
	REFERENCES [dbo].[tbUsuario] ([Consecutivo])

	ALTER TABLE [dbo].[Bitacora] CHECK CONSTRAINT [FK_Bitacora_tbUsuario]
END
GO

IF OBJECT_ID(N'dbo.FK_Notificaciones_tbUsuario', N'F') IS NULL
BEGIN
	ALTER TABLE [dbo].[Notificaciones]  WITH CHECK ADD  CONSTRAINT [FK_Notificaciones_tbUsuario] FOREIGN KEY([id_usuario_destino])
	REFERENCES [dbo].[tbUsuario] ([Consecutivo])

	ALTER TABLE [dbo].[Notificaciones] CHECK CONSTRAINT [FK_Notificaciones_tbUsuario]
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spRegistrarBitacora]
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

CREATE OR ALTER PROCEDURE [dbo].[spCambiarEstadoEspecialidad]
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

CREATE OR ALTER PROCEDURE [dbo].[spRegistrarDoctor]
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

CREATE OR ALTER PROCEDURE [dbo].[spCambiarEstadoDoctor]
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

CREATE OR ALTER PROCEDURE [dbo].[spValidarDisponibilidadCita]
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

IF NOT EXISTS (SELECT 1 FROM dbo.tbUsuario WHERE Correo = 'recepcion1@medicore.com')
BEGIN
    DECLARE @r1 INT; SELECT @r1=id_rol FROM dbo.tbRol WHERE nombre_rol='RECEPCIONISTA';
    INSERT INTO dbo.tbUsuario (id_rol,Nombre,Cedula,FechaNacimiento,Telefono,Correo,Contrasenna,Estado)
    VALUES (@r1,'Laura Jiménez Solís','106780001','1990-03-15','88001001','recepcion1@medicore.com','Laura123',1);
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.tbUsuario WHERE Correo = 'recepcion2@medicore.com')
BEGIN
    DECLARE @r2 INT; SELECT @r2=id_rol FROM dbo.tbRol WHERE nombre_rol='RECEPCIONISTA';
    INSERT INTO dbo.tbUsuario (id_rol,Nombre,Cedula,FechaNacimiento,Telefono,Correo,Contrasenna,Estado)
    VALUES (@r2,'Carlos Mora Vega','205670002','1985-07-22','88002002','recepcion2@medicore.com','Carlos123',1);
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.tbUsuario WHERE Correo = 'recepcion3@medicore.com')
BEGIN
    DECLARE @r3 INT; SELECT @r3=id_rol FROM dbo.tbRol WHERE nombre_rol='RECEPCIONISTA';
    INSERT INTO dbo.tbUsuario (id_rol,Nombre,Cedula,FechaNacimiento,Telefono,Correo,Contrasenna,Estado)
    VALUES (@r3,'Sofía Ramírez Torres','304560003','1992-11-08','88003003','recepcion3@medicore.com','Sofia123',1);
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.tbUsuario WHERE Correo = 'recepcion4@medicore.com')
BEGIN
    DECLARE @r4 INT; SELECT @r4=id_rol FROM dbo.tbRol WHERE nombre_rol='RECEPCIONISTA';
    INSERT INTO dbo.tbUsuario (id_rol,Nombre,Cedula,FechaNacimiento,Telefono,Correo,Contrasenna,Estado)
    VALUES (@r4,'Andrés Campos Rojas','401230004','1988-01-30','88004004','recepcion4@medicore.com','Andres123',1);
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.tbUsuario WHERE Correo = 'recepcion5@medicore.com')
BEGIN
    DECLARE @r5 INT; SELECT @r5=id_rol FROM dbo.tbRol WHERE nombre_rol='RECEPCIONISTA';
    INSERT INTO dbo.tbUsuario (id_rol,Nombre,Cedula,FechaNacimiento,Telefono,Correo,Contrasenna,Estado)
    VALUES (@r5,'María Vargas Núñez','502340005','1995-06-17','88005005','recepcion5@medicore.com','Maria123',1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Especialidades WHERE nombre='Medicina General')
    INSERT INTO dbo.Especialidades (nombre,descripcion,estado,fecha_creacion)
    VALUES ('Medicina General','Atención médica primaria y preventiva','ACTIVO',GETDATE());
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Especialidades WHERE nombre='Cardiología')
    INSERT INTO dbo.Especialidades (nombre,descripcion,estado,fecha_creacion)
    VALUES ('Cardiología','Diagnóstico y tratamiento de enfermedades del corazón','ACTIVO',GETDATE());
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Especialidades WHERE nombre='Pediatría')
    INSERT INTO dbo.Especialidades (nombre,descripcion,estado,fecha_creacion)
    VALUES ('Pediatría','Atención médica para niños y adolescentes','ACTIVO',GETDATE());
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Especialidades WHERE nombre='Ginecología')
    INSERT INTO dbo.Especialidades (nombre,descripcion,estado,fecha_creacion)
    VALUES ('Ginecología','Salud del sistema reproductivo femenino','ACTIVO',GETDATE());
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Especialidades WHERE nombre='Dermatología')
    INSERT INTO dbo.Especialidades (nombre,descripcion,estado,fecha_creacion)
    VALUES ('Dermatología','Diagnóstico y tratamiento de enfermedades de la piel','ACTIVO',GETDATE());
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Especialidades WHERE nombre='Neurología')
    INSERT INTO dbo.Especialidades (nombre,descripcion,estado,fecha_creacion)
    VALUES ('Neurología','Enfermedades del sistema nervioso central y periférico','ACTIVO',GETDATE());
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Doctores WHERE cedula='108900010')
BEGIN
    DECLARE @esp1 INT; SELECT @esp1=id_especialidad FROM dbo.Especialidades WHERE nombre='Medicina General';
    EXEC dbo.spRegistrarDoctor @NombreCompleto='Dr. Juan Pablo Herrera Ulate',@Cedula='108900010',
         @CodigoColegiado='MED-1001',@Correo='jherrera@medicore.com',@Telefono='89101001',
         @IdEspecialidad=@esp1,@Contrasenna='Juan1001';
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Doctores WHERE cedula='205800020')
BEGIN
    DECLARE @esp2 INT; SELECT @esp2=id_especialidad FROM dbo.Especialidades WHERE nombre='Cardiología';
    EXEC dbo.spRegistrarDoctor @NombreCompleto='Dra. Ana Lucía Brenes Fallas',@Cedula='205800020',
         @CodigoColegiado='MED-1002',@Correo='abrenes@medicore.com',@Telefono='89202002',
         @IdEspecialidad=@esp2,@Contrasenna='Ana1002';
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Doctores WHERE cedula='304700030')
BEGIN
    DECLARE @esp3 INT; SELECT @esp3=id_especialidad FROM dbo.Especialidades WHERE nombre='Pediatría';
    EXEC dbo.spRegistrarDoctor @NombreCompleto='Dr. Roberto Sáenz Quesada',@Cedula='304700030',
         @CodigoColegiado='MED-1003',@Correo='rsaenz@medicore.com',@Telefono='89303003',
         @IdEspecialidad=@esp3,@Contrasenna='Roberto103';
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Doctores WHERE cedula='401600040')
BEGIN
    DECLARE @esp4 INT; SELECT @esp4=id_especialidad FROM dbo.Especialidades WHERE nombre='Ginecología';
    EXEC dbo.spRegistrarDoctor @NombreCompleto='Dra. Patricia Solano Méndez',@Cedula='401600040',
         @CodigoColegiado='MED-1004',@Correo='psolano@medicore.com',@Telefono='89404004',
         @IdEspecialidad=@esp4,@Contrasenna='Patricia104';
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Doctores WHERE cedula='502500050')
BEGIN
    DECLARE @esp5 INT; SELECT @esp5=id_especialidad FROM dbo.Especialidades WHERE nombre='Dermatología';
    EXEC dbo.spRegistrarDoctor @NombreCompleto='Dr. Marcos Delgado Arce',@Cedula='502500050',
         @CodigoColegiado='MED-1005',@Correo='mdelgado@medicore.com',@Telefono='89505005',
         @IdEspecialidad=@esp5,@Contrasenna='Marcos1005';
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Doctores WHERE cedula='601400060')
BEGIN
    DECLARE @esp6 INT; SELECT @esp6=id_especialidad FROM dbo.Especialidades WHERE nombre='Neurología';
    EXEC dbo.spRegistrarDoctor @NombreCompleto='Dra. Valeria Castro Ugalde',@Cedula='601400060',
         @CodigoColegiado='MED-1006',@Correo='vcastro@medicore.com',@Telefono='89606006',
         @IdEspecialidad=@esp6,@Contrasenna='Valeria106';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Pacientes WHERE cedula='110200101')
    INSERT INTO dbo.Pacientes (nombre_completo,cedula,fecha_nacimiento,sexo,telefono,correo,direccion,estado,fecha_registro)
    VALUES ('Diego Alvarado Pérez','110200101','1980-04-12','M','87001001','dalvarado@gmail.com','San José, Montes de Oca, 50m norte del parque','ACTIVO',GETDATE());
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Pacientes WHERE cedula='209100202')
    INSERT INTO dbo.Pacientes (nombre_completo,cedula,fecha_nacimiento,sexo,telefono,correo,direccion,estado,fecha_registro)
    VALUES ('Gabriela Fonseca López','209100202','1995-09-25','F','87002002','gfonseca@gmail.com','Alajuela, La Unión, frente a la iglesia','ACTIVO',GETDATE());
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Pacientes WHERE cedula='308000303')
    INSERT INTO dbo.Pacientes (nombre_completo,cedula,fecha_nacimiento,sexo,telefono,correo,direccion,estado,fecha_registro)
    VALUES ('Esteban Quirós Arias','308000303','2010-02-14','M','87003003','equiros@gmail.com','Heredia, San Pablo, residencial Los Pinos','ACTIVO',GETDATE());
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Pacientes WHERE cedula='407900404')
    INSERT INTO dbo.Pacientes (nombre_completo,cedula,fecha_nacimiento,sexo,telefono,correo,direccion,estado,fecha_registro)
    VALUES ('Natalia Espinoza Bolaños','407900404','1975-12-01','F','87004004','nespinoza@gmail.com','Cartago, Tres Ríos, 100m sur del estadio','ACTIVO',GETDATE());
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Pacientes WHERE cedula='506800505')
    INSERT INTO dbo.Pacientes (nombre_completo,cedula,fecha_nacimiento,sexo,telefono,correo,direccion,estado,fecha_registro)
    VALUES ('Fernando Monge Zuñiga','506800505','1988-07-30','M','87005005','fmonge@gmail.com','Limón, Puerto Viejo, barrio El Centro','ACTIVO',GETDATE());
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Pacientes WHERE cedula='605700606')
    INSERT INTO dbo.Pacientes (nombre_completo,cedula,fecha_nacimiento,sexo,telefono,correo,direccion,estado,fecha_registro)
    VALUES ('Isabella Ruiz Montoya','605700606','2018-05-20','F','87006006','iruiz@gmail.com','Guanacaste, Liberia, contiguo al hospital','ACTIVO',GETDATE());
GO

IF NOT EXISTS (SELECT 1 FROM dbo.HorariosMedicos hm JOIN dbo.Doctores d ON hm.id_doctor=d.id_doctor WHERE d.cedula='108900010' AND hm.dia_semana=1)
BEGIN
    DECLARE @dh1 INT; SELECT @dh1=id_doctor FROM dbo.Doctores WHERE cedula='108900010';
    INSERT INTO dbo.HorariosMedicos (id_doctor,dia_semana,hora_inicio,hora_fin,duracion_cita_min,estado)
    VALUES (@dh1,1,'08:00','12:00',30,'ACTIVO');
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.HorariosMedicos hm JOIN dbo.Doctores d ON hm.id_doctor=d.id_doctor WHERE d.cedula='205800020' AND hm.dia_semana=2)
BEGIN
    DECLARE @dh2 INT; SELECT @dh2=id_doctor FROM dbo.Doctores WHERE cedula='205800020';
    INSERT INTO dbo.HorariosMedicos (id_doctor,dia_semana,hora_inicio,hora_fin,duracion_cita_min,estado)
    VALUES (@dh2,2,'09:00','13:00',45,'ACTIVO');
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.HorariosMedicos hm JOIN dbo.Doctores d ON hm.id_doctor=d.id_doctor WHERE d.cedula='304700030' AND hm.dia_semana=3)
BEGIN
    DECLARE @dh3 INT; SELECT @dh3=id_doctor FROM dbo.Doctores WHERE cedula='304700030';
    INSERT INTO dbo.HorariosMedicos (id_doctor,dia_semana,hora_inicio,hora_fin,duracion_cita_min,estado)
    VALUES (@dh3,3,'07:00','11:00',30,'ACTIVO');
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.HorariosMedicos hm JOIN dbo.Doctores d ON hm.id_doctor=d.id_doctor WHERE d.cedula='401600040' AND hm.dia_semana=4)
BEGIN
    DECLARE @dh4 INT; SELECT @dh4=id_doctor FROM dbo.Doctores WHERE cedula='401600040';
    INSERT INTO dbo.HorariosMedicos (id_doctor,dia_semana,hora_inicio,hora_fin,duracion_cita_min,estado)
    VALUES (@dh4,4,'13:00','17:00',30,'ACTIVO');
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.HorariosMedicos hm JOIN dbo.Doctores d ON hm.id_doctor=d.id_doctor WHERE d.cedula='502500050' AND hm.dia_semana=5)
BEGIN
    DECLARE @dh5 INT; SELECT @dh5=id_doctor FROM dbo.Doctores WHERE cedula='502500050';
    INSERT INTO dbo.HorariosMedicos (id_doctor,dia_semana,hora_inicio,hora_fin,duracion_cita_min,estado)
    VALUES (@dh5,5,'10:00','14:00',30,'ACTIVO');
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.HorariosMedicos hm JOIN dbo.Doctores d ON hm.id_doctor=d.id_doctor WHERE d.cedula='601400060' AND hm.dia_semana=1)
BEGIN
    DECLARE @dh6 INT; SELECT @dh6=id_doctor FROM dbo.Doctores WHERE cedula='601400060';
    INSERT INTO dbo.HorariosMedicos (id_doctor,dia_semana,hora_inicio,hora_fin,duracion_cita_min,estado)
    VALUES (@dh6,1,'14:00','18:00',45,'ACTIVO');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Citas c JOIN dbo.Pacientes p ON c.id_paciente=p.id_paciente JOIN dbo.Doctores d ON c.id_doctor=d.id_doctor WHERE p.cedula='110200101' AND d.cedula='108900010' AND c.fecha_cita='2026-08-18 08:00')
BEGIN
    DECLARE @cp1 INT,@cd1 INT;
    SELECT @cp1=id_paciente FROM dbo.Pacientes WHERE cedula='110200101';
    SELECT @cd1=id_doctor  FROM dbo.Doctores  WHERE cedula='108900010';
    INSERT INTO dbo.Citas (id_paciente,id_doctor,fecha_cita,duracion_min,motivo,estado,fecha_creacion)
    VALUES (@cp1,@cd1,'2026-08-18 08:00',30,'Consulta general por dolor de cabeza persistente','PENDIENTE',GETDATE());
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Citas c JOIN dbo.Pacientes p ON c.id_paciente=p.id_paciente JOIN dbo.Doctores d ON c.id_doctor=d.id_doctor WHERE p.cedula='209100202' AND d.cedula='205800020' AND c.fecha_cita='2026-08-19 09:00')
BEGIN
    DECLARE @cp2 INT,@cd2 INT;
    SELECT @cp2=id_paciente FROM dbo.Pacientes WHERE cedula='209100202';
    SELECT @cd2=id_doctor  FROM dbo.Doctores  WHERE cedula='205800020';
    INSERT INTO dbo.Citas (id_paciente,id_doctor,fecha_cita,duracion_min,motivo,estado,fecha_creacion)
    VALUES (@cp2,@cd2,'2026-08-19 09:00',45,'Control cardíaco semestral','CONFIRMADA',GETDATE());
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Citas c JOIN dbo.Pacientes p ON c.id_paciente=p.id_paciente JOIN dbo.Doctores d ON c.id_doctor=d.id_doctor WHERE p.cedula='308000303' AND d.cedula='304700030' AND c.fecha_cita='2026-08-20 07:30')
BEGIN
    DECLARE @cp3 INT,@cd3 INT;
    SELECT @cp3=id_paciente FROM dbo.Pacientes WHERE cedula='308000303';
    SELECT @cd3=id_doctor  FROM dbo.Doctores  WHERE cedula='304700030';
    INSERT INTO dbo.Citas (id_paciente,id_doctor,fecha_cita,duracion_min,motivo,estado,fecha_creacion)
    VALUES (@cp3,@cd3,'2026-08-20 07:30',30,'Vacunación y revisión de peso y talla','PROGRAMADA',GETDATE());
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Citas c JOIN dbo.Pacientes p ON c.id_paciente=p.id_paciente JOIN dbo.Doctores d ON c.id_doctor=d.id_doctor WHERE p.cedula='407900404' AND d.cedula='401600040' AND c.fecha_cita='2026-08-21 13:00')
BEGIN
    DECLARE @cp4 INT,@cd4 INT;
    SELECT @cp4=id_paciente FROM dbo.Pacientes WHERE cedula='407900404';
    SELECT @cd4=id_doctor  FROM dbo.Doctores  WHERE cedula='401600040';
    INSERT INTO dbo.Citas (id_paciente,id_doctor,fecha_cita,duracion_min,motivo,estado,fecha_creacion)
    VALUES (@cp4,@cd4,'2026-08-21 13:00',30,'Control prenatal tercer trimestre','PENDIENTE',GETDATE());
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Citas c JOIN dbo.Pacientes p ON c.id_paciente=p.id_paciente JOIN dbo.Doctores d ON c.id_doctor=d.id_doctor WHERE p.cedula='506800505' AND d.cedula='502500050' AND c.fecha_cita='2026-08-22 10:00')
BEGIN
    DECLARE @cp5 INT,@cd5 INT;
    SELECT @cp5=id_paciente FROM dbo.Pacientes WHERE cedula='506800505';
    SELECT @cd5=id_doctor  FROM dbo.Doctores  WHERE cedula='502500050';
    INSERT INTO dbo.Citas (id_paciente,id_doctor,fecha_cita,duracion_min,motivo,estado,fecha_creacion)
    VALUES (@cp5,@cd5,'2026-08-22 10:00',30,'Evaluación de dermatitis en antebrazo derecho','PENDIENTE',GETDATE());
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Citas c JOIN dbo.Pacientes p ON c.id_paciente=p.id_paciente JOIN dbo.Doctores d ON c.id_doctor=d.id_doctor WHERE p.cedula='605700606' AND d.cedula='601400060' AND c.fecha_cita='2026-08-25 14:00')
BEGIN
    DECLARE @cp6 INT,@cd6 INT;
    SELECT @cp6=id_paciente FROM dbo.Pacientes WHERE cedula='605700606';
    SELECT @cd6=id_doctor  FROM dbo.Doctores  WHERE cedula='601400060';
    INSERT INTO dbo.Citas (id_paciente,id_doctor,fecha_cita,duracion_min,motivo,estado,fecha_creacion)
    VALUES (@cp6,@cd6,'2026-08-25 14:00',45,'Evaluación neurológica por convulsiones febriles','CONFIRMADA',GETDATE());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Expedientes e JOIN dbo.Pacientes p ON e.id_paciente=p.id_paciente WHERE p.cedula='110200101')
BEGIN
    DECLARE @ep1 INT; SELECT @ep1=id_paciente FROM dbo.Pacientes WHERE cedula='110200101';
    INSERT INTO dbo.Expedientes (id_paciente,alergias,antecedentes,tipo_sangre,fecha_apertura)
    VALUES (@ep1,'Ninguna conocida','Hipertensión arterial diagnosticada en 2018','O+',GETDATE());
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Expedientes e JOIN dbo.Pacientes p ON e.id_paciente=p.id_paciente WHERE p.cedula='209100202')
BEGIN
    DECLARE @ep2 INT; SELECT @ep2=id_paciente FROM dbo.Pacientes WHERE cedula='209100202';
    INSERT INTO dbo.Expedientes (id_paciente,alergias,antecedentes,tipo_sangre,fecha_apertura)
    VALUES (@ep2,'Penicilina','Arritmia cardíaca leve desde 2020','A-',GETDATE());
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Expedientes e JOIN dbo.Pacientes p ON e.id_paciente=p.id_paciente WHERE p.cedula='308000303')
BEGIN
    DECLARE @ep3 INT; SELECT @ep3=id_paciente FROM dbo.Pacientes WHERE cedula='308000303';
    INSERT INTO dbo.Expedientes (id_paciente,alergias,antecedentes,tipo_sangre,fecha_apertura)
    VALUES (@ep3,'Polen y polvo','Asma leve controlado con broncodilatador','B+',GETDATE());
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Expedientes e JOIN dbo.Pacientes p ON e.id_paciente=p.id_paciente WHERE p.cedula='407900404')
BEGIN
    DECLARE @ep4 INT; SELECT @ep4=id_paciente FROM dbo.Pacientes WHERE cedula='407900404';
    INSERT INTO dbo.Expedientes (id_paciente,alergias,antecedentes,tipo_sangre,fecha_apertura)
    VALUES (@ep4,'Ibuprofeno','Diabetes gestacional en embarazo anterior','AB+',GETDATE());
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Expedientes e JOIN dbo.Pacientes p ON e.id_paciente=p.id_paciente WHERE p.cedula='506800505')
BEGIN
    DECLARE @ep5 INT; SELECT @ep5=id_paciente FROM dbo.Pacientes WHERE cedula='506800505';
    INSERT INTO dbo.Expedientes (id_paciente,alergias,antecedentes,tipo_sangre,fecha_apertura)
    VALUES (@ep5,'Mariscos','Sin antecedentes relevantes','O-',GETDATE());
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Expedientes e JOIN dbo.Pacientes p ON e.id_paciente=p.id_paciente WHERE p.cedula='605700606')
BEGIN
    DECLARE @ep6 INT; SELECT @ep6=id_paciente FROM dbo.Pacientes WHERE cedula='605700606';
    INSERT INTO dbo.Expedientes (id_paciente,alergias,antecedentes,tipo_sangre,fecha_apertura)
    VALUES (@ep6,'Ninguna','Convulsiones febriles desde los 6 meses de edad','A+',GETDATE());
END
GO

IF COL_LENGTH('dbo.HistorialMedico','medicamentos') IS NULL
    EXEC sp_executesql N'ALTER TABLE dbo.HistorialMedico ADD medicamentos NVARCHAR(MAX) NULL, proxima_cita DATETIME NULL';
GO
EXEC sp_executesql N'
UPDATE hm SET medicamentos=''Ibuprofeno 400mg cada 8 horas por 5 dias'', proxima_cita=''2026-08-18 08:00''
FROM dbo.HistorialMedico hm JOIN dbo.Expedientes e ON hm.id_expediente=e.id_expediente JOIN dbo.Pacientes p ON e.id_paciente=p.id_paciente WHERE p.cedula=''110200101'' AND hm.fecha_consulta=''2026-07-10 08:30'' AND hm.medicamentos IS NULL;
UPDATE hm SET medicamentos=''Propranolol 40mg cada 12 horas'', proxima_cita=''2026-08-19 09:00''
FROM dbo.HistorialMedico hm JOIN dbo.Expedientes e ON hm.id_expediente=e.id_expediente JOIN dbo.Pacientes p ON e.id_paciente=p.id_paciente WHERE p.cedula=''209100202'' AND hm.fecha_consulta=''2026-06-15 09:30'' AND hm.medicamentos IS NULL;
UPDATE hm SET medicamentos=''Salbutamol inhalador 100mcg segun necesidad'', proxima_cita=''2026-08-20 07:30''
FROM dbo.HistorialMedico hm JOIN dbo.Expedientes e ON hm.id_expediente=e.id_expediente JOIN dbo.Pacientes p ON e.id_paciente=p.id_paciente WHERE p.cedula=''308000303'' AND hm.fecha_consulta=''2026-05-20 07:00'' AND hm.medicamentos IS NULL;
UPDATE hm SET medicamentos=''Acido folico 5mg diario, hierro 300mg diario'', proxima_cita=''2026-08-21 13:00''
FROM dbo.HistorialMedico hm JOIN dbo.Expedientes e ON hm.id_expediente=e.id_expediente JOIN dbo.Pacientes p ON e.id_paciente=p.id_paciente WHERE p.cedula=''407900404'' AND hm.fecha_consulta=''2026-07-01 13:00'' AND hm.medicamentos IS NULL;
UPDATE hm SET medicamentos=''Hidrocortisona crema 1% dos veces al dia por 10 dias'', proxima_cita=''2026-08-22 10:00''
FROM dbo.HistorialMedico hm JOIN dbo.Expedientes e ON hm.id_expediente=e.id_expediente JOIN dbo.Pacientes p ON e.id_paciente=p.id_paciente WHERE p.cedula=''506800505'' AND hm.fecha_consulta=''2026-07-25 10:00'' AND hm.medicamentos IS NULL;
UPDATE hm SET medicamentos=''Diazepam rectal 5mg si convulsion mayor a 5 min'', proxima_cita=''2026-08-25 14:00''
FROM dbo.HistorialMedico hm JOIN dbo.Expedientes e ON hm.id_expediente=e.id_expediente JOIN dbo.Pacientes p ON e.id_paciente=p.id_paciente WHERE p.cedula=''605700606'' AND hm.fecha_consulta=''2026-07-30 14:00'' AND hm.medicamentos IS NULL;
';

IF NOT EXISTS (SELECT 1 FROM dbo.HistorialMedico hm JOIN dbo.Expedientes e ON hm.id_expediente=e.id_expediente JOIN dbo.Pacientes p ON e.id_paciente=p.id_paciente WHERE p.cedula='110200101' AND hm.fecha_consulta='2026-07-10 08:30')
BEGIN
    DECLARE @he1 INT,@hd1 INT;
    SELECT @he1=e.id_expediente FROM dbo.Expedientes e JOIN dbo.Pacientes p ON e.id_paciente=p.id_paciente WHERE p.cedula='110200101';
    SELECT @hd1=id_doctor FROM dbo.Doctores WHERE cedula='108900010';
    INSERT INTO dbo.HistorialMedico (id_expediente,id_cita,id_doctor,fecha_consulta,sintomas,diagnostico,tratamiento,observaciones)
    VALUES (@he1,NULL,@hd1,'2026-07-10 08:30','Cefalea frontal bilateral de 3 días, sin fiebre','Cefalea tensional','Reposo, hidratación y analgésico oral por 5 días','Control en 2 semanas si persisten síntomas');
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.HistorialMedico hm JOIN dbo.Expedientes e ON hm.id_expediente=e.id_expediente JOIN dbo.Pacientes p ON e.id_paciente=p.id_paciente WHERE p.cedula='209100202' AND hm.fecha_consulta='2026-06-15 09:30')
BEGIN
    DECLARE @he2 INT,@hd2 INT;
    SELECT @he2=e.id_expediente FROM dbo.Expedientes e JOIN dbo.Pacientes p ON e.id_paciente=p.id_paciente WHERE p.cedula='209100202';
    SELECT @hd2=id_doctor FROM dbo.Doctores WHERE cedula='205800020';
    INSERT INTO dbo.HistorialMedico (id_expediente,id_cita,id_doctor,fecha_consulta,sintomas,diagnostico,tratamiento,observaciones)
    VALUES (@he2,NULL,@hd2,'2026-06-15 09:30','Palpitaciones ocasionales y cansancio leve al subir escaleras','Arritmia supraventricular paroxística','Monitoreo Holter 24h y ajuste de medicación','Se solicita electrocardiograma de control');
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.HistorialMedico hm JOIN dbo.Expedientes e ON hm.id_expediente=e.id_expediente JOIN dbo.Pacientes p ON e.id_paciente=p.id_paciente WHERE p.cedula='308000303' AND hm.fecha_consulta='2026-05-20 07:00')
BEGIN
    DECLARE @he3 INT,@hd3 INT;
    SELECT @he3=e.id_expediente FROM dbo.Expedientes e JOIN dbo.Pacientes p ON e.id_paciente=p.id_paciente WHERE p.cedula='308000303';
    SELECT @hd3=id_doctor FROM dbo.Doctores WHERE cedula='304700030';
    INSERT INTO dbo.HistorialMedico (id_expediente,id_cita,id_doctor,fecha_consulta,sintomas,diagnostico,tratamiento,observaciones)
    VALUES (@he3,NULL,@hd3,'2026-05-20 07:00','Tos seca nocturna, sibilancias leves','Asma bronquial leve intermitente','Broncodilatador de rescate según necesidad','Evitar mascotas y polvo. Vacunas al día');
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.HistorialMedico hm JOIN dbo.Expedientes e ON hm.id_expediente=e.id_expediente JOIN dbo.Pacientes p ON e.id_paciente=p.id_paciente WHERE p.cedula='407900404' AND hm.fecha_consulta='2026-07-01 13:00')
BEGIN
    DECLARE @he4 INT,@hd4 INT;
    SELECT @he4=e.id_expediente FROM dbo.Expedientes e JOIN dbo.Pacientes p ON e.id_paciente=p.id_paciente WHERE p.cedula='407900404';
    SELECT @hd4=id_doctor FROM dbo.Doctores WHERE cedula='401600040';
    INSERT INTO dbo.HistorialMedico (id_expediente,id_cita,id_doctor,fecha_consulta,sintomas,diagnostico,tratamiento,observaciones)
    VALUES (@he4,NULL,@hd4,'2026-07-01 13:00','Semana 28, glucemia en ayunas 105 mg/dL','Embarazo 28 semanas, control rutinario','Dieta hipoglucémica y monitoreo de glucemia diaria','Presión arterial dentro de parámetros normales');
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.HistorialMedico hm JOIN dbo.Expedientes e ON hm.id_expediente=e.id_expediente JOIN dbo.Pacientes p ON e.id_paciente=p.id_paciente WHERE p.cedula='506800505' AND hm.fecha_consulta='2026-07-25 10:00')
BEGIN
    DECLARE @he5 INT,@hd5 INT;
    SELECT @he5=e.id_expediente FROM dbo.Expedientes e JOIN dbo.Pacientes p ON e.id_paciente=p.id_paciente WHERE p.cedula='506800505';
    SELECT @hd5=id_doctor FROM dbo.Doctores WHERE cedula='502500050';
    INSERT INTO dbo.HistorialMedico (id_expediente,id_cita,id_doctor,fecha_consulta,sintomas,diagnostico,tratamiento,observaciones)
    VALUES (@he5,NULL,@hd5,'2026-07-25 10:00','Lesiones eritematosas pruriginosas en antebrazo derecho','Dermatitis de contacto alérgica','Evitar irritantes, corticoide tópico','Se recomienda prueba de alergia cutánea');
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.HistorialMedico hm JOIN dbo.Expedientes e ON hm.id_expediente=e.id_expediente JOIN dbo.Pacientes p ON e.id_paciente=p.id_paciente WHERE p.cedula='605700606' AND hm.fecha_consulta='2026-07-30 14:00')
BEGIN
    DECLARE @he6 INT,@hd6 INT;
    SELECT @he6=e.id_expediente FROM dbo.Expedientes e JOIN dbo.Pacientes p ON e.id_paciente=p.id_paciente WHERE p.cedula='605700606';
    SELECT @hd6=id_doctor FROM dbo.Doctores WHERE cedula='601400060';
    INSERT INTO dbo.HistorialMedico (id_expediente,id_cita,id_doctor,fecha_consulta,sintomas,diagnostico,tratamiento,observaciones)
    VALUES (@he6,NULL,@hd6,'2026-07-30 14:00','Episodio convulsivo febril 2 min, temperatura 38.8°C','Convulsión febril simple','Manejo antipirético y observación neurológica','Sin recurrencia en 72 horas. EEG programado');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Notificaciones WHERE correo_destino='dalvarado@gmail.com' AND tipo='CITA_PROGRAMADA')
    INSERT INTO dbo.Notificaciones (correo_destino,tipo,asunto,cuerpo,estado,fecha_envio)
    VALUES ('dalvarado@gmail.com','CITA_PROGRAMADA','Confirmación de cita médica - MediCore','Estimado Diego, su cita con el Dr. Juan Pablo Herrera está programada para el 18 de agosto de 2026 a las 08:00 a.m.','ENVIADO',GETDATE());
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Notificaciones WHERE correo_destino='gfonseca@gmail.com' AND tipo='CITA_PROGRAMADA')
    INSERT INTO dbo.Notificaciones (correo_destino,tipo,asunto,cuerpo,estado,fecha_envio)
    VALUES ('gfonseca@gmail.com','CITA_PROGRAMADA','Confirmación de cita médica - MediCore','Estimada Gabriela, su cita con la Dra. Ana Lucía Brenes está confirmada para el 19 de agosto de 2026 a las 09:00 a.m.','ENVIADO',GETDATE());
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Notificaciones WHERE correo_destino='equiros@gmail.com' AND tipo='CITA_PROGRAMADA')
    INSERT INTO dbo.Notificaciones (correo_destino,tipo,asunto,cuerpo,estado,fecha_envio)
    VALUES ('equiros@gmail.com','CITA_PROGRAMADA','Recordatorio de cita pediátrica - MediCore','Estimada familia Quirós, la cita de Esteban con el Dr. Roberto Sáenz es el 20 de agosto de 2026 a las 07:30 a.m.','ENVIADO',GETDATE());
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Notificaciones WHERE correo_destino='nespinoza@gmail.com' AND tipo='CITA_PROGRAMADA')
    INSERT INTO dbo.Notificaciones (correo_destino,tipo,asunto,cuerpo,estado,fecha_envio)
    VALUES ('nespinoza@gmail.com','CITA_PROGRAMADA','Recordatorio de control prenatal - MediCore','Estimada Natalia, su control prenatal con la Dra. Patricia Solano es el 21 de agosto de 2026 a la 01:00 p.m.','ENVIADO',GETDATE());
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Notificaciones WHERE correo_destino='fmonge@gmail.com' AND tipo='CITA_PROGRAMADA')
    INSERT INTO dbo.Notificaciones (correo_destino,tipo,asunto,cuerpo,estado,fecha_envio)
    VALUES ('fmonge@gmail.com','CITA_PROGRAMADA','Confirmación de cita dermatológica - MediCore','Estimado Fernando, su cita con el Dr. Marcos Delgado está programada para el 22 de agosto de 2026 a las 10:00 a.m.','ENVIADO',GETDATE());
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Notificaciones WHERE correo_destino='iruiz@gmail.com' AND tipo='CITA_PROGRAMADA')
    INSERT INTO dbo.Notificaciones (correo_destino,tipo,asunto,cuerpo,estado,fecha_envio)
    VALUES ('iruiz@gmail.com','CITA_PROGRAMADA','Confirmación de evaluación neurológica - MediCore','Estimada familia Ruiz, la cita de Isabella con la Dra. Valeria Castro es el 25 de agosto de 2026 a las 02:00 p.m.','ENVIADO',GETDATE());
GO

USE [master]
GO
ALTER DATABASE [MediCore] SET  READ_WRITE
GO
