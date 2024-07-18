using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

class Program
{
    static async Task Main(string[] args)
    {
        // List of URLs to check
        List<string> urls = new List<string>
        {
            "https://example.com/page1",
            "https://example.com/page2",
            "https://example.com/page3"
        };

        // Iterate over each URL in the list
        foreach (var url in urls)
        {
            // Print the URL being checked
            Console.WriteLine($"Checking {url}");
            // Call the CheckPage method and await its result
            var result = await CheckPage(url);
            // Print the result of the check
            Console.WriteLine(result);
        }
    }

    static async Task<string> CheckPage(string url)
    {
        // Create an HttpClient instance
        using (HttpClient client = new HttpClient())
        {
            try
            {
                // Send an HTTP GET request to the URL
                HttpResponseMessage response = await client.GetAsync(url);
                // Check if the response status code is not successful
                if (!response.IsSuccessStatusCode)
                {
                    // Return an error message with the status code
                    return $"Error: {url} returned {response.StatusCode}";
                }

                // Read the response content as a string
                string content = await response.Content.ReadAsStringAsync();
                // Create an HtmlDocument instance and load the HTML content
                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                // Check for broken links on the page
                var brokenLinks = await CheckBrokenLinks(url, doc);
                // If there are broken links, return an error message with the details
                if (brokenLinks.Count > 0)
                {
                    return $"Error: {url} has broken links:\n{string.Join("\n", brokenLinks)}";
                }

                // Check for user flow issues on the page
                var userFlowIssues = CheckUserFlows(doc);
                // If there are user flow issues, return an error message with the details
                if (userFlowIssues.Count > 0)
                {
                    return $"Error: {url} has user flow issues:\n{string.Join("\n", userFlowIssues)}";
                }

                // If no issues are found, return a success message
                return $"{url} is OK";
            }
            catch (Exception ex)
            {
                // If an exception occurs, return an error message with the exception details
                return $"Exception: {url} encountered an error - {ex.Message}";
            }
        }
    }

    static async Task<List<string>> CheckBrokenLinks(string baseUrl, HtmlDocument doc)
    {
        // Initialize a list to store broken links
        List<string> brokenLinks = new List<string>();
        // Select all <a> tags with href attributes from the document
        var links = doc.DocumentNode.SelectNodes("//a[@href]");

        // If there are links on the page
        if (links != null)
        {
            // Create an HttpClient instance
            using (HttpClient client = new HttpClient())
            {
                // Iterate over each link
                foreach (var link in links)
                {
                    // Get the href attribute value
                    var href = link.Attributes["href"].Value;
                    // If the href is a relative URL, convert it to an absolute URL
                    if (Uri.IsWellFormedUriString(href, UriKind.Relative))
                    {
                        href = new Uri(new Uri(baseUrl), href).ToString();
                    }

                    try
                    {
                        // Send an HTTP GET request to the href URL
                        HttpResponseMessage response = await client.GetAsync(href);
                        // If the response status code is not successful, add the URL to the broken links list
                        if (!response.IsSuccessStatusCode)
                        {
                            brokenLinks.Add($"{href} returned {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        // If an exception occurs, add the URL and exception details to the broken links list
                        brokenLinks.Add($"{href} encountered an error - {ex.Message}");
                    }
                }
            }
        }

        // Return the list of broken links
        return brokenLinks;
    }

    static List<string> CheckUserFlows(HtmlDocument doc)
    {
        // Initialize a list to store user flow issues
        List<string> issues = new List<string>();

        // Check if there are any forms on the page
        if (doc.DocumentNode.SelectNodes("//form") == null)
        {
            // If no forms are found, add an issue to the list
            issues.Add("No forms found on the page.");
        }

        // Check if there are any buttons on the page
        if (doc.DocumentNode.SelectNodes("//button") == null)
        {
            // If no buttons are found, add an issue to the list
            issues.Add("No buttons found on the page.");
        }

        // Return the list of user flow issues
        return issues;
    }
}
