#r "Newtonsoft.Json"

using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    try
    {        
        log.LogInformation($"Start Translation: {DateTime.Now}");
        string text = req.Query["text"];

        string translationResult = await GetTranslation(log, text);
        log.LogInformation($"translationResult: {translationResult}");

        string sentimentResult = await GetSentiment(log, text);
        log.LogInformation($"sentimentResult: {sentimentResult}");


        dynamic translationData = JsonConvert.DeserializeObject(translationResult);
        log.LogInformation($"translationData: {translationData}");

        dynamic analyticsData = JsonConvert.DeserializeObject(sentimentResult);
        log.LogInformation($"analyticsData: {analyticsData}");

        var result = new {
            text = translationData[0].translations[0].text,
            sentiment =analyticsData.documents[0].score
        };

        log.LogInformation($"End Sucess Translation: {DateTime.Now}");
        return text != null
            ? (ActionResult)new OkObjectResult(result)
            : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
    } 
    catch (Exception ex) 
    {
        log.LogInformation($"End Error Translation: {DateTime.Now}");
        log.LogError($"Failure while downloading string from page: {ex}");
        return new BadRequestObjectResult($"Failure while downloading string from page: {ex}");
    }    
}


public static async Task<string> GetTranslation(ILogger log, string untranslateString) {
    
    var host = "https://api.cognitive.microsofttranslator.com/";
    var route = "/translate?api-version=3.0&to=de";
    var subscriptionKey = Environment.GetEnvironmentVariable("TRANSLATOR_TEXT_KEY");

    try{
        using (var client = new HttpClient())
        using (var request = new HttpRequestMessage()) 
        {   
            // Set the method to POST
            request.Method = HttpMethod.Post;

            // Construct the full URI
            request.RequestUri = new Uri(host + route);        
            log.LogInformation($"URL: {request.RequestUri}");

            // Add the serialized JSON object to your request
            var body = new { Text = untranslateString };
            var requestBody = "[" + JsonConvert.SerializeObject(body) +"]";
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            // Add the authorization header
            request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            // Send request, get response
            var response = (await client.SendAsync(request));
            var jsonResponse = response.Content.ReadAsStringAsync().Result;    
            return jsonResponse;
        }
    } catch (Exception ex) {
        log.LogError($"Failure while downloading string from page: {ex}");
        return "Sorry Something went wrong.";
    }
}

public static async Task<string> GetSentiment(ILogger log, string untranslateString) {
    
    var host = "https://westeurope.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment";
    //var route = "/translate?api-version=3.0&to=de";
    var subscriptionKey = Environment.GetEnvironmentVariable("ANALYTICS_TEXT_KEY");

    try{
        using (var client = new HttpClient())
        using (var request = new HttpRequestMessage()) 
        {   
            // Set the method to POST
            request.Method = HttpMethod.Post;

            // Construct the full URI
            //request.RequestUri = new Uri(host + route);      
            request.RequestUri = new Uri(host);     
            log.LogInformation($"URL: {request.RequestUri}");

            // Add the serialized JSON object to your request
            var body =  new { 
                language = "de", 
                id = "1",
                text = untranslateString
                };

            var requestBody = "{\"documents\": [" + JsonConvert.SerializeObject(body) +"]}";
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            // Add the authorization header
            request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            // Send request, get response
            var response = (await client.SendAsync(request));
            var jsonResponse = response.Content.ReadAsStringAsync().Result;    
            return jsonResponse;
        }
    } catch (Exception ex) {
        log.LogError($"Failure while downloading string from page: {ex}");
        return "Sorry Something went wrong.";
    }
}