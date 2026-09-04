create database RaceDay; 
GO

use RaceDay;
GO

create table users (
userID INT PRIMARY KEY IDENTITY(1,1),
FullName varchar(40) NOT NULL,
Email varchar(90) NOT NULL UNIQUE,
Password varchar(90) NOT NULL,
Role varchar(25) NOT NULL
);


create table Events (
EventID int primary key identity(1,1),
OrganiserID int NOT NULL, 
EventName varchar (200) NOT NULL, 
Description varchar(250) NOT NULL,
EventDate date NOT NULL, 
location varchar (50) NOT NULL, 
DistanceKm decimal(5,2) NOT NULL,
EventType varchar (25) NOT NULL, 
foreign key (OrganiserID) references users(UserID)
);


create table Category (
categoryID INT Primary key identity (1,1), 
EventID int NOT NULL, 
categoryName varchar (50) NOT NULL, 
DistanceKm Decimal (5,2) NOT NULL, 
EntryFee Decimal (8,2) NOT NULL,
MinAge int NULL, 
foreign key (EventID) references Events(EventID)
);


create table WeatherForecast (
WeatherID INT Primary key identity (1,1), 
EventID int NOT NULL, 
ForecastDate Date NOT NULL, 
Temperature Decimal (4,1) NULL, 
WindSpeedKph Decimal (4,1) NULL, 
foreign key (EventID) references Events(EventID)
);


create table Route (
RouteID INT Primary key identity (1,1), 
CategoryID int NOT NULL UNIQUE, 
RouteName varchar (50) NOT NULL, 
DistanceKm DECIMAL (5,2) NOT NULL, 
foreign key (CategoryID) references Category(CategoryID)
);


create table Registration (
RegistrationID INT Primary key identity (1,1), 
ParticipantID int NOT NULL, 
categoryID int NOT NULL, 
RegistrationDate datetime NOT NULL DEFAULT GETDATE(), 
RaceNumber varchar (10) NOT NULL UNIQUE, 
foreign key (ParticipantID) references users(UserID),
foreign key (categoryID) references Category(categoryID)
);

create table results (
ResultID int primary key identity (1,1), 
RegistrationID int NOT NULL UNIQUE, 
FinishTime Time NULL,
Position int NULL, 
Status varchar(20) NOT NULL , 
foreign key (RegistrationID) references Registration(RegistrationID)
);

create table payments (
PaymentID int primary key identity (1,1), 
RegistrationID int NOT NULL UNIQUE, 
Amount Decimal (8,2) NOT NULL, 
PaymentDate DATETIME NOT NULL, 
PaymentMethod varchar(30) NOT NULL, 
PaymentStatus varchar(20) NOT NULL, 
FOREIGN KEY (RegistrationID) REFERENCES Registration(RegistrationID)
);

ALTER TABLE payments
add constraint DF_payments_PaymentDate Default GETDATE () FOR PaymentDate;
GO

-- populaing the tables with data 

INSERT INTO users (FullName, Email, Password, Role) values 
('Tshiamo Nkosi', 'TshiamoNksoi@raceday.co.za', 'Hashed_PW_1', 'Organiser'), 
('Luyanda Xaba', 'LuyandaXaba@raceday.co.za', 'Hashed_PW_2', 'Organiser'), 
('RG Mokwena', 'RGMokwena@raceday.co.za', 'Hashed_PW_3', 'Participant'), 
('Aisha Patel', 'AishaPatel@raceday.co.za', 'Hashed_PW_4', 'Participant'); 

INSERT INTO Events (OrganiserID, EventName, Description, EventDate, location, DistanceKm, EventType) values
(1, 'Johannesburg City Run', 'Annual road running event through the Johannesburg CBD.', '2026-10-18','Johannesburg, Gauteng', 21.10, 'Run'),
(1, 'Soweto Community Walk',    'Family-friendly community charity walk.','2026-11-08', 'Soweto, Gauteng',5.00,'Walk'),
(2, 'Cape Peninsula Cycle Tour','Scenic cycling tour around the Cape Peninsula.','2026-09-27', 'Cape Town, Western Cape',109.00,'Cycle');

INSERT INTO Category (EventID, categoryName, DistanceKm, EntryFee, MinAge) VALUES
(1, '21km Half Marathon', 21.10, 250.00, 18),
(1, '10km Fun Run', 10.00, 150.00, 12),
(2, '5km Walk', 5.00,  50.00, NULL),
(3, '109km Full Tour', 109.00, 450.00, 18),
(3, '55km Half Tour', 55.00, 300.00, 16);

INSERT INTO WeatherForecast (EventID, ForecastDate, Temperature, WindSpeedKph) VALUES
(1, '2026-10-18', 21.0, 15.5),
(2, '2026-11-08', 24.0,  8.0),
(3, '2026-09-27', 17.0, 22.0);

INSERT INTO Route (CategoryID, RouteName, DistanceKm) VALUES
(1, 'CBD Loop Route', 21.10),
(2, 'CBD Short Loop', 10.00),
(3, 'Soweto Heritage Route', 5.00),
(4, 'Full Peninsula Route', 109.00),
(5, 'Half Peninsula Route', 55.00);

INSERT INTO Registration (ParticipantID, categoryID, RaceNumber) VALUES
(3, 1, 'JHB-1001'),																
(4, 2, 'JHB-1002'),																
(4, 3, 'SOW-3001');																

INSERT INTO Results (RegistrationID, FinishTime, Position, Status) VALUES
(1, '01:45:32', 152, 'Finished'),
(3, '00:52:10', 40,  'Finished');

INSERT INTO Payments (RegistrationID, Amount, PaymentMethod, PaymentStatus) VALUES
(1, 250.00, 'Card', 'Completed'),
(2, 450.00, 'EFT',  'Completed'),
(3, 150.00, 'Card', 'Completed');

select * from users;


