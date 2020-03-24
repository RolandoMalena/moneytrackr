using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTrackr.Data;
using MoneyTrackr.Data.DomainObjects;
using MoneyTrackr.Dtos;
using static MoneyTrackr.Constants.Role;

namespace MoneyTrackr.Controllers.API
{
    [Route("api/Users/{username}/[controller]")]
    [ApiController]
    [Authorize]
    public class EntriesController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<IdentityUser> userManager;

        #region Constructor
        public EntriesController(ApplicationDbContext _dbContext, UserManager<IdentityUser> _userManager)
        {
            dbContext = _dbContext;
            userManager = _userManager;
        }
        #endregion

        #region Get All (with optional Date Range)
        /// <summary>
        /// Get every Entry registered to a given User and optionally within two dates.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get(string username, DateTime? from, DateTime? to)
        {
            var validationResult = await ValidateUser(username);
            if (validationResult != null)
                return validationResult;

            //Get the entries and query by the dates (if provided) and then convert to Dto
            var entries = await (await GetEntriesQuery(username))
                .Where(e => !from.HasValue || from.Value.Date <= e.Date)
                .Where(e => !to.HasValue || e.Date <= to.Value.Date)
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.Id)
                .ToArrayAsync();

            var dtos = entries
                .Select(e => EntryDto.ConvertBack(e))
                .ToArray();

