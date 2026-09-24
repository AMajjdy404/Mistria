using System.Security.Claims;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GateOfEgypt.API.Dtos;
using GateOfEgypt.Domain.Interfaces;
using GateOfEgypt.Domain.Models;
using GateOfEgypt.Domain.Services;

namespace GateOfEgypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebsiteController : ControllerBase
    {
        private readonly IMailService _mailService;
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;
        private readonly IGenericRepository<TravelProgram> _travelProgramRepo;
        private readonly IGenericRepository<Destination> _destinationRepo;
        private readonly IGenericRepository<Activity> _activityRepo;
        private readonly IGenericRepository<Event> _eventRepo;
        private readonly IGenericRepository<Service> _serviceRepo;
        private readonly IGenericRepository<Wedding> _weddingRepo;
        private readonly IGenericRepository<AboutUs> _aboutUsRepo;
        private readonly IGenericRepository<Founder> _founderRepo;
        private readonly IGenericRepository<Blog> _blogRepo;
        private readonly IGenericRepository<BlogSub> _blogSubRepo;
        private readonly IGenericRepository<PaymentMethod> _paymentMethodRepo;
        private readonly IGenericRepository<Review> _reviewRepo;
        private readonly IGenericRepository<ReviewPlatform> _reviewPlatformRepo;
        private readonly IGenericRepository<Reel> _reelRepo;
        private readonly IGenericRepository<CustomerPhoto> _customerPhotoRepo;
        private readonly IGenericRepository<ReviewsSettings> _reviewsSettingsRepo;
        private readonly IGenericRepository<SocialMediaLink> _socialMediaLinkRepo;
        private readonly ILogger<DashboardController> _logger;
        private readonly IMapper _mapper;

        public WebsiteController(IMailService mailService,
            IConfiguration configuration,
            UserManager<AppUser> userManager,
            IGenericRepository<TravelProgram> travelProgramRepo,
            IGenericRepository<Destination> destinationRepo,
            IGenericRepository<Wedding> weddingRepo,
            IGenericRepository<Activity> activityRepo,
            IGenericRepository<Event> eventRepo,
            IGenericRepository<Service> serviceRepo,
            IGenericRepository<AboutUs> aboutUsRepo,
            IGenericRepository<Founder> founderRepo,
            IGenericRepository<Blog> blogRepo,
            IGenericRepository<BlogSub> blogSubRepo,
            IGenericRepository<PaymentMethod> paymentMethodRepo,
            IGenericRepository<Review> reviewRepo,
            IGenericRepository<ReviewPlatform> reviewPlatformRepo,
            IGenericRepository<Reel> reelRepo,
            IGenericRepository<CustomerPhoto> customerPhotoRepo,
            IGenericRepository<ReviewsSettings> reviewsSettingsRepo,
            IGenericRepository<SocialMediaLink> socialMediaLinkRepo,
            ILogger<DashboardController> logger,
            IMapper mapper)
        {
            _mailService = mailService;
            _configuration = configuration;
            _userManager = userManager;
            _travelProgramRepo = travelProgramRepo;
            _destinationRepo = destinationRepo;
            _activityRepo = activityRepo;
            _eventRepo = eventRepo;
            _serviceRepo = serviceRepo;
            _weddingRepo = weddingRepo;
            _aboutUsRepo = aboutUsRepo;
            _founderRepo = founderRepo;
            _blogRepo = blogRepo;
            _blogSubRepo = blogSubRepo;
            _paymentMethodRepo = paymentMethodRepo;
            _reviewRepo = reviewRepo;
            _reviewPlatformRepo = reviewPlatformRepo;
            _reelRepo = reelRepo;
            _customerPhotoRepo = customerPhotoRepo;
            _reviewsSettingsRepo = reviewsSettingsRepo;
            _socialMediaLinkRepo = socialMediaLinkRepo;
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
                       $"{(!string.IsNullOrEmpty(emailDto.PaymentMethod) ? $"\nPayment Method: {emailDto.PaymentMethod}" : "")}" +
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

            var programs = (await _travelProgramRepo.GetAllAsync(p => p.IsMain == true)).OrderBy(p => p.Order);
            var result = _mapper.Map<IQueryable<TravelProgram>,IReadOnlyList<ReturnedProgramDto>>(programs);
            return Ok(result);
        }


        [HttpGet("getAllProgramSummaries")]
        public async Task<ActionResult<List<TravelProgramSummaryDto>>> GetAllProgramSummaries()
        {
            _logger.LogInformation("Received GetAllProgramSummaries request");

            var programs = (await _travelProgramRepo.GetAllAsync()).OrderBy(p => p.Order).ToList();
            var result = _mapper.Map<List<TravelProgramSummaryDto>>(programs);

            _logger.LogInformation("Returned {Count} program summaries", result.Count);
            return Ok(result);
        }

        [HttpGet("getAllPrograms")]
        public async Task<ActionResult<List<ReturnedProgramDto>>> GetAllPrograms()
        {
            _logger.LogInformation("Received GetAllPrograms request");

            var programs = (await _travelProgramRepo.GetAllAsync()).OrderBy(p => p.Order).ToList();
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

            var selectedPrice = selectedProgram.GetStartingPrice() ?? 0;
            var similarPrograms = otherPrograms
                .Select(p => new { Program = p, Difference = Math.Abs((p.GetStartingPrice() ?? 0) - selectedPrice) })
                .OrderBy(x => x.Difference)
                .Take(3)
                .Select(x => x.Program)
                .ToList();

            var result = _mapper.Map<List<TravelProgramSummaryDto>>(similarPrograms);

            _logger.LogInformation("Returned {Count} similar programs for program Id: {Id}", result.Count, id);
            return Ok(result);
        }
        #endregion

        #region Destination

        [HttpGet("getMainDestinations")]
        public async Task<ActionResult<IReadOnlyList<DestinationReturnedDto>>> GetMainDestinations()
        {
            _logger.LogInformation("Received GetMainDestinations request");

            var destinations = (await _destinationRepo.GetAllAsync(d => d.IsMain == true)).OrderBy(d => d.Order);
            var result = _mapper.Map<IQueryable<Destination>, IReadOnlyList<DestinationReturnedDto>>(destinations);
            return Ok(result);
        }

        [HttpGet("getAllDestinations")]
        public async Task<ActionResult<List<DestinationReturnedDto>>> GetAllDestinations()
        {
            _logger.LogInformation("Received GetAllDestinations request");

            var destinations = (await _destinationRepo.GetAllAsync()).OrderBy(d => d.Order).ToList();
            var result = _mapper.Map<List<DestinationReturnedDto>>(destinations);

            _logger.LogInformation("Returned {Count} destinations", result.Count);
            return Ok(result);
        }

        [HttpGet("getDestinationSummaries")]
        public async Task<ActionResult<List<DestinationSummaryDto>>> GetDestinationSummaries()
        {
            _logger.LogInformation("Received GetDestinationSummaries request");

            var destinations = (await _destinationRepo.GetAllAsync()).OrderBy(d => d.Order).ToList();
            var result = _mapper.Map<List<DestinationSummaryDto>>(destinations);

            _logger.LogInformation("Returned {Count} destination summaries", result.Count);
            return Ok(result);
        }

        [HttpGet("getDestinationById/{id}")]
        public async Task<ActionResult<DestinationReturnedDto>> GetDestinationById(int id)
        {
            _logger.LogInformation("Received GetDestinationById request for Id: {Id}", id);

            var destination = await _destinationRepo.GetByIdAsync(id);
            if (destination == null)
            {
                _logger.LogWarning("Destination with Id {Id} not found", id);
                return NotFound("Destination not found");
            }

            var result = _mapper.Map<DestinationReturnedDto>(destination);

            _logger.LogInformation("Returned destination with Id: {Id}", id);
            return Ok(result);
        }

        [HttpGet("getSimilarDestinations")]
        public async Task<ActionResult<List<DestinationSummaryDto>>> GetSimilarDestinations([FromQuery] int id)
        {
            _logger.LogInformation("Received GetSimilarDestinations request for destination Id: {Id}", id);

            var selectedDestination = await _destinationRepo.GetByIdAsync(id);
            if (selectedDestination == null)
            {
                _logger.LogWarning("Destination with Id {Id} not found", id);
                return NotFound("Selected destination not found");
            }

            var allDestinations = await _destinationRepo.GetAllAsync();
            var otherDestinations = allDestinations.Where(dt => dt.Id != id).ToList();

            var selectedPrice = selectedDestination.GetStartingPrice() ?? 0;
            var similarDestinations = otherDestinations
                .GroupBy(dt => dt.City) // Group by City first
                .SelectMany(g => g.OrderBy(dt => Math.Abs((dt.GetStartingPrice() ?? 0) - selectedPrice)))
                .Take(3)
                .ToList();

            var result = _mapper.Map<List<DestinationSummaryDto>>(similarDestinations);

            _logger.LogInformation("Returned {Count} similar destinations for destination Id: {Id}", result.Count, id);
            return Ok(result);
        }

        [HttpGet("getAllDestinationCities")]
        public async Task<ActionResult<List<CityDto>>> GetAllDestinationCities()
        {
            _logger.LogInformation("Received GetAllDestinationCities request");

            var destinations = await _destinationRepo.GetAllAsync();

            var cities = destinations
                .GroupBy(dt => dt.City) // نجمع بالمدينة
                .OrderBy(g => g.Min(dt => dt.Order))
                .Select(g => new CityDto
                {
                    City = g.Key,
                    ImageUrl = g.FirstOrDefault()?.Images?.FirstOrDefault()
                               ?? g.FirstOrDefault()?.CoverImage ?? ""
                })
                .ToList();

            _logger.LogInformation("Returned {Count} unique destination cities", cities.Count);
            return Ok(cities);
        }

        [HttpGet("getDestinationsByCity")]
        public async Task<ActionResult<List<DestinationSummaryDto>>> GetDestinationsByCity([FromQuery] string city)
        {
            _logger.LogInformation("Received GetDestinationsByCity request for city: {City}", city);

            var destinations = (await _destinationRepo.GetAllAsync(dt => dt.City == city)).OrderBy(dt => dt.Order);
            if (!destinations.Any())
            {
                _logger.LogWarning("No destinations found for city: {City}", city);
                return NotFound($"No destinations found for city: {city}");
            }

            var result = _mapper.Map<List<DestinationSummaryDto>>(destinations);

            _logger.LogInformation("Returned {Count} destinations for city: {City}", result.Count, city);
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

        #region AboutUs

        [HttpGet("getAboutUs")]
        public async Task<ActionResult<AboutUsReturnedDto>> GetAboutUs()
        {
            _logger.LogInformation("Received GetAboutUs request");

            var aboutUs = (await _aboutUsRepo.GetAllAsync()).FirstOrDefault();
            if (aboutUs == null)
                return NotFound("About us has not been set up yet");

            var result = _mapper.Map<AboutUsReturnedDto>(aboutUs);
            return Ok(result);
        }

        [HttpGet("getAllFounders")]
        public async Task<ActionResult<List<FounderReturnedDto>>> GetAllFounders()
        {
            _logger.LogInformation("Received GetAllFounders request");

            var founders = await _founderRepo.GetAllAsync();
            var result = _mapper.Map<List<FounderReturnedDto>>(founders);

            _logger.LogInformation("Returned {Count} founders", result.Count);
            return Ok(result);
        }

        #endregion

        #region Blog

        [HttpGet("getAllBlogs")]
        public async Task<ActionResult<List<BlogReturnedDto>>> GetAllBlogs()
        {
            _logger.LogInformation("Received GetAllBlogs request");

            var blogs = await _blogRepo.GetAllAsync();
            var result = _mapper.Map<List<BlogReturnedDto>>(blogs);

            _logger.LogInformation("Returned {Count} blogs", result.Count);
            return Ok(result);
        }

        [HttpGet("getBlogById/{id}")]
        public async Task<ActionResult<BlogReturnedDto>> GetBlogById(int id)
        {
            _logger.LogInformation("Received GetBlogById request for Id: {Id}", id);

            var blog = await _blogRepo.GetByIdAsync(id);
            if (blog == null)
            {
                _logger.LogWarning("Blog with Id {Id} not found", id);
                return NotFound("Blog not found");
            }

            var result = _mapper.Map<BlogReturnedDto>(blog);

            _logger.LogInformation("Returned blog with Id: {Id}", id);
            return Ok(result);
        }

        [HttpGet("getBlogSubsByBlogId/{blogId}")]
        public async Task<ActionResult<List<BlogSubReturnedDto>>> GetBlogSubsByBlogId(int blogId)
        {
            _logger.LogInformation("Received GetBlogSubsByBlogId request for BlogId: {BlogId}", blogId);

            var blog = await _blogRepo.GetByIdAsync(blogId);
            if (blog == null)
                return NotFound("Blog not found");

            var blogSubs = await _blogSubRepo.GetAllAsync(s => s.BlogId == blogId);
            var result = _mapper.Map<List<BlogSubReturnedDto>>(blogSubs);

            _logger.LogInformation("Returned {Count} blog subs for BlogId: {BlogId}", result.Count, blogId);
            return Ok(result);
        }

        #endregion

        [HttpGet("getAllPaymentMethods")]
        public async Task<ActionResult<List<PaymentMethodReturnedDto>>> GetAllPaymentMethods()
        {
            _logger.LogInformation("Received GetAllPaymentMethods request");

            var paymentMethods = await _paymentMethodRepo.GetAllAsync();
            var result = _mapper.Map<List<PaymentMethodReturnedDto>>(paymentMethods);

            _logger.LogInformation("Returned {Count} payment methods", result.Count);
            return Ok(result);
        }

        #region Reviews

        [HttpGet("getAllReviews")]
        public async Task<ActionResult<List<ReviewReturnedDto>>> GetAllReviews()
        {
            _logger.LogInformation("Received GetAllReviews request");

            var reviews = await _reviewRepo.GetAllAsync();
            var result = _mapper.Map<List<ReviewReturnedDto>>(reviews);

            _logger.LogInformation("Returned {Count} reviews", result.Count);
            return Ok(result);
        }

        [HttpGet("getAllReviewPlatforms")]
        public async Task<ActionResult<List<ReviewPlatformReturnedDto>>> GetAllReviewPlatforms()
        {
            _logger.LogInformation("Received GetAllReviewPlatforms request");

            var platforms = await _reviewPlatformRepo.GetAllAsync();
            var result = _mapper.Map<List<ReviewPlatformReturnedDto>>(platforms);

            _logger.LogInformation("Returned {Count} review platforms", result.Count);
            return Ok(result);
        }

        [HttpGet("getAllReels")]
        public async Task<ActionResult<List<ReelReturnedDto>>> GetAllReels()
        {
            _logger.LogInformation("Received GetAllReels request");

            var reels = await _reelRepo.GetAllAsync();
            var result = _mapper.Map<List<ReelReturnedDto>>(reels);

            _logger.LogInformation("Returned {Count} reels", result.Count);
            return Ok(result);
        }

        [HttpGet("getAllCustomerPhotos")]
        public async Task<ActionResult<List<CustomerPhotoReturnedDto>>> GetAllCustomerPhotos()
        {
            _logger.LogInformation("Received GetAllCustomerPhotos request");

            var photos = await _customerPhotoRepo.GetAllAsync();
            var result = _mapper.Map<List<CustomerPhotoReturnedDto>>(photos);

            _logger.LogInformation("Returned {Count} customer photos", result.Count);
            return Ok(result);
        }

        [HttpGet("getReviewsSettings")]
        public async Task<ActionResult<ReviewsSettingsReturnedDto>> GetReviewsSettings()
        {
            _logger.LogInformation("Received GetReviewsSettings request");

            var settings = (await _reviewsSettingsRepo.GetAllAsync()).FirstOrDefault();
            if (settings == null)
                return NotFound("Reviews settings have not been set up yet");

            var result = _mapper.Map<ReviewsSettingsReturnedDto>(settings);
            return Ok(result);
        }

        #endregion

        [HttpGet("getAllSocialMediaLinks")]
        public async Task<ActionResult<List<SocialMediaLinkReturnedDto>>> GetAllSocialMediaLinks()
        {
            _logger.LogInformation("Received GetAllSocialMediaLinks request");

            var socialMediaLinks = await _socialMediaLinkRepo.GetAllAsync();
            var result = _mapper.Map<List<SocialMediaLinkReturnedDto>>(socialMediaLinks);

            _logger.LogInformation("Returned {Count} social media links", result.Count);
            return Ok(result);
        }

    }
}
