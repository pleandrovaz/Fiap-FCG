-- migration.sql
BEGIN;

-- Verificar se a tabela existe e recriar
DROP TABLE IF EXISTS "users";

CREATE TABLE "Users" (
    "id" SERIAL PRIMARY KEY,
    "name" VARCHAR(100) NOT NULL,
    "email" VARCHAR(100) NOT NULL UNIQUE,
    "passwordhash" VARCHAR(255) NOT NULL,
    "role" VARCHAR(50) NOT NULL DEFAULT 'User',
    "createdat" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedat" TIMESTAMP,
    "isactive" BOOLEAN NOT NULL DEFAULT TRUE
);

-- Índices
CREATE INDEX idx_users_email ON "Users"("email");
CREATE INDEX idx_users_isactive ON "Users"("isactive");

-- Comentários (opcional)
COMMENT ON TABLE "Users" IS 'Tabela de usuários do sistema';
COMMENT ON COLUMN "Users"."id" IS 'identificador único do usuário';
COMMENT ON COLUMN "Users"."name" IS 'Nome completo do usuário';
COMMENT ON COLUMN "Users"."email" IS 'E-mail do usuário (único)';
COMMENT ON COLUMN "Users"."passwordhash" IS 'Hash da senha (SHA256)';
COMMENT ON COLUMN "Users"."role" IS 'Papel do usuário (Admin/User)';
COMMENT ON COLUMN "Users"."createdat" IS 'Data de criação do registro';
COMMENT ON COLUMN "Users"."updatedat" IS 'Data da última atualização';
COMMENT ON COLUMN "Users"."isactive" IS 'Registro ativo (soft delete)';

-- Inserir dados iniciais
INSERT INTO "Users" ("name", "email", "passwordhash", "role") VALUES
    ('Admin User', 'admin@example.com', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 'Admin'),
    ('Usuário Teste', 'user@example.com', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 'User');

COMMIT;


-- CREATE TABLE Users (
    -- id INT idENTITY(1,1) PRIMARY KEY,
    -- name NVARCHAR(100) NOT NULL,
    -- email NVARCHAR(100) NOT NULL UNIQUE,
    -- passwordhash NVARCHAR(255) NOT NULL,
    -- role NVARCHAR(50) NOT NULL DEFAULT 'User',
    -- createdat DATETIME NOT NULL DEFAULT GETDATE(),
    -- updatedat DATETIME NULL,
    -- isactive BIT NOT NULL DEFAULT 1
-- );

-- -- Inserir usuário admin (senha: admin123)
-- INSERT INTO Users (name, email, passwordhash, role, createdat, isactive)
-- VALUES (
    -- 'Admin User',
    -- 'admin@example.com',
    -- 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', -- admin123 em base64
    -- 'Admin',
    -- GETDATE(),
    -- 1
-- );
