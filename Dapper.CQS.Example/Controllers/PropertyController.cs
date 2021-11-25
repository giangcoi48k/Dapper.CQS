using Dapper.CQS.Example.CommandQueries;
using Microsoft.AspNetCore.Mvc;

namespace Dapper.CQS.Example.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class PropertyController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public PropertyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<Property>> GetById([FromQuery] int id)
        {
            var property = await _unitOfWork.QueryAsync(new PropertyGetByIdQuery(id));
            return property == null ? NoContent() : Ok(property);
        }

        [HttpGet]
        public async Task<ActionResult<List<Property>>> Filter([FromQuery] string? name)
        {
            var properties = await _unitOfWork.QueryAsync(new PropertyFilterQuery(name));
            return Ok(properties);
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<Property>>> PagedFilter([FromQuery] string? name, int page = 1, int pageSize = 5)
        {
            var properties = await _unitOfWork.QueryAsync(new PropertyPagedFilterQuery(name, page, pageSize));
            return Ok(properties);
        }

        [HttpPost]
        public async Task<ActionResult<Property>> Create([FromBody] Property property)
        {
            var createdId = await _unitOfWork.ExecuteAsync(new PropertyCreateCommand(property));
            await _unitOfWork.CommitAsync();
            property.Id = createdId;
            return Ok(property);
        }

        [HttpDelete]
        public async Task<ActionResult> Delete([FromQuery] int id)
        {
            await _unitOfWork.ExecuteAsync(new PropertyDeleteCommand(id));
            await _unitOfWork.CommitAsync();
            return Ok();
        }
    }
}