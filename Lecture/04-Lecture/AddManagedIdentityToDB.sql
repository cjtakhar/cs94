-- The <user principal name> can be the user assigned managed identity name
-- the Azure Active Directory Group Name the managed identity is in or
-- The App Service name if the managed identity is a system assigned managed identity

CREATE USER [<user principal name>] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [<user principal name>];
ALTER ROLE db_datawriter ADD MEMBER [<user principal name>];
ALTER ROLE db_ddladmin ADD MEMBER [<user principal name>];

GO