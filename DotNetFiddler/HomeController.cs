using DotNetForce;
using Newtonsoft.Json.Linq;
using System;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetForceSample
{
	public class HomeController : Controller
	{
		protected string RedirectUri = "https://ste80.github.io/DotNetForce/oauth2";
		protected string ClientId = "3MVG910YPh8zrcR3w3cOaVxURhJtcv8fxvL19jvXzqO_F819av8P2cc9VMnBOKkKTdK.uMAfUGRU_4aYDm5A3";
		
		[HttpGet]
		public ActionResult Index()
		{
			return View(new ViewModel
			{
				RedirectUri = RedirectUri,
				ClientId = ClientId,
			});
		}
		
		[HttpPost]
		public async Task<ActionResult> Schema(PostData data)
		{
			try {
				return Json(data);
				var client = await DNFClient.OAuthLoginAsync(new OAuthProfile
				{
					LoginUri = new Uri(data.instance_url),
					ClientId = ClientId,
					RedirectUri = RedirectUri,
					Code = data.access_token
				});
				var generator = new SchemaGenerator();
				var schemaCode = await generator.GenerateAsync(client, "SalesforceSchema");

				return Content(schemaCode);	
			}
			catch (Exception ex) {
				return Content(ex.ToString());
			}
		}

        public class PostData {
            public string access_token { get; set; }
            public string instance_url { get; set; }
            public string id { get; set; }
			public string refresh_token { get; set; }
            public string issued_at { get; set; }
            public string signature { get; set; }
            public string state { get; set; }
            public string scope { get; set; }
            public string token_type { get; set; }
        }
	}
}