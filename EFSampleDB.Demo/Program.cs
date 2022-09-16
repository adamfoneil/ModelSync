// See https://aka.ms/new-console-template for more information

using EFSampleDB.Demo;

    using (EFSampleDBContext context = new EFSampleDBContext())
    {
        context.Database.Log = Console.Write;
        context.Employees.Where(e => e.EmployeeID == 0).ToList();
        Console.ReadKey();
    }

//CREATE TABLE[dbo].[Airports] (
//    [AirportID][int] NOT NULL IDENTITY,
//    [Name] [nvarchar] (max),
//    CONSTRAINT[PK_dbo.Airports] PRIMARY KEY([AirportID])
//)

//CREATE TABLE[dbo].[Flights] (
//    [FlightID][int] NOT NULL IDENTITY,
//    [Name] [nvarchar] (max),
//    [ArrivalAirport_AirportID][int],
//    [DepartureAirport_AirportID][int],
//    CONSTRAINT[PK_dbo.Flights] PRIMARY KEY([FlightID])
//)

//CREATE TABLE[dbo].[CollegeStudentMaster] (
//    [Address1][nvarchar](4000),
//    [Address2][varchar](8000),
//    [Address3][text],
//    [ID][int] NOT NULL IDENTITY,
//    [StudnetName] [nvarchar] (max),
//    [Donation][money] NOT NULL,
//    [Height] [decimal] (18, 2) NOT NULL,
//    [Weight] [float] NOT NULL,
//    [Address] [nvarchar] (max),
//    [Remarks][nvarchar] (50) NOT NULL,
//    [Hobbies] [nvarchar] (50),
//    [DOB][datetime] NOT NULL,
//    [Age] [int],
//    CONSTRAINT[PK_dbo.CollegeStudentMaster] PRIMARY KEY([ID])
//)

//CREATE TABLE[dbo].[Customers] (
//    [CustomerID][int] NOT NULL IDENTITY,
//    [CustomerName] [nvarchar] (max),
//    [CustomerContact_Email][nvarchar] (max),
//    [CustomerContact_Phone][nvarchar] (max),
//    CONSTRAINT[PK_dbo.Customers] PRIMARY KEY([CustomerID])
//)

//CREATE TABLE[dbo].[Employees] (
//    [EmployeeID][int] NOT NULL IDENTITY,
//    [Name] [nvarchar] (max),
//    [GradeID][int] NOT NULL,
//    [Department_DepartmentID] [int],
//    CONSTRAINT[PK_dbo.Employees] PRIMARY KEY([EmployeeID])
//)

//CREATE TABLE[dbo].[Departments] (
//    [DepartmentID][int] NOT NULL IDENTITY,
//    [Name] [nvarchar] (max),
//    CONSTRAINT[PK_dbo.Departments] PRIMARY KEY([DepartmentID])
//)

//CREATE TABLE[dbo].[EmployeeAddresses] (
//    [EmployeeID][int] NOT NULL,
//    [Address] [nvarchar] (max),
//    [City][nvarchar] (max),
//    [State][nvarchar] (max),
//    [Country][nvarchar] (max),
//    CONSTRAINT[PK_dbo.EmployeeAddresses] PRIMARY KEY([EmployeeID])
//)

//CREATE TABLE[dbo].[Grades] (
//    [GradeID][int] NOT NULL IDENTITY,
//    [Name] [nvarchar] (max),
//    CONSTRAINT[PK_dbo.Grades] PRIMARY KEY([GradeID])
//)

//CREATE TABLE[dbo].[Projects] (
//    [ProjectID][int] NOT NULL IDENTITY,
//    [Name] [nvarchar] (max),
//    CONSTRAINT[PK_dbo.Projects] PRIMARY KEY([ProjectID])
//)

