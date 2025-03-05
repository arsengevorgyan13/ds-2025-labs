using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Globalization;

namespace Valuator.Pages;
public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly IConnectionMultiplexer _redis;

    public SummaryModel(ILogger<SummaryModel> logger, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redis = redis;
    }

    public double Rank { get; set; }
    public double Similarity { get; set; }

    public void OnGet(string id)
    {
        _logger.LogDebug(id);
        var db = _redis.GetDatabase();

        // TODO: (pa1) проинициализировать свойства Rank и Similarity значениями из БД (Redis)
        string similarityKey = $"SIMILARITY-{id}";
        string rankKey = $"RANK-{id}";

        var rankValue = db.StringGet(rankKey);
        if (double.TryParse(rankValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double rank))
        {
            Rank = rank;
        }
        else
        {
            Rank = 5.2;
        }

        var similarityValue = db.StringGet(similarityKey);
        if (double.TryParse(similarityValue, out double similarity))
        {
            Similarity = similarity;
        }
        else
        {
            Similarity = 0.0;
        }
    }
}