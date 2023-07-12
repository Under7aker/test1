CREATE TABLE Produs (
    Id VARCHAR(200) PRIMARY KEY,
    Name VARCHAR(50) NOT NULL,
    CategoryName VARCHAR(50) NOT NULL,
    MeasureUnitName VARCHAR(50) NOT NULL,
    UnitPriceWithVat DECIMAL(50) NOT NULL,
    Uid VARCHAR(200) NOT NULL,
    VatName VARCHAR(50)NOT NULL,
    RepositoryName VARCHAR(50) NOT NULL
);

INSERT INTO Produs (Id, Name, CategoryName, MeasureUnitName, UnitPriceWithVat, Uid, VatName, RepositoryName)
VALUES ('1', 'Lapte', 'Lactate', 'cutie', 5.99, '789012', '0.19', 'Depozit B');

INSERT INTO Produs (Id, Name, CategoryName, MeasureUnitName, UnitPriceWithVat, Uid, VatName, RepositoryName)
VALUES ('2', 'Carne de porc', 'Carne', 'kg', 10.99, '789013', '0.19', 'Depozit C');

INSERT INTO Produs (Id, Name, CategoryName, MeasureUnitName, UnitPriceWithVat, Uid, VatName, RepositoryName)
VALUES ('3', 'Mar', 'Fructe', 'kg', 4.99, '789014', '0.19', 'Depozit D');

INSERT INTO Produs (Id, Name, CategoryName, MeasureUnitName, UnitPriceWithVat, Uid, VatName, RepositoryName)
VALUES ('4', 'Ciuperci', 'Alimente la conserva', 'Bucata', 7.99, '789015', '0.19', 'Depozit A');
