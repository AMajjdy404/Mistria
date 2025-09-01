using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mistria.API.Dtos;
using Mistria.Domain.Interfaces;
using Mistria.Domain.Models;
using Mistria.Domain.Services;

namespace Mistria.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebsiteController : ControllerBase
    {
        private readonly IMailService _mailService;
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;
        private readonly IGenericRepository<TravelProgram> _travelProgramRepo;
        private readonly IGenericRepository<DayTrip> _dayTripRepo;
        private readonly IGenericRepository<Activity> _activityRepo;
        private readonly IGenericRepository<Event> _eventRepo;
        private readonly IGenericRepository<Service> _serviceRepo;
        private readonly IGenericRepository<Wedding> _weddingRepo;
        private readonly ILogger<DashboardController> _logger;
        private readonly IMapper _mapper;

        public WebsiteController(IMailService mailService,
            IConfiguration configuration,
            UserManager<AppUser> userManager,
            IGenericRepository<TravelProgram> travelProgramRepo,
            IGenericRepository<DayTrip> dayTripRepo,
            IGenericRepository<Wedding> weddingRepo,
            IGenericRepository<Activity> activityRepo,
            IGenericRepository<Event> eventRepo,
            IGenericRepository<Service> serviceRepo,
            ILogger<DashboardController> logger,
            IMapper mapper)
        {
            _mailService = mailService;
            _configuration = configuration;
            _userManager = userManager;
            _travelProgramRepo = travelProgramRepo;
            _dayTripRepo = dayTripRepo;
            _activityRepo = activityRepo;
            _eventRepo = eventRepo;
            _serviceRepo = serviceRepo;
            _weddingRepo = weddingRepo;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendContactEmail([FromBody] EmailDto emailDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = new Email
            {
                To = _configuration["MailSettings:Email"],
                Subject = emailDto.Title ?? "New Contact Form Submission",
                Body = $"Name: {emailDto.Name}\nEmail: {emailDto.EmailAddress}\nPhone: {emailDto.Phone}\nNationality: {emailDto.Nationality}\nNumber of People: {emailDto.NumberOfPeople}" +
                       $"{(!string.IsNullOrEmpty(emailDto.Title) ? $"\nTitle: {emailDto.Title}" : "")}" +
                       $"{(!string.IsNullOrEmpty(emailDto.Message) ? $"\nMessage: {emailDto.Message}" : "")}"
            };

            await _mailService.SendEmailAsync(email);
            return Ok(new { Message = "Email sent successfully" });
        }

        #region Program
        [HttpGet("getMainProgram")]
        public async Task<ActionResult<IReadOnlyList<ReturnedProgramDto>>> GetMainProgram()
        {
            _logger.LogInformation("Received GetMainProgram request");

            var programs = await _travelProgramRepo.GetAllAsync(p => p.IsMain == true);
            var result = _mapper.Map<IQueryable<TravelProgram>,IReadOnlyList<ReturnedProgramDto>>(programs);
            return Ok(result);
        }


        [HttpGet("getAllProgramSummaries")]
        public async Task<ActionResult<List<TravelProgramSummaryDto>>> GetAllProgramSummaries()
        {
            _logger.LogInformation("Received GetAllProgramSummaries request");

            var programs = await _travelProgramRepo.GetAllAsync();
            var result = _mapper.Map<List<TravelProgramSummaryDto>>(programs);

            _logger.LogInformation("Returned {Count} program summaries", result.Count);
            return Ok(result);
        }

        [HttpGet("getAllPrograms")]
        public async Task<ActionResult<List<ReturnedProgramDto>>> GetAllPrograms()
        {
            _logger.LogInformation("Received GetAllPrograms request");

            var programs = await _travelProgramRepo.GetAllAsync();
            var result = _mapper.Map<List<ReturnedProgramDto>>(programs);

            _logger.LogInformation("Returned {Count} programs", result.Count);
            return Ok(result);
        }

        [HttpGet("getProgramById/{id}")]
        public async Task<ActionResult<ReturnedProgramDto>> GetProgramById(int id)
        {
            _logger.LogInformation("Received GetProgramById request for Id: {Id}", id);

            var program = await _travelProgramRepo.GetByIdAsync(id);
            if (program == null)
            {
                _logger.LogWarning("Program with Id {Id} not found", id);
                return NotFound("Program not found");
            }

            var result = _mapper.Map<ReturnedProgramDto>(program);

            _logger.LogInformation("Returned program with Id: {Id}", id);
            return Ok(result);
        }

        [HttpGet("getSimilarPrograms")]
        public async Task<ActionResult<List<TravelProgramSummaryDto>>> GetSimilarPrograms([FromQuery] int id)
        {
            _logger.LogInformation("Received GetSimilarPrograms request for program Id: {Id}", id);

            var selectedProgram = await _travelProgramRepo.GetByIdAsync(id);
            if (selectedProgram == null)
            {
                _logger.LogWarning("Program with Id {Id} not found", id);
                return NotFound("Selected program not found");
            }

            var allPrograms = await _travelProgramRepo.GetAllAsync();
            var otherPrograms = allPrograms.Where(p => p.Id != id).ToList();

            var similarPrograms = otherPrograms
                .Select(p => new { Program = p, Difference = Math.Abs(p.PricePerPerson - selectedProgram.PricePerPerson) })
                .OrderBy(x => x.Difference)
                .Take(3)
                .Select(x => x.Program)
                .ToList();

            var result = _mapper.Map<List<TravelProgramSummaryDto>>(similarPrograms);

            _logger.LogInformation("Returned {Count} similar programs for program Id: {Id}", result.Count, id);
            return Ok(result);
        }
        #endregion

        #region DayTrip

        [HttpGet("getAllDayTrips")]
        public async Task<ActionResult<List<DayTripReturnedDto>>> GetAllDayTrips()
        {
            _logger.LogInformation("Received GetAllDayTrips request");

            var dayTrips = await _dayTripRepo.GetAllAsync();
            var result = _mapper.Map<List<DayTripReturnedDto>>(dayTrips);

            _logger.LogInformation("Returned {Count} day trips", result.Count);
            return Ok(result);
        }

        [HttpGet("getDayTripSummaries")]
        public async Task<ActionResult<List<DayTripSummaryDto>>> GetDayTripSummaries()
        {
            _logger.LogInformation("Received GetDayTripSummaries request");

            var dayTrips = await _dayTripRepo.GetAllAsync();
            var result = _mapper.Map<List<DayTripSummaryDto>>(dayTrips);

            _logger.LogInformation("Returned {Count} day trip summaries", result.Count);
            return Ok(result);
        }

        [HttpGet("getDayTripById/{id}")]
        public async Task<ActionResult<DayTripReturnedDto>> GetDayTripById(int id)
        {
            _logger.LogInformation("Received GetDayTripById request for Id: {Id}", id);

            var dayTrip = await _dayTripRepo.GetByIdAsync(id);
            if (dayTrip == null)
            {
                _logger.LogWarning("DayTrip with Id {Id} not found", id);
                return NotFound("DayTrip not found");
            }

            var result = _mapper.Map<DayTripReturnedDto>(dayTrip);

            _logger.LogInformation("Returned day trip with Id: {Id}", id);
            return Ok(result);
        }

        [HttpGet("getSimilarDayTrips")]
        public async Task<ActionResult<List<DayTripSummaryDto>>> GetSimilarDayTrips([FromQuery] int id)
        {
            _logger.LogInformation("Received GetSimilarDayTrips request for day trip Id: {Id}", id);

            var selectedDayTrip = await _dayTripRepo.GetByIdAsync(id);
            if (selectedDayTrip == null)
            {
                _logger.LogWarning("Day trip with Id {Id} not found", id);
                return NotFound("Selected day trip not found");
            }

            var allDayTrips = await _dayTripRepo.GetAllAsync();
            var otherDayTrips = allDayTrips.Where(dt => dt.Id != id).ToList();

            var similarDayTrips = otherDayTrips
                .GroupBy(dt => dt.City) // Group by City first
                .SelectMany(g => g.OrderBy(dt => Math.Abs(dt.PricePerPerson - selectedDayTrip.PricePerPerson)))
                .Take(3)
                .ToList();

            var result = _mapper.Map<List<DayTripSummaryDto>>(similarDayTrips);

            _logger.LogInformation("Returned {Count} similar day trips for day trip Id: {Id}", result.Count, id);
            return Ok(result);
        }

        [HttpGet("getAllDayTripCities")]
        public async Task<ActionResult<List<CityDto>>> GetAllDayTripCities()
        {
            _logger.LogInformation("Received GetAllDayTripCities request");

            var dayTrips = await _dayTripRepo.GetAllAsync();

            var cities = dayTrips
                .GroupBy(dt => dt.City) // نجمع بالمدينة
                .Select(g => new CityDto
                {
                    City = g.Key,
                    ImageUrl = g.FirstOrDefault()?.Images?.FirstOrDefault()
                               ?? g.FirstOrDefault()?.CoverImage ?? ""
                })
                .ToList();

            _logger.LogInformation("Returned {Count} unique day trip cities", cities.Count);
            return Ok(cities);
        }

        [HttpGet("getDayTripsByCity")]
        public async Task<ActionResult<List<DayTripSummaryDto>>> GetDayTripsByCity([FromQuery] string city)
        {
            _logger.LogInformation("Received GetDayTripsByCity request for city: {City}", city);

            var dayTrips = await _dayTripRepo.GetAllAsync(dt => dt.City == city);
            if (!dayTrips.Any())
            {
                _logger.LogWarning("No day trips found for city: {City}", city);
                return NotFound($"No day trips found for city: {city}");
            }

            var result = _mapper.Map<List<DayTripSummaryDto>>(dayTrips);

            _logger.LogInformation("Returned {Count} day trips for city: {City}", result.Count, city);
            return Ok(result);
        }
        #endregion

        [HttpGet("getAllServices")]
        public async Task<ActionResult<List<ServiceReturnedDto>>> GetAllServices()
        {
            _logger.LogInformation("Received GetAllServices request");

            var services = await _serviceRepo.GetAllAsync();
            var result = _mapper.Map<List<ServiceReturnedDto>>(services);

            _logger.LogInformation("Returned {Count} services", result.Count);
            return Ok(result);
        }

        [HttpGet("getAllActivities")]
        public async Task<ActionResult<List<ActivityReturnedDto>>> GetAllActivities()
        {
            _logger.LogInformation("Received GetAllActivities request");

            var activities = await _activityRepo.GetAllAsync();
            var result = _mapper.Map<List<ActivityReturnedDto>>(activities);

            _logger.LogInformation("Returned {Count} activities", result.Count);
            return Ok(result);
        }

        [HttpGet("getAllEvents")]
        public async Task<ActionResult<List<EventReturnedDto>>> GetAllEvents()
        {
            _logger.LogInformation("Received GetAllEvents request");

            var events = await _eventRepo.GetAllAsync();
            var result = _mapper.Map<List<EventReturnedDto>>(events);

            _logger.LogInformation("Returned {Count} events", result.Count);
            return Ok(result);
        }

        [HttpGet("getAllWeddings")]
        public async Task<ActionResult<List<WeddingReturnedDto>>> GetAllWeddings()
        {
            _logger.LogInformation("Received GetAllWeddings request");

            var weddings = await _weddingRepo.GetAllAsync();
            var result = _mapper.Map<List<WeddingReturnedDto>>(weddings);

            _logger.LogInformation("Returned {Count} weddings", result.Count);
            return Ok(result);
        }


    }
}
