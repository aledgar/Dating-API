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
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(LogUserActivity))]
    [Route("api/users/{userId}/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IDatingRepository _datingRepository;
        private readonly IMapper _mapper;

        public MessageController(IDatingRepository datingRepository, IMapper mapper)
        {
            _datingRepository = datingRepository;
            _mapper = mapper;
        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var messageFromRepo = await _datingRepository.GetMessage(id);

            if (messageFromRepo == null)
            {
                return NotFound();
            }

            var messageToReturn = _mapper.Map<MessageToReturnDto>(messageFromRepo);

            return Ok(messageToReturn);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(int userId, [FromQuery] MessageParams messageParams)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            messageParams.UserId = userId;

            var messagesFromRepo = await _datingRepository.GetMessagesForUser(messageParams);
            var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

            Response.AddPagination(messagesFromRepo.CurrentPage, messagesFromRepo.PageSize, messagesFromRepo.TotalCount,
                messagesFromRepo.TotalPages);
            return Ok(messages);
        }

        [HttpGet("thread/{recipentId}")]
        public async Task<IActionResult> GetMessageThread(int userId, int recipentId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var messagesFromRepo = await _datingRepository.GetMessageThread(userId, recipentId);

            var messageThread = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

            return Ok(messageThread);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            messageForCreationDto.SenderId = userId;
            var recipientExists = await _datingRepository.GetUser(messageForCreationDto.RecipientId);

            if (recipientExists==null)
            {
                return BadRequest("The user does not exist");
            }

            var message = _mapper.Map<Message>(messageForCreationDto);
            message.Sender = await _datingRepository.GetUser(messageForCreationDto.SenderId);
            await _datingRepository.Add(message);
            if (await _datingRepository.SaveAll())
            {
                var messageToReturn = _mapper.Map<MessageToReturnDto>(message);
                return CreatedAtRoute("GetMessage", new {userId, id = message.Id}, messageToReturn);
            }

            throw new Exception("Creating the message failed on save");
        }

        [HttpPut("read/{messageId}")]
        public async Task<IActionResult> MarkAsReaded(int userId, int messageId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var message = await _datingRepository.GetMessage(messageId);

            if (message.SenderId != userId)
            {
                return BadRequest();
            }

            message.IsRead = true;
            message.DateRead = DateTime.Now;

            await _datingRepository.SaveAll();

            return NoContent();

        }

    }
}
