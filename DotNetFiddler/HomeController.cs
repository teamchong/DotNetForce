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
		public ActionResult Schema(PostData data)
		{
			var client = DNFClient.OAuthLoginAsync(new OAuthProfile
			{
				LoginUri = new Uri("https://" + data.state),
				ClientId = ClientId,
				RedirectUri = RedirectUri,
				Code = data.code
			}).Result;
			var generator = new SchemaGenerator();
			var schemaCode = generator.GenerateAsync(client, "SalesforceSchema").Result;

			return Content(schemaCode);	
		}

        public class PostData {
            public string code { get; set; }
            public string state { get; set; }
        }
	}
}