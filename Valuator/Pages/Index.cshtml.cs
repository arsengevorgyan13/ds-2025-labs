using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IDatabase _redisDb;
    public IndexModel(ILogger<IndexModel> logger, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redisDb = redis.GetDatabase();
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost(string text)
    {
        string id = Guid.NewGuid().ToString();

        string similarityKey = "SIMILARITY-" + id;

        double similarity = CountSimilarity(text);
        _redisDb.StringSet(similarityKey, similarity);

        string textKey = "TEXT-" + id;
        _redisDb.StringSet(textKey, text);

        string rankKey = "RANK-" + id;        
        double rank = CountRank(text);
        _redisDb.StringSet(rankKey, rank);

        return Redirect($"summary?id={id}");
    }

    private double CountRank(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0.0;

        int totalChars = text.Length;
        int nonAlphabeticCount = 0;

        foreach (char c in text)
        {
            //if (char.IsLetter)
            if (!IsAlphabetic(c))
                nonAlphabeticCount++;
        }

        return (double)nonAlphabeticCount / totalChars;
    }

    private double CountSimilarity(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0.0;
        // убрать перебор ключей
        var server = _redisDb.Multiplexer.GetServer("localhost", 6379);
        var keys = server.Keys(database: _redisDb.Database, pattern: "TEXT-*");

        foreach (var key in keys)
        {
            var storedText = _redisDb.StringGet(key);
            if (storedText == text)
            {
                return 1.0;
            }
        }

        return 0.0;
    }
    // узнать что такое char в c#
    private bool IsAlphabetic(char c)
    {
        if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
            return true;

        if ((c >= 'А' && c <= 'Я') || (c >= 'а' && c <= 'я') || c == 'Ё' || c == 'ё')
            return true;

        return false;
    }
}