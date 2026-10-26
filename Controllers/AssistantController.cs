using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using tp2.Models.Help;
using tp2.ViewModels;

namespace tp2.Controllers
{
    public class AssistantController : Controller
    {
        private readonly ChatbotService chatbotService;
        private readonly UserManager<IdentityUser> userManager;

        public AssistantController(ChatbotService chatbotService, UserManager<IdentityUser> userManager)
        {
            this.chatbotService = chatbotService;
            this.userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ask([FromBody] ChatRequest? request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return Json(new ChatResponse
                {
                    Reply = "Écrivez-moi une question et je vous aiderai avec plaisir.",
                    Suggestions = new List<string> { "Voir les produits", "Livraison", "Modes de paiement" }
                });
            }

            IdentityUser? user = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                user = await userManager.GetUserAsync(User);
            }

            var response = chatbotService.Respond(request.Message, user);
            return Json(response);
        }
    }
}
