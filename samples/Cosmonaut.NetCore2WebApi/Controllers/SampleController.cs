using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Cosmonaut.NetCore2WebApi.Contract;
using Cosmonaut.NetCore2WebApi.Model;
using Microsoft.AspNetCore.Mvc;

namespace Cosmonaut.NetCore2WebApi.Controllers
{
    public class SampleController : Controller
    {
        private readonly ICosmosStore<Person> _peopleCosmosStore;

        public SampleController(ICosmosStore<Person> peopleCosmosStore)
        {
            _peopleCosmosStore = peopleCosmosStore;
        }

        [HttpGet("/api/people")]
        public async Task<IActionResult> GetAllPaged([FromQuery] string continuationToken,
            CancellationToken cancellationToken, [FromQuery] int pageSize = 10)
        {
            if (pageSize > 50 || pageSize <= 0)
            {
                return BadRequest("Page Size Invalid! Must be 0 < pageSize < 50");
            }

            if (continuationToken == null || continuationToken.Trim().Length == 0)
            {
                var initialPage = await _peopleCosmosStore.Query().OrderBy(person => person.Name)
                    .WithPagination(1, pageSize).ToPagedListAsync(cancellationToken: cancellationToken);

                return Ok(initialPage);
            }
            
            var page = await _peopleCosmosStore.Query().OrderBy(person => person.Name)
                .WithPagination(continuationToken, pageSize).ToPagedListAsync(cancellationToken: cancellationToken);

            return Ok(page);
        }

        [HttpGet("/api/people/{id:Guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Invalid Person Id");
            }

            var person =
                await _peopleCosmosStore.FindAsync(id.ToString(), cancellationToken: cancellationToken);

            if (person == null)
            {
                return NotFound();
            }

            var response = new GetPersonResponse
            {
                Identifier = person.Id,
                Name = person.Name,
            };

            return Ok(response);
        }

        [HttpPost("/api/people")]
        public async Task<IActionResult> Create([FromBody] CreatePersonRequest request,
            CancellationToken cancellationToken)
        {
            var personToCreate = new Person
            {
                Name = request.Name
            };

            var result = await _peopleCosmosStore.UpsertAsync(personToCreate, cancellationToken: cancellationToken);

            var personId = result.Entity.Id;

            if (personId == null)
            {
                return BadRequest();
            }

            return Ok(personId);
        }

        [HttpPut("/api/people/{id:Guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] EditPersonRequest request,
            CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Invalid Person Id");
            }

            if (request.Name.Trim().Length == 0)
            {
                return BadRequest("Person Name can not be empty");
            }

            var existingPerson =
                await _peopleCosmosStore.FindAsync(id.ToString(), cancellationToken: cancellationToken);

            if (existingPerson == null)
            {
                return NotFound();
            }

            var personToSave = new Person
            {
                Id = id.ToString(),
                Name = request.Name
            };

            var result = await _peopleCosmosStore.UpdateAsync(personToSave, cancellationToken: cancellationToken);

            var personId = result.Entity.Id;

            if (personId == null)
            {
                return BadRequest();
            }

            return Ok(personId);
        }

        [HttpDelete("/api/people/{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            await _peopleCosmosStore.RemoveByIdAsync(id.ToString(), cancellationToken: cancellationToken);

            return Ok();
        }

        //Alternative to having 1 endpoint for Creation and 1 for Update -> Upsert
        [HttpPut("/api/people")]
        public async Task<IActionResult> Upsert([FromBody] UpsertPersonRequest request,
            CancellationToken cancellationToken)
        {
            var personToUpsert = new Person
            {
                Id = request.Id,
                Name = request.Name
            };

            var result = await _peopleCosmosStore.UpsertAsync(personToUpsert, cancellationToken: cancellationToken);

            var personId = result.Entity.Id;

            if (personId == null)
            {
                return BadRequest();
            }

            return Ok(personId);
        }
    }
}