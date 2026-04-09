using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Moq;

namespace FCG.Tests.Services
{
    public class UsuarioServiceTests
    {
        private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly UsuarioService _service;

        public UsuarioServiceTests()
        {
            _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
            _tokenServiceMock = new Mock<ITokenService>();
            _service = new UsuarioService(_usuarioRepositoryMock.Object, _tokenServiceMock.Object);
        }

        #region GetAllUsuariosAsync

        [Fact]
        public async Task GetAllUsuariosAsync_AsAdmin_ReturnsAllUsuarios()
        {
            var usuarios = new List<Usuario>
            {
                new() { Id = Guid.NewGuid(), Name = "Admin", Email = "admin@test.com", Role = "Admin" },
                new() { Id = Guid.NewGuid(), Name = "User1", Email = "user1@test.com", Role = "User" }
            };
            _usuarioRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuarios);

            var result = await _service.GetAllUsuariosAsync("Admin");

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data!.Count());
        }

        [Fact]
        public async Task GetAllUsuariosAsync_User_ExcluindoAdmins()
        {
            var usuarios = new List<Usuario>
            {
                new() { Id = Guid.NewGuid(), Name = "Admin", Email = "admin@test.com", Role = "Admin" },
                new() { Id = Guid.NewGuid(), Name = "User1", Email = "user1@test.com", Role = "User" }
            };
            _usuarioRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuarios);

            var result = await _service.GetAllUsuariosAsync("User");

            Assert.True(result.IsSuccess);
            Assert.Single(result.Data!);
            Assert.DoesNotContain(result.Data!, u => u.Role == "Admin");
        }

        #endregion

        #region GetUsuarioByIdAsync

        [Fact]
        public async Task GetUsuarioByIdAsync_NaoEncontrado_RetornaFalha_404()
        {
            _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Usuario?)null);

            var result = await _service.GetUsuarioByIdAsync(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task GetUsuarioByIdAsync_NonAdminAccessingAdmin_ReturnsFailure403()
        {
            var admin = new Usuario { Id = Guid.NewGuid(), Name = "Admin", Email = "admin@test.com", Role = "Admin" };
            _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(admin.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(admin);

            var result = await _service.GetUsuarioByIdAsync(admin.Id, "User");

            Assert.False(result.IsSuccess);
            Assert.Equal(403, result.StatusCode);
        }

        [Fact]
        public async Task GetUsuarioByIdAsync_AdminAccessingAdmin_ReturnsSuccess()
        {
            var admin = new Usuario { Id = Guid.NewGuid(), Name = "Admin", Email = "admin@test.com", Role = "Admin" };
            _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(admin.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(admin);

            var result = await _service.GetUsuarioByIdAsync(admin.Id, "Admin");

            Assert.True(result.IsSuccess);
            Assert.Equal(admin.Id, result.Data!.Id);
        }

        #endregion

        #region CreateUsuarioAsync

        [Fact]
        public async Task CreateUsuarioAsync_ValidData_ReturnsSuccess201()
        {
            var dto = new RegisterUsuarioDTO
            {
                Name = "Test",
                Email = "test@email.com",
                Password = "Abc@1234"
            };
            _usuarioRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Usuario?)null);

            var result = await _service.CreateUsuarioAsync(dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(201, result.StatusCode);
            _usuarioRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateUsuarioAsync_DuplicateEmail_ReturnsFailure409()
        {
            var dto = new RegisterUsuarioDTO
            {
                Name = "Test",
                Email = "existing@email.com",
                Password = "Abc@1234"
            };
            _usuarioRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Usuario { Email = dto.Email });

            var result = await _service.CreateUsuarioAsync(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(409, result.StatusCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("invalid-email")]
        [InlineData("no@dots")]
        public async Task CreateUsuarioAsync_InvalidEmail_ReturnsFailure400(string? email)
        {
            var dto = new RegisterUsuarioDTO
            {
                Name = "Test",
                Email = email,
                Password = "Abc@1234"
            };

            var result = await _service.CreateUsuarioAsync(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("short")]
        [InlineData("noespecial1")]
        [InlineData("NoNumbers!")]
        [InlineData("12345678!")]
        public async Task CreateUsuarioAsync_PasswordNaoSegura_RetornaFalha_400(string? password)
        {
            var dto = new RegisterUsuarioDTO
            {
                Name = "Test",
                Email = "test@email.com",
                Password = password
            };

            var result = await _service.CreateUsuarioAsync(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        #endregion

        #region AuthenticateAsync

        [Fact]
        public async Task AuthenticateAsync_CredenciaisValidas_RetornaToken()
        {
            var loginDto = new LoginDTO { Email = "test@email.com", Password = "Abc@1234" };
            var usuario = new Usuario { Id = Guid.NewGuid(), Email = loginDto.Email, Role = "User" };
            _usuarioRepositoryMock.Setup(r => r.GetByEmailAndPasswordAsync(loginDto.Email, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);
            _tokenServiceMock.Setup(t => t.GenerateToken(usuario)).Returns("fake-token");

            var result = await _service.AuthenticateAsync(loginDto);

            Assert.True(result.IsSuccess);
            Assert.Equal("fake-token", result.Data!.Token);
        }

        [Fact]
        public async Task AuthenticateAsync_CredenciaisInvalidas_ReturnsFailure401()
        {
            var loginDto = new LoginDTO { Email = "test@email.com", Password = "WrongPass1!" };
            _usuarioRepositoryMock.Setup(r => r.GetByEmailAndPasswordAsync(loginDto.Email, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Usuario?)null);

            var result = await _service.AuthenticateAsync(loginDto);

            Assert.False(result.IsSuccess);
            Assert.Equal(401, result.StatusCode);
        }

        #endregion

        #region UpdateUsuarioAsync

        [Fact]
        public async Task UpdateUsuarioAsync_DataValida_RetornoSucesso()
        {
            var id = Guid.NewGuid();
            var dto = new RegisterUsuarioDTO
            {
                Name = "Updated",
                Email = "updated@email.com",
                Password = "NewPass1!"
            };
            _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Usuario { Id = id, Name = "Old", Email = "old@email.com" });

            var result = await _service.UpdateUsuarioAsync(id, dto);

            Assert.True(result.IsSuccess);
            _usuarioRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUsuarioAsync_NaoEncontrado_RetornaFalha_404()
        {
            var dto = new RegisterUsuarioDTO
            {
                Name = "Test",
                Email = "test@email.com",
                Password = "Abc@1234"
            };
            _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Usuario?)null);

            var result = await _service.UpdateUsuarioAsync(Guid.NewGuid(), dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task UpdateUsuarioAsync_EmailInvalido_RetornoFalha_400()
        {
            var dto = new RegisterUsuarioDTO
            {
                Name = "Test",
                Email = "invalid-email",
                Password = "Abc@1234"
            };

            var result = await _service.UpdateUsuarioAsync(Guid.NewGuid(), dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task UpdateUsuarioAsync_SenhaInsegura_ReturnoFalha_400()
        {
            var dto = new RegisterUsuarioDTO
            {
                Name = "Test",
                Email = "test@email.com",
                Password = "weak"
            };

            var result = await _service.UpdateUsuarioAsync(Guid.NewGuid(), dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        #endregion

        #region DeleteUsuarioAsync

        [Fact]
        public async Task DeleteUsuarioAsync_Existe_ReturnoSucesso()
        {
            var id = Guid.NewGuid();
            _usuarioRepositoryMock.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _service.DeleteUsuarioAsync(id);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task DeleteUsuarioAsync_NaoEncontrado_RetornaFalha_404()
        {
            _usuarioRepositoryMock.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _service.DeleteUsuarioAsync(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        #endregion

        #region EsqueciSenha

        [Fact]
        public async Task EsqueciSenha_EmailValido_RetornaSucessoComToken()
        {
            var usuario = new Usuario { Id = Guid.NewGuid(), Email = "test@email.com", Name = "Test" };
            _usuarioRepositoryMock.Setup(r => r.GetByEmailAsync("test@email.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);
            _usuarioRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _service.ForgotPasswordAsync(new ForgotPasswordDTO { Email = "test@email.com" });

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data!.ResetToken);
            Assert.NotEmpty(result.Data.ResetToken);
            _usuarioRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Usuario>(u =>
                u.PasswordResetToken != null && u.PasswordResetTokenExpiration != null), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task EsqueciSenha_EmailNaoEncontrado_RetornaFalha_404()
        {
            _usuarioRepositoryMock.Setup(r => r.GetByEmailAsync("naoexiste@email.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Usuario?)null);

            var result = await _service.ForgotPasswordAsync(new ForgotPasswordDTO { Email = "naoexiste@email.com" });

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("invalid-email")]
        public async Task EsqueciSenha_EmailInvalido_RetornaFalha_400(string? email)
        {
            var result = await _service.ForgotPasswordAsync(new ForgotPasswordDTO { Email = email });

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        #endregion

        #region ResetPasswordAsync

        [Fact]
        public async Task RResetarSenha_TokenValido_RetornaSucesso()
        {
            var token = "valid-token-123";
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = "test@email.com",
                PasswordResetToken = token,
                PasswordResetTokenExpiration = DateTime.UtcNow.AddMinutes(30)
            };
            _usuarioRepositoryMock.Setup(r => r.GetByEmailAsync("test@email.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);
            _usuarioRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var dto = new ResetPasswordDTO { Email = "test@email.com", Token = token, NewPassword = "NovaSenha@1" };
            var result = await _service.ResetPasswordAsync(dto);

            Assert.True(result.IsSuccess);
            _usuarioRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Usuario>(u =>
                u.PasswordResetToken == null && u.PasswordResetTokenExpiration == null), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RResetarSenha_TokenExpirado_RetornaFalha_400()
        {
            var token = "expired-token";
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = "test@email.com",
                PasswordResetToken = token,
                PasswordResetTokenExpiration = DateTime.UtcNow.AddHours(-1)
            };
            _usuarioRepositoryMock.Setup(r => r.GetByEmailAsync("test@email.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);

            var dto = new ResetPasswordDTO { Email = "test@email.com", Token = token, NewPassword = "NovaSenha@1" };
            var result = await _service.ResetPasswordAsync(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task RResetarSenha_TokenInvalido_RetornaFalha_400()
        {
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = "test@email.com",
                PasswordResetToken = "token-correto",
                PasswordResetTokenExpiration = DateTime.UtcNow.AddMinutes(30)
            };
            _usuarioRepositoryMock.Setup(r => r.GetByEmailAsync("test@email.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);

            var dto = new ResetPasswordDTO { Email = "test@email.com", Token = "token-errado", NewPassword = "NovaSenha@1" };
            var result = await _service.ResetPasswordAsync(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task RResetarSenha_UsuarioNaoEncontrado_RetornaFalha_404()
        {
            _usuarioRepositoryMock.Setup(r => r.GetByEmailAsync("naoexiste@email.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Usuario?)null);

            var dto = new ResetPasswordDTO { Email = "naoexiste@email.com", Token = "qualquer", NewPassword = "NovaSenha@1" };
            var result = await _service.ResetPasswordAsync(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("invalid")]
        public async Task RResetarSenha_EmailInvalido_RetornaFalha_400(string? email)
        {
            var dto = new ResetPasswordDTO { Email = email, Token = "token", NewPassword = "NovaSenha@1" };
            var result = await _service.ResetPasswordAsync(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("curta1!")]
        [InlineData("semcaracterespecial1")]
        [InlineData("SemNumero!")]
        public async Task RResetarSenha_SenhaInsegura_RetornaFalha_400(string? newPassword)
        {
            var dto = new ResetPasswordDTO { Email = "test@email.com", Token = "token", NewPassword = newPassword };
            var result = await _service.ResetPasswordAsync(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task RResetarSenha_SemTokenGerado_RetornaFalha_400()
        {
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = "test@email.com",
                PasswordResetToken = null,
                PasswordResetTokenExpiration = null
            };
            _usuarioRepositoryMock.Setup(r => r.GetByEmailAsync("test@email.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);

            var dto = new ResetPasswordDTO { Email = "test@email.com", Token = "qualquer", NewPassword = "NovaSenha@1" };
            var result = await _service.ResetPasswordAsync(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        #endregion
    }
}
