using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Multi.Models;
using Multi.DataAccess.Repository.IUnitOfWorks;

namespace MultiWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork m_unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            m_unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var products = m_unitOfWork.ProductBikeRepo.GetAll(includeProp: "Category");

            return View(products);
        }

        public IActionResult Details(uint id)
        {
            var product = m_unitOfWork.ProductBikeRepo.Get(u => u.Id == id, includeProp: "Category");

            return View(product);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
