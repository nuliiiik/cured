-----------------------------------------------------------
-- ЧАСТЬ 1: СОЗДАНИЕ СТРУКТУРЫ ТАБЛИЦ
-----------------------------------------------------------

-- 1. Пользователи
CREATE TABLE [Users] (
    [UserID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Login] NVARCHAR(50) NOT NULL,
    [Password] NVARCHAR(50) NOT NULL,
    [FullName] NVARCHAR(100) NULL,
    [Phone] NVARCHAR(20) NULL,
    [Role] NVARCHAR(20) DEFAULT 'user'
);

-- 2. Мотоциклы
CREATE TABLE [Motorcycles] (
    [MotorcycleID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Brand] NVARCHAR(50) NOT NULL,
    [Model] NVARCHAR(50) NOT NULL,
    [Year] INT NULL,
    [Price] DECIMAL(18, 2) NOT NULL,
    [Quantity] INT DEFAULT 0,
    [ImageURL] NVARCHAR(MAX) NULL,
    [Description] NVARCHAR(MAX) NULL
);

-- 3. Запчасти
CREATE TABLE [Parts] (
    [PartID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [PartName] NVARCHAR(100) NOT NULL,
    [Brand] NVARCHAR(50) NULL,
    [Price] DECIMAL(18, 2) NOT NULL,
    [Quantity] INT DEFAULT 0,
    [ForModels] NVARCHAR(MAX) NULL,
    [ImageURL] NVARCHAR(MAX) NULL,
    [Description] NVARCHAR(MAX) NULL
);

-- 4. Корзина
CREATE TABLE [Cart] (
    [CartID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserID] INT NOT NULL,
    [MotorcycleID] INT NULL,
    [PartID] INT NULL,
    [Quantity] INT NOT NULL DEFAULT 1,
    [AddedDate] DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Cart_Users FOREIGN KEY (UserID) REFERENCES [Users](UserID),
    CONSTRAINT FK_Cart_Motorcycles FOREIGN KEY (MotorcycleID) REFERENCES [Motorcycles](MotorcycleID),
    CONSTRAINT FK_Cart_Parts FOREIGN KEY (PartID) REFERENCES [Parts](PartID)
);

-- 5. Заказы
CREATE TABLE [Orders] (
    [OrderID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserID] INT NOT NULL,
    [OrderDate] DATETIME DEFAULT GETDATE(),
    [TotalAmount] DECIMAL(18, 2) NOT NULL,
    [Status] NVARCHAR(50) DEFAULT N'Новый',
    [Phone] NVARCHAR(20) NULL,
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserID) REFERENCES [Users](UserID)
);

