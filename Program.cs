using Newtonsoft.Json;
using System.Text;

namespace GeminiTestApp
{
    class Program
    {
        public static string apiKey = "{YOUR-API-KEY-HERE}";
        public static string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key=" + apiKey;
        static async Task Main(string[] args)
        {
            while (true)
            {
                Console.Write("Input : ");
                var tempInput = Console.ReadLine();
                var requestBody = PrepareRequestBody(tempInput);
                var response = await PrepareOutput(requestBody);

                Console.WriteLine(response);
            }

            //Console.ReadKey();
        }

        public static object PrepareRequestBody(string input)
        {
            return new
            {
                contents = new[]
                {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = input
                        }
                    }
                }
            },
                generationConfig = new
                {
                    temperature = 0.9,
                    topK = 1,
                    topP = 1,
                    maxOutputTokens = 2048,
                    stopSequences = new string[] { }
                },
                safetySettings = new[]
                {
                    new
                    {
                        category = "HARM_CATEGORY_HARASSMENT",
                        threshold = "BLOCK_MEDIUM_AND_ABOVE"
                    },
                    new
                    {
                        category = "HARM_CATEGORY_HATE_SPEECH",
                        threshold = "BLOCK_MEDIUM_AND_ABOVE"
                    },
                    new
                    {
                        category = "HARM_CATEGORY_SEXUALLY_EXPLICIT",
                        threshold = "BLOCK_MEDIUM_AND_ABOVE"
                    },
                    new
                    {
                        category = "HARM_CATEGORY_DANGEROUS_CONTENT",
                        threshold = "BLOCK_MEDIUM_AND_ABOVE"
                    }
                }
            };
        }

        public static async Task<string> PrepareOutput(object parametre)
        {
            string responseOutput = String.Empty;
            string jsonRequestBody = Newtonsoft.Json.JsonConvert.SerializeObject(parametre);
            StringContent content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    string output;
                    try
                    {
                        var responseObj = JsonConvert.DeserializeObject<ResponseModel>(responseContent);
                        output = responseObj.Candidates[0]?.Content?.Parts?[0]?.Text;

                    }
                    catch (Exception)
                    {
                        output = String.Empty;
                    }


                    if (!String.IsNullOrEmpty(output))
                    {
                        responseOutput = output;
                    }
                    else
                    {
                        responseOutput = responseContent;//TO-DO
                    }
                }
                else
                {
                    responseOutput = $"Error: {response.StatusCode} - {response.ReasonPhrase}";
                }
            }

            return responseOutput;
        }

        public class ResponseModel
        {
            public List<Candidate> Candidates { get; set; }
            public PromptFeedback PromptFeedback { get; set; }
        }

        public class Candidate
        {
            public Content Content { get; set; }
        }

        public class Content
        {
            public List<Part> Parts { get; set; }
        }

        public class Part
        {
            public string Text { get; set; }
        }

        public class PromptFeedback
        {
            public List<SafetyRating> SafetyRatings { get; set; }
        }

        public class SafetyRating
        {
            public string Category { get; set; }
            public string Probability { get; set; }
        }
    }
}