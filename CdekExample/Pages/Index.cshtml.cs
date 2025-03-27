using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CdekExample.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public string YandexMapsApiKey { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IConfigurationRoot config)
        {
            _logger = logger;
            this.YandexMapsApiKey = config["YandexMapsApiKey"];
        }

        public void OnGet()
        {
        }
    }
}
