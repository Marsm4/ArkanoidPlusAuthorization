using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication2;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(AuthRequest request)
    {
        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Coins = 0
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "User registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(AuthRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized();

        var token = GenerateJwtToken(user);
        return Ok(new { Token = token });
    }

    [HttpGet("coins")]
    public async Task<IActionResult> GetCoins(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return NotFound("User not found");

        return Ok(new { Coins = user.Coins });
    }

    [HttpGet("skins")]
    public async Task<IActionResult> GetSkins()
    {
        var skins = await _context.Skins.ToListAsync();
        return Ok(skins);
    }

    [HttpPost("buy-skin")]
    public async Task<IActionResult> BuySkin(BuySkinRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
            return NotFound("User not found");

        var skin = await _context.Skins.FindAsync(request.SkinId);
        if (skin == null)
            return NotFound("Skin not found");

        if (user.Coins < skin.Price)
            return BadRequest("Not enough coins");

        var userSkin = await _context.UserSkins
            .FirstOrDefaultAsync(us => us.UserId == user.Id && us.SkinId == skin.Id);

        if (userSkin != null)
            return BadRequest("User already owns this skin");

        user.Coins -= skin.Price;
        _context.UserSkins.Add(new UserSkin { UserId = user.Id, SkinId = skin.Id });

        await _context.SaveChangesAsync();

        return Ok(new { Coins = user.Coins });
    }
    [HttpGet("user-skins")]
    public async Task<IActionResult> GetUserSkins(string email)
    {
        var user = await _context.Users
            .Include(u => u.UserSkins)
            .ThenInclude(us => us.Skin)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return NotFound("User not found");

        var userSkins = user.UserSkins.Select(us => us.Skin).ToList();
        return Ok(userSkins);
    }
    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: new[] { new Claim(ClaimTypes.Email, user.Email) },
            expires: DateTime.Now.AddDays(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}