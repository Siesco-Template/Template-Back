using Auth.Business.Dtos.UserDtos;
using Auth.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController(IUserService _userService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserDto user)
        {
            await _userService.CreateAsync(user);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(UpdateUserDto dto)
        {
            await _userService.UpdateAsync(dto);
            return Ok();
        }
    }
}