using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiAutores.DTOs.DTOBase;
using WebApiAutores.DTOs.DTOSECURITY;
using WebApiAutores.Servicios;

namespace WebApiAutores.Controllers.V2
{
    [ApiController]
    [Route("api/V2/cuentas")]
    public class CuentasController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly HashService hashService;
        private readonly IDataProtector dataProtector;

        public CuentasController(UserManager<IdentityUser> userManager, IConfiguration configuration, SignInManager<IdentityUser> signInManager,
            IDataProtectionProvider dataProtectionProvider, HashService hashService)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.hashService = hashService;
            dataProtector = dataProtectionProvider.CreateProtector("valor_unico_y_secreto");
        }

        [HttpGet("hash/{textoplano}")]
        public ActionResult RealizarHash(string textoplano)
        {
            var resultado1 = hashService.Hash(textoplano);
            var resultado2 = hashService.Hash(textoplano);

            return Ok(new
            {
                TextoPlano = textoplano,
                Hash1 = resultado1,
                Hash2 = resultado2,
            });
        }


        [HttpGet("Encriptar/{textoplano}")]
        public ActionResult Encriptar(string textoplano)
        {
            var texto_plano = textoplano;
            var texto_cifrado = dataProtector.Protect(texto_plano);
            var texto_descifrado = dataProtector.Unprotect(texto_cifrado);

            return Ok(new
            {
                texto_plano,
                texto_cifrado,
                texto_descifrado
            });
        }

        [HttpGet("EncriptarPorTiempo/{textoplano}")]
        public ActionResult EncriptarPorTiempo(string textoplano)
        {
            var protectorLimitadoPorTiempo = dataProtector.ToTimeLimitedDataProtector();

            var texto_plano = textoplano;
            var texto_cifrado = protectorLimitadoPorTiempo.Protect(texto_plano, lifetime: TimeSpan.FromSeconds(5));

            Thread.Sleep(6000);

            var texto_descifrado = protectorLimitadoPorTiempo.Unprotect(texto_cifrado);

            return Ok(new
            {
                texto_plano,
                texto_cifrado,
                texto_descifrado
            });
        }



        [Route("registrar", Name = "registrarUsuariov2")] // api/cuentas/registrarç
        [HttpPost]
        public async Task<ActionResult<RespuestaAutenticacion>> Registrar(CredencialesUsuario credencialesUsuario)
        {
            var usuario = new IdentityUser
            {
                UserName = credencialesUsuario.Email,
                Email = credencialesUsuario.Email,
            };

            var resultado = await userManager.CreateAsync(usuario, credencialesUsuario.Password);


            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuario);
            }
            else
            {
                return BadRequest(resultado.Errors);

            }

        }

        [HttpPost("login", Name = "loginUsuariov2")]
        public async Task<ActionResult<RespuestaAutenticacion>> Login(CredencialesUsuario credencialesUsuario)
        {
            var resultado = await signInManager.PasswordSignInAsync(credencialesUsuario.Email, credencialesUsuario.Password, isPersistent: false, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuario);
            }
            else
            {
                return BadRequest("Credenciales inválidas.");
            }

        }

        [HttpGet("RenovarToken", Name = "renovarTokenv2")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<RespuestaAutenticacion>> Renovar()
        {
            //--------------------------------------------
            // esto solo se puede hacer bajo un authorize
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type.Contains("emailaddres")).FirstOrDefault(); // se puede limpiar el mapeo de los claims // los tipos aparecen con un nombre largo
            var email = emailClaim.Value;

            //var usuario = userManager.FindByEmailAsync(email);
            //var usuarioId = usuario.Id;

            //--------------------------------------------
            var credenciales = new CredencialesUsuario()
            {
                Email = email,
            };

            return await ConstruirToken(credenciales);
        }



        private async Task<RespuestaAutenticacion> ConstruirToken(CredencialesUsuario credencialesUsuario)
        {
            var claims = new List<Claim>
            {
                new Claim("email",credencialesUsuario.Email),
                new Claim("otro claim","otro valor"),
            };

            var usuario = await userManager.FindByEmailAsync(credencialesUsuario.Email);

            var claimsDB = await userManager.GetClaimsAsync(usuario);

            claims.AddRange(claimsDB);



            var expiracion = DateTime.UtcNow.AddSeconds(300);

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llavejwt"]));

            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims, expires: expiracion, signingCredentials: creds);

            return new RespuestaAutenticacion()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiracion = expiracion
            };
        }

        [HttpPost("HacerAdmin", Name = "hacerAdminv2")]
        public async Task<ActionResult> HacerAdming(EditarAdminDTO editarAdminDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarAdminDTO.Email);

            await userManager.AddClaimAsync(usuario, new Claim("EsAdmin", "1"));

            return NoContent();
        }

        [HttpPost("RemoverAdmin", Name = "removerAdminv2")]
        public async Task<ActionResult> RemoverAdmin(EditarAdminDTO editarAdminDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarAdminDTO.Email);

            await userManager.RemoveClaimAsync(usuario, new Claim("EsAdmin", "1"));

            return NoContent();
        }

    }
}
