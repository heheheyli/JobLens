using JobLens.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace JobLens.Web.Controllers;

public class VersionController : Controller
{
    public IActionResult Index()
    {
        var info = new VersionInfo(
            Environment.GetEnvironmentVariable("BUILD_COMMIT") ?? "local-build",
            Environment.GetEnvironmentVariable("BUILD_TIME") ?? "not set",
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown");

        return View(info);
    }
}