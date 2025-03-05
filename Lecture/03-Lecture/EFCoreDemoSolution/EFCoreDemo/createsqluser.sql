CREATE USER [UserName] WITH PASSWORD = '<strongpassword>';
EXEC sp_addrolemember 'db_datareader', 'UserName';
EXEC sp_addrolemember 'db_datawriter', 'UserName';
EXEC sp_addrolemember 'db_ddladmin', 'UserName';
GO