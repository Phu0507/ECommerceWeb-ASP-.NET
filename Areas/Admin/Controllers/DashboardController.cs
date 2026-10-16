using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_web.Areas.Admin.Models;
using Shopping_web.Repositories;

namespace Shopping_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Publisher,Auhtor")]
    [Route("Admin/Dashboard")]
    public class DashboardController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public DashboardController(DataContext dataContext, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = dataContext;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var count_Product = _dataContext.Products.Count();
            var count_Category = _dataContext.Categories.Count();
            var count_Brand = _dataContext.Brands.Count();
            var count_Order = _dataContext.Orders.Count();
            var count_User = _dataContext.Users.Count();

            ViewBag.count_Product = count_Product;
            ViewBag.count_Category = count_Category;
            ViewBag.count_Brand = count_Brand;
            ViewBag.count_Order = count_Order;
            ViewBag.count_User = count_User;

            return View();
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetChartData([FromBody] ChartFilterDto filter)
        {
            var query =
                from o in _dataContext.Orders
                join od in _dataContext.OrderDetails
                    on o.OrderCode equals od.OrderCode
                where o.Status == 2
                select new { o, od };

            if (filter.FromDate.HasValue)
            {
                var from = filter.FromDate.Value.Date;
                query = query.Where(x => x.o.CreatedDate >= from);
            }

            if (filter.ToDate.HasValue)
            {
                var to = filter.ToDate.Value.Date.AddDays(1);
                query = query.Where(x => x.o.CreatedDate < to);
            }

            var data = await query
                .GroupBy(x => x.o.CreatedDate.Date)
                .Select(g => new
                {
                    DateCreated = g.Key,
                    Quantity = g.Select(x => x.o.OrderCode).Distinct().Count(),
                    Sold = g.Sum(x => x.od.Quantity),
                    Revenue = g.Sum(x => x.od.Price * x.od.Quantity),
                    Profit = g.Sum(x => x.od.Price * x.od.Quantity) * 0.3m // Giả sử lợi nhuận là 30% doanh thu
                })
                .OrderBy(x => x.DateCreated)
                .ToListAsync();

            return Json(data);
        }

    }
}
