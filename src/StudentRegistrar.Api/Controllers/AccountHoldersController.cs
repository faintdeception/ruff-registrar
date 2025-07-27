using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using StudentRegistrar.Api.Services;
using StudentRegistrar.Api.DTOs;
using System.Security.Claims;

namespace StudentRegistrar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountHoldersController : ControllerBase
{
    private readonly IAccountHolderService _accountHolderService;
    private readonly ILogger<AccountHoldersController> _logger;

    public AccountHoldersController(
        IAccountHolderService accountHolderService,
        ILogger<AccountHoldersController> logger)
    {
        _accountHolderService = accountHolderService;
        _logger = logger;
    }

    /// <summary>
    /// Get the current user's account holder information
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<AccountHolderDto>> GetMyAccountHolder()
    {
        var keycloakUserId = GetCurrentKeycloakId();
        if (string.IsNullOrEmpty(keycloakUserId))
        {
            return Unauthorized("User ID not found in token");
        }

        var accountHolder = await _accountHolderService.GetAccountHolderByUserIdAsync(keycloakUserId);
        if (accountHolder == null)
        {
            // Auto-create account holder from JWT token claims
            var email = GetCurrentUserEmail();
            var firstName = GetCurrentUserFirstName();
            var lastName = GetCurrentUserLastName();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                return BadRequest("Insufficient user information in token to create account holder");
            }

            var createDto = new CreateAccountHolderDto
            {
                FirstName = firstName,
                LastName = lastName,
                EmailAddress = email,
                // Set default values for required fields
                AddressJson = new AddressInfo
                {
                    Street = "",
                    City = "",
                    State = "",
                    PostalCode = "",
                    Country = "US"
                },
                EmergencyContactJson = new EmergencyContactInfo
                {
                    FirstName = "",
                    LastName = "",
                    Email = ""
                }
            };

            try
            {
                accountHolder = await _accountHolderService.CreateAccountHolderAsync(createDto, keycloakUserId);
                _logger.LogInformation("Auto-created account holder for user {UserId}", keycloakUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-creating account holder for user {UserId}", keycloakUserId);
                return StatusCode(500, "Error creating account holder");
            }
        }

        return Ok(accountHolder);
    }

    /// <summary>
    /// Get account holder by ID (admin only)
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<AccountHolderDto>> GetAccountHolder(Guid id)
    {
        var accountHolder = await _accountHolderService.GetAccountHolderByIdAsync(id);
        if (accountHolder == null)
        {
            return NotFound();
        }

        return Ok(accountHolder);
    }

