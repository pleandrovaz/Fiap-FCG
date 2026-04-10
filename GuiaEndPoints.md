# 📡 FCG API — Guia de Uso dos Endpoints

## Autenticação

Todas as rotas protegidas exigem o header:

```
Authorization: Bearer <token>
```

---

## Auth

### Registrar usuário

```
Basta cadastrar e pegar o token para loga a API.
POST /api/Auth/register
```

```json
{
  "name": "João Silva",
  "email": "joao@email.com",
  "password": "Senha@123"
}
```

> Senha: mínimo 8 caracteres, com letras, números e caracteres especiais.

**Resposta:** `201 Created` com os dados do usuário.

---

### Login

```
POST /api/Auth/login
```

```json
{
  "email": "joao@email.com",
  "password": "Senha@123"
}
```

**Resposta:** `200 OK`

```json
{
  "token": "eyJhbGciOi...",
  "usuario": {
    "id": "guid",
    "name": "João Silva",
    "email": "joao@email.com",
    "role": "User"
  },
  "expiresAt": "2025-01-01T08:00:00Z"
}
```

---

### Obter usuário por ID

```
GET /api/Auth/user/{id}
```

**Resposta:** `200 OK` com os dados do usuário.

---

### Recuperar senha (Solicitar token)

```
POST /api/Auth/recuperar-senha
```

🔓 Público.

```json
{
  "email": "joao@email.com"
}
```

**Resposta:** `200 OK`

```json
{
  "message": "Token de recuperação de senha gerado com sucesso",
  "resetToken": "base64-token-gerado..."
}
```

> ⚠️ Em produção o token seria enviado por e-mail. Na versão atual ele é retornado na resposta para fins de desenvolvimento/testes.
> O token expira em **1 hora**.

---

### Redefinir senha (com token)

```
POST /api/Auth/reset-password
```

🔓 Público.

```json
{
  "email": "joao@email.com",
  "token": "base64-token-recebido...",
  "newPassword": "NovaSenha@456"
}
```

> A nova senha deve ter no mínimo 8 caracteres, com letras, números e caracteres especiais.

**Resposta:** `200 OK`

```json
{
  "message": "Senha alterada com sucesso"
}
```

**Erros possíveis:**

| Código | Cenário |
|---|---|
| `400` | Token inválido, expirado ou senha não atende critérios |
| `404` | E-mail não encontrado |

---

## Jogos

### Listar todos os jogos

```
GET /api/Jogos
```

🔓 Público — não requer autenticação.

**Resposta:** `200 OK` com array de jogos.

---

### Obter jogo por ID

```
GET /api/Jogos/{id}
```

🔓 Público.

**Resposta:** `200 OK`

```json
{
  "id": "guid",
  "nome": "The Legend of Zelda",
  "descricao": "Aventura épica em mundo aberto",
  "preco": 299.90,
  "ativo": true
}
```

---

### Criar jogo

```
POST /api/Jogos
```

🔒 Requer role **Admin**.

```json
{
  "nome": "The Legend of Zelda",
  "descricao": "Aventura épica em mundo aberto",
  "preco": 299.90
}
```

**Resposta:** `201 Created`

---

### Atualizar jogo

```
PUT /api/Jogos/{id}
```

🔒 Requer role **Admin**.

```json
{
  "nome": "The Legend of Zelda: TOTK",
  "descricao": "Aventura épica em mundo aberto atualizada",
  "preco": 349.90,
  "ativo": true
}
```

**Resposta:** `204 No Content`

---

### Excluir jogo

```
DELETE /api/Jogos/{id}
```

🔒 Requer role **Admin**.

**Resposta:** `204 No Content`

---

## Promoções

### Listar todas as promoções

```
GET /api/Promocoes
```

🔓 Público.

**Resposta:** `200 OK` com array de promoções.

---

### Obter promoção por ID

```
GET /api/Promocoes/{id}
```

🔓 Público.

**Resposta:** `200 OK`

