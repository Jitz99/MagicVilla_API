using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace MagicVilla_VillaAPI.Repository
{
	public class UserRepository :IUserRepository
		
	{
        private readonly ApplicationDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private string secretKey;
		private readonly IMapper _mapper;
        public UserRepository(ApplicationDbContext db,IConfiguration configuration, UserManager<ApplicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager)
        {
            _db= db;
			// villaUrl = configuration.GetValue<string>("ServiceUrls:VillaAPI");
			_userManager = userManager;
			_mapper= mapper;
			_roleManager = roleManager;
			 secretKey = configuration.GetValue<string>("ApiSettings:Secret");
			//secretkey = configuration.GetValue<string>("ApiSettings:Secret");
        }

        public bool IsUniqueUser(string username)
		{
			var user = _db.ApplicationUsers.FirstOrDefault(x=>x.UserName == username);	
			//var user=_db.LocalUsers.FirstOrDefault(x=>x.UserName == username);
			if (user == null)
			{
				return true;
			}
			return false;
		}

		public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
		{
			var user = _db.ApplicationUsers.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower());

			bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);
			if (user == null)
			{
				return new LoginResponseDTO()
				{ 
				Token = "",
				User = null
			};
			}
			var roles = await _userManager.GetRolesAsync(user);
			var tokenHanlder = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes(secretKey);

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new Claim[] {
					new Claim(ClaimTypes.Name,user.UserName.ToString()),
					new Claim(ClaimTypes.Role,roles.FirstOrDefault())
				}),
				Expires = DateTime.UtcNow.AddDays(7),
				SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};
			var token= tokenHanlder.CreateToken(tokenDescriptor);
			LoginResponseDTO loginResponseDTO = new LoginResponseDTO()
			{
				Token = tokenHanlder.WriteToken(token),
				User = _mapper.Map<UserDTO>(user),
				//Role=roles.FirstOrDefault()
			};
			return loginResponseDTO;
		}

		public async Task<UserDTO> Register(RegistrationRequestDTO registrationRequestDTO)
		{
			ApplicationUser user = new()
			{
				UserName=registrationRequestDTO.UserName,
				Email=registrationRequestDTO.UserName,
				NormalizedEmail = registrationRequestDTO.UserName.ToUpper(),
				Name=registrationRequestDTO.Name,
			};
			try
			{
				var result = await _userManager.CreateAsync(user, registrationRequestDTO.Password);
				if (result.Succeeded)
				{
					if (!_roleManager.RoleExistsAsync("admin").GetAwaiter().GetResult())
					{
						await _roleManager.CreateAsync(new IdentityRole("admin"));
						await _roleManager.CreateAsync(new IdentityRole("customer"));
					}
					await _userManager.AddToRoleAsync(user, "admin");
					var userToReturn = _db.ApplicationUsers
						.FirstOrDefault(u => u.UserName == registrationRequestDTO.UserName);
					return _mapper.Map<UserDTO>(userToReturn);	
				}
			}
			catch (Exception e) 
			{ 
				
			}
			return new UserDTO();
		}
	}
}
