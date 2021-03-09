using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stemmesystem.Web.Data;

namespace Stemmesystem.Web.Pages
{
    public class EksporterArrangement : PageModel
    {
        private readonly ArrangementService _arrangementService;

        public EksporterArrangement(ArrangementService arrangementService)
        {
            _arrangementService = arrangementService;
        }

        public async Task<IActionResult> OnGet(int id)
        {
            var arrangementInfo =  await _arrangementService.HentArrangementInfoAsync(id);
            if(arrangementInfo == null) return NotFound();

            var arrangement = await _arrangementService.HentArrangementAsync(id);
            arrangement.
            
        }
    }
}