using System.Diagnostics.Eventing.Reader;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using Asp.Versioning;
using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using MagicVilla_VillaAPI.Logging;

namespace MagicVilla_VillaAPI.Controllers.v1
{
    [Route("api/v{version:ApiVersion}/VillaAPI")]
    [ApiController]
    [ApiVersion("1.0")]
    public class VillaAPIController : ControllerBase
    {
        protected APIResponse _response;
        private readonly IVillaRepository _dbVilla;
        private readonly IMapper _mapper;
        public VillaAPIController(IVillaRepository dbVilla, IMapper mapper)
        {
            _dbVilla = dbVilla;
            _mapper = mapper;
            _response = new();
        }

        //private readonly    ILogging _logger;
        //private readonly    ILogger<VillaAPIController> _logger;
        //public VillaAPIController()//ILogging logger)
        //{
        //    //_logger = logger  ;      
        //}



        [HttpGet]
        [ResponseCache(Duration =30)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> GetVillas([FromQuery(Name="filterOccupancy")]int? occupancy,
            [FromQuery] string? search, int pageSize = 0, int pageNumebr = 1)
        {
            // _logger.Log("Getting all villas","");
            try
            {

                IEnumerable<Villa> villaList;
                if (occupancy >0) {
                        villaList = await _dbVilla.GetAllAsync(u=>u.Occupancy==occupancy,pageSize:pageSize,pageNumebr:pageNumebr);
                }
                else
                {
					villaList = await _dbVilla.GetAllAsync(pageSize: pageSize, pageNumebr: pageNumebr);
				}
                if (!string.IsNullOrEmpty(search)) 
                { 
                    villaList = villaList.Where(u=>u.Name.ToLower().Contains(search));//u.Amenity.ToLower().Contains(search)|| 
				}
                Pagination pagination = new() { PageNumber = pageNumebr,PageSize = pageSize};


				Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagination));
                _response.Result = _mapper.Map<List<VillaDTO>>(villaList);
                _response.StatusCode = System.Net.HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;

        }
        [HttpGet("{id:int}", Name = "GetVilla")]
        //[ResponseCache(Duration =30)]
        [ResponseCache(CacheProfileName ="Default30")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(404)]
        public async Task<ActionResult<APIResponse>> GetVilla(int id)
        {
            try
            {


                if (id == 0)
                {
                    // _logger.Log("Get villa with id" + id,"error");
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                    return BadRequest(_response);
                }
                var villa = await _dbVilla.GetAsync(u => u.Id == id);
                if (villa == null)
                {
                    _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                    return NotFound(_response);
                }
                _response.Result = _mapper.Map<VillaDTO>(villa);
                _response.StatusCode = System.Net.HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }
        [HttpPost(Name = "CreateVilla")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> CreateVilla([FromBody] VillaCreateDTO createDTO)
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    return BadRequest(ModelState);
                //}
                if (await _dbVilla.GetAsync(u => u.Name.ToLower() == createDTO.Name.ToLower()) != null)
                {
                    ModelState.AddModelError("ErrorMessages", "Villa Already Exists!");
                _response.IsSuccess = false;
                    return BadRequest(ModelState);
                }
                if (createDTO == null)
                {
                _response.IsSuccess = false;
                    return BadRequest();
                }
                //if (villaDTO.Id > 0)
                //{
                //    return StatusCode(StatusCodes.Status500InternalServerError);
                //}
                //villaDTO.Id = _db.Villas.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
                Villa villa = _mapper.Map<Villa>(createDTO);
                //Villa model = new()
                //{
                //    Amenity = createDTO.Amenity,
                //    Details = createDTO.Details,
                //    Name = createDTO.Name,
                //    ImageUrl = createDTO.ImageUrl,
                //    Occupancy = createDTO.Occupancy,
                //    Rate = createDTO.Rate,
                //    Sqft = createDTO.Sqft,
                //};
                await _dbVilla.CreateAsync(villa);
                //await _dbVilla.Save();
                _response.Result = _mapper.Map<VillaDTO>(villa);
                _response.StatusCode = System.Net.HttpStatusCode.Created;
                return CreatedAtRoute("GetVilla", new { id = villa.Id }, _response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> DeleteVilla(int id)
        {
            try
            {


                if (id == 0)
                {
                _response.IsSuccess = false;
                    return BadRequest();
                }
                var villa = await _dbVilla.GetAsync(u => u.Id == id);
                if (villa == null)
                {
                _response.IsSuccess = false;
                    return NotFound();
                }
                await _dbVilla.RemoveAsync(villa);
                //await _db.SaveChangesAsync();
                _response.StatusCode = System.Net.HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
                //return NoContent();
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateDTO)
        {
            try
            {


                if (updateDTO == null || id != updateDTO.Id)
                {
                _response.IsSuccess = false;
                    return BadRequest();
                }
                Villa model = _mapper.Map<Villa>(updateDTO);
                //Villa model = new()
                //{
                //    Amenity = villaDTO.Amenity,
                //    Details = villaDTO.Details,
                //    Name = villaDTO.Name,
                //    Id = villaDTO.Id,
                //    ImageUrl = villaDTO.ImageUrl,
                //    Occupancy = villaDTO.Occupancy,
                //    Rate = villaDTO.Rate,
                //    Sqft = villaDTO.Sqft,
                //};
                await _dbVilla.UpdateAsync(model);
                //await _db.SaveChangesAsync();
                _response.StatusCode = System.Net.HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }
        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
        public async Task<ActionResult<APIResponse>> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
        {
            try
            {


                if (patchDTO == null || id == 0)
                {
                _response.IsSuccess = false;
					return BadRequest();
                }
                var villa = await _dbVilla.GetAsync(u => u.Id == id, tracked: false);
                VillaUpdateDTO villaDTO = _mapper.Map<VillaUpdateDTO>(villa);
                //VillaUpdateDTO villaDTO = new()
                //{
                //    Amenity = villa.Amenity,
                //    Details = villa.Details,
                //    Name = villa.Name,
                //    Id = villa.Id,
                //    ImageUrl = villa.ImageUrl,
                //    Occupancy = villa.Occupancy,
                //    Rate = villa.Rate,
                //    Sqft = villa.Sqft,
                //};



                if (villa == null)
                {
                _response.IsSuccess = false;
					return BadRequest();
                }
                patchDTO.ApplyTo(villaDTO, ModelState);
                Villa model = _mapper.Map<Villa>(villaDTO);
                //Villa model = new()
                //{
                //    Amenity = villaDTO.Amenity,
                //    Details = villaDTO.Details,
                //    Name = villaDTO.Name,
                //    Id = villaDTO.Id,
                //    ImageUrl = villaDTO.ImageUrl,
                //    Occupancy = villaDTO.Occupancy,
                //    Rate = villaDTO.Rate,
                //    Sqft = villaDTO.Sqft,
                //};

                await _dbVilla.UpdateAsync(model);
                //await _dbVilla.SaveChangesAsync();
                if (!ModelState.IsValid)
                {
                _response.IsSuccess = false;
					return BadRequest(ModelState);
                }
                _response.StatusCode = System.Net.HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }
    }
}
