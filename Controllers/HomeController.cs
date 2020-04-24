using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using sm_coding_challenge.Models;
using sm_coding_challenge.Services.DataProvider;

namespace sm_coding_challenge.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDataProvider _dataProvider;

        public HomeController(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Player(string id)
        {
            return Json(await _dataProvider.GetPlayerById(id));
        }

        [HttpGet]
        public async Task<IActionResult> Players(string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return Json(Enumerable.Empty<PlayerModel>());
            }
            var idList = ids.Split(',');
            return Json(await _dataProvider.GetPlayersById(idList));
        }

        [HttpGet]
        public async Task<IActionResult> LatestPlayers(string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return Json(Enumerable.Empty<PlayerModel>());
            }
            var idList = ids.Split(',');
            return Json(await _dataProvider.GetLatestPlayersById(idList));
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
