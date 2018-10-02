using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Cosmonaut.NetCore2WebApi.Contract;
using Cosmonaut.NetCore2WebApi.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.EntityFrameworkCore;

namespace Cosmonaut.NetCore2WebApi.Controllers
{
    public class SampleController : Controller
    {
        private readonly ICosmosStore<Person> _personsCosmosStore;

        public SampleController(ICosmosStore<Person> personsCosmosStore)
        {
            _personsCosmosStore = personsCosmosStore;
        }

        [HttpGet("/api/persons")]
        public async Task<IActionResult> GetAllPaged([FromQuery] int pageSize, [FromQuery] string continuationToken,
            CancellationToken cancellationToken)
        {
            if (pageSize > 50)
            {
                return BadRequest("Page Size too big! Must be < 50");
            }

            if (continuationToken == null || continuationToken.Trim().Length == 0)
            {
                var initialPage = await _personsCosmosStore.Query().OrderBy(person => person.Name)
                    .WithPagination(0, pageSize).ToPagedListAsync(cancellationToken: cancellationToken);

                return Ok(initialPage);
            }
            
            var page = await _personsCosmosStore.Query().OrderBy(person => person.Name)
                .WithPagination(continuationToken, pageSize).ToPagedListAsync(cancellationToken: cancellationToken);

            return Ok(page);
        }

        [HttpGet("/api/persons/{id:Guid}")]
        public async Task<IActionResult> ReadById(Guid id, CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Invalid Person Id");
            }

            var person =
                await _personsCosmosStore.FindAsync(id.ToString(), cancellationToken: cancellationToken);

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

        [HttpPost("/api/persons")]
        public async Task<IActionResult> Create([FromBody] CreatePersonRequest request,
            CancellationToken cancellationToken)
        {
            var personToCreate = new Person
            {
                Name = request.Name
            };

            var result = await _personsCosmosStore.UpsertAsync(personToCreate, cancellationToken: cancellationToken);

            var personId = result.Entity.Id;

            if (personId == null)
            {
                return BadRequest();
            }

            return Ok(personId);
        }

        [HttpPut("/api/persons/{id:Guid}")]
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
                await _personsCosmosStore.FindAsync(id.ToString(), cancellationToken: cancellationToken);

            if (existingPerson == null)
            {
                return NotFound();
            }

            var personToSave = new Person
            {
                Id = id.ToString(),
                Name = request.Name
            };

            var result = await _personsCosmosStore.UpdateAsync(personToSave, cancellationToken: cancellationToken);

            var personId = result.Entity.Id;

            if (personId == null)
            {
                return BadRequest();
            }

            return Ok(personId);
        }

        [HttpDelete("/api/persons/{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            await _personsCosmosStore.RemoveByIdAsync(id.ToString(), cancellationToken: cancellationToken);

            return Ok();
        }

        //Alternative to having 1 endpoint for Creation and 1 for Update -> Upsert
        [HttpPut("/api/persons")]
        public async Task<IActionResult> Upsert([FromBody] UpsertPersonRequest request,
            CancellationToken cancellationToken)
        {
            var personToUpsert = new Person
            {
                Id = request.Id,
                Name = request.Name
            };

            var result = await _personsCosmosStore.UpsertAsync(personToUpsert, cancellationToken: cancellationToken);

            var personId = result.Entity.Id;

            if (personId == null)
            {
                return BadRequest();
            }

            return Ok(personId);
        }
    }
}