//CREATE TABLE[dbo].[SchoolStudents] (
//    [ID][int] NOT NULL IDENTITY,
//    [Name] [nvarchar] (max),
//    [Address1][nvarchar] (max),
//    [Address2][nvarchar] (max),
//    [Address3][nvarchar] (max),
//    [Donation][decimal] (18, 2) NOT NULL,
//    [Height] [decimal] (18, 2) NOT NULL,
//    [Weight] [float] NOT NULL,
//    [Address] [nvarchar] (max),
//    [Remarks][nvarchar] (max),
//    [Hobbies][nvarchar] (max),
//    [DOB][datetime] NOT NULL,
//    [Age] [int],
//    [Photo][varbinary] (max),
//    [BloodGroup][int] NOT NULL,
//    CONSTRAINT[PK_dbo.SchoolStudents] PRIMARY KEY ([ID])
//)

//CREATE TABLE[dbo].[Suppliers] (
//    [SupplierID][int] NOT NULL IDENTITY,
//    [SupplierName] [nvarchar] (max),
//    [SupplierContact_Email][nvarchar] (max),
//    [SupplierContact_Phone][nvarchar] (max),
//    CONSTRAINT[PK_dbo.Suppliers] PRIMARY KEY([SupplierID])
//)

//CREATE TABLE[dbo].[ProjectEmployees] (
//    [Project_ProjectID][int] NOT NULL,
//    [Employee_EmployeeID] [int] NOT NULL,
//    CONSTRAINT[PK_dbo.ProjectEmployees] PRIMARY KEY ([Project_ProjectID], [Employee_EmployeeID])
//)

//CREATE INDEX[IX_ArrivalAirport_AirportID] ON [dbo].[Flights] ([ArrivalAirport_AirportID])

//CREATE INDEX[IX_DepartureAirport_AirportID] ON [dbo].[Flights] ([DepartureAirport_AirportID])

//CREATE INDEX[IX_GradeID] ON [dbo].[Employees] ([GradeID])

//CREATE INDEX[IX_Department_DepartmentID] ON [dbo].[Employees] ([Department_DepartmentID])

//CREATE INDEX[IX_EmployeeID] ON [dbo].[EmployeeAddresses] ([EmployeeID])

//CREATE INDEX[IX_Project_ProjectID] ON [dbo].[ProjectEmployees] ([Project_ProjectID])

//CREATE INDEX[IX_Employee_EmployeeID] ON [dbo].[ProjectEmployees] ([Employee_EmployeeID])

//ALTER TABLE[dbo].[Flights] ADD CONSTRAINT[FK_dbo.Flights_dbo.Airports_ArrivalAirport_AirportID] FOREIGN KEY ([ArrivalAirport_AirportID]) REFERENCES[dbo].[Airports]([AirportID])

//ALTER TABLE[dbo].[Flights] ADD CONSTRAINT[FK_dbo.Flights_dbo.Airports_DepartureAirport_AirportID] FOREIGN KEY ([DepartureAirport_AirportID]) REFERENCES[dbo].[Airports]([AirportID])

//ALTER TABLE[dbo].[Employees] ADD CONSTRAINT[FK_dbo.Employees_dbo.Departments_Department_DepartmentID] FOREIGN KEY ([Department_DepartmentID]) REFERENCES[dbo].[Departments]([DepartmentID])

//ALTER TABLE[dbo].[Employees] ADD CONSTRAINT[FK_dbo.Employees_dbo.Grades_GradeID] FOREIGN KEY ([GradeID]) REFERENCES[dbo].[Grades]([GradeID]) ON DELETE CASCADE

//ALTER TABLE [dbo].[EmployeeAddresses] ADD CONSTRAINT[FK_dbo.EmployeeAddresses_dbo.Employees_EmployeeID] FOREIGN KEY ([EmployeeID]) REFERENCES[dbo].[Employees]([EmployeeID])

//ALTER TABLE[dbo].[ProjectEmployees] ADD CONSTRAINT[FK_dbo.ProjectEmployees_dbo.Projects_Project_ProjectID] FOREIGN KEY ([Project_ProjectID]) REFERENCES[dbo].[Projects]([ProjectID]) ON DELETE CASCADE

//ALTER TABLE [dbo].[ProjectEmployees] ADD CONSTRAINT[FK_dbo.ProjectEmployees_dbo.Employees_Employee_EmployeeID] FOREIGN KEY ([Employee_EmployeeID]) REFERENCES[dbo].[Employees]([EmployeeID]) ON DELETE CASCADE