            return Ok(dtos);
        }
        #endregion

        #region Get
        /// <summary>
        /// Gets a single Entry by its Id and for the given User.
        /// </summary>
        /// <param name="username">The username to whom the entry belongs</param>
        /// <param name="id">The Id of the entry to be found</param>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string username, int id)
        {
            var validationResult = await ValidateUser(username);
            if (validationResult != null)
                return validationResult;

            //Get the Entry
            var entry = await GetEntry(username, id);

            //If null, return NotFound
            if (entry == null)
                return NotFound();

            //Return the Entry
            return Ok(EntryDto.ConvertBack(entry));
        }
        #endregion

        #region Get Report
        /// <summary>
        /// Get a Detailed Account Report
        /// </summary>
        [HttpGet("Report")]
        public async Task<IActionResult> GetReport(string username, DateTime? from, DateTime? to)
        {
            var validationResult = await ValidateUser(username);
            if (validationResult != null)
                return validationResult;

            //Get the entries
            var entries = await (await GetEntriesQuery(username))
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.Id)
                .ToArrayAsync();

            //If there are no entries at all, there is nothing to be done
            if (entries.Length == 0)
                return Ok(new ReportDto()
                {
                    Entries = new ReportDetailDto[0]
                });

            //Get the summary data
            int rowCount = entries.Length;
            double currentBalance = entries.Sum(e => e.Amount);

            var deposits = entries.Where(e => e.Amount > 0);
            int depositCount = deposits.Count();
            double totalDeposit = deposits.Sum(e => e.Amount);
            double depositAvg = depositCount > 0 ? deposits.Average(e => e.Amount) : 0;

            var withdrawals = entries.Where(e => e.Amount < 0);
            int withdrawalsCount = withdrawals.Count();
            double totalWithdrawals = withdrawals.Sum(e => e.Amount);
            double withdrawalAvg = withdrawalsCount > 0 ? withdrawals.Average(e => e.Amount) : 0;

            //Convert to DTOs
            var dtos = entries
                .Select(e => new ReportDetailDto(EntryDto.ConvertBack(e)))
                .ToArray();

            //Add the Balance to all rows
            double balance = 0;
            foreach (var dto in dtos.OrderBy(d => d.Date))
            {
                balance += dto.Amount.Value;
                dto.Balance = balance;
            }

            //Filter by the optional date limits
            dtos = dtos
                .Where(e => !from.HasValue || from.Value.Date <= e.Date)
                .Where(e => !to.HasValue || e.Date <= to.Value.Date)
                .ToArray();

            var reportDto = new ReportDto()
            {
                CurrentBalance = currentBalance,
                EntryCount = rowCount,

                DepositCount = depositCount,
                TotalDeposits = totalDeposit,
                DepositAverage = double.IsNaN(depositAvg) ? 0 : depositAvg,

                WithdrawCount = withdrawalsCount,
                TotalWithdraws = totalWithdrawals,
                WithdrawAverage = double.IsNaN(withdrawalAvg) ? 0 : withdrawalAvg,

                Entries = dtos
            };

            return Ok(reportDto);
        }
        #endregion

        #region Post
        /// <summary>
        /// Create a new Entry.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post(string username, [FromBody]EntryDto dto)
        {
            var validationResult = await ValidateUser(username);
            if (validationResult != null)
                return validationResult;

            if (dto.Amount == 0)
                return BadRequest("The Amount cannot be 0.");

            var userId = (await userManager.FindByNameAsync(username)).Id;

            //Convert Dto to Model Object
            var newEntry = dto.Convert(userId);
            
            //Add Entry and save the changes
            dbContext.Entries.Add(newEntry);
            await dbContext.SaveChangesAsync();

            //Everything went Ok, prepare the Dto
            dto.Id = newEntry.Id;

            //Return location of the new Entry along with the Dto
            return Created(Url.Action("Get", "Entries", new { username, dto.Id }), dto);
        }
        #endregion

        #region Put
        /// <summary>
        /// Updates an Entry.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string username, int id, [FromBody]EntryDto dto)
        {
            var validationResult = await ValidateUser(username);
            if (validationResult != null)
                return validationResult;

            if (dto.Amount == 0)
                return BadRequest("The Amount cannot be 0.");

            //Get Entry in the DB
            var entryInDb = await GetEntry(username, id);

            //Return NotFound if null
            if (entryInDb == null) 
                return NotFound();

            //Make changes to the other Model properties and save changes
            entryInDb.Description = dto.Description;
            entryInDb.Amount = dto.Amount.Value;
            entryInDb.Date = dto.Date.Value;
            await dbContext.SaveChangesAsync();

            return Ok();
        }
        #endregion

        #region Delete
        /// <summary>
        /// Deletes an Entry.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string username, int id)
        {
            var validationResult = await ValidateUser(username);
            if (validationResult != null)
                return validationResult;

            //Get the Entry
            var entry = await GetEntry(username, id);

            //Return NotFound if null
            if (entry == null)
                return NotFound();

            //Removes the Entry
            dbContext.Entries.Remove(entry);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Validates the provided username.
        /// </summary>
        /// <returns>Returns the corresponding ActionResult, if it passes validation, returns null.</returns>
        async Task<IActionResult> ValidateUser(string username)
        {
            //If you are a RegularUser, you can only get and save your own Entries
            if (User.IsInRole(RegularUserRoleName) && User.FindFirst(ClaimTypes.NameIdentifier).Value.ToUpper() != username.ToUpper())
                return Forbid();

            var user = await userManager.FindByNameAsync(username);
            if (user == null)
                return NotFound();

            //If you are a UserManager, you cannot get or save Entries from/for Administrators
            if (User.IsInRole(UserManagerRoleName) && (await userManager.GetRolesAsync(user)).Single() == AdministratorRoleName)
                return Forbid();

            return null;
        }

        /// <summary>
        /// Get a query that returns all the entries if Administrator otherwise all personal entries
        /// </summary>
        /// <returns>The Query with the Entries</returns>
        async Task<IQueryable<Entry>> GetEntriesQuery(string username)
        {
            var userId = (await userManager.FindByNameAsync(username)).Id;

            return dbContext.Entries
                .Where(e => e.UserId == userId);
        }

        /// <summary>
        /// Gets a specific Entry for a given User.
        /// </summary>
        /// <param name="username">The Username of the User.</param>
        /// <param name="id">The Id of the Entry to look for.</param>
        /// <returns>The Entry</returns>
        async Task<Entry> GetEntry(string username, int id)
        {
            return (await GetEntriesQuery(username))
                .Where(e => e.Id == id)
                .SingleOrDefault();
        }
        #endregion
    }
}