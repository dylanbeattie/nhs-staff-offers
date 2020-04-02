using System.Collections.Generic;

namespace nhs_help.Models {
  public class OffersViewData {
    public string OriginalWebUrl { get; set; }
    public string SpreadsheetUrl { get; set; }

    public Dictionary<string, List<List<string>>> Offers { get; set; }
  }
}