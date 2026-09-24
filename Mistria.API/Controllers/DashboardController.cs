using System.Linq.Expressions;
using System.Numerics;
using System.Security.Claims;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mistria.API.Dtos;
using Mistria.API.Helpers;
using Mistria.Domain.Interfaces;
using Mistria.Domain.Models;
using Mistria.Domain.Services;
using System.Text.Json;

namespace Mistria.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AuditLogActionFilter))]
    public class DashboardController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly IMailService _mailService;
        private readonly IGenericRepository<TravelProgram> _travelProgramRepo;
        private readonly IGenericRepository<Destination> _destinationRepo;
        private readonly IGenericRepository<Wedding> _weddingRepo;
        private readonly IGenericRepository<Event> _eventRepo;
        private readonly IGenericRepository<Activity> _activityRepo;
        private readonly IGenericRepository<Service> _serviceRepo;
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
        private readonly IGenericRepository<AuditLog> _auditLogRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService tokenService,
            IConfiguration configuration,
            IMailService mailService,
            IGenericRepository<TravelProgram> travelProgramRepo,
            IGenericRepository<Destination> destinationRepo,
            IGenericRepository<Wedding> weddingRepo,
            IGenericRepository<Event> eventRepo,
            IGenericRepository<Activity> activityRepo,
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
            IGenericRepository<AuditLog> auditLogRepo,
            IMapper mapper,
            ILogger<DashboardController> logger

            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _configuration = configuration;
            _mailService = mailService;
            _travelProgramRepo = travelProgramRepo;
            _destinationRepo = destinationRepo;
            _weddingRepo = weddingRepo;
            _eventRepo = eventRepo;
            _activityRepo = activityRepo;
            _serviceRepo = serviceRepo;
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
            _auditLogRepo = auditLogRepo;
            _mapper = mapper;
            _logger = logger;
        }



        #region User
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> LoginOwner([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                return Unauthorized("Invalid email or password.");

            var result = await _signInManager.PasswordSignInAsync(user.UserName, loginDto.Password, loginDto.RememberMe, false);
            if (!result.Succeeded)
                return Unauthorized("Invalid email or password.");

            var token = await _tokenService.CreateTokenAsync(user, _userManager, loginDto.RememberMe);
            var expiration = loginDto.RememberMe
                ? DateTime.Now.AddDays(double.Parse(_configuration["JWT:RememberMeDurationInDays"]))
                : DateTime.Now.AddDays(double.Parse(_configuration["JWT:DurationInDays"]));

            _tokenService.StoreTokenInCookie(token, expiration, HttpContext);

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new LoginResponseDto
            {
                Email = user.Email,
                UserName = user.UserName,
                Roles = roles.ToList(),
                Token = token
            });
        }

        [HttpPost("addUser")]
        [Authorize]
        public async Task<ActionResult> AddUser([FromBody] RegisterDto registerDto)
        {
            _logger.LogInformation("Received AddUser request for email: {Email}", registerDto.Email);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new AppUser
            {
                UserName = registerDto.Email.Split("@")[0],
                Email = registerDto.Email,
                EmailConfirmed = true,
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to add user with email: {Email}, Errors: {Errors}", registerDto.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                return BadRequest(result.Errors);
            }

            await _userManager.AddToRoleAsync(user, "Admin");

            _logger.LogInformation("Successfully added user with email: {Email}", registerDto.Email);
            return Ok("User added successfully");
        }

        [HttpGet("getAllUsers")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userResponses = new List<UserSummaryDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var response = new UserSummaryDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = roles.FirstOrDefault()
                };
                userResponses.Add(response);
            }

            return Ok(new { Message = "Users retrieved successfully", Users = userResponses });
        }

        [HttpDelete("deleteUser/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteUser(string id)
        {
            _logger.LogInformation("Received DeleteUser request for user Id: {Id}", id);

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var userToDelete = await _userManager.FindByIdAsync(id);
            if (userToDelete == null)
            {
                _logger.LogWarning("User with Id {Id} not found", id);
                return NotFound("User not found");
            }

            var result = await _userManager.DeleteAsync(userToDelete);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to delete user with Id: {Id}, Errors: {Errors}", id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return BadRequest(result.Errors);
            }

            _logger.LogInformation("Successfully deleted user with Id: {Id}", id);
            return Ok("User deleted successfully");
        }

        [HttpPost("forgotpasswordOwner")]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest("Email is not exist");

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

            var email = new Email()
            {
                To = dto.Email,
                Subject = "Reset Password",
                Body = $"Your password reset code is: {code}\nThis code will expire shortly."
            };

            await _mailService.SendEmailAsync(email);

            return Ok("Check your inbox you have recieved Reset Password Code ");
        }

        [HttpPost("resetpasswordOwner")]
        public async Task<ActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest("Email is not exist");

            var result = await _userManager.ResetPasswordAsync(user, dto.Code, dto.NewPassword);

            if (result.Succeeded)
                return Ok("Password Changed Sucessfuly");

            var errors = result.Errors.Select(e => e.Description);

            return BadRequest(new { message = "Failed to Change Password", errors });
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            Response.Cookies.Delete("yourAppCookie");

            return Ok();
        }

        #endregion

        #region Audit Log

        [HttpGet("getAuditLogs")]
        [Authorize]
        public async Task<ActionResult<PagedResult<AuditLogReturnedDto>>> GetAuditLogs(
            [FromQuery] string? userEmail,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            _logger.LogInformation("Received GetAuditLogs request. UserEmail: {UserEmail}, FromDate: {FromDate}, ToDate: {ToDate}", userEmail, fromDate, toDate);

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            Expression<Func<AuditLog, bool>> predicate = log =>
                (string.IsNullOrEmpty(userEmail) || log.UserEmail == userEmail) &&
                (!fromDate.HasValue || log.Timestamp >= fromDate.Value) &&
                (!toDate.HasValue || log.Timestamp <= toDate.Value);

            var paged = await _auditLogRepo.GetPagedAsync(page, pageSize, predicate, log => log.Timestamp, descending: true);

            var result = new PagedResult<AuditLogReturnedDto>
            {
                Items = _mapper.Map<List<AuditLogReturnedDto>>(paged.Items),
                TotalItems = paged.TotalItems
            };

            _logger.LogInformation("Returned {Count} of {Total} audit log entries", result.Items.Count, result.TotalItems);
            return Ok(result);
        }

        #endregion

        #region Program

        private static readonly JsonSerializerOptions ProgramJsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        private bool TryParseItinerary(string? json, out List<ItineraryDay> itinerary, out string error)
        {
            itinerary = new List<ItineraryDay>();
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(json))
            {
                error = "Itinerary JSON is required";
                return false;
            }

            try
            {
                var parsed = JsonSerializer.Deserialize<List<ItineraryDay>>(json.Trim(), ProgramJsonOptions);
                if (parsed == null || parsed.Count == 0)
                {
                    error = "Itinerary is required and cannot be empty";
                    return false;
                }
                if (parsed.Any(d => string.IsNullOrWhiteSpace(d.Title)))
                {
                    error = "Each itinerary day requires a Title";
                    return false;
                }

                itinerary = parsed;
                return true;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize itinerary JSON: {Message} | Raw JSON: {Json}", ex.Message, json);
                error = "Invalid itinerary JSON format. Expected an array of day objects, e.g. [{\"dayNumber\":1,\"title\":\"...\",\"description\":\"...\"}]";
                return false;
            }
        }

        private bool TryParsePricingTiers(string? json, out List<PricingTier> tiers, out string error)
        {
            tiers = new List<PricingTier>();
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(json))
            {
                error = "Pricing tiers JSON is required";
                return false;
            }

            try
            {
                var parsed = JsonSerializer.Deserialize<List<PricingTier>>(json.Trim(), ProgramJsonOptions);
                if (parsed == null || parsed.Count == 0)
                {
                    error = "Pricing tiers are required and cannot be empty";
                    return false;
                }
                if (parsed.Any(t => string.IsNullOrWhiteSpace(t.Name)))
                {
                    error = "Each pricing tier requires a Name";
                    return false;
                }

                tiers = parsed;
                return true;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize pricing tiers JSON: {Message} | Raw JSON: {Json}", ex.Message, json);
                error = "Invalid pricing tiers JSON format. Expected an array of tier objects, e.g. [{\"name\":\"Gold\",\"dateRanges\":[...]}]";
                return false;
            }
        }

        // Applies ItineraryDayImages (matched by ItineraryDayImageIndexes) onto the parsed itinerary days.
        // Uploaded file URLs are appended to uploadedImageUrls so callers can clean them up on failure.
        private ActionResult? ApplyItineraryDayImages(List<ItineraryDay> itinerary, List<IFormFile>? dayImages, List<int>? dayImageIndexes, List<string> uploadedImageUrls, string uploadFolder = "ProgramsItinerary")
        {
            if (dayImages == null || !dayImages.Any())
                return null;

            if (dayImageIndexes == null || dayImageIndexes.Count != dayImages.Count)
                return BadRequest("ItineraryDayImageIndexes must be provided and match the number of ItineraryDayImages");

            for (int i = 0; i < dayImages.Count; i++)
            {
                var idx = dayImageIndexes[i];
                if (idx < 0 || idx >= itinerary.Count)
                    return BadRequest($"ItineraryDayImageIndexes contains an out-of-range index: {idx}");

                var file = dayImages[i];
                if (file?.Length > 0)
                {
                    var url = DocumentSettings.UploadFile(file, uploadFolder);
                    if (string.IsNullOrEmpty(url))
                        return BadRequest("Failed to upload itinerary day image");

                    uploadedImageUrls.Add(url);
                    itinerary[idx].Image = url;
                }
            }

            return null;
        }

        // Resolves the Order for a newly created item: uses the requested value if provided (after
        // checking it isn't already taken), otherwise appends it after the current last order.
        private async Task<(int Order, ActionResult? Error)> ResolveOrderForCreateAsync<T>(IGenericRepository<T> repo, int? requestedOrder, Func<T, int> orderSelector) where T : class
        {
            var all = await repo.GetAllAsync();

            if (requestedOrder.HasValue)
            {
                if (all.Any(x => orderSelector(x) == requestedOrder.Value))
                    return (0, BadRequest($"Order {requestedOrder.Value} is already used by another item. Please choose a different order."));

                return (requestedOrder.Value, null);
            }

            var nextOrder = all.Any() ? all.Max(orderSelector) + 1 : 1;
            return (nextOrder, null);
        }

        // Finds the item (other than currentId) that already holds newOrder, if any, so the caller
        // can swap the two orders instead of rejecting the update.
        private async Task<T?> FindOrderConflictAsync<T>(IGenericRepository<T> repo, int currentId, int newOrder, Func<T, int> idSelector, Func<T, int> orderSelector) where T : class
        {
            var all = await repo.GetAllAsync();

            return all.FirstOrDefault(x => idSelector(x) != currentId && orderSelector(x) == newOrder);
        }

        [HttpPost("addProgram")]
        public async Task<ActionResult> AddProgram([FromForm] ProgramDto programDto)
        {
            _logger.LogInformation("Received AddProgram request. ItineraryJson: '{Json}'", programDto.ItineraryJson ?? "null");
            _logger.LogInformation("ModelState Errors: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

            if (!TryParseItinerary(programDto.ItineraryJson, out var itinerary, out var itineraryError))
            {
                _logger.LogWarning("Invalid itinerary: {Error}", itineraryError);
                return BadRequest(itineraryError);
            }

            if (!TryParsePricingTiers(programDto.PricingTiersJson, out var pricingTiers, out var pricingError))
            {
                _logger.LogWarning("Invalid pricing tiers: {Error}", pricingError);
                return BadRequest(pricingError);
            }

            var (order, orderError) = await ResolveOrderForCreateAsync(_travelProgramRepo, programDto.Order, p => p.Order);
            if (orderError != null)
            {
                _logger.LogWarning("Invalid order: {Order}", programDto.Order);
                return orderError;
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            var imageUrls = new List<string>();
            var itineraryImageUrls = new List<string>();
            string cover = string.Empty;

            using var transaction = await _travelProgramRepo.BeginTransactionAsync();
            try
            {
                if (programDto.CoverImage != null)
                {
                    cover = DocumentSettings.UploadFile(programDto.CoverImage, "ProgramsCover");
                    if (string.IsNullOrEmpty(cover))
                        return BadRequest("Failed to upload cover image");
                }

                if (programDto.Images != null && programDto.Images.Any())
                {
                    foreach (var image in programDto.Images)
                    {
                        if (image?.Length > 0)
                        {
                            var fileUrl = DocumentSettings.UploadFile(image, "Programs");
                            if (!string.IsNullOrEmpty(fileUrl))
                                imageUrls.Add(fileUrl);
                        }
                    }
                    if (!imageUrls.Any() && programDto.Images.Any())
                        return BadRequest("Failed to upload images");
                }

                var imagesResult = ApplyItineraryDayImages(itinerary, programDto.ItineraryDayImages, programDto.ItineraryDayImageIndexes, itineraryImageUrls);
                if (imagesResult != null)
                    return imagesResult;

                var program = new TravelProgram
                {
                    Title = programDto.Title?.Trim(),
                    Description = programDto.Description?.Trim(),
                    Location = programDto.Location?.Trim(),
                    Duration = programDto.Duration?.Trim(),
                    Images = imageUrls,
                    CoverImage = cover,
                    Included = programDto.Included ?? new List<string>(),
                    Excluded = programDto.Excluded ?? new List<string>(),
                    IsMain = programDto.IsMain,
                    Order = order,
                    Itinerary = itinerary,
                    PricingTiers = pricingTiers
                };

                await _travelProgramRepo.AddAsync(program);
                await _travelProgramRepo.SaveChangesAsync();

                if (program.Id == 0)
                {
                    _logger.LogError("Failed to generate TravelProgram Id");
                    throw new InvalidOperationException("Failed to generate TravelProgram Id");
                }

                _logger.LogInformation("TravelProgram created with Id: {ProgramId}", program.Id);

                await _travelProgramRepo.CommitAsync(transaction);
                return Ok(new { Message = "Program created successfully", ProgramId = program.Id });
            }
            catch (Exception ex)
            {
                await _travelProgramRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover))
                    DocumentSettings.DeleteFile(cover, "ProgramsCover");

                foreach (var imageUrl in imageUrls)
                {
                    DocumentSettings.DeleteFile(imageUrl, "Programs");
                }

                foreach (var imageUrl in itineraryImageUrls)
                {
                    DocumentSettings.DeleteFile(imageUrl, "ProgramsItinerary");
                }

                _logger.LogError(ex, "Failed to create program: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while creating the program: {ex.Message}");
            }
        }

        [HttpPut("updateProgram/{id}")]
        public async Task<ActionResult> UpdateProgram(int id, [FromForm] UpdateProgramDto programDto)
        {
            _logger.LogInformation("Received UpdateProgram request for Id: {Id}. ItineraryJson: '{Json}'", id, programDto.ItineraryJson ?? "null");
            _logger.LogInformation("ModelState Errors: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

            var program = await _travelProgramRepo.GetByIdAsync(id);
            if (program == null)
            {
                _logger.LogWarning("Program with Id {Id} not found", id);
                return NotFound("Program not found");
            }

            var itinerary = program.Itinerary ?? new List<ItineraryDay>();
            if (!string.IsNullOrWhiteSpace(programDto.ItineraryJson))
            {
                if (!TryParseItinerary(programDto.ItineraryJson, out itinerary, out var itineraryError))
                {
                    _logger.LogWarning("Invalid itinerary: {Error}", itineraryError);
                    return BadRequest(itineraryError);
                }
            }

            var pricingTiers = program.PricingTiers ?? new List<PricingTier>();
            if (!string.IsNullOrWhiteSpace(programDto.PricingTiersJson))
            {
                if (!TryParsePricingTiers(programDto.PricingTiersJson, out pricingTiers, out var pricingError))
                {
                    _logger.LogWarning("Invalid pricing tiers: {Error}", pricingError);
                    return BadRequest(pricingError);
                }
            }

            TravelProgram? programToSwapOrderWith = null;
            if (programDto.Order.HasValue)
            {
                programToSwapOrderWith = await FindOrderConflictAsync(_travelProgramRepo, id, programDto.Order.Value, p => p.Id, p => p.Order);
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            var imageUrls = program.Images ?? new List<string>();
            var itineraryImageUrls = new List<string>();
            string cover = program.CoverImage ?? string.Empty;

            using var transaction = await _travelProgramRepo.BeginTransactionAsync();
            try
            {
                // Update only if new values are provided
                if (!string.IsNullOrWhiteSpace(programDto.Title))
                    program.Title = programDto.Title.Trim();
                if (!string.IsNullOrWhiteSpace(programDto.Description))
                    program.Description = programDto.Description.Trim();
                if (!string.IsNullOrWhiteSpace(programDto.Location))
                    program.Location = programDto.Location.Trim();
                if (!string.IsNullOrWhiteSpace(programDto.Duration))
                    program.Duration = programDto.Duration.Trim();
                if (programDto.IsMain.HasValue)
                    program.IsMain = programDto.IsMain.Value;
                if (programDto.Order.HasValue)
                {
                    if (programToSwapOrderWith != null)
                    {
                        programToSwapOrderWith.Order = program.Order;
                        _travelProgramRepo.Update(programToSwapOrderWith);
                    }
                    program.Order = programDto.Order.Value;
                }
                if (programDto.Included != null)
                    program.Included = programDto.Included;
                if (programDto.Excluded != null)
                    program.Excluded = programDto.Excluded;
                if (programDto.PricingTiersJson != null)
                    program.PricingTiers = pricingTiers;

                var imagesResult = ApplyItineraryDayImages(itinerary, programDto.ItineraryDayImages, programDto.ItineraryDayImageIndexes, itineraryImageUrls);
                if (imagesResult != null)
                    return imagesResult;

                if (programDto.ItineraryJson != null) // Check for null instead of empty
                    program.Itinerary = itinerary;

                // Handle images and cover image updates
                if (programDto.CoverImage != null)
                {
                    if (!string.IsNullOrEmpty(cover))
                        DocumentSettings.DeleteFile(cover, "ProgramsCover");
                    cover = DocumentSettings.UploadFile(programDto.CoverImage, "ProgramsCover");
                    if (string.IsNullOrEmpty(cover))
                        return BadRequest("Failed to upload cover image");
                }

                if (programDto.Images != null && programDto.Images.Any())
                {
                    foreach (var imageUrl in program.Images ?? new List<string>())
                    {
                        DocumentSettings.DeleteFile(imageUrl, "Programs");
                    }
                    imageUrls.Clear();
                    foreach (var image in programDto.Images)
                    {
                        if (image?.Length > 0)
                        {
                            var fileUrl = DocumentSettings.UploadFile(image, "Programs");
                            if (!string.IsNullOrEmpty(fileUrl))
                                imageUrls.Add(fileUrl);
                        }
                    }
                    if (!imageUrls.Any() && programDto.Images.Any())
                        return BadRequest("Failed to upload images");
                }

                program.Images = imageUrls;
                program.CoverImage = cover;

                _travelProgramRepo.Update(program);
                await _travelProgramRepo.SaveChangesAsync();

                _logger.LogInformation("TravelProgram updated with Id: {ProgramId}", program.Id);

                await _travelProgramRepo.CommitAsync(transaction);
                return Ok(new { Message = "Program updated successfully", ProgramId = program.Id });
            }
            catch (Exception ex)
            {
                await _travelProgramRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover) && programDto.CoverImage != null)
                    DocumentSettings.DeleteFile(cover, "ProgramsCover");

                foreach (var imageUrl in imageUrls)
                {
                    DocumentSettings.DeleteFile(imageUrl, "Programs");
                }

                foreach (var imageUrl in itineraryImageUrls)
                {
                    DocumentSettings.DeleteFile(imageUrl, "ProgramsItinerary");
                }

                _logger.LogError(ex, "Failed to update program: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while updating the program: {ex.Message}");
            }
        }

        [HttpDelete("deleteProgram/{id}")]
        public async Task<ActionResult> DeleteProgram(int id)
        {
            _logger.LogInformation("Received DeleteProgram request for Id: {Id}", id);

            var program = await _travelProgramRepo.GetByIdAsync(id);
            if (program == null)
            {
                _logger.LogWarning("Program with Id {Id} not found", id);
                return NotFound("Program not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            using var transaction = await _travelProgramRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrEmpty(program.CoverImage))
                    DocumentSettings.DeleteFile(program.CoverImage, "ProgramsCover");

                foreach (var imageUrl in program.Images ?? new List<string>())
                {
                    DocumentSettings.DeleteFile(imageUrl, "Programs");
                }

                foreach (var day in program.Itinerary ?? new List<ItineraryDay>())
                {
                    if (!string.IsNullOrEmpty(day.Image))
                        DocumentSettings.DeleteFile(day.Image, "ProgramsItinerary");
                }

                _travelProgramRepo.Delete(program);
                await _travelProgramRepo.SaveChangesAsync();

                _logger.LogInformation("TravelProgram deleted with Id: {ProgramId}", program.Id);

                await _travelProgramRepo.CommitAsync(transaction);
                return Ok(new { Message = "Program deleted successfully", ProgramId = program.Id });
            }
            catch (Exception ex)
            {
                await _travelProgramRepo.RollbackAsync(transaction);
                _logger.LogError(ex, "Failed to delete program: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while deleting the program: {ex.Message}");
            }
        }

        [HttpGet("getAllPrograms")]
        public async Task<ActionResult<List<ReturnedProgramDto>>> GetAllPrograms()
        {
            _logger.LogInformation("Received GetAllPrograms request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var programs = (await _travelProgramRepo.GetAllAsync()).OrderBy(p => p.Order).ToList();
            var result = _mapper.Map<List<ReturnedProgramDto>>(programs);

            _logger.LogInformation("Returned {Count} programs", result.Count);
            return Ok(result);
        }

        [HttpGet("getProgramById/{id}")]
        public async Task<ActionResult<ReturnedProgramDto>> GetProgramById(int id)
        {
            _logger.LogInformation("Received GetProgramById request for Id: {Id}", id);

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

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
        #endregion

        #region Destination
        [HttpPost("addDestination")]
        [Authorize]
        public async Task<ActionResult> AddDestination([FromForm] DestinationDto destinationDto)
        {
            _logger.LogInformation("Received AddDestination request. ItineraryJson: '{Json}'", destinationDto.ItineraryJson ?? "null");
            _logger.LogInformation("ModelState Errors: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

            if (!TryParseItinerary(destinationDto.ItineraryJson, out var itinerary, out var itineraryError))
            {
                _logger.LogWarning("Invalid itinerary: {Error}", itineraryError);
                return BadRequest(itineraryError);
            }

            if (!TryParsePricingTiers(destinationDto.PricingTiersJson, out var pricingTiers, out var pricingError))
            {
                _logger.LogWarning("Invalid pricing tiers: {Error}", pricingError);
                return BadRequest(pricingError);
            }

            var (order, orderError) = await ResolveOrderForCreateAsync(_destinationRepo, destinationDto.Order, d => d.Order);
            if (orderError != null)
            {
                _logger.LogWarning("Invalid order: {Order}", destinationDto.Order);
                return orderError;
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            var imageUrls = new List<string>();
            var itineraryImageUrls = new List<string>();
            string cover = string.Empty;

            using var transaction = await _destinationRepo.BeginTransactionAsync();
            try
            {
                if (destinationDto.CoverImage != null)
                {
                    cover = DocumentSettings.UploadFile(destinationDto.CoverImage, "DestinationsCover");
                    if (string.IsNullOrEmpty(cover))
                        return BadRequest("Failed to upload cover image");
                }

                if (destinationDto.Images != null && destinationDto.Images.Any())
                {
                    foreach (var image in destinationDto.Images)
                    {
                        if (image?.Length > 0)
                        {
                            var fileUrl = DocumentSettings.UploadFile(image, "Destinations");
                            if (!string.IsNullOrEmpty(fileUrl))
                                imageUrls.Add(fileUrl);
                        }
                    }
                    if (!imageUrls.Any() && destinationDto.Images.Any())
                        return BadRequest("Failed to upload images");
                }

                var imagesResult = ApplyItineraryDayImages(itinerary, destinationDto.ItineraryDayImages, destinationDto.ItineraryDayImageIndexes, itineraryImageUrls, "DestinationsItinerary");
                if (imagesResult != null)
                    return imagesResult;

                var destination = new Destination
                {
                    Title = destinationDto.Title?.Trim(),
                    Description = destinationDto.Description?.Trim(),
                    Location = destinationDto.Location?.Trim(),
                    Duration = destinationDto.Duration?.Trim(),
                    Images = imageUrls,
                    CoverImage = cover,
                    Included = destinationDto.Included ?? new List<string>(),
                    Excluded = destinationDto.Excluded ?? new List<string>(),
                    IsMain = destinationDto.IsMain,
                    Order = order,
                    Itinerary = itinerary,
                    PricingTiers = pricingTiers,
                    City = destinationDto.City?.Trim()
                };

                await _destinationRepo.AddAsync(destination);
                await _destinationRepo.SaveChangesAsync();

                if (destination.Id == 0)
                {
                    _logger.LogError("Failed to generate Destination Id");
                    throw new InvalidOperationException("Failed to generate Destination Id");
                }

                _logger.LogInformation("Destination created with Id: {DestinationId}", destination.Id);

                await _destinationRepo.CommitAsync(transaction);
                return Ok(new { Message = "Destination created successfully", DestinationId = destination.Id });
            }
            catch (Exception ex)
            {
                await _destinationRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover))
                    DocumentSettings.DeleteFile(cover, "DestinationsCover");

                foreach (var imageUrl in imageUrls)
                {
                    DocumentSettings.DeleteFile(imageUrl, "Destinations");
                }

                foreach (var imageUrl in itineraryImageUrls)
                {
                    DocumentSettings.DeleteFile(imageUrl, "DestinationsItinerary");
                }

                _logger.LogError(ex, "Failed to create destination: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while creating the destination: {ex.Message}");
            }
        }

        [HttpPut("updateDestination/{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateDestination(int id, [FromForm] UpdateDestinationDto destinationDto)
        {
            _logger.LogInformation("Received UpdateDestination request for Id: {Id}. ItineraryJson: '{Json}'", id, destinationDto.ItineraryJson ?? "null");
            _logger.LogInformation("ModelState Errors: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

            var destination = await _destinationRepo.GetByIdAsync(id);
            if (destination == null)
            {
                _logger.LogWarning("Destination with Id {Id} not found", id);
                return NotFound("Destination not found");
            }

            var itinerary = destination.Itinerary ?? new List<ItineraryDay>();
            if (!string.IsNullOrWhiteSpace(destinationDto.ItineraryJson))
            {
                if (!TryParseItinerary(destinationDto.ItineraryJson, out itinerary, out var itineraryError))
                {
                    _logger.LogWarning("Invalid itinerary: {Error}", itineraryError);
                    return BadRequest(itineraryError);
                }
            }

            var pricingTiers = destination.PricingTiers ?? new List<PricingTier>();
            if (!string.IsNullOrWhiteSpace(destinationDto.PricingTiersJson))
            {
                if (!TryParsePricingTiers(destinationDto.PricingTiersJson, out pricingTiers, out var pricingError))
                {
                    _logger.LogWarning("Invalid pricing tiers: {Error}", pricingError);
                    return BadRequest(pricingError);
                }
            }

            Destination? destinationToSwapOrderWith = null;
            if (destinationDto.Order.HasValue)
            {
                destinationToSwapOrderWith = await FindOrderConflictAsync(_destinationRepo, id, destinationDto.Order.Value, d => d.Id, d => d.Order);
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            var imageUrls = destination.Images ?? new List<string>();
            var itineraryImageUrls = new List<string>();
            string cover = destination.CoverImage ?? string.Empty;

            using var transaction = await _destinationRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrWhiteSpace(destinationDto.Title))
                    destination.Title = destinationDto.Title.Trim();
                if (!string.IsNullOrWhiteSpace(destinationDto.Description))
                    destination.Description = destinationDto.Description.Trim();
                if (!string.IsNullOrWhiteSpace(destinationDto.Location))
                    destination.Location = destinationDto.Location.Trim();
                if (!string.IsNullOrWhiteSpace(destinationDto.Duration))
                    destination.Duration = destinationDto.Duration.Trim();
                if (destinationDto.IsMain.HasValue)
                    destination.IsMain = destinationDto.IsMain.Value;
                if (destinationDto.Order.HasValue)
                {
                    if (destinationToSwapOrderWith != null)
                    {
                        destinationToSwapOrderWith.Order = destination.Order;
                        _destinationRepo.Update(destinationToSwapOrderWith);
                    }
                    destination.Order = destinationDto.Order.Value;
                }
                if (destinationDto.Included != null)
                    destination.Included = destinationDto.Included;
                if (destinationDto.Excluded != null)
                    destination.Excluded = destinationDto.Excluded;
                if (destinationDto.PricingTiersJson != null)
                    destination.PricingTiers = pricingTiers;
                if (!string.IsNullOrWhiteSpace(destinationDto.City))
                    destination.City = destinationDto.City.Trim();

                var imagesResult = ApplyItineraryDayImages(itinerary, destinationDto.ItineraryDayImages, destinationDto.ItineraryDayImageIndexes, itineraryImageUrls, "DestinationsItinerary");
                if (imagesResult != null)
                    return imagesResult;

                if (destinationDto.ItineraryJson != null)
                    destination.Itinerary = itinerary;

                if (destinationDto.CoverImage != null)
                {
                    if (!string.IsNullOrEmpty(cover))
                        DocumentSettings.DeleteFile(cover, "DestinationsCover");
                    cover = DocumentSettings.UploadFile(destinationDto.CoverImage, "DestinationsCover");
                    if (string.IsNullOrEmpty(cover))
                        return BadRequest("Failed to upload cover image");
                }

                if (destinationDto.Images != null && destinationDto.Images.Any())
                {
                    foreach (var imageUrl in destination.Images ?? new List<string>())
                    {
                        DocumentSettings.DeleteFile(imageUrl, "Destinations");
                    }
                    imageUrls.Clear();
                    foreach (var image in destinationDto.Images)
                    {
                        if (image?.Length > 0)
                        {
                            var fileUrl = DocumentSettings.UploadFile(image, "Destinations");
                            if (!string.IsNullOrEmpty(fileUrl))
                                imageUrls.Add(fileUrl);
                        }
                    }
                    if (!imageUrls.Any() && destinationDto.Images.Any())
                        return BadRequest("Failed to upload images");
                }

                destination.Images = imageUrls;
                destination.CoverImage = cover;

                _destinationRepo.Update(destination);
                await _destinationRepo.SaveChangesAsync();

                _logger.LogInformation("Destination updated with Id: {DestinationId}", destination.Id);

                await _destinationRepo.CommitAsync(transaction);
                return Ok(new { Message = "Destination updated successfully", DestinationId = destination.Id });
            }
            catch (Exception ex)
            {
                await _destinationRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover) && destinationDto.CoverImage != null)
                    DocumentSettings.DeleteFile(cover, "DestinationsCover");

                foreach (var imageUrl in imageUrls)
                {
                    DocumentSettings.DeleteFile(imageUrl, "Destinations");
                }

                foreach (var imageUrl in itineraryImageUrls)
                {
                    DocumentSettings.DeleteFile(imageUrl, "DestinationsItinerary");
                }

                _logger.LogError(ex, "Failed to update destination: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while updating the destination: {ex.Message}");
            }
        }

        [HttpGet("getAllDestinations")]
        [Authorize]
        public async Task<ActionResult<List<DestinationReturnedDto>>> GetAllDestinations()
        {
            _logger.LogInformation("Received GetAllDestinations request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var destinations = (await _destinationRepo.GetAllAsync()).OrderBy(d => d.Order).ToList();
            var result = _mapper.Map<List<DestinationReturnedDto>>(destinations);

            _logger.LogInformation("Returned {Count} destinations", result.Count);
            return Ok(result);
        }

        [HttpGet("getDestinationById/{id}")]
        [Authorize]
        public async Task<ActionResult<DestinationReturnedDto>> GetDestinationById(int id)
        {
            _logger.LogInformation("Received GetDestinationById request for Id: {Id}", id);

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

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

        [HttpDelete("deleteDestination/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteDestination(int id)
        {
            _logger.LogInformation("Received DeleteDestination request for Id: {Id}", id);

            var destination = await _destinationRepo.GetByIdAsync(id);
            if (destination == null)
            {
                _logger.LogWarning("Destination with Id {Id} not found", id);
                return NotFound("Destination not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            using var transaction = await _destinationRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrEmpty(destination.CoverImage))
                    DocumentSettings.DeleteFile(destination.CoverImage, "DestinationsCover");

                foreach (var imageUrl in destination.Images ?? new List<string>())
                {
                    DocumentSettings.DeleteFile(imageUrl, "Destinations");
                }

                foreach (var day in destination.Itinerary ?? new List<ItineraryDay>())
                {
                    if (!string.IsNullOrEmpty(day.Image))
                        DocumentSettings.DeleteFile(day.Image, "DestinationsItinerary");
                }

                _destinationRepo.Delete(destination);
                await _destinationRepo.SaveChangesAsync();

                _logger.LogInformation("Destination deleted with Id: {DestinationId}", destination.Id);

                await _destinationRepo.CommitAsync(transaction);
                return Ok(new { Message = "Destination deleted successfully", DestinationId = destination.Id });
            }
            catch (Exception ex)
            {
                await _destinationRepo.RollbackAsync(transaction);
                _logger.LogError(ex, "Failed to delete destination: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while deleting the destination: {ex.Message}");
            }
        }
        #endregion

        #region Wedding
        [HttpPost("addWedding")]
        [Authorize]
        public async Task<ActionResult> AddWedding([FromForm] WeddingDto weddingDto)
        {
            _logger.LogInformation("Received AddWedding request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            string cover = string.Empty;

            using var transaction = await _weddingRepo.BeginTransactionAsync();
            try
            {
                if (weddingDto.CoverImage != null)
                {
                    cover = DocumentSettings.UploadFile(weddingDto.CoverImage, "WeddingsCover");
                    if (string.IsNullOrEmpty(cover))
                        return BadRequest("Failed to upload cover image");
                }

                var wedding = new Wedding
                {
                    Title = weddingDto.Title?.Trim(),
                    Description = weddingDto.Description?.Trim(),
                    CoverImage = cover
                };

                await _weddingRepo.AddAsync(wedding);
                await _weddingRepo.SaveChangesAsync();

                if (wedding.Id == 0)
                {
                    _logger.LogError("Failed to generate Wedding Id");
                    throw new InvalidOperationException("Failed to generate Wedding Id");
                }

                _logger.LogInformation("Wedding created with Id: {WeddingId}", wedding.Id);

                await _weddingRepo.CommitAsync(transaction);
                return Ok(new { Message = "Wedding created successfully", WeddingId = wedding.Id });
            }
            catch (Exception ex)
            {
                await _weddingRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover))
                    DocumentSettings.DeleteFile(cover, "WeddingsCover");

                _logger.LogError(ex, "Failed to create wedding: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while creating the wedding: {ex.Message}");
            }
        }

        [HttpPut("updateWedding/{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateWedding(int id, [FromForm] UpdateWeddingDto weddingDto)
        {
            _logger.LogInformation("Received UpdateWedding request for Id: {Id}", id);

            var wedding = await _weddingRepo.GetByIdAsync(id);
            if (wedding == null)
            {
                _logger.LogWarning("Wedding with Id {Id} not found", id);
                return NotFound("Wedding not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            string cover = wedding.CoverImage ?? string.Empty;

            using var transaction = await _weddingRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrWhiteSpace(weddingDto.Title))
                    wedding.Title = weddingDto.Title.Trim();
                if (!string.IsNullOrWhiteSpace(weddingDto.Description))
                    wedding.Description = weddingDto.Description.Trim();

                if (weddingDto.CoverImage != null)
                {
                    if (!string.IsNullOrEmpty(cover))
                        DocumentSettings.DeleteFile(cover, "WeddingsCover");
                    cover = DocumentSettings.UploadFile(weddingDto.CoverImage, "WeddingsCover");
                    if (string.IsNullOrEmpty(cover))
                        return BadRequest("Failed to upload cover image");
                }

                wedding.CoverImage = cover;

                _weddingRepo.Update(wedding);
                await _weddingRepo.SaveChangesAsync();

                _logger.LogInformation("Wedding updated with Id: {WeddingId}", wedding.Id);

                await _weddingRepo.CommitAsync(transaction);
                return Ok(new { Message = "Wedding updated successfully", WeddingId = wedding.Id });
            }
            catch (Exception ex)
            {
                await _weddingRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover) && weddingDto.CoverImage != null)
                    DocumentSettings.DeleteFile(cover, "WeddingsCover");

                _logger.LogError(ex, "Failed to update wedding: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while updating the wedding: {ex.Message}");
            }
        }

        [HttpGet("getAllWeddings")]
        [Authorize]
        public async Task<ActionResult<List<WeddingReturnedDto>>> GetAllWeddings()
        {
            _logger.LogInformation("Received GetAllWeddings request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var weddings = await _weddingRepo.GetAllAsync();
            var result = _mapper.Map<List<WeddingReturnedDto>>(weddings);

            _logger.LogInformation("Returned {Count} weddings", result.Count);
            return Ok(result);
        }

        [HttpGet("getWeddingById/{id}")]
        [Authorize]
        public async Task<ActionResult<WeddingReturnedDto>> GetWeddingById(int id)
        {
            _logger.LogInformation("Received GetWeddingById request for Id: {Id}", id);

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var wedding = await _weddingRepo.GetByIdAsync(id);
            if (wedding == null)
            {
                _logger.LogWarning("Wedding with Id {Id} not found", id);
                return NotFound("Wedding not found");
            }

            var result = _mapper.Map<WeddingReturnedDto>(wedding);

            _logger.LogInformation("Returned wedding with Id: {Id}", id);
            return Ok(result);
        }

        [HttpDelete("deleteWedding/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteWedding(int id)
        {
            _logger.LogInformation("Received DeleteWedding request for Id: {Id}", id);

            var wedding = await _weddingRepo.GetByIdAsync(id);
            if (wedding == null)
            {
                _logger.LogWarning("Wedding with Id {Id} not found", id);
                return NotFound("Wedding not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            using var transaction = await _weddingRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrEmpty(wedding.CoverImage))
                    DocumentSettings.DeleteFile(wedding.CoverImage, "WeddingsCover");

                _weddingRepo.Delete(wedding);
                await _weddingRepo.SaveChangesAsync();

                _logger.LogInformation("Wedding deleted with Id: {WeddingId}", wedding.Id);

                await _weddingRepo.CommitAsync(transaction);
                return Ok(new { Message = "Wedding deleted successfully", WeddingId = wedding.Id });
            }
            catch (Exception ex)
            {
                await _weddingRepo.RollbackAsync(transaction);
                _logger.LogError(ex, "Failed to delete wedding: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while deleting the wedding: {ex.Message}");
            }
        }

        #endregion


        #region Event

        [HttpPost("addEvent")]
        [Authorize]
        public async Task<ActionResult> AddEvent([FromForm] EventDto eventDto)
        {
            _logger.LogInformation("Received AddEvent request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            string cover = string.Empty;

            using var transaction = await _eventRepo.BeginTransactionAsync();
            try
            {
                if (eventDto.CoverImage != null)
                {
                    cover = DocumentSettings.UploadFile(eventDto.CoverImage, "EventsCover");
                    if (string.IsNullOrEmpty(cover))
                        return BadRequest("Failed to upload cover image");
                }

                var ev = new Event
                {
                    Title = eventDto.Title?.Trim(),
                    Description = eventDto.Description?.Trim(),
                    CoverImage = cover
                };

                await _eventRepo.AddAsync(ev);
                await _eventRepo.SaveChangesAsync();

                if (ev.Id == 0)
                {
                    _logger.LogError("Failed to generate Event Id");
                    throw new InvalidOperationException("Failed to generate Event Id");
                }

                _logger.LogInformation("Event created with Id: {EventId}", ev.Id);

                await _eventRepo.CommitAsync(transaction);
                return Ok(new { Message = "Event created successfully", EventId = ev.Id });
            }
            catch (Exception ex)
            {
                await _eventRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover))
                    DocumentSettings.DeleteFile(cover, "EventsCover");

                _logger.LogError(ex, "Failed to create event: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while creating the event: {ex.Message}");
            }
        }

        [HttpPut("updateEvent/{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateEvent(int id, [FromForm] UpdateEventDto eventDto)
        {
            _logger.LogInformation("Received UpdateEvent request for Id: {Id}", id);

            var ev = await _eventRepo.GetByIdAsync(id);
            if (ev == null)
            {
                _logger.LogWarning("Event with Id {Id} not found", id);
                return NotFound("Event not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            string cover = ev.CoverImage ?? string.Empty;

            using var transaction = await _eventRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrWhiteSpace(eventDto.Title))
                    ev.Title = eventDto.Title.Trim();
                if (!string.IsNullOrWhiteSpace(eventDto.Description))
                    ev.Description = eventDto.Description.Trim();

                if (eventDto.CoverImage != null)
                {
                    if (!string.IsNullOrEmpty(cover))
                        DocumentSettings.DeleteFile(cover, "EventsCover");
                    cover = DocumentSettings.UploadFile(eventDto.CoverImage, "EventsCover");
                    if (string.IsNullOrEmpty(cover))
                        return BadRequest("Failed to upload cover image");
                }

                ev.CoverImage = cover;

                 _eventRepo.Update(ev);
                await _eventRepo.SaveChangesAsync();

                _logger.LogInformation("Event updated with Id: {EventId}", ev.Id);

                await _eventRepo.CommitAsync(transaction);
                return Ok(new { Message = "Event updated successfully", EventId = ev.Id });
            }
            catch (Exception ex)
            {
                await _eventRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover) && eventDto.CoverImage != null)
                    DocumentSettings.DeleteFile(cover, "EventsCover");

                _logger.LogError(ex, "Failed to update event: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while updating the event: {ex.Message}");
            }
        }

        [HttpGet("getAllEvents")]
        [Authorize]
        public async Task<ActionResult<List<EventReturnedDto>>> GetAllEvents()
        {
            _logger.LogInformation("Received GetAllEvents request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var events = await _eventRepo.GetAllAsync();
            var result = _mapper.Map<List<EventReturnedDto>>(events);

            _logger.LogInformation("Returned {Count} events", result.Count);
            return Ok(result);
        }

        [HttpGet("getEventById/{id}")]
        [Authorize]
        public async Task<ActionResult<EventReturnedDto>> GetEventById(int id)
        {
            _logger.LogInformation("Received GetEventById request for Id: {Id}", id);

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var ev = await _eventRepo.GetByIdAsync(id);
            if (ev == null)
            {
                _logger.LogWarning("Event with Id {Id} not found", id);
                return NotFound("Event not found");
            }

            var result = _mapper.Map<EventReturnedDto>(ev);

            _logger.LogInformation("Returned event with Id: {Id}", id);
            return Ok(result);
        }

        [HttpDelete("deleteEvent/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteEvent(int id)
        {
            _logger.LogInformation("Received DeleteEvent request for Id: {Id}", id);

            var ev = await _eventRepo.GetByIdAsync(id);
            if (ev == null)
            {
                _logger.LogWarning("Event with Id {Id} not found", id);
                return NotFound("Event not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            using var transaction = await _eventRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrEmpty(ev.CoverImage))
                    DocumentSettings.DeleteFile(ev.CoverImage, "EventsCover");

                 _eventRepo.Delete(ev);
                await _eventRepo.SaveChangesAsync();

                _logger.LogInformation("Event deleted with Id: {EventId}", ev.Id);

                await _eventRepo.CommitAsync(transaction);
                return Ok(new { Message = "Event deleted successfully", EventId = ev.Id });
            }
            catch (Exception ex)
            {
                await _eventRepo.RollbackAsync(transaction);
                _logger.LogError(ex, "Failed to delete event: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while deleting the event: {ex.Message}");
            }
        }

        #endregion


        #region Activity

        [HttpPost("addActivity")]
        [Authorize]
        public async Task<ActionResult> AddActivity([FromForm] ActivityDto activityDto)
        {
            _logger.LogInformation("Received AddActivity request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            string cover = string.Empty;

            using var transaction = await _activityRepo.BeginTransactionAsync();
            try
            {
                if (activityDto.CoverImage != null)
                {
                    cover = DocumentSettings.UploadFile(activityDto.CoverImage, "ActivitiesCover");
                    if (string.IsNullOrEmpty(cover))
                        return BadRequest("Failed to upload cover image");
                }

                var activity = new Activity
                {
                    Title = activityDto.Title?.Trim(),
                    Description = activityDto.Description?.Trim(),
                    CoverImage = cover,
                    Price = activityDto.Price
                };

                await _activityRepo.AddAsync(activity);
                await _activityRepo.SaveChangesAsync();

                if (activity.Id == 0)
                {
                    _logger.LogError("Failed to generate Activity Id");
                    throw new InvalidOperationException("Failed to generate Activity Id");
                }

                _logger.LogInformation("Activity created with Id: {ActivityId}", activity.Id);

                await _activityRepo.CommitAsync(transaction);
                return Ok(new { Message = "Activity created successfully", ActivityId = activity.Id });
            }
            catch (Exception ex)
            {
                await _activityRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover))
                    DocumentSettings.DeleteFile(cover, "ActivitiesCover");

                _logger.LogError(ex, "Failed to create activity: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while creating the activity: {ex.Message}");
            }
        }

        [HttpPut("updateActivity/{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateActivity(int id, [FromForm] UpdateActivityDto activityDto)
        {
            _logger.LogInformation("Received UpdateActivity request for Id: {Id}", id);

            var activity = await _activityRepo.GetByIdAsync(id);
            if (activity == null)
            {
                _logger.LogWarning("Activity with Id {Id} not found", id);
                return NotFound("Activity not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            string cover = activity.CoverImage ?? string.Empty;

            using var transaction = await _activityRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrWhiteSpace(activityDto.Title))
                    activity.Title = activityDto.Title.Trim();
                if (!string.IsNullOrWhiteSpace(activityDto.Description))
                    activity.Description = activityDto.Description.Trim();
                if (activityDto.Price.HasValue)
                    activity.Price = activityDto.Price.Value;

                if (activityDto.CoverImage != null)
                {
                    if (!string.IsNullOrEmpty(cover))
                        DocumentSettings.DeleteFile(cover, "ActivitiesCover");
                    cover = DocumentSettings.UploadFile(activityDto.CoverImage, "ActivitiesCover");
                    if (string.IsNullOrEmpty(cover))
                        return BadRequest("Failed to upload cover image");
                }

                activity.CoverImage = cover;

                 _activityRepo.Update(activity);
                await _activityRepo.SaveChangesAsync();

                _logger.LogInformation("Activity updated with Id: {ActivityId}", activity.Id);

                await _activityRepo.CommitAsync(transaction);
                return Ok(new { Message = "Activity updated successfully", ActivityId = activity.Id });
            }
            catch (Exception ex)
            {
                await _activityRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover) && activityDto.CoverImage != null)
                    DocumentSettings.DeleteFile(cover, "ActivitiesCover");

                _logger.LogError(ex, "Failed to update activity: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while updating the activity: {ex.Message}");
            }
        }

        [HttpGet("getAllActivities")]
        [Authorize]
        public async Task<ActionResult<List<ActivityReturnedDto>>> GetAllActivities()
        {
            _logger.LogInformation("Received GetAllActivities request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var activities = await _activityRepo.GetAllAsync();
            var result = _mapper.Map<List<ActivityReturnedDto>>(activities);

            _logger.LogInformation("Returned {Count} activities", result.Count);
            return Ok(result);
        }

        [HttpGet("getActivityById/{id}")]
        [Authorize]
        public async Task<ActionResult<ActivityReturnedDto>> GetActivityById(int id)
        {
            _logger.LogInformation("Received GetActivityById request for Id: {Id}", id);

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var activity = await _activityRepo.GetByIdAsync(id);
            if (activity == null)
            {
                _logger.LogWarning("Activity with Id {Id} not found", id);
                return NotFound("Activity not found");
            }

            var result = _mapper.Map<ActivityReturnedDto>(activity);

            _logger.LogInformation("Returned activity with Id: {Id}", id);
            return Ok(result);
        }

        [HttpDelete("deleteActivity/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteActivity(int id)
        {
            _logger.LogInformation("Received DeleteActivity request for Id: {Id}", id);

            var activity = await _activityRepo.GetByIdAsync(id);
            if (activity == null)
            {
                _logger.LogWarning("Activity with Id {Id} not found", id);
                return NotFound("Activity not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            using var transaction = await _activityRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrEmpty(activity.CoverImage))
                    DocumentSettings.DeleteFile(activity.CoverImage, "ActivitiesCover");

                 _activityRepo.Delete(activity);
                await _activityRepo.SaveChangesAsync();

                _logger.LogInformation("Activity deleted with Id: {ActivityId}", activity.Id);

                await _activityRepo.CommitAsync(transaction);
                return Ok(new { Message = "Activity deleted successfully", ActivityId = activity.Id });
            }
            catch (Exception ex)
            {
                await _activityRepo.RollbackAsync(transaction);
                _logger.LogError(ex, "Failed to delete activity: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while deleting the activity: {ex.Message}");
            }
        }


        #endregion

        #region Service

        [HttpPost("addService")]
        [Authorize]
        public async Task<ActionResult> AddService([FromForm] ServiceDto serviceDto)
        {
            _logger.LogInformation("Received AddService request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            string cover = string.Empty;

            using var transaction = await _serviceRepo.BeginTransactionAsync();
            try
            {
                if (serviceDto.CoverImage != null)
                {
                    cover = DocumentSettings.UploadFile(serviceDto.CoverImage, "ServicesCover");
                    if (string.IsNullOrEmpty(cover))
                        return BadRequest("Failed to upload cover image");
                }

                var service = new Service
                {
                    Title = serviceDto.Title?.Trim(),
                    Description = serviceDto.Description?.Trim(),
                    CoverImage = cover,
                    Price = serviceDto.Price
                };

                await _serviceRepo.AddAsync(service);
                await _serviceRepo.SaveChangesAsync();

                if (service.Id == 0)
                {
                    _logger.LogError("Failed to generate Service Id");
                    throw new InvalidOperationException("Failed to generate Service Id");
                }

                _logger.LogInformation("Service created with Id: {ServiceId}", service.Id);

                await _serviceRepo.CommitAsync(transaction);
                return Ok(new { Message = "Service created successfully", ServiceId = service.Id });
            }
            catch (Exception ex)
            {
                await _serviceRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover))
                    DocumentSettings.DeleteFile(cover, "ServicesCover");

                _logger.LogError(ex, "Failed to create service: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while creating the service: {ex.Message}");
            }
        }

        [HttpPut("updateService/{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateService(int id, [FromForm] UpdateServiceDto serviceDto)
        {
            _logger.LogInformation("Received UpdateService request for Id: {Id}", id);

            var service = await _serviceRepo.GetByIdAsync(id);
            if (service == null)
            {
                _logger.LogWarning("Service with Id {Id} not found", id);
                return NotFound("Service not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            string cover = service.CoverImage ?? string.Empty;

            using var transaction = await _serviceRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrWhiteSpace(serviceDto.Title))
                    service.Title = serviceDto.Title.Trim();
                if (!string.IsNullOrWhiteSpace(serviceDto.Description))
                    service.Description = serviceDto.Description.Trim();
                if (serviceDto.Price.HasValue)
                    service.Price = serviceDto.Price.Value;

                if (serviceDto.CoverImage != null)
                {
                    if (!string.IsNullOrEmpty(cover))
                        DocumentSettings.DeleteFile(cover, "ServicesCover");
                    cover = DocumentSettings.UploadFile(serviceDto.CoverImage, "ServicesCover");
                    if (string.IsNullOrEmpty(cover))
                        return BadRequest("Failed to upload cover image");
                }

                service.CoverImage = cover;

                _serviceRepo.Update(service);
                await _serviceRepo.SaveChangesAsync();

                _logger.LogInformation("Service updated with Id: {ServiceId}", service.Id);

                await _serviceRepo.CommitAsync(transaction);
                return Ok(new { Message = "Service updated successfully", ServiceId = service.Id });
            }
            catch (Exception ex)
            {
                await _serviceRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover) && serviceDto.CoverImage != null)
                    DocumentSettings.DeleteFile(cover, "ServicesCover");

                _logger.LogError(ex, "Failed to update service: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while updating the service: {ex.Message}");
            }
        }

        [HttpGet("getAllServices")]
        [Authorize]
        public async Task<ActionResult<List<ServiceReturnedDto>>> GetAllServices()
        {
            _logger.LogInformation("Received GetAllServices request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var services = await _serviceRepo.GetAllAsync();
            var result = _mapper.Map<List<ServiceReturnedDto>>(services);

            _logger.LogInformation("Returned {Count} services", result.Count);
            return Ok(result);
        }

        [HttpGet("getServiceById/{id}")]
        [Authorize]
        public async Task<ActionResult<ServiceReturnedDto>> GetServiceById(int id)
        {
            _logger.LogInformation("Received GetServiceById request for Id: {Id}", id);

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var service = await _serviceRepo.GetByIdAsync(id);
            if (service == null)
            {
                _logger.LogWarning("Service with Id {Id} not found", id);
                return NotFound("Service not found");
            }

            var result = _mapper.Map<ServiceReturnedDto>(service);

            _logger.LogInformation("Returned service with Id: {Id}", id);
            return Ok(result);
        }

        [HttpDelete("deleteService/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteService(int id)
        {
            _logger.LogInformation("Received DeleteService request for Id: {Id}", id);

            var service = await _serviceRepo.GetByIdAsync(id);
            if (service == null)
            {
                _logger.LogWarning("Service with Id {Id} not found", id);
                return NotFound("Service not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            using var transaction = await _serviceRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrEmpty(service.CoverImage))
                    DocumentSettings.DeleteFile(service.CoverImage, "ServicesCover");

                _serviceRepo.Delete(service);
                await _serviceRepo.SaveChangesAsync();

                _logger.LogInformation("Service deleted with Id: {ServiceId}", service.Id);

                await _serviceRepo.CommitAsync(transaction);
                return Ok(new { Message = "Service deleted successfully", ServiceId = service.Id });
            }
            catch (Exception ex)
            {
                await _serviceRepo.RollbackAsync(transaction);
                _logger.LogError(ex, "Failed to delete service: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while deleting the service: {ex.Message}");
            }
        }

        #endregion

        #region AboutUs

        [HttpPost("addOrUpdateAboutUs")]
        [Authorize]
        public async Task<ActionResult> AddOrUpdateAboutUs([FromBody] AboutUsDto aboutUsDto)
        {
            _logger.LogInformation("Received AddOrUpdateAboutUs request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            using var transaction = await _aboutUsRepo.BeginTransactionAsync();
            try
            {
                var aboutUs = (await _aboutUsRepo.GetAllAsync()).FirstOrDefault();

                if (aboutUs == null)
                {
                    aboutUs = new AboutUs
                    {
                        MainDescription = aboutUsDto.MainDescription.Trim(),
                        OurStory = aboutUsDto.OurStory.Trim()
                    };
                    await _aboutUsRepo.AddAsync(aboutUs);
                }
                else
                {
                    aboutUs.MainDescription = aboutUsDto.MainDescription.Trim();
                    aboutUs.OurStory = aboutUsDto.OurStory.Trim();
                    _aboutUsRepo.Update(aboutUs);
                }

                await _aboutUsRepo.SaveChangesAsync();

                _logger.LogInformation("AboutUs saved with Id: {AboutUsId}", aboutUs.Id);

                await _aboutUsRepo.CommitAsync(transaction);
                return Ok(new { Message = "About us saved successfully", AboutUsId = aboutUs.Id });
            }
            catch (Exception ex)
            {
                await _aboutUsRepo.RollbackAsync(transaction);
                _logger.LogError(ex, "Failed to save about us: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while saving about us: {ex.Message}");
            }
        }

        [HttpGet("getAboutUs")]
        [Authorize]
        public async Task<ActionResult<AboutUsReturnedDto>> GetAboutUs()
        {
            _logger.LogInformation("Received GetAboutUs request");

            var aboutUs = (await _aboutUsRepo.GetAllAsync()).FirstOrDefault();
            if (aboutUs == null)
                return NotFound("About us has not been set up yet");

            var result = _mapper.Map<AboutUsReturnedDto>(aboutUs);
            return Ok(result);
        }

        #region Founders

        [HttpPost("addFounder")]
        [Authorize]
        public async Task<ActionResult> AddFounder([FromForm] FounderDto founderDto)
        {
            _logger.LogInformation("Received AddFounder request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            string cover = string.Empty;

            using var transaction = await _founderRepo.BeginTransactionAsync();
            try
            {
                if (founderDto.CoverImage != null)
                {
                    cover = DocumentSettings.UploadFile(founderDto.CoverImage, "FoundersCover");
                    if (string.IsNullOrEmpty(cover))
                        return BadRequest("Failed to upload cover image");
                }

                var founder = new Founder
                {
                    Title = founderDto.Title?.Trim(),
                    Description = founderDto.Description?.Trim(),
                    CoverImage = cover
                };

                await _founderRepo.AddAsync(founder);
                await _founderRepo.SaveChangesAsync();

                if (founder.Id == 0)
                {
                    _logger.LogError("Failed to generate Founder Id");
                    throw new InvalidOperationException("Failed to generate Founder Id");
                }

                _logger.LogInformation("Founder created with Id: {FounderId}", founder.Id);

                await _founderRepo.CommitAsync(transaction);
                return Ok(new { Message = "Founder created successfully", FounderId = founder.Id });
            }
            catch (Exception ex)
            {
                await _founderRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover))
                    DocumentSettings.DeleteFile(cover, "FoundersCover");

                _logger.LogError(ex, "Failed to create founder: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while creating the founder: {ex.Message}");
            }
        }

        [HttpPut("updateFounder/{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateFounder(int id, [FromForm] UpdateFounderDto founderDto)
        {
            _logger.LogInformation("Received UpdateFounder request for Id: {Id}", id);

            var founder = await _founderRepo.GetByIdAsync(id);
            if (founder == null)
            {
                _logger.LogWarning("Founder with Id {Id} not found", id);
                return NotFound("Founder not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            string cover = founder.CoverImage ?? string.Empty;

            using var transaction = await _founderRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrWhiteSpace(founderDto.Title))
                    founder.Title = founderDto.Title.Trim();
                if (!string.IsNullOrWhiteSpace(founderDto.Description))
                    founder.Description = founderDto.Description.Trim();

                if (founderDto.CoverImage != null)
                {
                    if (!string.IsNullOrEmpty(cover))
                        DocumentSettings.DeleteFile(cover, "FoundersCover");
                    cover = DocumentSettings.UploadFile(founderDto.CoverImage, "FoundersCover");
                    if (string.IsNullOrEmpty(cover))
                        return BadRequest("Failed to upload cover image");
                }

                founder.CoverImage = cover;

                _founderRepo.Update(founder);
                await _founderRepo.SaveChangesAsync();

                _logger.LogInformation("Founder updated with Id: {FounderId}", founder.Id);

                await _founderRepo.CommitAsync(transaction);
                return Ok(new { Message = "Founder updated successfully", FounderId = founder.Id });
            }
            catch (Exception ex)
            {
                await _founderRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover) && founderDto.CoverImage != null)
                    DocumentSettings.DeleteFile(cover, "FoundersCover");

                _logger.LogError(ex, "Failed to update founder: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while updating the founder: {ex.Message}");
            }
        }

        [HttpGet("getAllFounders")]
        [Authorize]
        public async Task<ActionResult<List<FounderReturnedDto>>> GetAllFounders()
        {
            _logger.LogInformation("Received GetAllFounders request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var founders = await _founderRepo.GetAllAsync();
            var result = _mapper.Map<List<FounderReturnedDto>>(founders);

            _logger.LogInformation("Returned {Count} founders", result.Count);
            return Ok(result);
        }

        [HttpGet("getFounderById/{id}")]
        [Authorize]
        public async Task<ActionResult<FounderReturnedDto>> GetFounderById(int id)
        {
            _logger.LogInformation("Received GetFounderById request for Id: {Id}", id);

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var founder = await _founderRepo.GetByIdAsync(id);
            if (founder == null)
            {
                _logger.LogWarning("Founder with Id {Id} not found", id);
                return NotFound("Founder not found");
            }

            var result = _mapper.Map<FounderReturnedDto>(founder);

            _logger.LogInformation("Returned founder with Id: {Id}", id);
            return Ok(result);
        }

        [HttpDelete("deleteFounder/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteFounder(int id)
        {
            _logger.LogInformation("Received DeleteFounder request for Id: {Id}", id);

            var founder = await _founderRepo.GetByIdAsync(id);
            if (founder == null)
            {
                _logger.LogWarning("Founder with Id {Id} not found", id);
                return NotFound("Founder not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            using var transaction = await _founderRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrEmpty(founder.CoverImage))
                    DocumentSettings.DeleteFile(founder.CoverImage, "FoundersCover");

                _founderRepo.Delete(founder);
                await _founderRepo.SaveChangesAsync();

                _logger.LogInformation("Founder deleted with Id: {FounderId}", founder.Id);

                await _founderRepo.CommitAsync(transaction);
                return Ok(new { Message = "Founder deleted successfully", FounderId = founder.Id });
            }
            catch (Exception ex)
            {
                await _founderRepo.RollbackAsync(transaction);
                _logger.LogError(ex, "Failed to delete founder: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while deleting the founder: {ex.Message}");
            }
        }

        #endregion

        #endregion

        #region Blog

        [HttpPost("addBlog")]
        [Authorize]
        public async Task<ActionResult> AddBlog([FromForm] BlogDto blogDto)
        {
            _logger.LogInformation("Received AddBlog request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            string cover = string.Empty;

            using var transaction = await _blogRepo.BeginTransactionAsync();
            try
            {
                if (blogDto.CoverImage != null)
                {
                    cover = DocumentSettings.UploadFile(blogDto.CoverImage, "BlogsCover");
                    if (string.IsNullOrEmpty(cover))
                        return BadRequest("Failed to upload cover image");
                }

                var blog = new Blog
                {
                    Title = blogDto.Title?.Trim(),
                    Description = blogDto.Description?.Trim(),
                    CoverImage = cover
                };

                await _blogRepo.AddAsync(blog);
                await _blogRepo.SaveChangesAsync();

                if (blog.Id == 0)
                {
                    _logger.LogError("Failed to generate Blog Id");
                    throw new InvalidOperationException("Failed to generate Blog Id");
                }

                _logger.LogInformation("Blog created with Id: {BlogId}", blog.Id);

                await _blogRepo.CommitAsync(transaction);
                return Ok(new { Message = "Blog created successfully", BlogId = blog.Id });
            }
            catch (Exception ex)
            {
                await _blogRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover))
                    DocumentSettings.DeleteFile(cover, "BlogsCover");

                _logger.LogError(ex, "Failed to create blog: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while creating the blog: {ex.Message}");
            }
        }

        [HttpPost("addBlogWithSubs")]
        [Authorize]
        public async Task<ActionResult> AddBlogWithSubs([FromForm] BlogWithSubsDto blogDto)
        {
            _logger.LogInformation("Received AddBlogWithSubs request. SubsJson: '{Json}'", blogDto.SubsJson ?? "null");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(blogDto.SubsJson))
                return BadRequest("Subs JSON is required");

            List<BlogSubItemDto> subs;
            try
            {
                subs = JsonSerializer.Deserialize<List<BlogSubItemDto>>(blogDto.SubsJson.Trim(), ProgramJsonOptions) ?? new List<BlogSubItemDto>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize subs JSON: {Message} | Raw JSON: {Json}", ex.Message, blogDto.SubsJson);
                return BadRequest("Invalid subs JSON format. Expected an array of objects, e.g. [{\"title\":\"FAQ\",\"content\":{\"key\":\"value\"}}]");
            }

            if (subs.Count == 0)
                return BadRequest("Subs are required and cannot be empty");

            if (subs.Any(s => string.IsNullOrWhiteSpace(s.Title)))
                return BadRequest("Each sub requires a Title");

            if (subs.Any(s => s.Content == null || s.Content.Count == 0))
                return BadRequest("Each sub requires non-empty Content");

            var subImages = blogDto.SubImages ?? new List<IFormFile>();
            if (subImages.Any())
            {
                if (blogDto.SubImageIndexes == null || blogDto.SubImageIndexes.Count != subImages.Count)
                    return BadRequest("SubImageIndexes must be provided and match the number of SubImages");

                if (blogDto.SubImageIndexes.Any(idx => idx < 0 || idx >= subs.Count))
                    return BadRequest("SubImageIndexes contains an out-of-range index");

                if (blogDto.SubImageIndexes.Distinct().Count() != blogDto.SubImageIndexes.Count)
                    return BadRequest("SubImageIndexes must not contain duplicate indexes");
            }

            string cover = string.Empty;
            var subCoverUrls = new List<string>();

            using var transaction = await _blogRepo.BeginTransactionAsync();
            try
            {
                cover = DocumentSettings.UploadFile(blogDto.CoverImage, "BlogsCover");
                if (string.IsNullOrEmpty(cover))
                    return BadRequest("Failed to upload cover image");

                var blog = new Blog
                {
                    Title = blogDto.Title?.Trim(),
                    Description = blogDto.Description?.Trim(),
                    CoverImage = cover
                };

                await _blogRepo.AddAsync(blog);
                await _blogRepo.SaveChangesAsync();

                if (blog.Id == 0)
                {
                    _logger.LogError("Failed to generate Blog Id");
                    throw new InvalidOperationException("Failed to generate Blog Id");
                }

                var blogSubIds = new List<int>();

                for (int i = 0; i < subs.Count; i++)
                {
                    var subCover = string.Empty;
                    var imagePos = blogDto.SubImageIndexes?.IndexOf(i) ?? -1;
                    if (imagePos >= 0)
                    {
                        subCover = DocumentSettings.UploadFile(subImages[imagePos], "BlogSubsCover");
                        if (string.IsNullOrEmpty(subCover))
                            return BadRequest($"Failed to upload cover image for sub at index {i}");

                        subCoverUrls.Add(subCover);
                    }

                    var blogSub = new BlogSub
                    {
                        BlogId = blog.Id,
                        Title = subs[i].Title.Trim(),
                        CoverImage = subCover,
                        Content = subs[i].Content
                    };

                    await _blogSubRepo.AddAsync(blogSub);
                    await _blogSubRepo.SaveChangesAsync();

                    blogSubIds.Add(blogSub.Id);
                }

                _logger.LogInformation("Blog created with Id: {BlogId} and {Count} subs", blog.Id, blogSubIds.Count);

                await _blogRepo.CommitAsync(transaction);
                return Ok(new { Message = "Blog with subs created successfully", BlogId = blog.Id, BlogSubIds = blogSubIds });
            }
            catch (Exception ex)
            {
                await _blogRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover))
                    DocumentSettings.DeleteFile(cover, "BlogsCover");

                foreach (var subCoverUrl in subCoverUrls)
                {
                    DocumentSettings.DeleteFile(subCoverUrl, "BlogSubsCover");
                }

                _logger.LogError(ex, "Failed to create blog with subs: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while creating the blog with subs: {ex.Message}");
            }
        }

        [HttpPut("updateBlog/{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateBlog(int id, [FromForm] UpdateBlogDto blogDto)
        {
            _logger.LogInformation("Received UpdateBlog request for Id: {Id}", id);

            var blog = await _blogRepo.GetByIdAsync(id);
            if (blog == null)
            {
                _logger.LogWarning("Blog with Id {Id} not found", id);
                return NotFound("Blog not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            string cover = blog.CoverImage ?? string.Empty;

            using var transaction = await _blogRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrWhiteSpace(blogDto.Title))
                    blog.Title = blogDto.Title.Trim();
                if (!string.IsNullOrWhiteSpace(blogDto.Description))
                    blog.Description = blogDto.Description.Trim();

                if (blogDto.CoverImage != null)
                {
                    if (!string.IsNullOrEmpty(cover))
                        DocumentSettings.DeleteFile(cover, "BlogsCover");
                    cover = DocumentSettings.UploadFile(blogDto.CoverImage, "BlogsCover");
                    if (string.IsNullOrEmpty(cover))
                        return BadRequest("Failed to upload cover image");
                }

                blog.CoverImage = cover;

                _blogRepo.Update(blog);
                await _blogRepo.SaveChangesAsync();

                _logger.LogInformation("Blog updated with Id: {BlogId}", blog.Id);

                await _blogRepo.CommitAsync(transaction);
                return Ok(new { Message = "Blog updated successfully", BlogId = blog.Id });
            }
            catch (Exception ex)
            {
                await _blogRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover) && blogDto.CoverImage != null)
                    DocumentSettings.DeleteFile(cover, "BlogsCover");

                _logger.LogError(ex, "Failed to update blog: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while updating the blog: {ex.Message}");
            }
        }

        [HttpGet("getAllBlogs")]
        [Authorize]
        public async Task<ActionResult<List<BlogReturnedDto>>> GetAllBlogs()
        {
            _logger.LogInformation("Received GetAllBlogs request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var blogs = await _blogRepo.GetAllAsync();
            var result = _mapper.Map<List<BlogReturnedDto>>(blogs);

            _logger.LogInformation("Returned {Count} blogs", result.Count);
            return Ok(result);
        }

        [HttpGet("getBlogById/{id}")]
        [Authorize]
        public async Task<ActionResult<BlogReturnedDto>> GetBlogById(int id)
        {
            _logger.LogInformation("Received GetBlogById request for Id: {Id}", id);

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

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

        [HttpDelete("deleteBlog/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteBlog(int id)
        {
            _logger.LogInformation("Received DeleteBlog request for Id: {Id}", id);

            var blog = await _blogRepo.GetByIdAsync(id);
            if (blog == null)
            {
                _logger.LogWarning("Blog with Id {Id} not found", id);
                return NotFound("Blog not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            using var transaction = await _blogRepo.BeginTransactionAsync();
            try
            {
                var subs = await _blogSubRepo.GetAllAsync(s => s.BlogId == id);
                foreach (var sub in subs)
                {
                    if (!string.IsNullOrEmpty(sub.CoverImage))
                        DocumentSettings.DeleteFile(sub.CoverImage, "BlogSubsCover");

                    _blogSubRepo.Delete(sub);
                }
                await _blogSubRepo.SaveChangesAsync();

                if (!string.IsNullOrEmpty(blog.CoverImage))
                    DocumentSettings.DeleteFile(blog.CoverImage, "BlogsCover");

                _blogRepo.Delete(blog);
                await _blogRepo.SaveChangesAsync();

                _logger.LogInformation("Blog deleted with Id: {BlogId}", blog.Id);

                await _blogRepo.CommitAsync(transaction);
                return Ok(new { Message = "Blog deleted successfully", BlogId = blog.Id });
            }
            catch (Exception ex)
            {
                await _blogRepo.RollbackAsync(transaction);
                _logger.LogError(ex, "Failed to delete blog: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while deleting the blog: {ex.Message}");
            }
        }

        #region BlogSub

        [HttpPost("addBlogSub")]
        [Authorize]
        public async Task<ActionResult> AddBlogSub([FromForm] BlogSubDto blogSubDto)
        {
            _logger.LogInformation("Received AddBlogSub request. ContentJson: '{Json}'", blogSubDto.ContentJson ?? "null");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            var blog = await _blogRepo.GetByIdAsync(blogSubDto.BlogId);
            if (blog == null)
                return NotFound("Blog not found");

            if (string.IsNullOrWhiteSpace(blogSubDto.ContentJson))
                return BadRequest("Content JSON is required");

            Dictionary<string, string> content;
            try
            {
                var cleanedJson = blogSubDto.ContentJson.Trim();
                using var doc = JsonDocument.Parse(cleanedJson);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                    return BadRequest("Content JSON must be an object (e.g., {\"key\": \"value\"})");

                content = JsonSerializer.Deserialize<Dictionary<string, string>>(cleanedJson) ?? new Dictionary<string, string>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize content JSON: {Message} | Raw JSON: {Json}", ex.Message, blogSubDto.ContentJson);
                return BadRequest("Invalid content JSON format. Use {\"key\": \"value\", \"key2\": \"value2\"}");
            }

            if (content.Count == 0)
                return BadRequest("Content is required and cannot be empty");

            string cover = string.Empty;

            using var transaction = await _blogSubRepo.BeginTransactionAsync();
            try
            {
                if (blogSubDto.CoverImage != null)
                {
                    cover = DocumentSettings.UploadFile(blogSubDto.CoverImage, "BlogSubsCover");
                    if (string.IsNullOrEmpty(cover))
                        return BadRequest("Failed to upload cover image");
                }

                var blogSub = new BlogSub
                {
                    BlogId = blogSubDto.BlogId,
                    Title = blogSubDto.Title?.Trim(),
                    CoverImage = cover,
                    Content = content
                };

                await _blogSubRepo.AddAsync(blogSub);
                await _blogSubRepo.SaveChangesAsync();

                if (blogSub.Id == 0)
                {
                    _logger.LogError("Failed to generate BlogSub Id");
                    throw new InvalidOperationException("Failed to generate BlogSub Id");
                }

                _logger.LogInformation("BlogSub created with Id: {BlogSubId}", blogSub.Id);

                await _blogSubRepo.CommitAsync(transaction);
                return Ok(new { Message = "Blog sub created successfully", BlogSubId = blogSub.Id });
            }
            catch (Exception ex)
            {
                await _blogSubRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover))
                    DocumentSettings.DeleteFile(cover, "BlogSubsCover");

                _logger.LogError(ex, "Failed to create blog sub: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while creating the blog sub: {ex.Message}");
            }
        }

        [HttpPut("updateBlogSub/{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateBlogSub(int id, [FromForm] UpdateBlogSubDto blogSubDto)
        {
            _logger.LogInformation("Received UpdateBlogSub request for Id: {Id}. ContentJson: '{Json}'", id, blogSubDto.ContentJson ?? "null");

            var blogSub = await _blogSubRepo.GetByIdAsync(id);
            if (blogSub == null)
            {
                _logger.LogWarning("BlogSub with Id {Id} not found", id);
                return NotFound("Blog sub not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            var content = blogSub.Content ?? new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(blogSubDto.ContentJson))
            {
                try
                {
                    var cleanedJson = blogSubDto.ContentJson.Trim();
                    using var doc = JsonDocument.Parse(cleanedJson);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                    {
                        var firstObject = doc.RootElement[0];
                        if (firstObject.ValueKind == JsonValueKind.Object)
                        {
                            content = JsonSerializer.Deserialize<Dictionary<string, string>>(firstObject.GetRawText()) ?? new Dictionary<string, string>();
                        }
                        else
                        {
                            return BadRequest("Content JSON array must contain at least one object (e.g., [{\"key\": \"value\"}])");
                        }
                    }
                    else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        content = JsonSerializer.Deserialize<Dictionary<string, string>>(cleanedJson) ?? new Dictionary<string, string>();
                    }
                    else
                    {
                        return BadRequest("Content JSON must be an object or array of objects (e.g., {\"key\": \"value\"} or [{\"key\": \"value\"}])");
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize content JSON: {Message} | Raw JSON: {Json}", ex.Message, blogSubDto.ContentJson);
                    return BadRequest("Invalid content JSON format. Use {\"key\": \"value\", ...} or [{\"key\": \"value\", ...}]");
                }

                if (content.Count == 0)
                    return BadRequest("Content is required and cannot be empty");
            }

            string cover = blogSub.CoverImage ?? string.Empty;

            using var transaction = await _blogSubRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrWhiteSpace(blogSubDto.Title))
                    blogSub.Title = blogSubDto.Title.Trim();
                if (blogSubDto.ContentJson != null)
                    blogSub.Content = content;

                if (blogSubDto.CoverImage != null)
                {
                    if (!string.IsNullOrEmpty(cover))
                        DocumentSettings.DeleteFile(cover, "BlogSubsCover");
                    cover = DocumentSettings.UploadFile(blogSubDto.CoverImage, "BlogSubsCover");
                    if (string.IsNullOrEmpty(cover))
                        return BadRequest("Failed to upload cover image");
                }
                else if (blogSubDto.RemoveCoverImage == true)
                {
                    if (!string.IsNullOrEmpty(cover))
                        DocumentSettings.DeleteFile(cover, "BlogSubsCover");
                    cover = string.Empty;
                }

                blogSub.CoverImage = cover;

                _blogSubRepo.Update(blogSub);
                await _blogSubRepo.SaveChangesAsync();

                _logger.LogInformation("BlogSub updated with Id: {BlogSubId}", blogSub.Id);

                await _blogSubRepo.CommitAsync(transaction);
                return Ok(new { Message = "Blog sub updated successfully", BlogSubId = blogSub.Id });
            }
            catch (Exception ex)
            {
                await _blogSubRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover) && blogSubDto.CoverImage != null)
                    DocumentSettings.DeleteFile(cover, "BlogSubsCover");

                _logger.LogError(ex, "Failed to update blog sub: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while updating the blog sub: {ex.Message}");
            }
        }

        [HttpGet("getAllBlogSubs")]
        [Authorize]
        public async Task<ActionResult<List<BlogSubReturnedDto>>> GetAllBlogSubs()
        {
            _logger.LogInformation("Received GetAllBlogSubs request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var blogSubs = await _blogSubRepo.GetAllAsync();
            var result = _mapper.Map<List<BlogSubReturnedDto>>(blogSubs);

            _logger.LogInformation("Returned {Count} blog subs", result.Count);
            return Ok(result);
        }

        [HttpGet("getBlogSubById/{id}")]
        [Authorize]
        public async Task<ActionResult<BlogSubReturnedDto>> GetBlogSubById(int id)
        {
            _logger.LogInformation("Received GetBlogSubById request for Id: {Id}", id);

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var blogSub = await _blogSubRepo.GetByIdAsync(id);
            if (blogSub == null)
            {
                _logger.LogWarning("BlogSub with Id {Id} not found", id);
                return NotFound("Blog sub not found");
            }

            var result = _mapper.Map<BlogSubReturnedDto>(blogSub);

            _logger.LogInformation("Returned blog sub with Id: {Id}", id);
            return Ok(result);
        }

        [HttpGet("getBlogSubsByBlogId/{blogId}")]
        [Authorize]
        public async Task<ActionResult<List<BlogSubReturnedDto>>> GetBlogSubsByBlogId(int blogId)
        {
            _logger.LogInformation("Received GetBlogSubsByBlogId request for BlogId: {BlogId}", blogId);

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var blog = await _blogRepo.GetByIdAsync(blogId);
            if (blog == null)
                return NotFound("Blog not found");

            var blogSubs = await _blogSubRepo.GetAllAsync(s => s.BlogId == blogId);
            var result = _mapper.Map<List<BlogSubReturnedDto>>(blogSubs);

            _logger.LogInformation("Returned {Count} blog subs for BlogId: {BlogId}", result.Count, blogId);
            return Ok(result);
        }

        [HttpDelete("deleteBlogSub/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteBlogSub(int id)
        {
            _logger.LogInformation("Received DeleteBlogSub request for Id: {Id}", id);

            var blogSub = await _blogSubRepo.GetByIdAsync(id);
            if (blogSub == null)
            {
                _logger.LogWarning("BlogSub with Id {Id} not found", id);
                return NotFound("Blog sub not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            using var transaction = await _blogSubRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrEmpty(blogSub.CoverImage))
                    DocumentSettings.DeleteFile(blogSub.CoverImage, "BlogSubsCover");

                _blogSubRepo.Delete(blogSub);
                await _blogSubRepo.SaveChangesAsync();

                _logger.LogInformation("BlogSub deleted with Id: {BlogSubId}", blogSub.Id);

                await _blogSubRepo.CommitAsync(transaction);
                return Ok(new { Message = "Blog sub deleted successfully", BlogSubId = blogSub.Id });
            }
            catch (Exception ex)
            {
                await _blogSubRepo.RollbackAsync(transaction);
                _logger.LogError(ex, "Failed to delete blog sub: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while deleting the blog sub: {ex.Message}");
            }
        }

        #endregion

        #endregion

        #region PaymentMethod

        [HttpPost("addPaymentMethod")]
        [Authorize]
        public async Task<ActionResult> AddPaymentMethod([FromBody] PaymentMethodDto paymentMethodDto)
        {
            _logger.LogInformation("Received AddPaymentMethod request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            using var transaction = await _paymentMethodRepo.BeginTransactionAsync();
            try
            {
                var paymentMethod = new PaymentMethod
                {
                    Name = paymentMethodDto.Name.Trim()
                };

                await _paymentMethodRepo.AddAsync(paymentMethod);
                await _paymentMethodRepo.SaveChangesAsync();

                if (paymentMethod.Id == 0)
                {
                    _logger.LogError("Failed to generate PaymentMethod Id");
                    throw new InvalidOperationException("Failed to generate PaymentMethod Id");
                }

                _logger.LogInformation("PaymentMethod created with Id: {PaymentMethodId}", paymentMethod.Id);

                await _paymentMethodRepo.CommitAsync(transaction);
                return Ok(new { Message = "Payment method created successfully", PaymentMethodId = paymentMethod.Id });
            }
            catch (Exception ex)
            {
                await _paymentMethodRepo.RollbackAsync(transaction);
                _logger.LogError(ex, "Failed to create payment method: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while creating the payment method: {ex.Message}");
            }
        }

        [HttpPut("updatePaymentMethod/{id}")]
        [Authorize]
        public async Task<ActionResult> UpdatePaymentMethod(int id, [FromBody] UpdatePaymentMethodDto paymentMethodDto)
        {
            _logger.LogInformation("Received UpdatePaymentMethod request for Id: {Id}", id);

            var paymentMethod = await _paymentMethodRepo.GetByIdAsync(id);
            if (paymentMethod == null)
            {
                _logger.LogWarning("PaymentMethod with Id {Id} not found", id);
                return NotFound("Payment method not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            using var transaction = await _paymentMethodRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrWhiteSpace(paymentMethodDto.Name))
                    paymentMethod.Name = paymentMethodDto.Name.Trim();

                _paymentMethodRepo.Update(paymentMethod);
                await _paymentMethodRepo.SaveChangesAsync();

                _logger.LogInformation("PaymentMethod updated with Id: {PaymentMethodId}", paymentMethod.Id);

                await _paymentMethodRepo.CommitAsync(transaction);
                return Ok(new { Message = "Payment method updated successfully", PaymentMethodId = paymentMethod.Id });
            }
            catch (Exception ex)
            {
                await _paymentMethodRepo.RollbackAsync(transaction);
                _logger.LogError(ex, "Failed to update payment method: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while updating the payment method: {ex.Message}");
            }
        }

        [HttpGet("getAllPaymentMethods")]
        [Authorize]
        public async Task<ActionResult<List<PaymentMethodReturnedDto>>> GetAllPaymentMethods()
        {
            _logger.LogInformation("Received GetAllPaymentMethods request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var paymentMethods = await _paymentMethodRepo.GetAllAsync();
            var result = _mapper.Map<List<PaymentMethodReturnedDto>>(paymentMethods);

            _logger.LogInformation("Returned {Count} payment methods", result.Count);
            return Ok(result);
        }

        [HttpGet("getPaymentMethodById/{id}")]
        [Authorize]
        public async Task<ActionResult<PaymentMethodReturnedDto>> GetPaymentMethodById(int id)
        {
            _logger.LogInformation("Received GetPaymentMethodById request for Id: {Id}", id);

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var paymentMethod = await _paymentMethodRepo.GetByIdAsync(id);
            if (paymentMethod == null)
            {
                _logger.LogWarning("PaymentMethod with Id {Id} not found", id);
                return NotFound("Payment method not found");
            }

            var result = _mapper.Map<PaymentMethodReturnedDto>(paymentMethod);

            _logger.LogInformation("Returned payment method with Id: {Id}", id);
            return Ok(result);
        }

        [HttpDelete("deletePaymentMethod/{id}")]
        [Authorize]
        public async Task<ActionResult> DeletePaymentMethod(int id)
        {
            _logger.LogInformation("Received DeletePaymentMethod request for Id: {Id}", id);

            var paymentMethod = await _paymentMethodRepo.GetByIdAsync(id);
            if (paymentMethod == null)
            {
                _logger.LogWarning("PaymentMethod with Id {Id} not found", id);
                return NotFound("Payment method not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            using var transaction = await _paymentMethodRepo.BeginTransactionAsync();
            try
            {
                _paymentMethodRepo.Delete(paymentMethod);
                await _paymentMethodRepo.SaveChangesAsync();

                _logger.LogInformation("PaymentMethod deleted with Id: {PaymentMethodId}", paymentMethod.Id);

                await _paymentMethodRepo.CommitAsync(transaction);
                return Ok(new { Message = "Payment method deleted successfully", PaymentMethodId = paymentMethod.Id });
            }
            catch (Exception ex)
            {
                await _paymentMethodRepo.RollbackAsync(transaction);
                _logger.LogError(ex, "Failed to delete payment method: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while deleting the payment method: {ex.Message}");
            }
        }

        #endregion

        #region Reviews

        #region Review

        [HttpPost("addReview")]
        [Authorize]
        public async Task<ActionResult> AddReview([FromForm] ReviewDto reviewDto)
        {
            _logger.LogInformation("Received AddReview request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            string cover = string.Empty;

            using var transaction = await _reviewRepo.BeginTransactionAsync();
            try
            {
                if (reviewDto.CoverImage != null)
                {
                    cover = DocumentSettings.UploadFile(reviewDto.CoverImage, "ReviewsCover");
                    if (string.IsNullOrEmpty(cover))
                        return BadRequest("Failed to upload cover image");
                }

                var review = new Review
                {
                    Name = reviewDto.Name?.Trim(),
                    Country = reviewDto.Country?.Trim(),
                    Rating = reviewDto.Rating,
                    Comment = reviewDto.Comment?.Trim(),
                    ReviewLink = reviewDto.ReviewLink?.Trim(),
                    CoverImage = cover
                };

                await _reviewRepo.AddAsync(review);
                await _reviewRepo.SaveChangesAsync();

                if (review.Id == 0)
                {
                    _logger.LogError("Failed to generate Review Id");
                    throw new InvalidOperationException("Failed to generate Review Id");
                }

                _logger.LogInformation("Review created with Id: {ReviewId}", review.Id);

                await _reviewRepo.CommitAsync(transaction);
                return Ok(new { Message = "Review created successfully", ReviewId = review.Id });
            }
            catch (Exception ex)
            {
                await _reviewRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover))
                    DocumentSettings.DeleteFile(cover, "ReviewsCover");

                _logger.LogError(ex, "Failed to create review: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while creating the review: {ex.Message}");
            }
        }

        [HttpPut("updateReview/{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateReview(int id, [FromForm] UpdateReviewDto reviewDto)
        {
            _logger.LogInformation("Received UpdateReview request for Id: {Id}", id);

            var review = await _reviewRepo.GetByIdAsync(id);
            if (review == null)
            {
                _logger.LogWarning("Review with Id {Id} not found", id);
                return NotFound("Review not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            string cover = review.CoverImage ?? string.Empty;

            using var transaction = await _reviewRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrWhiteSpace(reviewDto.Name))
                    review.Name = reviewDto.Name.Trim();
                if (!string.IsNullOrWhiteSpace(reviewDto.Country))
                    review.Country = reviewDto.Country.Trim();
                if (reviewDto.Rating.HasValue)
                    review.Rating = reviewDto.Rating.Value;
                if (!string.IsNullOrWhiteSpace(reviewDto.Comment))
                    review.Comment = reviewDto.Comment.Trim();
                if (!string.IsNullOrWhiteSpace(reviewDto.ReviewLink))
                    review.ReviewLink = reviewDto.ReviewLink.Trim();

                if (reviewDto.CoverImage != null)
                {
                    if (!string.IsNullOrEmpty(cover))
                        DocumentSettings.DeleteFile(cover, "ReviewsCover");
                    cover = DocumentSettings.UploadFile(reviewDto.CoverImage, "ReviewsCover");
                    if (string.IsNullOrEmpty(cover))
                        return BadRequest("Failed to upload cover image");
                }

                review.CoverImage = cover;

                _reviewRepo.Update(review);
                await _reviewRepo.SaveChangesAsync();

                _logger.LogInformation("Review updated with Id: {ReviewId}", review.Id);

                await _reviewRepo.CommitAsync(transaction);
                return Ok(new { Message = "Review updated successfully", ReviewId = review.Id });
            }
            catch (Exception ex)
            {
                await _reviewRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover) && reviewDto.CoverImage != null)
                    DocumentSettings.DeleteFile(cover, "ReviewsCover");

                _logger.LogError(ex, "Failed to update review: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while updating the review: {ex.Message}");
            }
        }

        [HttpGet("getAllReviews")]
        [Authorize]
        public async Task<ActionResult<List<ReviewReturnedDto>>> GetAllReviews()
        {
            _logger.LogInformation("Received GetAllReviews request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var reviews = await _reviewRepo.GetAllAsync();
            var result = _mapper.Map<List<ReviewReturnedDto>>(reviews);

            _logger.LogInformation("Returned {Count} reviews", result.Count);
            return Ok(result);
        }

        [HttpGet("getReviewById/{id}")]
        [Authorize]
        public async Task<ActionResult<ReviewReturnedDto>> GetReviewById(int id)
        {
            _logger.LogInformation("Received GetReviewById request for Id: {Id}", id);

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var review = await _reviewRepo.GetByIdAsync(id);
            if (review == null)
            {
                _logger.LogWarning("Review with Id {Id} not found", id);
                return NotFound("Review not found");
            }

            var result = _mapper.Map<ReviewReturnedDto>(review);

            _logger.LogInformation("Returned review with Id: {Id}", id);
            return Ok(result);
        }

        [HttpDelete("deleteReview/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteReview(int id)
        {
            _logger.LogInformation("Received DeleteReview request for Id: {Id}", id);

            var review = await _reviewRepo.GetByIdAsync(id);
            if (review == null)
            {
                _logger.LogWarning("Review with Id {Id} not found", id);
                return NotFound("Review not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            using var transaction = await _reviewRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrEmpty(review.CoverImage))
                    DocumentSettings.DeleteFile(review.CoverImage, "ReviewsCover");

                _reviewRepo.Delete(review);
                await _reviewRepo.SaveChangesAsync();

                _logger.LogInformation("Review deleted with Id: {ReviewId}", review.Id);

                await _reviewRepo.CommitAsync(transaction);
                return Ok(new { Message = "Review deleted successfully", ReviewId = review.Id });
            }
            catch (Exception ex)
            {
                await _reviewRepo.RollbackAsync(transaction);
                _logger.LogError(ex, "Failed to delete review: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while deleting the review: {ex.Message}");
            }
        }

        #endregion

        #region ReviewPlatform

        [HttpPost("addReviewPlatform")]
        [Authorize]
        public async Task<ActionResult> AddReviewPlatform([FromForm] ReviewPlatformDto platformDto)
        {
            _logger.LogInformation("Received AddReviewPlatform request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            string icon = string.Empty;

            using var transaction = await _reviewPlatformRepo.BeginTransactionAsync();
            try
            {
                if (platformDto.Icon != null)
                {
                    icon = DocumentSettings.UploadFile(platformDto.Icon, "ReviewPlatformsIcon");
                    if (string.IsNullOrEmpty(icon))
                        return BadRequest("Failed to upload icon");
                }

                var platform = new ReviewPlatform
                {
                    Name = platformDto.Name?.Trim(),
                    ReviewsCountLabel = platformDto.ReviewsCountLabel?.Trim(),
                    Link = platformDto.Link?.Trim(),
                    Icon = icon
                };

                await _reviewPlatformRepo.AddAsync(platform);
                await _reviewPlatformRepo.SaveChangesAsync();

                if (platform.Id == 0)
                {
                    _logger.LogError("Failed to generate ReviewPlatform Id");
                    throw new InvalidOperationException("Failed to generate ReviewPlatform Id");
                }

                _logger.LogInformation("ReviewPlatform created with Id: {ReviewPlatformId}", platform.Id);

                await _reviewPlatformRepo.CommitAsync(transaction);
                return Ok(new { Message = "Review platform created successfully", ReviewPlatformId = platform.Id });
            }
            catch (Exception ex)
            {
                await _reviewPlatformRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(icon))
                    DocumentSettings.DeleteFile(icon, "ReviewPlatformsIcon");

                _logger.LogError(ex, "Failed to create review platform: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while creating the review platform: {ex.Message}");
            }
        }

        [HttpPut("updateReviewPlatform/{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateReviewPlatform(int id, [FromForm] UpdateReviewPlatformDto platformDto)
        {
            _logger.LogInformation("Received UpdateReviewPlatform request for Id: {Id}", id);

            var platform = await _reviewPlatformRepo.GetByIdAsync(id);
            if (platform == null)
            {
                _logger.LogWarning("ReviewPlatform with Id {Id} not found", id);
                return NotFound("Review platform not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            string icon = platform.Icon ?? string.Empty;

            using var transaction = await _reviewPlatformRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrWhiteSpace(platformDto.Name))
                    platform.Name = platformDto.Name.Trim();
                if (!string.IsNullOrWhiteSpace(platformDto.ReviewsCountLabel))
                    platform.ReviewsCountLabel = platformDto.ReviewsCountLabel.Trim();
                if (!string.IsNullOrWhiteSpace(platformDto.Link))
                    platform.Link = platformDto.Link.Trim();

                if (platformDto.Icon != null)
                {
                    if (!string.IsNullOrEmpty(icon))
                        DocumentSettings.DeleteFile(icon, "ReviewPlatformsIcon");
                    icon = DocumentSettings.UploadFile(platformDto.Icon, "ReviewPlatformsIcon");
                    if (string.IsNullOrEmpty(icon))
                        return BadRequest("Failed to upload icon");
                }

                platform.Icon = icon;

                _reviewPlatformRepo.Update(platform);
                await _reviewPlatformRepo.SaveChangesAsync();

                _logger.LogInformation("ReviewPlatform updated with Id: {ReviewPlatformId}", platform.Id);

                await _reviewPlatformRepo.CommitAsync(transaction);
                return Ok(new { Message = "Review platform updated successfully", ReviewPlatformId = platform.Id });
            }
            catch (Exception ex)
            {
                await _reviewPlatformRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(icon) && platformDto.Icon != null)
                    DocumentSettings.DeleteFile(icon, "ReviewPlatformsIcon");

                _logger.LogError(ex, "Failed to update review platform: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while updating the review platform: {ex.Message}");
            }
        }

        [HttpGet("getAllReviewPlatforms")]
        [Authorize]
        public async Task<ActionResult<List<ReviewPlatformReturnedDto>>> GetAllReviewPlatforms()
        {
            _logger.LogInformation("Received GetAllReviewPlatforms request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var platforms = await _reviewPlatformRepo.GetAllAsync();
            var result = _mapper.Map<List<ReviewPlatformReturnedDto>>(platforms);

            _logger.LogInformation("Returned {Count} review platforms", result.Count);
            return Ok(result);
        }

        [HttpGet("getReviewPlatformById/{id}")]
        [Authorize]
        public async Task<ActionResult<ReviewPlatformReturnedDto>> GetReviewPlatformById(int id)
        {
            _logger.LogInformation("Received GetReviewPlatformById request for Id: {Id}", id);

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var platform = await _reviewPlatformRepo.GetByIdAsync(id);
            if (platform == null)
            {
                _logger.LogWarning("ReviewPlatform with Id {Id} not found", id);
                return NotFound("Review platform not found");
            }

            var result = _mapper.Map<ReviewPlatformReturnedDto>(platform);

            _logger.LogInformation("Returned review platform with Id: {Id}", id);
            return Ok(result);
        }

        [HttpDelete("deleteReviewPlatform/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteReviewPlatform(int id)
        {
            _logger.LogInformation("Received DeleteReviewPlatform request for Id: {Id}", id);

            var platform = await _reviewPlatformRepo.GetByIdAsync(id);
            if (platform == null)
            {
                _logger.LogWarning("ReviewPlatform with Id {Id} not found", id);
                return NotFound("Review platform not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            using var transaction = await _reviewPlatformRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrEmpty(platform.Icon))
                    DocumentSettings.DeleteFile(platform.Icon, "ReviewPlatformsIcon");

                _reviewPlatformRepo.Delete(platform);
                await _reviewPlatformRepo.SaveChangesAsync();

                _logger.LogInformation("ReviewPlatform deleted with Id: {ReviewPlatformId}", platform.Id);

                await _reviewPlatformRepo.CommitAsync(transaction);
                return Ok(new { Message = "Review platform deleted successfully", ReviewPlatformId = platform.Id });
            }
            catch (Exception ex)
            {
                await _reviewPlatformRepo.RollbackAsync(transaction);
                _logger.LogError(ex, "Failed to delete review platform: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while deleting the review platform: {ex.Message}");
            }
        }

        #endregion

        #region Reel

        [HttpPost("addReel")]
        [Authorize]
        public async Task<ActionResult> AddReel([FromForm] ReelDto reelDto)
        {
            _logger.LogInformation("Received AddReel request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            string cover = string.Empty;

            using var transaction = await _reelRepo.BeginTransactionAsync();
            try
            {
                if (reelDto.CoverImage != null)
                {
                    cover = DocumentSettings.UploadFile(reelDto.CoverImage, "ReelsCover");
                    if (string.IsNullOrEmpty(cover))
                        return BadRequest("Failed to upload cover image");
                }

                var reel = new Reel
                {
                    Title = reelDto.Title?.Trim(),
                    IframeLink = reelDto.IframeLink?.Trim(),
                    CoverImage = cover
                };

                await _reelRepo.AddAsync(reel);
                await _reelRepo.SaveChangesAsync();

                if (reel.Id == 0)
                {
                    _logger.LogError("Failed to generate Reel Id");
                    throw new InvalidOperationException("Failed to generate Reel Id");
                }

                _logger.LogInformation("Reel created with Id: {ReelId}", reel.Id);

                await _reelRepo.CommitAsync(transaction);
                return Ok(new { Message = "Reel created successfully", ReelId = reel.Id });
            }
            catch (Exception ex)
            {
                await _reelRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover))
                    DocumentSettings.DeleteFile(cover, "ReelsCover");

                _logger.LogError(ex, "Failed to create reel: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while creating the reel: {ex.Message}");
            }
        }

        [HttpPut("updateReel/{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateReel(int id, [FromForm] UpdateReelDto reelDto)
        {
            _logger.LogInformation("Received UpdateReel request for Id: {Id}", id);

            var reel = await _reelRepo.GetByIdAsync(id);
            if (reel == null)
            {
                _logger.LogWarning("Reel with Id {Id} not found", id);
                return NotFound("Reel not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            string cover = reel.CoverImage ?? string.Empty;

            using var transaction = await _reelRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrWhiteSpace(reelDto.Title))
                    reel.Title = reelDto.Title.Trim();
                if (!string.IsNullOrWhiteSpace(reelDto.IframeLink))
                    reel.IframeLink = reelDto.IframeLink.Trim();

                if (reelDto.CoverImage != null)
                {
                    if (!string.IsNullOrEmpty(cover))
                        DocumentSettings.DeleteFile(cover, "ReelsCover");
                    cover = DocumentSettings.UploadFile(reelDto.CoverImage, "ReelsCover");
                    if (string.IsNullOrEmpty(cover))
                        return BadRequest("Failed to upload cover image");
                }

                reel.CoverImage = cover;

                _reelRepo.Update(reel);
                await _reelRepo.SaveChangesAsync();

                _logger.LogInformation("Reel updated with Id: {ReelId}", reel.Id);

                await _reelRepo.CommitAsync(transaction);
                return Ok(new { Message = "Reel updated successfully", ReelId = reel.Id });
            }
            catch (Exception ex)
            {
                await _reelRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover) && reelDto.CoverImage != null)
                    DocumentSettings.DeleteFile(cover, "ReelsCover");

                _logger.LogError(ex, "Failed to update reel: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while updating the reel: {ex.Message}");
            }
        }

        [HttpGet("getAllReels")]
        [Authorize]
        public async Task<ActionResult<List<ReelReturnedDto>>> GetAllReels()
        {
            _logger.LogInformation("Received GetAllReels request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var reels = await _reelRepo.GetAllAsync();
            var result = _mapper.Map<List<ReelReturnedDto>>(reels);

            _logger.LogInformation("Returned {Count} reels", result.Count);
            return Ok(result);
        }

        [HttpGet("getReelById/{id}")]
        [Authorize]
        public async Task<ActionResult<ReelReturnedDto>> GetReelById(int id)
        {
            _logger.LogInformation("Received GetReelById request for Id: {Id}", id);

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var reel = await _reelRepo.GetByIdAsync(id);
            if (reel == null)
            {
                _logger.LogWarning("Reel with Id {Id} not found", id);
                return NotFound("Reel not found");
            }

            var result = _mapper.Map<ReelReturnedDto>(reel);

            _logger.LogInformation("Returned reel with Id: {Id}", id);
            return Ok(result);
        }

        [HttpDelete("deleteReel/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteReel(int id)
        {
            _logger.LogInformation("Received DeleteReel request for Id: {Id}", id);

            var reel = await _reelRepo.GetByIdAsync(id);
            if (reel == null)
            {
                _logger.LogWarning("Reel with Id {Id} not found", id);
                return NotFound("Reel not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            using var transaction = await _reelRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrEmpty(reel.CoverImage))
                    DocumentSettings.DeleteFile(reel.CoverImage, "ReelsCover");

                _reelRepo.Delete(reel);
                await _reelRepo.SaveChangesAsync();

                _logger.LogInformation("Reel deleted with Id: {ReelId}", reel.Id);

                await _reelRepo.CommitAsync(transaction);
                return Ok(new { Message = "Reel deleted successfully", ReelId = reel.Id });
            }
            catch (Exception ex)
            {
                await _reelRepo.RollbackAsync(transaction);
                _logger.LogError(ex, "Failed to delete reel: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while deleting the reel: {ex.Message}");
            }
        }

        #endregion

        #region CustomerPhoto

        [HttpPost("addCustomerPhoto")]
        [Authorize]
        public async Task<ActionResult> AddCustomerPhoto([FromForm] CustomerPhotoDto photoDto)
        {
            _logger.LogInformation("Received AddCustomerPhoto request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            if (photoDto.Images == null || !photoDto.Images.Any())
                return BadRequest("At least one image is required");

            var uploadedImages = new List<string>();

            using var transaction = await _customerPhotoRepo.BeginTransactionAsync();
            try
            {
                var photoIds = new List<int>();

                foreach (var image in photoDto.Images)
                {
                    var imageUrl = DocumentSettings.UploadFile(image, "CustomerPhotos");
                    if (string.IsNullOrEmpty(imageUrl))
                        return BadRequest("Failed to upload one or more images");

                    uploadedImages.Add(imageUrl);

                    var photo = new CustomerPhoto
                    {
                        Image = imageUrl
                    };

                    await _customerPhotoRepo.AddAsync(photo);
                    await _customerPhotoRepo.SaveChangesAsync();

                    if (photo.Id == 0)
                    {
                        _logger.LogError("Failed to generate CustomerPhoto Id");
                        throw new InvalidOperationException("Failed to generate CustomerPhoto Id");
                    }

                    photoIds.Add(photo.Id);
                }

                _logger.LogInformation("Created {Count} customer photos with Ids: {Ids}", photoIds.Count, string.Join(", ", photoIds));

                await _customerPhotoRepo.CommitAsync(transaction);
                return Ok(new { Message = "Customer photos created successfully", CustomerPhotoIds = photoIds });
            }
            catch (Exception ex)
            {
                await _customerPhotoRepo.RollbackAsync(transaction);

                foreach (var imageUrl in uploadedImages)
                {
                    DocumentSettings.DeleteFile(imageUrl, "CustomerPhotos");
                }

                _logger.LogError(ex, "Failed to create customer photos: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while creating the customer photos: {ex.Message}");
            }
        }

        [HttpPut("updateCustomerPhoto/{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateCustomerPhoto(int id, [FromForm] UpdateCustomerPhotoDto photoDto)
        {
            _logger.LogInformation("Received UpdateCustomerPhoto request for Id: {Id}", id);

            var photo = await _customerPhotoRepo.GetByIdAsync(id);
            if (photo == null)
            {
                _logger.LogWarning("CustomerPhoto with Id {Id} not found", id);
                return NotFound("Customer photo not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            string image = photo.Image ?? string.Empty;

            using var transaction = await _customerPhotoRepo.BeginTransactionAsync();
            try
            {
                if (photoDto.Image != null)
                {
                    if (!string.IsNullOrEmpty(image))
                        DocumentSettings.DeleteFile(image, "CustomerPhotos");
                    image = DocumentSettings.UploadFile(photoDto.Image, "CustomerPhotos");
                    if (string.IsNullOrEmpty(image))
                        return BadRequest("Failed to upload image");
                }

                photo.Image = image;

                _customerPhotoRepo.Update(photo);
                await _customerPhotoRepo.SaveChangesAsync();

                _logger.LogInformation("CustomerPhoto updated with Id: {CustomerPhotoId}", photo.Id);

                await _customerPhotoRepo.CommitAsync(transaction);
                return Ok(new { Message = "Customer photo updated successfully", CustomerPhotoId = photo.Id });
            }
            catch (Exception ex)
            {
                await _customerPhotoRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(image) && photoDto.Image != null)
                    DocumentSettings.DeleteFile(image, "CustomerPhotos");

                _logger.LogError(ex, "Failed to update customer photo: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while updating the customer photo: {ex.Message}");
            }
        }

        [HttpGet("getAllCustomerPhotos")]
        [Authorize]
        public async Task<ActionResult<List<CustomerPhotoReturnedDto>>> GetAllCustomerPhotos()
        {
            _logger.LogInformation("Received GetAllCustomerPhotos request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var photos = await _customerPhotoRepo.GetAllAsync();
            var result = _mapper.Map<List<CustomerPhotoReturnedDto>>(photos);

            _logger.LogInformation("Returned {Count} customer photos", result.Count);
            return Ok(result);
        }

        [HttpGet("getCustomerPhotoById/{id}")]
        [Authorize]
        public async Task<ActionResult<CustomerPhotoReturnedDto>> GetCustomerPhotoById(int id)
        {
            _logger.LogInformation("Received GetCustomerPhotoById request for Id: {Id}", id);

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var photo = await _customerPhotoRepo.GetByIdAsync(id);
            if (photo == null)
            {
                _logger.LogWarning("CustomerPhoto with Id {Id} not found", id);
                return NotFound("Customer photo not found");
            }

            var result = _mapper.Map<CustomerPhotoReturnedDto>(photo);

            _logger.LogInformation("Returned customer photo with Id: {Id}", id);
            return Ok(result);
        }

        [HttpDelete("deleteCustomerPhoto/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteCustomerPhoto(int id)
        {
            _logger.LogInformation("Received DeleteCustomerPhoto request for Id: {Id}", id);

            var photo = await _customerPhotoRepo.GetByIdAsync(id);
            if (photo == null)
            {
                _logger.LogWarning("CustomerPhoto with Id {Id} not found", id);
                return NotFound("Customer photo not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            using var transaction = await _customerPhotoRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrEmpty(photo.Image))
                    DocumentSettings.DeleteFile(photo.Image, "CustomerPhotos");

                _customerPhotoRepo.Delete(photo);
                await _customerPhotoRepo.SaveChangesAsync();

                _logger.LogInformation("CustomerPhoto deleted with Id: {CustomerPhotoId}", photo.Id);

                await _customerPhotoRepo.CommitAsync(transaction);
                return Ok(new { Message = "Customer photo deleted successfully", CustomerPhotoId = photo.Id });
            }
            catch (Exception ex)
            {
                await _customerPhotoRepo.RollbackAsync(transaction);
                _logger.LogError(ex, "Failed to delete customer photo: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while deleting the customer photo: {ex.Message}");
            }
        }

        #endregion

        #region ReviewsSettings

        [HttpPost("addOrUpdateReviewsSettings")]
        [Authorize]
        public async Task<ActionResult> AddOrUpdateReviewsSettings([FromForm] ReviewsSettingsDto settingsDto)
        {
            _logger.LogInformation("Received AddOrUpdateReviewsSettings request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            using var transaction = await _reviewsSettingsRepo.BeginTransactionAsync();
            var uploadedImage = string.Empty;
            try
            {
                var settings = (await _reviewsSettingsRepo.GetAllAsync()).FirstOrDefault();
                var previousImage = settings?.Image;

                if (settingsDto.Image != null)
                {
                    uploadedImage = DocumentSettings.UploadFile(settingsDto.Image, "ReviewsSettings");
                    if (string.IsNullOrEmpty(uploadedImage))
                        return BadRequest("Failed to upload image");
                }

                if (settings == null)
                {
                    settings = new ReviewsSettings
                    {
                        Title = settingsDto.Title.Trim(),
                        Description = settingsDto.Description.Trim(),
                        YoutubeChannelLink = settingsDto.YoutubeChannelLink.Trim(),
                        Image = uploadedImage
                    };
                    await _reviewsSettingsRepo.AddAsync(settings);
                }
                else
                {
                    settings.Title = settingsDto.Title.Trim();
                    settings.Description = settingsDto.Description.Trim();
                    settings.YoutubeChannelLink = settingsDto.YoutubeChannelLink.Trim();
                    if (!string.IsNullOrEmpty(uploadedImage))
                        settings.Image = uploadedImage;
                    _reviewsSettingsRepo.Update(settings);
                }

                await _reviewsSettingsRepo.SaveChangesAsync();

                if (!string.IsNullOrEmpty(uploadedImage) && !string.IsNullOrEmpty(previousImage))
                    DocumentSettings.DeleteFile(previousImage, "ReviewsSettings");

                _logger.LogInformation("ReviewsSettings saved with Id: {ReviewsSettingsId}", settings.Id);

                await _reviewsSettingsRepo.CommitAsync(transaction);
                return Ok(new { Message = "Reviews settings saved successfully", ReviewsSettingsId = settings.Id });
            }
            catch (Exception ex)
            {
                await _reviewsSettingsRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(uploadedImage))
                    DocumentSettings.DeleteFile(uploadedImage, "ReviewsSettings");

                _logger.LogError(ex, "Failed to save reviews settings: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while saving reviews settings: {ex.Message}");
            }
        }

        [HttpGet("getReviewsSettings")]
        [Authorize]
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

        #endregion

        #region SocialMediaLink

        [HttpPost("addSocialMediaLink")]
        [Authorize]
        public async Task<ActionResult> AddSocialMediaLink([FromForm] SocialMediaLinkDto socialMediaLinkDto)
        {
            _logger.LogInformation("Received AddSocialMediaLink request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            string icon = string.Empty;

            using var transaction = await _socialMediaLinkRepo.BeginTransactionAsync();
            try
            {
                if (socialMediaLinkDto.Icon != null)
                {
                    icon = DocumentSettings.UploadFile(socialMediaLinkDto.Icon, "SocialMediaLinksIcon");
                    if (string.IsNullOrEmpty(icon))
                        return BadRequest("Failed to upload icon");
                }

                var socialMediaLink = new SocialMediaLink
                {
                    Title = socialMediaLinkDto.Title?.Trim(),
                    Link = socialMediaLinkDto.Link?.Trim(),
                    Icon = icon
                };

                await _socialMediaLinkRepo.AddAsync(socialMediaLink);
                await _socialMediaLinkRepo.SaveChangesAsync();

                if (socialMediaLink.Id == 0)
                {
                    _logger.LogError("Failed to generate SocialMediaLink Id");
                    throw new InvalidOperationException("Failed to generate SocialMediaLink Id");
                }

                _logger.LogInformation("SocialMediaLink created with Id: {SocialMediaLinkId}", socialMediaLink.Id);

                await _socialMediaLinkRepo.CommitAsync(transaction);
                return Ok(new { Message = "Social media link created successfully", SocialMediaLinkId = socialMediaLink.Id });
            }
            catch (Exception ex)
            {
                await _socialMediaLinkRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(icon))
                    DocumentSettings.DeleteFile(icon, "SocialMediaLinksIcon");

                _logger.LogError(ex, "Failed to create social media link: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while creating the social media link: {ex.Message}");
            }
        }

        [HttpPut("updateSocialMediaLink/{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateSocialMediaLink(int id, [FromForm] UpdateSocialMediaLinkDto socialMediaLinkDto)
        {
            _logger.LogInformation("Received UpdateSocialMediaLink request for Id: {Id}", id);

            var socialMediaLink = await _socialMediaLinkRepo.GetByIdAsync(id);
            if (socialMediaLink == null)
            {
                _logger.LogWarning("SocialMediaLink with Id {Id} not found", id);
                return NotFound("Social media link not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
                return BadRequest(ModelState);
            }

            string icon = socialMediaLink.Icon ?? string.Empty;

            using var transaction = await _socialMediaLinkRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrWhiteSpace(socialMediaLinkDto.Title))
                    socialMediaLink.Title = socialMediaLinkDto.Title.Trim();
                if (!string.IsNullOrWhiteSpace(socialMediaLinkDto.Link))
                    socialMediaLink.Link = socialMediaLinkDto.Link.Trim();

                if (socialMediaLinkDto.Icon != null)
                {
                    if (!string.IsNullOrEmpty(icon))
                        DocumentSettings.DeleteFile(icon, "SocialMediaLinksIcon");
                    icon = DocumentSettings.UploadFile(socialMediaLinkDto.Icon, "SocialMediaLinksIcon");
                    if (string.IsNullOrEmpty(icon))
                        return BadRequest("Failed to upload icon");
                }

                socialMediaLink.Icon = icon;

                _socialMediaLinkRepo.Update(socialMediaLink);
                await _socialMediaLinkRepo.SaveChangesAsync();

                _logger.LogInformation("SocialMediaLink updated with Id: {SocialMediaLinkId}", socialMediaLink.Id);

                await _socialMediaLinkRepo.CommitAsync(transaction);
                return Ok(new { Message = "Social media link updated successfully", SocialMediaLinkId = socialMediaLink.Id });
            }
            catch (Exception ex)
            {
                await _socialMediaLinkRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(icon) && socialMediaLinkDto.Icon != null)
                    DocumentSettings.DeleteFile(icon, "SocialMediaLinksIcon");

                _logger.LogError(ex, "Failed to update social media link: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while updating the social media link: {ex.Message}");
            }
        }

        [HttpGet("getAllSocialMediaLinks")]
        [Authorize]
        public async Task<ActionResult<List<SocialMediaLinkReturnedDto>>> GetAllSocialMediaLinks()
        {
            _logger.LogInformation("Received GetAllSocialMediaLinks request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var socialMediaLinks = await _socialMediaLinkRepo.GetAllAsync();
            var result = _mapper.Map<List<SocialMediaLinkReturnedDto>>(socialMediaLinks);

            _logger.LogInformation("Returned {Count} social media links", result.Count);
            return Ok(result);
        }

        [HttpGet("getSocialMediaLinkById/{id}")]
        [Authorize]
        public async Task<ActionResult<SocialMediaLinkReturnedDto>> GetSocialMediaLinkById(int id)
        {
            _logger.LogInformation("Received GetSocialMediaLinkById request for Id: {Id}", id);

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var socialMediaLink = await _socialMediaLinkRepo.GetByIdAsync(id);
            if (socialMediaLink == null)
            {
                _logger.LogWarning("SocialMediaLink with Id {Id} not found", id);
                return NotFound("Social media link not found");
            }

            var result = _mapper.Map<SocialMediaLinkReturnedDto>(socialMediaLink);

            _logger.LogInformation("Returned social media link with Id: {Id}", id);
            return Ok(result);
        }

        [HttpDelete("deleteSocialMediaLink/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteSocialMediaLink(int id)
        {
            _logger.LogInformation("Received DeleteSocialMediaLink request for Id: {Id}", id);

            var socialMediaLink = await _socialMediaLinkRepo.GetByIdAsync(id);
            if (socialMediaLink == null)
            {
                _logger.LogWarning("SocialMediaLink with Id {Id} not found", id);
                return NotFound("Social media link not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            using var transaction = await _socialMediaLinkRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrEmpty(socialMediaLink.Icon))
                    DocumentSettings.DeleteFile(socialMediaLink.Icon, "SocialMediaLinksIcon");

                _socialMediaLinkRepo.Delete(socialMediaLink);
                await _socialMediaLinkRepo.SaveChangesAsync();

                _logger.LogInformation("SocialMediaLink deleted with Id: {SocialMediaLinkId}", socialMediaLink.Id);

                await _socialMediaLinkRepo.CommitAsync(transaction);
                return Ok(new { Message = "Social media link deleted successfully", SocialMediaLinkId = socialMediaLink.Id });
            }
            catch (Exception ex)
            {
                await _socialMediaLinkRepo.RollbackAsync(transaction);
                _logger.LogError(ex, "Failed to delete social media link: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while deleting the social media link: {ex.Message}");
            }
        }

        #endregion

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var stats = new DashboardStatisticsDto
            {
                TravelPrograms = await _travelProgramRepo.CountAsync(),
                Services = await _serviceRepo.CountAsync(),
                Weddings = await _weddingRepo.CountAsync(),
                Destinations = await _destinationRepo.CountAsync(),
                Activities = await _activityRepo.CountAsync(),
                Events = await _eventRepo.CountAsync(),
                Users = _userManager.Users.Count()
            };

            return Ok(stats);
        }
    }
}
