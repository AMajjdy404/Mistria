using System.Net;
using System.Numerics;
using System.Security.Claims;
using AutoMapper;
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
    public class DashboardController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly IMailService _mailService;
        private readonly IGenericRepository<TravelProgram> _travelProgramRepo;
        private readonly IGenericRepository<DayTrip> _dayTripRepo;
        private readonly IGenericRepository<Wedding> _weddingRepo;
        private readonly IGenericRepository<Event> _eventRepo;
        private readonly IGenericRepository<Activity> _activityRepo;
        private readonly IGenericRepository<Service> _serviceRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService tokenService,
            IConfiguration configuration,
            IMailService mailService,
            IGenericRepository<TravelProgram> travelProgramRepo,
            IGenericRepository<DayTrip> dayTripRepo,
            IGenericRepository<Wedding> weddingRepo,
            IGenericRepository<Event> eventRepo,
            IGenericRepository<Activity> activityRepo,
            IGenericRepository<Service> serviceRepo,
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
            _dayTripRepo = dayTripRepo;
            _weddingRepo = weddingRepo;
            _eventRepo = eventRepo;
            _activityRepo = activityRepo;
            _serviceRepo = serviceRepo;
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

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);

            var email = new Email()
            {
                To = dto.Email,
                Subject = "Reset Password",
                Body = encodedToken
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

            var decodedToken = WebUtility.UrlDecode(dto.Code);

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);

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

        #region Program
        [HttpPost("addProgram")]
        public async Task<ActionResult> AddProgram([FromForm] ProgramDto programDto)
        {
            _logger.LogInformation("Received AddProgram request. ItineraryJson: '{Json}'", programDto.ItineraryJson ?? "null");
            _logger.LogInformation("ModelState Errors: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

            Dictionary<string, string> itinerary = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(programDto.ItineraryJson))
            {
                try
                {
                    var cleanedJson = programDto.ItineraryJson.Trim();
                    _logger.LogInformation("Attempting to deserialize ItineraryJson: '{Json}'", cleanedJson);
                    using var doc = JsonDocument.Parse(cleanedJson);
                    if (doc.RootElement.ValueKind != JsonValueKind.Object)
                    {
                        _logger.LogWarning("ItineraryJson is not a valid object: {Json}", cleanedJson);
                        return BadRequest("Itinerary JSON must be an object (e.g., {\"key\": \"value\"})");
                    }
                    itinerary = JsonSerializer.Deserialize<Dictionary<string, string>>(cleanedJson) ?? new Dictionary<string, string>();
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize itinerary JSON: {Message} | Raw JSON: {Json}", ex.Message, programDto.ItineraryJson);
                    return BadRequest("Invalid itinerary JSON format. Use {\"key\": \"value\", \"key2\": \"value2\"}");
                }
            }
            else
            {
                _logger.LogWarning("ItineraryJson is null or empty");
                return BadRequest("Itinerary JSON is required");
            }

            if (itinerary.Count == 0)
            {
                _logger.LogWarning("Itinerary is empty after deserialization");
                return BadRequest("Itinerary is required and cannot be empty");
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

                var program = new TravelProgram
                {
                    Title = programDto.Title?.Trim(),
                    Description = programDto.Description?.Trim(),
                    Location = programDto.Location?.Trim(),
                    LocationUrl = programDto.LocationUrl?.Trim(),
                    Images = imageUrls,
                    CoverImage = cover,
                    Included = programDto.Included ?? new List<string>(),
                    PricePerPerson = programDto.PricePerPerson,
                    IsMain = programDto.IsMain,
                    Itinerary = itinerary
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

            Dictionary<string, string> itinerary = program.Itinerary ?? new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(programDto.ItineraryJson))
            {
                try
                {
                    var cleanedJson = programDto.ItineraryJson.Trim();
                    _logger.LogInformation("Attempting to deserialize ItineraryJson: '{Json}'", cleanedJson);
                    using var doc = JsonDocument.Parse(cleanedJson);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                    {
                        var firstObject = doc.RootElement[0];
                        if (firstObject.ValueKind == JsonValueKind.Object)
                        {
                            itinerary = JsonSerializer.Deserialize<Dictionary<string, string>>(firstObject.GetRawText()) ?? new Dictionary<string, string>();
                        }
                        else
                        {
                            _logger.LogWarning("First element in ItineraryJson array is not an object: {Json}", cleanedJson);
                            return BadRequest("Itinerary JSON array must contain at least one object (e.g., [{\"key\": \"value\"}])");
                        }
                    }
                    else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        itinerary = JsonSerializer.Deserialize<Dictionary<string, string>>(cleanedJson) ?? new Dictionary<string, string>();
                    }
                    else
                    {
                        _logger.LogWarning("ItineraryJson is not a valid object or array: {Json}", cleanedJson);
                        return BadRequest("Itinerary JSON must be an object or array of objects (e.g., {\"key\": \"value\"} or [{\"key\": \"value\"}])");
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize itinerary JSON: {Message} | Raw JSON: {Json}", ex.Message, programDto.ItineraryJson);
                    return BadRequest("Invalid itinerary JSON format. Use {\"key\": \"value\", ...} or [{\"key\": \"value\", ...}]");
                }
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
                if (!string.IsNullOrWhiteSpace(programDto.LocationUrl))
                    program.LocationUrl = programDto.LocationUrl.Trim();
                if (programDto.PricePerPerson.HasValue)
                    program.PricePerPerson = programDto.PricePerPerson.Value;
                if (programDto.IsMain.HasValue)
                    program.IsMain = programDto.IsMain.Value;
                if (programDto.Included != null)
                    program.Included = programDto.Included;
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

            var programs = await _travelProgramRepo.GetAllAsync();
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

        #region DayTrip
        [HttpPost("addDayTrip")]
        [Authorize]
        public async Task<ActionResult> AddDayTrip([FromForm] DayTripDto dayTripDto)
        {
            _logger.LogInformation("Received AddDayTrip request. ItineraryJson: '{Json}'", dayTripDto.ItineraryJson ?? "null");
            _logger.LogInformation("ModelState Errors: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

            Dictionary<string, string> itinerary = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(dayTripDto.ItineraryJson))
            {
                try
                {
                    var cleanedJson = dayTripDto.ItineraryJson.Trim();
                    _logger.LogInformation("Attempting to deserialize ItineraryJson: '{Json}'", cleanedJson);
                    using var doc = JsonDocument.Parse(cleanedJson);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                    {
                        var firstObject = doc.RootElement[0];
                        if (firstObject.ValueKind == JsonValueKind.Object)
                        {
                            itinerary = JsonSerializer.Deserialize<Dictionary<string, string>>(firstObject.GetRawText()) ?? new Dictionary<string, string>();
                        }
                        else
                        {
                            _logger.LogWarning("First element in ItineraryJson array is not an object: {Json}", cleanedJson);
                            return BadRequest("Itinerary JSON array must contain at least one object (e.g., [{\"key\": \"value\"}])");
                        }
                    }
                    else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        itinerary = JsonSerializer.Deserialize<Dictionary<string, string>>(cleanedJson) ?? new Dictionary<string, string>();
                    }
                    else
                    {
                        _logger.LogWarning("ItineraryJson is not a valid object or array: {Json}", cleanedJson);
                        return BadRequest("Itinerary JSON must be an object or array of objects (e.g., {\"key\": \"value\"} or [{\"key\": \"value\"}])");
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize itinerary JSON: {Message} | Raw JSON: {Json}", ex.Message, dayTripDto.ItineraryJson);
                    return BadRequest("Invalid itinerary JSON format. Use {\"key\": \"value\", ...} or [{\"key\": \"value\", ...}]");
                }
            }
            else
            {
                _logger.LogWarning("ItineraryJson is null or empty");
                return BadRequest("Itinerary JSON is required");
            }

            if (itinerary.Count == 0)
            {
                _logger.LogWarning("Itinerary is empty after deserialization");
                return BadRequest("Itinerary is required and cannot be empty");
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
            string cover = string.Empty;

            using var transaction = await _dayTripRepo.BeginTransactionAsync();
            try
            {
                if (dayTripDto.CoverImage != null)
                {
                    cover = DocumentSettings.UploadFile(dayTripDto.CoverImage, "DayTripsCover");
                    if (string.IsNullOrEmpty(cover))
                        return BadRequest("Failed to upload cover image");
                }

                if (dayTripDto.Images != null && dayTripDto.Images.Any())
                {
                    foreach (var image in dayTripDto.Images)
                    {
                        if (image?.Length > 0)
                        {
                            var fileUrl = DocumentSettings.UploadFile(image, "DayTrips");
                            if (!string.IsNullOrEmpty(fileUrl))
                                imageUrls.Add(fileUrl);
                        }
                    }
                    if (!imageUrls.Any() && dayTripDto.Images.Any())
                        return BadRequest("Failed to upload images");
                }

                var dayTrip = new DayTrip
                {
                    Title = dayTripDto.Title?.Trim(),
                    Description = dayTripDto.Description?.Trim(),
                    Location = dayTripDto.Location?.Trim(),
                    LocationUrl = dayTripDto.LocationUrl?.Trim(),
                    Images = imageUrls,
                    CoverImage = cover,
                    Included = dayTripDto.Included ?? new List<string>(),
                    PricePerPerson = dayTripDto.PricePerPerson,
                    IsMain = dayTripDto.IsMain,
                    Itinerary = itinerary,
                    City = dayTripDto.City
                };

                await _dayTripRepo.AddAsync(dayTrip);
                await _dayTripRepo.SaveChangesAsync();

                if (dayTrip.Id == 0)
                {
                    _logger.LogError("Failed to generate DayTrip Id");
                    throw new InvalidOperationException("Failed to generate DayTrip Id");
                }

                _logger.LogInformation("DayTrip created with Id: {DayTripId}", dayTrip.Id);

                await _dayTripRepo.CommitAsync(transaction);
                return Ok(new { Message = "DayTrip created successfully", DayTripId = dayTrip.Id });
            }
            catch (Exception ex)
            {
                await _dayTripRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover))
                    DocumentSettings.DeleteFile(cover, "DayTripsCover");

                foreach (var imageUrl in imageUrls)
                {
                    DocumentSettings.DeleteFile(imageUrl, "DayTrips");
                }

                _logger.LogError(ex, "Failed to create day trip: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while creating the day trip: {ex.Message}");
            }
        }

        [HttpPut("updateDayTrip/{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateDayTrip(int id, [FromForm] UpdateDayTripDto dayTripDto)
        {
            _logger.LogInformation("Received UpdateDayTrip request for Id: {Id}. ItineraryJson: '{Json}'", id, dayTripDto.ItineraryJson ?? "null");
            _logger.LogInformation("ModelState Errors: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

            var dayTrip = await _dayTripRepo.GetByIdAsync(id);
            if (dayTrip == null)
            {
                _logger.LogWarning("DayTrip with Id {Id} not found", id);
                return NotFound("DayTrip not found");
            }

            Dictionary<string, string> itinerary = dayTrip.Itinerary ?? new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(dayTripDto.ItineraryJson))
            {
                try
                {
                    var cleanedJson = dayTripDto.ItineraryJson.Trim();
                    _logger.LogInformation("Attempting to deserialize ItineraryJson: '{Json}'", cleanedJson);
                    using var doc = JsonDocument.Parse(cleanedJson);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                    {
                        var firstObject = doc.RootElement[0];
                        if (firstObject.ValueKind == JsonValueKind.Object)
                        {
                            itinerary = JsonSerializer.Deserialize<Dictionary<string, string>>(firstObject.GetRawText()) ?? new Dictionary<string, string>();
                        }
                        else
                        {
                            _logger.LogWarning("First element in ItineraryJson array is not an object: {Json}", cleanedJson);
                            return BadRequest("Itinerary JSON array must contain at least one object (e.g., [{\"key\": \"value\"}])");
                        }
                    }
                    else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        itinerary = JsonSerializer.Deserialize<Dictionary<string, string>>(cleanedJson) ?? new Dictionary<string, string>();
                    }
                    else
                    {
                        _logger.LogWarning("ItineraryJson is not a valid object or array: {Json}", cleanedJson);
                        return BadRequest("Itinerary JSON must be an object or array of objects (e.g., {\"key\": \"value\"} or [{\"key\": \"value\"}])");
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize itinerary JSON: {Message} | Raw JSON: {Json}", ex.Message, dayTripDto.ItineraryJson);
                    return BadRequest("Invalid itinerary JSON format. Use {\"key\": \"value\", ...} or [{\"key\": \"value\", ...}]");
                }
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

            var imageUrls = dayTrip.Images ?? new List<string>();
            string cover = dayTrip.CoverImage ?? string.Empty;

            using var transaction = await _dayTripRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrWhiteSpace(dayTripDto.Title))
                    dayTrip.Title = dayTripDto.Title.Trim();
                if (!string.IsNullOrWhiteSpace(dayTripDto.Description))
                    dayTrip.Description = dayTripDto.Description.Trim();
                if (!string.IsNullOrWhiteSpace(dayTripDto.Location))
                    dayTrip.Location = dayTripDto.Location.Trim();
                if (!string.IsNullOrWhiteSpace(dayTripDto.LocationUrl))
                    dayTrip.LocationUrl = dayTripDto.LocationUrl.Trim();
                if (dayTripDto.PricePerPerson.HasValue)
                    dayTrip.PricePerPerson = dayTripDto.PricePerPerson.Value;
                if (dayTripDto.IsMain.HasValue)
                    dayTrip.IsMain = dayTripDto.IsMain.Value;
                if (dayTripDto.Included != null)
                    dayTrip.Included = dayTripDto.Included;
                if (dayTripDto.ItineraryJson != null)
                    dayTrip.Itinerary = itinerary;
                if (!string.IsNullOrWhiteSpace(dayTripDto.City))
                    dayTrip.City = dayTripDto.City.Trim();

                if (dayTripDto.CoverImage != null)
                {
                    if (!string.IsNullOrEmpty(cover))
                        DocumentSettings.DeleteFile(cover, "DayTripsCover");
                    cover = DocumentSettings.UploadFile(dayTripDto.CoverImage, "DayTripsCover");
                    if (string.IsNullOrEmpty(cover))
                        return BadRequest("Failed to upload cover image");
                }

                if (dayTripDto.Images != null && dayTripDto.Images.Any())
                {
                    foreach (var imageUrl in dayTrip.Images ?? new List<string>())
                    {
                        DocumentSettings.DeleteFile(imageUrl, "DayTrips");
                    }
                    imageUrls.Clear();
                    foreach (var image in dayTripDto.Images)
                    {
                        if (image?.Length > 0)
                        {
                            var fileUrl = DocumentSettings.UploadFile(image, "DayTrips");
                            if (!string.IsNullOrEmpty(fileUrl))
                                imageUrls.Add(fileUrl);
                        }
                    }
                    if (!imageUrls.Any() && dayTripDto.Images.Any())
                        return BadRequest("Failed to upload images");
                }

                dayTrip.Images = imageUrls;
                dayTrip.CoverImage = cover;

                _dayTripRepo.Update(dayTrip);
                await _dayTripRepo.SaveChangesAsync();

                _logger.LogInformation("DayTrip updated with Id: {DayTripId}", dayTrip.Id);

                await _dayTripRepo.CommitAsync(transaction);
                return Ok(new { Message = "DayTrip updated successfully", DayTripId = dayTrip.Id });
            }
            catch (Exception ex)
            {
                await _dayTripRepo.RollbackAsync(transaction);

                if (!string.IsNullOrEmpty(cover) && dayTripDto.CoverImage != null)
                    DocumentSettings.DeleteFile(cover, "DayTripsCover");

                foreach (var imageUrl in imageUrls)
                {
                    DocumentSettings.DeleteFile(imageUrl, "DayTrips");
                }

                _logger.LogError(ex, "Failed to update day trip: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while updating the day trip: {ex.Message}");
            }
        }

        [HttpGet("getAllDayTrips")]
        [Authorize]
        public async Task<ActionResult<List<DayTripReturnedDto>>> GetAllDayTrips()
        {
            _logger.LogInformation("Received GetAllDayTrips request");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var dayTrips = await _dayTripRepo.GetAllAsync();
            var result = _mapper.Map<List<DayTripReturnedDto>>(dayTrips);

            _logger.LogInformation("Returned {Count} day trips", result.Count);
            return Ok(result);
        }

        [HttpGet("getDayTripById/{id}")]
        [Authorize]
        public async Task<ActionResult<DayTripReturnedDto>> GetDayTripById(int id)
        {
            _logger.LogInformation("Received GetDayTripById request for Id: {Id}", id);

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

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

        [HttpDelete("deleteDayTrip/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteDayTrip(int id)
        {
            _logger.LogInformation("Received DeleteDayTrip request for Id: {Id}", id);

            var dayTrip = await _dayTripRepo.GetByIdAsync(id);
            if (dayTrip == null)
            {
                _logger.LogWarning("DayTrip with Id {Id} not found", id);
                return NotFound("DayTrip not found");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user data");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            using var transaction = await _dayTripRepo.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrEmpty(dayTrip.CoverImage))
                    DocumentSettings.DeleteFile(dayTrip.CoverImage, "DayTripsCover");

                foreach (var imageUrl in dayTrip.Images ?? new List<string>())
                {
                    DocumentSettings.DeleteFile(imageUrl, "DayTrips");
                }

                _dayTripRepo.Delete(dayTrip);
                await _dayTripRepo.SaveChangesAsync();

                _logger.LogInformation("DayTrip deleted with Id: {DayTripId}", dayTrip.Id);

                await _dayTripRepo.CommitAsync(transaction);
                return Ok(new { Message = "DayTrip deleted successfully", DayTripId = dayTrip.Id });
            }
            catch (Exception ex)
            {
                await _dayTripRepo.RollbackAsync(transaction);
                _logger.LogError(ex, "Failed to delete day trip: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while deleting the day trip: {ex.Message}");
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

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var stats = new DashboardStatisticsDto
            {
                TravelPrograms = await _travelProgramRepo.CountAsync(),
                Services = await _serviceRepo.CountAsync(),
                Weddings = await _weddingRepo.CountAsync(),
                DayTrips = await _dayTripRepo.CountAsync(),
                Activities = await _activityRepo.CountAsync(),
                Events = await _eventRepo.CountAsync(),
                Users = _userManager.Users.Count()
            };

            return Ok(stats);
        }
    }
}
