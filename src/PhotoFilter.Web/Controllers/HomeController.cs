using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Blob;
using PhotoFilter.Web.Infrastructure;
using System.Threading.Tasks;

namespace PhotoFilter.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ImageStore _imageStore;
        public HomeController(ImageStore imageStore)
        {
            _imageStore = imageStore;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> LoaderApi(int count, string nextMarker)
        {
            var continuationToken = new BlobContinuationToken
            {
                NextMarker = nextMarker,
                TargetLocation = Microsoft.WindowsAzure.Storage.StorageLocation.Primary,
            };

            return Json(new
            {
                images = await _imageStore.FetchAsync(count, continuationToken),
            });
        }

        public async Task<ActionResult> PosterApi(PostModel data)
        {
            return Json(new
            {
                result = await _imageStore.Sort(data.Images),
            });
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
