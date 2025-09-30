using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models.Domain;
using WebAPI.Models.DTO;
using WebAPI.Repositories;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublishersController : ControllerBase
    {
        private readonly IPublisherRepository _publisherRepository;

        public PublishersController(IPublisherRepository publisherRepository)
        {
            _publisherRepository = publisherRepository;
        }

        [HttpGet]
        public IActionResult GetAllPublishers()
        {
            return Ok(_publisherRepository.GetAllPublishers());
        }

        [HttpGet("{id}")]
        public IActionResult GetPublisherById(int id)
        {
            var publisher = _publisherRepository.GetPublisherById(id);
            if (publisher == null) return NotFound();
            return Ok(publisher);
        }

        [HttpPost]
        public IActionResult AddPublisher([FromBody] AddPublisherRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_publisherRepository.ExistsByName(dto.Name))
            {
                ModelState.AddModelError(nameof(dto.Name), "Publisher name already exists");
                return BadRequest(ModelState);
            }

            // map DTO -> domain
            var publisher = new Publisher { Name = dto.Name };

            var added = _publisherRepository.AddPublisher(publisher);

            // map domain -> output DTO (ví dụ PublisherDTO)
            var result = new PublisherDTO { Id = added.Id, Name = added.Name };

            return CreatedAtAction(nameof(GetPublisherById), new { id = added.Id }, result);
        }


        [HttpPut("{id}")]
        public IActionResult UpdatePublisherById(int id, PublisherNoIdDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_publisherRepository.ExistsByName(dto.Name, id))
            {
                ModelState.AddModelError(nameof(dto.Name), "Publisher name already exists");
                return BadRequest(ModelState);
            }

            // map DTO -> Domain
            var publisher = new Publisher { Name = dto.Name };

            var updated = _publisherRepository.UpdatePublisherById(id, publisher);

            if (updated == null) return NotFound();

            // map Domain -> DTO output nếu muốn trả DTO
            var result = new PublisherDTO { Id = updated.Id, Name = updated.Name };

            return Ok(result);
        }


        [HttpDelete("{id}")]
        public IActionResult DeletePublisherById(int id)
        {
            try
            {
                var deleted = _publisherRepository.DeletePublisherById(id);
                if (deleted == null) return NotFound();
                return Ok(new { message = $"Publisher {id} deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = "PublisherInUse", message = ex.Message });
            }
        }
    }
}
