
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobLens.Web.Models;
using JobLens.Web.Data;

public class ApplicationsController : Controller
{
    private readonly JobLensDbContext _context;

    public ApplicationsController(JobLensDbContext context)
    {
        _context = context;
    }

    // GET: JOBAPPLICATIONS
    public async Task<IActionResult> Index(string? status, string? employer, string? sort)
    {
        var query = _context.JobApplications.AsQueryable();

        if (!string.IsNullOrWhiteSpace(employer))
        {
            query = query.Where(a => EF.Functions.ILike(a.Employer, $"%{employer}%")
                || EF.Functions.ILike(a.Role, $"%{employer}%"));
        }

        ViewBag.StatusCounts = await query
            .GroupBy(a => a.Status)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
        ViewBag.TotalCount = await _context.JobApplications.CountAsync();

        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<ApplicationStatus>(status, out var parsed))
        {
            query = query.Where(a => a.Status == parsed);
        }

        query = sort switch
        {
            "employer" => query.OrderBy(a => a.Employer),
            "oldest" => query.OrderBy(a => a.DateApplied),
            "followup" => query.OrderBy(a => a.NextActionDate == null).ThenBy(a => a.NextActionDate),
            _ => query.OrderByDescending(a => a.DateApplied).ThenByDescending(a => a.CreatedAt)
        };

        ViewBag.Status = status;
        ViewBag.Employer = employer;
        ViewBag.Sort = sort;

        return View(await query.ToListAsync());
    }

    // GET: JOBAPPLICATIONS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var jobapplication = await _context.JobApplications
            .FirstOrDefaultAsync(m => m.Id == id);
        if (jobapplication == null)
        {
            return NotFound();
        }

        return View(jobapplication);
    }

    // GET: JOBAPPLICATIONS/Create
    public async Task<IActionResult> Create()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        await LoadSuggestionsAsync();
        return View(new JobApplication
        {
            DateApplied = today,
            Status = ApplicationStatus.Applied,
            NextActionDate = today.AddDays(7),
        });
    }

    // POST: JOBAPPLICATIONS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Employer,Role,Location,Source,Url,DateApplied,Status,NextActionDate,Notes,AdvertisementText")] JobApplication jobapplication)
    {
        if (ModelState.IsValid)
        {
            _context.Add(jobapplication);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        await LoadSuggestionsAsync();
        return View(jobapplication);
    }

    // GET: JOBAPPLICATIONS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var jobapplication = await _context.JobApplications.FindAsync(id);
        if (jobapplication == null)
        {
            return NotFound();
        }
        await LoadSuggestionsAsync();
        return View(jobapplication);
    }

    // POST: JOBAPPLICATIONS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Employer,Role,Location,Source,Url,DateApplied,Status,NextActionDate,Notes,AdvertisementText")] JobApplication jobapplication)
    {
        if (id != jobapplication.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(jobapplication);
                _context.Entry(jobapplication).Property(a => a.CreatedAt).IsModified = false;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JobApplicationExists(jobapplication.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        await LoadSuggestionsAsync();
        return View(jobapplication);
    }

    // GET: JOBAPPLICATIONS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var jobapplication = await _context.JobApplications
            .FirstOrDefaultAsync(m => m.Id == id);
        if (jobapplication == null)
        {
            return NotFound();
        }

        return View(jobapplication);
    }

    // POST: JOBAPPLICATIONS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var jobapplication = await _context.JobApplications.FindAsync(id);
        if (jobapplication != null)
        {
            _context.JobApplications.Remove(jobapplication);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadSuggestionsAsync()
    {
        ViewBag.Employers = await _context.JobApplications
            .Select(a => a.Employer).Distinct().OrderBy(e => e).ToListAsync();
        ViewBag.Roles = await _context.JobApplications
            .Select(a => a.Role).Distinct().OrderBy(r => r).ToListAsync();
        ViewBag.Locations = await _context.JobApplications
            .Where(a => a.Location != null && a.Location != "")
            .Select(a => a.Location!).Distinct().OrderBy(l => l).ToListAsync();
    }

    private bool JobApplicationExists(int? id)
    {
        return _context.JobApplications.Any(e => e.Id == id);
    }
}
