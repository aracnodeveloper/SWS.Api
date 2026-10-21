using System.Net;
using App.Shared.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Shared.Infrastructure;

public class BaseController<T> : Controller
    where T : class
{
    protected ApplicationContext context;
    protected DbSet<T> repository;

    public BaseController(ApplicationContext context)
    {
        this.context = context;
        repository = this.context.Set<T>();
    }

    protected ActionResult ErrorMessage(
        string message,
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError
    )
    {
        return new ObjectResult(new { message }) { StatusCode = (int)statusCode };
    }
}
