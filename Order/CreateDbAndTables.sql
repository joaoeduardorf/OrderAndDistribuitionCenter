-- Database: OrderDB

-- DROP DATABASE IF EXISTS "OrderDB";

CREATE DATABASE "OrderDB"
    WITH
    OWNER = root
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.utf8'
    LC_CTYPE = 'en_US.utf8'
    LOCALE_PROVIDER = 'libc'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;

CREATE TABLE IF NOT EXISTS "Orders" (
    "Id" SERIAL PRIMARY KEY,
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "OrderItems" (
    "ItemId" SERIAL PRIMARY KEY,
    "OrderId" INT NOT NULL,
    "IdSku" VARCHAR(50) NOT NULL,
    "DistributionCenter" VARCHAR(100),
    CONSTRAINT "FK_Order_OrderItem" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE CASCADE
);