-- 6. Состав заказа
CREATE TABLE [OrderItems] (
    [OrderItemID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [OrderID] INT NOT NULL,
    [MotorcycleID] INT NULL,
    [PartID] INT NULL,
    [Quantity] INT NOT NULL,
    [Price] DECIMAL(18, 2) NOT NULL,
    CONSTRAINT FK_Items_Orders FOREIGN KEY (OrderID) REFERENCES [Orders](OrderID),
    CONSTRAINT FK_Items_Motorcycles FOREIGN KEY (MotorcycleID) REFERENCES [Motorcycles](MotorcycleID),
    CONSTRAINT FK_Items_Parts FOREIGN KEY (PartID) REFERENCES [Parts](PartID)
);
GO

-----------------------------------------------------------
-- ЧАСТЬ 2: ЗАПОЛНЕНИЕ ДАННЫМИ
-----------------------------------------------------------

-- 1. ПОЛЬЗОВАТЕЛИ
SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([UserID], [Login], [Password], [FullName], [Phone], [Role]) VALUES
(2, 'test', 'test', N'костя емае', '7-913-123-45-67', 'user'),
(4, 'admin1', 'admin123', N'Admin User', '000-00-00', 'admin'),
(5, N'такса', N'такса', N'такса', '7-800-555-35-35', 'user'),
(6, 'WW', 'WW', N'WWadmin', '7-812-812-12-12', 'admin'),
(7, '1234567890', '1234567890', N'Серёга', '7-933-930-81-34', 'user'),
(8, N'Шнейне', N'ватафа', N'Пэпэ', '7-123-478-90-89', 'user'),
(9, '1', '1', '1', '7-0 - - ', 'user');
SET IDENTITY_INSERT [Users] OFF;

-- 2. МОТОЦИКЛЫ
SET IDENTITY_INSERT [Motorcycles] ON;
INSERT INTO [Motorcycles] ([MotorcycleID], [Brand], [Model], [Year], [Price], [Quantity], [ImageURL], [Description]) VALUES
(1, 'Honba', 'CBR 600 RR', 2022, 1250000.00, 11, '/images/motocycles/Honba.png', N'Легендарный суперспорт от Honda. Идеальный баланс мощности и управляемости для трека и города. Оснащен передовой электроникой и двухканальной ABS.'),
(2, 'Yamaka', 'YZF-R1', 2023, 1850000.00, 11, '/images/motocycles/Yamaha.png', N'Флагманский спортбайк Yamaha с двигателем Crossplane. Невероятная динамика, титановый выхлоп и продвинутая система контроля тяги для профессионалов.'),
(3, 'Kashasaki', 'Ninja 400', 2022, 650000.00, 11, '/images/motocycles/Kawasaki.png', N'Компактный и легкий спортивный мотоцикл, идеальный для начинающих. Обладает агрессивным дизайном серии Ninja и отличной эргономикой.'),
(4, 'Suzukl', 'GSX-S750', 2021, 890000.00, 11, '/images/motocycles/Suzuki.png', N'Мощный городской нейкед с двигателем от легендарного GSX-R. Сочетает в себе агрессивный стиль и комфортную вертикальную посадку.'),
(5, 'BWM', 'S1000RR', 2023, 2250000.00, 11, '/images/motocycles/BMW.png', N'Премиальный немецкий супербайт. Технологическое совершенство: динамическая регулировка подвески, ShiftCam и более 200 лошадиных сил.'),
(6, 'Dukati', 'Panigale V2', 2022, 1650000.00, 11, '/images/motocycles/Ducati.png', N'Итальянское произведение искусства. Двухцилиндровый двигатель Superquadro и изысканный дизайн, подчеркивающий гоночную родословную Ducati.'),
(7, 'Horley-Davibson', 'Iron 883', 2021, 980000.00, 11, '/images/motocycles/Harley-Davidson.png', N'Классический американский круизер в стиле Dark Custom. Минималистичный дизайн, мощный звук V-Twin и неповторимый характер Harley-Davidson.'),
(8, 'TKM', '390 Duke', 2023, 520000.00, 11, '/images/motocycles/KTM.png', N'Король городских джунглей. Легкая стальная рама, отзывчивый одноцилиндровый двигатель и топовые компоненты подвески WP.'),
(9, 'Triumf', 'Street Triple', 2022, 1150000.00, 11, '/images/motocycles/Triumph.png', N'Уникальный британский стритфайтер с трехцилиндровым двигателем. Невероятный крутящий момент и лучшая в классе управляемость.'),
(10, 'Aprilya', 'RS 660', 2022, 1050000.00, 11, '/images/motocycles/Aprilia.png', N'Новое слово в классе среднекубатурных спортбайков. Легкий, технологичный, с развитой аэродинамикой и электроникой уровня топовых супербайков.');
SET IDENTITY_INSERT [Motorcycles] OFF;

-- 3. ЗАПЧАСТИ
SET IDENTITY_INSERT [Parts] ON;
INSERT INTO [Parts] ([PartID], [PartName], [Brand], [Price], [Quantity], [ForModels], [ImageURL], [Description]) VALUES
(1, N'Тормозные колодки', 'Brembo', 4500.00, 111, 'Honda CBR, Yamaha R6', '/images/parts/Тормозные колодки.png', N'Спеченные металлические колодки с высоким коэффициентом трения. Обеспечивают минимальный тормозной путь и устойчивы к перегреву при агрессивной езде.'),
(2, N'Масло моторное', 'Motul', 1200.00, 111, N'Универсальное', '/images/parts/Масло моторное.png', N'Полностью синтетическое масло высшего класса. Гарантирует максимальную защиту двигателя и плавное переключение передач в мокром сцеплении.'),
(3, N'Цепь', 'DID', 8500.00, 111, N'520 размер', '/images/parts/Цепь.png', N'Усиленная приводная цепь с уплотнениями типа X-Ring. Обладает повышенным ресурсом и низким коэффициентом трения для передачи максимальной мощности.'),
(4, N'Свечи', 'NGK', 450.00, 111, 'CR8E', '/images/parts/Свечи.png', N'Иридиевые свечи зажигания с тонким центральным электродом. Обеспечивают стабильную искру, улучшенный запуск и экономию топлива.'),
(5, N'Фильтр воздушный', 'K&N', 3200.00, 111, N'Спортивные', '/images/parts/Фильтр воздушный.png', N'Многоразовый фильтр с высоким уровнем фильтрации и увеличенным воздушным потоком. Позволяет двигателю "дышать" свободнее.'),
(6, N'Диски тормозные', 'Galfer', 12500.00, 111, 'Wave', '/images/parts/Диски тормозные.png', N'Легкосплавный плавающий диск из высокоуглеродистой стали. Специальная перфорация эффективно отводит тепло и очищает поверхность колодок.'),
(7, N'Сцепление', 'Barnett', 8900.00, 111, N'Усиленное', '/images/parts/Сцепление.png', N'Комплект усиленных фрикционных дисков. Устойчивы к пробуксовке под нагрузкой, идеально подходят для замены оригинальных деталей.'),
(8, N'Покрышка', 'Michelin', 14500.00, 111, 'Road 5', '/images/parts/Покрышка.png', N'Двухкомпонентная резина для спортивно-туристических поездок. Отличный зацеп в наклоне и высокая износостойкость центральной части.'),
(9, N'Аккумулятор', 'Yuasa', 6800.00, 111, '12V', '/images/parts/Аккумулятор.png', N'Герметичный AGM аккумулятор с высоким пусковым током. Устойчив к вибрациям и не требует обслуживания в процессе эксплуатации.'),
(10, N'Тормозная жидкость', 'Castrol', 850.00, 111, 'DOT 4', '/images/parts/Тормозная жидкость.png', N'Высокотемпературная жидкость стандарта DOT 4. Сохраняет свои свойства при экстремальных нагрузках, предотвращая провалы рычага тормоза.');
SET IDENTITY_INSERT [Parts] OFF;

-- 4. КОРЗИНА
SET IDENTITY_INSERT [Cart] ON;
INSERT INTO [Cart] ([CartID], [UserID], [MotorcycleID], [PartID], [Quantity], [AddedDate]) VALUES
(14, 6, 10, NULL, 5, '2026-03-19 21:33:24.180');
SET IDENTITY_INSERT [Cart] OFF;

-- 5. ЗАКАЗЫ
SET IDENTITY_INSERT [Orders] ON;
INSERT INTO [Orders] ([OrderID], [UserID], [OrderDate], [TotalAmount], [Status], [Phone]) VALUES
(37, 2, '2026-03-25 21:31:45.060', 17800.00, N'Отменен', '7-913-123-45-67'),
(38, 2, '2026-03-25 21:40:43.173', 9600.00, N'Новый', '7-913-123-45-67');
SET IDENTITY_INSERT [Orders] OFF;

-- 6. СОСТАВ ЗАКАЗА
SET IDENTITY_INSERT [OrderItems] ON;
INSERT INTO [OrderItems] ([OrderItemID], [OrderID], [MotorcycleID], [PartID], [Quantity], [Price]) VALUES
(1, 38, NULL, 5, 3, 3200.00);
SET IDENTITY_INSERT [OrderItems] OFF;

GO