```json
{
  "id": "guid",
  "nome": "Black Friday",
  "idJogo": "guid",
  "jogoNome": "The Legend of Zelda",
  "tipoDesconto": 1,
  "valor": 30.0,
  "dataInicio": "2025-11-20T00:00:00",
  "dataFim": "2025-11-30T23:59:59",
  "ativa": true
}
```

---

### Criar promoção

```
POST /api/Promocoes
```

🔒 Requer role **Admin**.

```json
{
  "nome": "Black Friday",
  "idJogo": "guid-do-jogo",
  "tipoDesconto": 1,
  "valor": 30.0,
  "dataInicio": "2025-11-20T00:00:00",
  "dataFim": "2025-11-30T23:59:59"
}
```

> **tipoDesconto:** `1` = Percentual | `2` = Valor Fixo

**Resposta:** `201 Created`

---

### Atualizar promoção

```
PUT /api/Promocoes/{id}
```

🔒 Requer role **Admin**.

```json
{
  "nome": "Black Friday Extended",
  "idJogo": "guid-do-jogo",
  "tipoDesconto": 1,
  "valor": 40.0,
  "dataInicio": "2025-11-20T00:00:00",
  "dataFim": "2025-12-05T23:59:59"
}
```

**Resposta:** `204 No Content`

---

### Excluir promoção

```
DELETE /api/Promocoes/{id}
```

🔒 Requer role **Admin**.

**Resposta:** `204 No Content`

---

## Usuários

### Listar todos os usuários

```
GET /api/Usuarios
```

🔒 Requer autenticação.

**Resposta:** `200 OK` com array de usuários.

---

### Obter usuário por ID

```
GET /api/Usuarios/{id}
```

🔒 Requer autenticação. Usuários comuns só podem consultar seus próprios dados.

**Resposta:** `200 OK`

```json
{
  "id": "guid",
  "name": "João Silva",
  "email": "joao@email.com",
  "role": "User"
}
```

---

### Atualizar usuário

```
PUT /api/Usuarios/{id}
```

🔒 Requer autenticação. Usuários comuns só podem atualizar seus próprios dados.

```json
{
  "name": "João Silva Atualizado",
  "email": "joao.novo@email.com",
  "password": "NovaSenha@456"
}
```

**Resposta:** `204 No Content`

---

### Excluir usuário

```
DELETE /api/Usuarios/{id}
```

🔒 Requer role **Admin**.

**Resposta:** `204 No Content`

---

## Biblioteca de Jogos

### Listar minha biblioteca

```
GET /api/BibliotecaJogos
```

🔒 Requer autenticação. Retorna os jogos do usuário logado.

**Resposta:** `200 OK`

```json
[
  {
    "id": "guid",
    "usuarioId": "guid",
    "jogoId": "guid",
    "jogoNome": "The Legend of Zelda",
    "dataCompra": "2025-01-15T10:30:00",
    "precoPago": 209.93,
    "promocaoId": "guid-ou-null"
  }
]
```

---

### Obter item da biblioteca por ID

```
GET /api/BibliotecaJogos/{id}
```

🔒 Requer autenticação.

**Resposta:** `200 OK`

---

### Adicionar jogo à biblioteca

```
POST /api/BibliotecaJogos
```

🔒 Requer autenticação.

```json
{
  "jogoId": "guid-do-jogo",
  "promocaoId": "guid-da-promocao-ou-null"
}
```

> `promocaoId` é opcional. Se informado, o desconto da promoção será aplicado ao preço.

**Resposta:** `201 Created`

---

### Remover jogo da biblioteca

```
DELETE /api/BibliotecaJogos/{id}
```

🔒 Requer autenticação.

**Resposta:** `204 No Content`

---

## Códigos de Erro Comuns

| Código | Significado |
|---|---|
| `400` | Dados inválidos (validação falhou) |
| `401` | Não autenticado (token ausente ou inválido) |
| `403` | Sem permissão (role insuficiente ou acesso a recurso de outro usuário) |
| `404` | Recurso não encontrado |
| `409` | Conflito (ex.: e-mail já cadastrado) |
| `500` | Erro interno do servidor |

Todas as respostas de erro seguem o formato:

```json
{
  "message": "Descrição do erro"
}
```
