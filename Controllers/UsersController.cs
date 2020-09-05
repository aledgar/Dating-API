using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _datingRepository;
        private readonly IMapper _mapper;

        public UsersController(IDatingRepository datingRepository, IMapper mapper)
        {
            _datingRepository = datingRepository;
            _mapper = mapper;
        }

        // GET
        public async Task<IActionResult> Index([FromQuery] UserParams userParams)
        {
            var users = await _datingRepository.GetUsers(userParams);
            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);
            Response.AddPagination(users.CurrentPage, users.PageSize,
                users.TotalCount, users.TotalPages);
            return Ok(usersToReturn);
        }

        [HttpGet("find/{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _datingRepository.GetUser(id);
            var userToReturn = _mapper.Map<UserDetailedDto>(user);
            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var userFromRepo = await _datingRepository.GetUser(id);

            _mapper.Map(userForUpdateDto, userFromRepo);

            if (await _datingRepository.SaveAll())
            {
                return NoContent();
            }

            throw new Exception($"Updating user {id} failed on save");
        }

        [HttpPost("add-like")]
        public async Task<IActionResult> AddLike(LikeForCreationDto like)
        {
            if (like.LikerId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var likeGiven = await _datingRepository.GetLike(like.LikerId, like.LikeeId);

            if (likeGiven!=null)
            {
                _datingRepository.Delete(likeGiven);
                return Ok();
            }

            Like likeToCreate = new Like
            {
                LikeeId = like.LikeeId,
                LikerId = like.LikerId
            };

            await _datingRepository.Add(likeToCreate);

            if (await _datingRepository.SaveAll())
            {
                return Ok();
            }

            return BadRequest("Failed to like the user");
        }
    }
}
