using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using DSharpPlus.Entities;
using Newtonsoft.Json;
namespace GalacticSwampBot.Library.OCR.Api {
    public static class OCRSpaceRequester {
        
        private const string Url = "https://api.ocr.space/parse/image";
        private const string RateLimitReached = "You may only perform this action upto maximum 180 number of times within 3600 seconds";

        private static readonly List<string> Keys = new()
        {
            "K81595927788957",
            "K86792185788957",
            "K82143533788957",
            "K82778418788957"
        };
        public static ParseRequest PostRequest(DiscordChannel[] discordChannels, string imagePath, short ocrEngine, bool detectOrientation)
        {
            var imageBytes = File.ReadAllBytes(imagePath);

            using (var client = new HttpClient())
            {
                // Set the API key header
                client.DefaultRequestHeaders.Add("apikey", Keys[0]);

                // Create the form-data content
                var formContent = new MultipartFormDataContent();

                // Add the image as a content stream
                var imageContent = new StreamContent(new MemoryStream(imageBytes));
                imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                var fileName = Path.GetFileName(imagePath);
                formContent.Add(imageContent, fileName.Replace(".png", string.Empty), fileName);

                // Add the parameters as string content
                formContent.Add(new StringContent("eng"), "language");
                formContent.Add(new StringContent("false"), "isOverlayRequired");
                formContent.Add(new StringContent("PNG"), "filetype");
                formContent.Add(new StringContent(detectOrientation ? "true" : "false"), "detectOrientation");
                formContent.Add(new StringContent("false"), "isCreateSearchablePdf");
                formContent.Add(new StringContent("true"), "scale");
                formContent.Add(new StringContent(ocrEngine.ToString()), "OCREngine");

                // Send the POST request
                HttpResponseMessage response = null;

                bool ok = false;
                for (var i = 0; !ok && i < 5; i++){
                    response = client.PostAsync(Url, formContent).Result;
                    ok = response.IsSuccessStatusCode;
                }
                if (response != null){
                    if (response.IsSuccessStatusCode){
                        var responseContent = response.Content.ReadAsStringAsync().Result;
                        if (responseContent.Contains(Constants.OCRApiAnswers.TimedOut)){
                            Wait(discordChannels, message:Constants.OCRApiAnswers.TimedOut);
                            return PostRequest(discordChannels,imagePath, ocrEngine, detectOrientation);
                        }
                        // Display the response
                        return JsonConvert.DeserializeObject<ParseRequest>(responseContent);
                    }
                    if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        //rate limit reached
                        Wait(discordChannels);
                        return PostRequest(discordChannels,imagePath, ocrEngine, detectOrientation);
                    }
                }
            }
            return null;
        } 
        private static void Wait(DiscordChannel[] discordChannels, string message = "Rate limit reached")
        {
            var totalMilliSeconds = (int)TimeSpan.FromMinutes(10).TotalMilliseconds;
            var dtAfter = DateTime.Now.AddMilliseconds(totalMilliSeconds);
            if (!Bot.IsWinter(dtAfter)){
                dtAfter = dtAfter.AddHours(1);
            }
            Bot.WriteInfoLog($"Waiting started at: {DateTime.Now} untill {dtAfter}");
            foreach (var channel in discordChannels){
                channel.ModifyAsync(c => c.Topic = message + "! Waiting untill " + dtAfter).Wait();
            }
            Thread.Sleep(totalMilliSeconds);
            foreach (var channel in discordChannels){
                channel.ModifyAsync(c => c.Topic = string.Empty).Wait();
            }
            Keys.Add(Keys[0]);
            Keys.RemoveAt(0);
        }
    }
}
