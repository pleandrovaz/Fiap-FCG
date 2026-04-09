-- Script para criar tabela "Users" (identificadores em PascalCase) para PostgreSQL
-- Execute com: psql "host=<HOST> port=<PORT> dbname=<DB> user=<USER> password=<PASS>" -f init_postgres.sql

-- Create table with unquoted lowercase identifier 'users' and lowercase column names (Postgres default)
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    passwordhash TEXT NOT NULL,
    role VARCHAR(50) NOT NULL DEFAULT 'User',
    createdat TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT now(),
    updatedat TIMESTAMP WITH TIME ZONE,
    isactive BOOLEAN NOT NULL DEFAULT true
);

-- Caso sua aplicação use outro schema, ajuste o schema ou altere search_path.
-- Observação: identificadores não entre aspas no PostgreSQL são convertidos para minúsculas.
-- Recomenda-se usar nomes minúsculos sem aspas para evitar problemas de sensibilidade a maiúsculas.
