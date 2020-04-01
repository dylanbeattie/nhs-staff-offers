using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ClosedXML.Excel;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using nhs_help.Models;

namespace nhs_help.Controllers {
	public class HomeController : Controller {
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger) {
			_logger = logger;
		}

		public async Task<IActionResult> Index() {
            var cells = await GetCells();
            var model = AnalyseCells(cells);
            return(View(model));
		}

        private Dictionary<string, List<List<string>>> AnalyseCells(List<List<string>> cells) {
            var heading = String.Empty;
            Dictionary<String, List<List<String>>> result = new Dictionary<string, List<List<string>>>();
            var headers = cells.First().Where(cell => ! String.IsNullOrWhiteSpace(cell)).ToList();
            foreach(var row in cells.Skip(1)) {
                if (row.All(s => String.IsNullOrWhiteSpace(s))) continue;
                if (!String.IsNullOrWhiteSpace(row[0]) && row.Skip(1).All(s => String.IsNullOrWhiteSpace(s))) {
                    heading = row[0];
                    result.Add(heading, new List<List<string>>());
                    result[heading].Add(headers.ToList());                    
                    continue;
                }
                result[heading].Add(row.Trim(String.IsNullOrWhiteSpace).ToList());
            }
            foreach(var grid in result.Values) {
                var widest = grid.Max(row => row.Count);
                foreach(var row in grid) {
                    while (row.Count < widest) row.Add(String.Empty);
                }
            }
            return(result);
        }

		// public IActionResult Privacy() {
		// 	return View();
		// }

		// [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		// public IActionResult Error() {
		// 	return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		// }

		private async Task<List<List<string>>> GetCells() {
			using (HttpClient client = new HttpClient()) {
				var response = await client.GetAsync("https://www.england.nhs.uk/coronavirus/publication/list-of-nhs-staff-offers/");
				var pageContents = await response.Content.ReadAsStringAsync();
				var pageDocument = new HtmlDocument();
				pageDocument.LoadHtml(pageContents);
				//return(Json(pageContents));                
				//				var xlsLink = pageDocument.DocumentNode.SelectNodes("//a[ends-with(@href, '.xlsx')]");
				var links = pageDocument.DocumentNode.SelectNodes("//a");
				var xlsHrefAttribute = links.SelectMany(link => link.Attributes)
					.Where(attr => attr.Name == "href" && attr.Value.EndsWith(".xlsx"))
					.FirstOrDefault();

				Console.WriteLine(xlsHrefAttribute.Value);
				const string filename = "nhs-data.xlsx";
				using (var wc = new WebClient()) wc.DownloadFile(xlsHrefAttribute.Value, filename);
				var workbook = new XLWorkbook(filename);
				var sheet = workbook.Worksheets.FirstOrDefault();
                var rows = new List<List<string>>();
                for (var i = 2; i < 255; i++) {
                    var row = new List<string>();
                    for (var j = 2; j < 26; j++) {
                        row.Add(sheet.Cell(i,j).Value.ToString().Trim());
                    }
                    rows.Add(row);
                };
				// var rows = sheet.Rows().Select(r => r.Ce.Select(cell => cell.Value.ToString()).ToList()).ToList();
				return (rows);
			}
		}
	}

    public static class ListExtensions {
        public static IList<T> Trim<T>(this IList<T> list, Func<T,bool> predicate) {
            var last = list.Count - 1;            
            while(last >= 0 && predicate(list[last])) list.RemoveAt(last--);
            return(list);
        }
    }
}