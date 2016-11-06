using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Blob;
using PhotoFilter.Web.Infrastructure;
using System.Threading.Tasks;

namespace PhotoFilter.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ImageStore _imageStore;
        private readonly ImageCount _imageCount;

        public HomeController(ImageStore imageStore, ImageCount imageCount)
        {
            _imageStore = imageStore;
            _imageCount = imageCount;
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

        public async Task<ActionResult> CountApi()
        {
            return Json(new
            {
                result = await _imageCount.CountAsync(),
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
