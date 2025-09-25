using MainProject.API.Business.Dtos.UserDtos;
using MainProject.API.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace MainProject.API.Controllers
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

        [HttpPut("[action]")]
        public async Task<IActionResult> Update(UpdateUserDto dto)
        {
            await _userService.UpdateAsync(dto);
            return Ok();
        }

        [HttpDelete("[action]")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _userService.DeleteAsync(id);
            return Ok();
        }
    }
}