    /// <summary>
    /// Create a new account holder (admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<AccountHolderDto>> CreateAccountHolder(CreateAccountHolderDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var accountHolder = await _accountHolderService.CreateAccountHolderAsync(createDto);
            return CreatedAtAction(nameof(GetAccountHolder), new { id = accountHolder.Id }, accountHolder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account holder");
            return StatusCode(500, "An error occurred while creating the account holder");
        }
    }

    /// <summary>
    /// Update account holder information
    /// Users can update their own profile, admins can update any profile
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AccountHolderDto>> UpdateAccountHolder(Guid id, UpdateAccountHolderDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check if user can update this account holder
        var canUpdate = await CanUserUpdateAccountHolder(id);
        if (!canUpdate)
        {
            return Forbid("You can only update your own account");
        }

        try
        {
            var updatedAccountHolder = await _accountHolderService.UpdateAccountHolderAsync(id, updateDto);
            if (updatedAccountHolder == null)
            {
                return NotFound();
            }

            return Ok(updatedAccountHolder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating account holder {Id}", id);
            return StatusCode(500, "An error occurred while updating the account holder");
        }
    }

    /// <summary>
    /// Add a student to the current user's account
    /// </summary>
    [HttpPost("me/students")]
    public async Task<ActionResult<StudentDto>> AddStudentToMyAccount(CreateStudentForAccountDto createStudentDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var keycloakUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(keycloakUserId))
        {
            return Unauthorized("User ID not found in token");
        }

        // Get the account holder to find their ID
        var accountHolder = await _accountHolderService.GetAccountHolderByUserIdAsync(keycloakUserId);
        if (accountHolder == null)
        {
            return NotFound("Account holder not found");
        }

        if (!Guid.TryParse(accountHolder.Id, out var accountHolderId))
        {
            return BadRequest("Invalid account holder ID");
        }

        try
        {
            var student = await _accountHolderService.AddStudentToAccountAsync(accountHolderId, createStudentDto);
            return CreatedAtAction("GetStudent", "Students", new { id = student.Id }, student);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding student to account holder {AccountHolderId}", accountHolderId);
            return StatusCode(500, "An error occurred while adding the student");
        }
    }

    /// <summary>
    /// Add a student to a specific account holder (admin only)
    /// </summary>
    [HttpPost("{accountHolderId:guid}/students")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<StudentDto>> AddStudentToAccount(Guid accountHolderId, CreateStudentForAccountDto createStudentDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var student = await _accountHolderService.AddStudentToAccountAsync(accountHolderId, createStudentDto);
            return CreatedAtAction("GetStudent", "Students", new { id = student.Id }, student);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding student to account holder {AccountHolderId}", accountHolderId);
            return StatusCode(500, "An error occurred while adding the student");
        }
    }

    /// <summary>
    /// Remove a student from the current user's account
    /// </summary>
    [HttpDelete("me/students/{studentId:guid}")]
    public async Task<IActionResult> RemoveStudentFromMyAccount(Guid studentId)
    {
        var keycloakUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(keycloakUserId))
        {
            return Unauthorized("User ID not found in token");
        }

        // Get the account holder to find their ID
        var accountHolder = await _accountHolderService.GetAccountHolderByUserIdAsync(keycloakUserId);
        if (accountHolder == null)
        {
            return NotFound("Account holder not found");
        }

        if (!Guid.TryParse(accountHolder.Id, out var accountHolderId))
        {
            return BadRequest("Invalid account holder ID");
        }

        var success = await _accountHolderService.RemoveStudentFromAccountAsync(accountHolderId, studentId);
        if (!success)
        {
            return NotFound("Student not found or does not belong to this account");
        }

        return NoContent();
    }

    /// <summary>
    /// Remove a student from a specific account holder (admin only)
    /// </summary>
    [HttpDelete("{accountHolderId:guid}/students/{studentId:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> RemoveStudentFromAccount(Guid accountHolderId, Guid studentId)
    {
        var success = await _accountHolderService.RemoveStudentFromAccountAsync(accountHolderId, studentId);
        if (!success)
        {
            return NotFound("Student not found or does not belong to this account");
        }

        return NoContent();
    }

    private async Task<bool> CanUserUpdateAccountHolder(Guid accountHolderId)
    {
        // Admins can update any account holder
        if (User.IsInRole("Administrator"))
        {
            return true;
        }

        // Users can only update their own account
        var keycloakUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(keycloakUserId))
        {
            return false;
        }

        var currentUserAccountHolder = await _accountHolderService.GetAccountHolderByUserIdAsync(keycloakUserId);
        if (currentUserAccountHolder == null)
        {
            return false;
        }

        return Guid.TryParse(currentUserAccountHolder.Id, out var currentAccountHolderId) && 
               currentAccountHolderId == accountHolderId;
    }

    private string GetCurrentKeycloakId()
    {
        // Try multiple claim types that might contain the subject ID
        var subClaim = User.FindFirst("sub")?.Value 
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        
        if (string.IsNullOrEmpty(subClaim))
        {
            throw new UnauthorizedAccessException("No user ID in token");
        }
        return subClaim;
    }

    private string GetCurrentUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value ?? "";
    }

    private string GetCurrentUserFirstName()
    {
        return User.FindFirst(ClaimTypes.GivenName)?.Value ?? User.FindFirst("given_name")?.Value ?? "";
    }

    private string GetCurrentUserLastName()
    {
        return User.FindFirst(ClaimTypes.Surname)?.Value ?? User.FindFirst("family_name")?.Value ?? "";
    }
}
