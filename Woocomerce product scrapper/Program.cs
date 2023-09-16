using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Woocomerce_product_scrapper
{
    public class Product
    {
        public string Url { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string PriceCurrency { get; set; }
        public string Sku { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string PrimaryImage { get; set; }
        public string PrimaryImageAlt { get; set; }
        public bool InStock { get; set; }
        public double? AverageRating { get; set; }
        public int? ReviewCount { get; set; }
        public List<string> Images { get; set; }
        public string Thumbnail { get; set; }
        public List<string> Categories { get; set; }
        public Dictionary<string, string> Attributes { get; set; }  // Key-Value pairs for attribute name and its value
        public List<string> Tags { get; set; }

        // Constructor to initialize lists and dictionary
        public Product()
        {
            Images = new List<string>();
            Categories = new List<string>();
            Attributes = new Dictionary<string, string>();
            Tags = new List<string>();
        }
    }





    class Program
    {
        // Limit the number of simultaneous requests to, e.g., 5
        private static SemaphoreSlim semaphore = new SemaphoreSlim(5);
        static string BuildURL(string domain, string entryTitle, string entryID)
        {
            var s = Regex.Replace($"{entryTitle}-{entryID}", "[^0-9a-zA-Z]+", "-");
            if (s.EndsWith('-'))
                s = s[..^1];
            return $"https://{domain}/product/{s}";
        }

        //static async Task Main(string[] args)
        //{
        //    string domain = "dromex.co.za";
        //    int page_number = 1;
        //    int page_limit = 999;

        //    var catalog = new List<(string, string, string, string, string, string)>();

        //    using (HttpClient client = new HttpClient())
        //    {
        //        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537");

        //        while (page_number <= page_limit)
        //        {
        //            try
        //            {
        //                var url = $"https://{domain}/page/{page_number}/?s=&post_type=product";
        //                var response = await client.GetStringAsync(url);

        //                var doc = new HtmlDocument();
        //                doc.LoadHtml(response);

        //                var products = doc.DocumentNode.SelectNodes("//h2[@class='woocommerce-loop-product__title']")?.Select(n => n.InnerText).ToList();
        //                var productURLs = new List<string>();
        //                var Thumnails = new List<string>();
        //                if (products == null || !products.Any())
        //                {
        //                    products = doc.DocumentNode.SelectNodes("//p[contains(@class, 'product-title')]")?.Select(n => n.InnerText).ToList();
        //                    productURLs = doc.DocumentNode.SelectNodes("//p[contains(@class, 'product-title')]/a")?.Select(n => n.GetAttributeValue("href", "")).ToList();
        //                    Thumnails = doc.DocumentNode.SelectNodes("//img[contains(@class, 'woocommerce_thumbnail')]")?.Select(n => n.GetAttributeValue("src", "")).ToList();
        //                }
        //                var prices = doc.DocumentNode.SelectNodes("//span[@class='woocommerce-Price-amount']")?.Select(n => n.InnerText).ToList();
        //                var skus = doc.DocumentNode.SelectNodes("//p[contains(@class, 'product-cat')]")?.Select(n => n.InnerText).ToList();
        //                var imageNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'image-fade_in_back')]/a/img");

        //                if (imageNodes != null && imageNodes.Any())
        //                {
        //                    var imageSrcList = imageNodes.Select(n => n.GetAttributeValue("src", "")).ToList();
        //                    var imageSrcSetList = imageNodes.Select(n => n.GetAttributeValue("srcset", "")).ToList();

        //                    // Now you can process or store the extracted src and srcset values.
        //                    for (int i = 0; i < imageSrcList.Count; i++)
        //                    {
        //                        var src = imageSrcList[i];
        //                        var srcset = imageSrcSetList[i];
        //                    }
        //                }
        //                Console.WriteLine($"Adding to catalog array... {page_number}/{page_limit}");
        //                for (int i = 0; i < products.Count; i++)
        //                {
        //                    var keywords = products[i].Split(' ').ToList();
        //                    var entryID = keywords.Last();
        //                    keywords.RemoveAt(keywords.Count - 1);
        //                    var item = string.Join(" ", keywords);
        //                    var entryPrice = (prices == null ? "0" : prices[i]);
        //                    var entryUrl = productURLs[i];
        //                    var thumb = Thumnails[i];
        //                    //BuildURL(domain, item, entryID);
        //                    var sku = (skus == null ? "0" : skus[i]);
        //                    catalog.Add((item, entryID, entryPrice, entryUrl, sku.Trim(), thumb));
        //                }



        //                page_number++;
        //            }
        //            catch (Exception ex)
        //            {
        //                page_number = 999;
        //                break;
        //            }
        //        }

        //        var csvFile = $"{domain}_{DateTime.Now.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.csv";
        //        using (var writer = new StreamWriter(csvFile))
        //        {
        //            Console.WriteLine($"Loading Products ({catalog.Count})");
        //            writer.WriteLine("Product,Product ID,Price,URL,sku,thumb,img");

        //            // Process products concurrently
        //            var tasks = catalog.Select(product => LimitConcurrencyAndProcessProductAsync(product, client)).ToList();
        //            var results = await Task.WhenAll(tasks);

        //            foreach (var result in results)
        //            {
        //                writer.WriteLine(result);
        //            }
        //        }
        //    }
        //}
        private static async Task<string> LimitConcurrencyAndProcessProductAsync((string entryTitle, string entryID, string entryPrice, string url, string sku, string thumb) product, HttpClient client)
        {
            await semaphore.WaitAsync();
            try
            {
                return await ProcessProductAsync(product, client);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private static async Task<string> ProcessProductAsync((string entryTitle, string entryID, string entryPrice, string url, string sku, string thumb) product, HttpClient client)
        {
            try
            {
                Console.WriteLine($"Loading Product {product.entryTitle}");
                var productDetailsResponse = await client.GetStringAsync(product.url);
                var productDoc = new HtmlDocument();
                productDoc.LoadHtml(productDetailsResponse);
                var extractedData = new Dictionary<string, string>();
                var jsonLdNode = productDoc.DocumentNode.SelectSingleNode("//script[@type='application/ld+json']");
                if (jsonLdNode != null)
                {
                    var jsonLd = jsonLdNode.InnerHtml;

                    using (JsonDocument document = JsonDocument.Parse(jsonLd))
                    {
                        var root = document.RootElement;


                        if (root.TryGetProperty("@graph", out var graphElement) && graphElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var element in graphElement.EnumerateArray())
                            {
                                var typeValue = element.GetProperty("@type").GetString();
                                switch (typeValue)
                                {
                                    case "WebPage":
                                        extractedData["type"] = typeValue;
                                        extractedData["name"] = element.GetProperty("name").GetString();
                                        extractedData["url"] = element.GetProperty("url").GetString();
                                        //extractedData["primaryImage"] = element.GetProperty("contentUrl").GetString();
                                        extractedData["thumbnailUrl"] = element.GetProperty("thumbnailUrl").GetString();
                                        break;

                                    case "Organization":
                                        extractedData["organization_name"] = element.GetProperty("name").GetString();
                                        extractedData["organization_url"] = element.GetProperty("url").GetString();
                                        extractedData["organization_logo"] = element.GetProperty("logo").GetProperty("url").GetString();
                                        break;
                                    case "ImageObject":
                                        extractedData["primaryImage"] = element.GetProperty("url").GetString();
                                        break;
                                        // Extend with other cases or types as needed
                                }
                            }
                        }

                        // Print out the extracted data
                        foreach (var kvp in extractedData)
                        {
                            Console.WriteLine($"{kvp.Key}: {kvp.Value}");
                        }
                    }
                }

                List<string> productCategories = new List<string>();
                string productTypes = string.Empty;
                var productNodes = productDoc.DocumentNode.SelectNodes("//div[starts-with(@id, 'product-')]");
                if (productNodes != null)
                    foreach (var productNode in productNodes)
                    {
                        var classValue = productNode.GetAttributeValue("class", "");

                        // Extracting product ID
                        var productIdMatch = Regex.Match(classValue, @"product-(\d+)");
                        if (productIdMatch.Success)
                        {
                            var productId = productIdMatch.Groups[1].Value;
                        }

                        // Extracting categories from the class value
                        var categoryMatches = Regex.Matches(classValue, @"product_cat-([a-zA-Z0-9\-]+)");
                        foreach (Match match in categoryMatches)
                        {
                            productCategories.Add(match.Groups[1].Value);
                        }

                        // Extracting product type
                        if (classValue.Contains("product-type-"))
                        {
                            var typeMatch = Regex.Match(classValue, @"product-type-([a-zA-Z0-9\-]+)");
                            if (typeMatch.Success)
                            {
                                productTypes = (typeMatch.Groups[1].Value);
                            }
                        }
                    }

                return $"\"{product.entryTitle}\",\"{product.entryID}\",\"{product.entryPrice}\",\"{product.url}\",\"{product.sku}\",\"{product.thumb}\",\"{extractedData["thumbnailUrl"]}\"";
            }
            catch (Exception ex)
            {
                return $"\"{product.entryTitle}\",\"{product.entryID}\",\"{product.entryPrice}\",\"{product.url}\",\"{product.sku}\",\"{product.thumb}\",\"\"";
                return null;
            }
        }

        static void DisplayUsage()
        {
            Console.WriteLine("Usage: woo4all domain.com [-useSitemap] [-useCustomShopPage] [-outputJson] [-outputCSV]");
            Console.WriteLine("Flags:");
            Console.WriteLine("\t-useSitemap: Use sitemap for scraping.");
            Console.WriteLine("\t-useCustomShopPage: Use a custom shop page for scraping.");
            Console.WriteLine("\t-outputJson: Output results as JSON.");
            Console.WriteLine("\t-outputCSV: Output results as CSV.");
            Console.WriteLine("Example: woo4all.exe domain.com -outputJson");

        }

        static List<Product> DownloadWooDefault(string domain)
        {
            List<Product> Products = new List<Product>();
            int page_number = 1;
            int page_limit = 999;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537");

                while (page_number <= page_limit)
                {
                    try
                    {

                        var url = $"https://{domain}/page/{page_number}/?s=&post_type=product";
                        var response = client.GetStringAsync(url).Result;
                        int xi = 0;
                        var doc = new HtmlDocument();
                        doc.LoadHtml(response);
                        // Fetch the primary product nodes
                        var productNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'product-small col has-hover')]");
                        Console.WriteLine($"Adding Page {page_number} with {productNodes.Count} products");
                        foreach (var productNode in productNodes)
                        {
                            Console.WriteLine($"Adding product {(xi + 1)} of {productNodes.Count} for page {page_number}");
                            xi++;
                            Product product = new Product();
                            // Fetch product title
                            var productTitleNode = productNode.SelectSingleNode(".//h2[@class='woocommerce-loop-product__title']") ??
                                                   productNode.SelectSingleNode(".//p[contains(@class, 'product-title')]");
                            var productTitle = System.Net.WebUtility.HtmlDecode(productTitleNode?.InnerText);

                            // Fetch product URL
                            var productUrlNode = productNode.SelectSingleNode(".//p[contains(@class, 'product-title')]/a");
                            var productUrl = productUrlNode?.GetAttributeValue("href", "");

                            // Fetch product thumbnail
                            var thumbnailNode = productNode.SelectSingleNode(".//img[contains(@class, 'woocommerce_thumbnail')]");
                            var thumbnail = thumbnailNode?.GetAttributeValue("src", "");

                            // Fetch product price
                            var priceNode = productNode.SelectSingleNode(".//span[@class='woocommerce-Price-amount']");
                            var price = priceNode?.InnerText;

                            // Fetch product SKU
                            var skuNodes = productNode.SelectNodes(".//p[contains(@class, 'product-cat')]");
                            var skus = skuNodes?.Select(n => System.Net.WebUtility.HtmlDecode(n.InnerText.Trim())).ToList();

                            // Fetch primary product image src and srcset
                            var imageNode = productNode.SelectSingleNode(".//div[contains(@class, 'box-image')]/div[contains(@class, 'image-fade_in_back')]/a/img");
                            var imageSrc = imageNode?.GetAttributeValue("src", "");
                            var imageSrcSet = imageNode?.GetAttributeValue("srcset", "");
                            if (imageNode == null)
                            {
                                var imageDiv = productNode.SelectSingleNode(".//div[contains(@class, 'box-image')]");
                                if (imageDiv != null)
                                {
                                    var imgNode = imageDiv.SelectSingleNode(".//img[contains(@class, 'attachment-woocommerce_thumbnail')]");
                                    if (imgNode != null)
                                    {
                                        var src = imgNode.GetAttributeValue("src", "");
                                        var srcset = imgNode.GetAttributeValue("srcset", "");

                                        // Decoding any HTML entities (like &#8217;)
                                        imageSrc = System.Net.WebUtility.HtmlDecode(src);
                                        imageSrcSet = System.Net.WebUtility.HtmlDecode(srcset);
                                    }
                                }
                            }

                            // Fetch product type
                            var productType = "unknown";
                            var classAttr = productNode.GetAttributeValue("class", "");
                            var match = Regex.Match(classAttr, @"product-type-(\w+)");
                            if (match.Success && match.Groups.Count > 1)
                            {
                                productType = match.Groups[1].Value;
                            }

                            //get the product desctions here 
                            //item should go to the producturl and download the text then look for product-short-description
                            //and also get the drescription from <meta property="og:description" content=
                            var shortDesc = "";
                            var metaDesc = "";
                            try
                            {
                                var productPageResponse = client.GetStringAsync(productUrl).Result;
                                var productPageDoc = new HtmlDocument();
                                productPageDoc.LoadHtml(productPageResponse);

                                // Extract product short description
                                var shortDescNode = productPageDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'product-short-description')]");
                                if (shortDescNode != null)
                                {
                                    shortDesc = WebUtility.HtmlDecode(shortDescNode.InnerText.Trim());

                                }

                                // Extract description from meta tag
                                var metaDescNode = productPageDoc.DocumentNode.SelectSingleNode("//meta[@property='og:description']");
                                if (metaDescNode != null)
                                {
                                    metaDesc = metaDescNode.GetAttributeValue("content", "");
                                    metaDesc = WebUtility.HtmlDecode(metaDesc.Trim());

                                }

                                var jsonLdNode = productPageDoc.DocumentNode.SelectSingleNode("//script[@type='application/ld+json' and contains(@class, 'yoast-schema-graph')]");
                                if (jsonLdNode != null)
                                {
                                    var jsonContent = jsonLdNode.InnerText;
                                    try
                                    {
                                        var structuredData = JsonConvert.DeserializeObject<dynamic>(jsonContent);

                                        // Now you can access different parts of the structured data
                                        var webPageName = structuredData["@graph"][0]["name"]?.ToString();

                                        var primaryImageUrl = structuredData["@graph"][0]["primaryImageOfPage"]["@id"]?.ToString();
                                        product.PrimaryImage = primaryImageUrl;
                                        var primaryImageUrlAlt = structuredData["@graph"][0]["image"]["@id"]?.ToString();
                                        product.PrimaryImageAlt = primaryImageUrlAlt;
                                        var thumbnailUrl = structuredData["@graph"][0]["thumbnailUrl"]?.ToString();

                                        //... and so on for other fields


                                        //... print other fields as required

                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine($"Error parsing JSON-LD: {e.Message}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                            }

                            product.Name = productTitle;
                            product.Url = productUrl;
                            product.Thumbnail = thumbnail;
                            product.Type = productType;
                            Decimal.TryParse(price, out decimal _price);
                            product.Price = _price;
                            product.MinPrice = 0;
                            product.Sku = string.Join("; ", skus);
                            product.ShortDescription = shortDesc;
                            product.Description = metaDesc;
                            product.Images = imageSrcSet.Split(",").ToList();

                            // Your logic here
                            //Console.WriteLine($"Product: {productTitle}");
                            //Console.WriteLine($"URL: {productUrl}");
                            //Console.WriteLine($"Thumbnail: {thumbnail}");
                            //Console.WriteLine($"Price: {price}");
                            //Console.WriteLine($"SKU: {string.Join(", ", skus)}");
                            //Console.WriteLine($"Image src: {imageSrc}");
                            //Console.WriteLine($"Image srcset: {imageSrcSet}");
                            //Console.WriteLine($"Product Type: {productType}");
                            //Console.WriteLine($"Product Short Description: {shortDesc}");
                            //Console.WriteLine($"Meta Description: {metaDesc}");

                            //Console.WriteLine("-------------------------------");
                            Products.Add(product);
                        }

                        //var products = doc.DocumentNode.SelectNodes("//h2[@class='woocommerce-loop-product__title']")?.Select(n => n.InnerText).ToList();
                        //var productURLs = new List<string>();
                        //var Thumnails = new List<string>();
                        //if (products == null || !products.Any())
                        //{
                        //    products = doc.DocumentNode.SelectNodes("//p[contains(@class, 'product-title')]")?.Select(n => n.InnerText).ToList();
                        //    productURLs = doc.DocumentNode.SelectNodes("//p[contains(@class, 'product-title')]/a")?.Select(n => n.GetAttributeValue("href", "")).ToList();
                        //    Thumnails = doc.DocumentNode.SelectNodes("//img[contains(@class, 'woocommerce_thumbnail')]")?.Select(n => n.GetAttributeValue("src", "")).ToList();
                        //}
                        //var prices = doc.DocumentNode.SelectNodes("//span[@class='woocommerce-Price-amount']")?.Select(n => n.InnerText).ToList();
                        //var skus = doc.DocumentNode.SelectNodes("//p[contains(@class, 'product-cat')]")?.Select(n => n.InnerText).ToList();
                        //// Adjusted XPath to be more specific
                        //var imageNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'box-image')]/div[contains(@class, 'image-fade_in_back')]/a/img");

                        //if (imageNodes != null && imageNodes.Any())
                        //{
                        //    var imageSrcList = imageNodes.Select(n => n.GetAttributeValue("src", "")).ToList();
                        //    var imageSrcSetList = imageNodes.Select(n => n.GetAttributeValue("srcset", "")).ToList();

                        //    // Now you can process or store the extracted src and srcset values.
                        //    for (int i = 0; i < imageSrcList.Count; i++)
                        //    {
                        //        var src = imageSrcList[i];
                        //        var srcset = imageSrcSetList[i];

                        //        // Your logic here
                        //        Console.WriteLine($"Image {i + 1}:");
                        //        Console.WriteLine($"src: {src}");
                        //        Console.WriteLine($"srcset: {srcset}");
                        //    }
                        //}

                        //Console.WriteLine($"Adding to catalog array... {page_number}/{page_limit}");
                        //for (int i = 0; i < products.Count; i++)
                        //{
                        //    var keywords = products[i].Split(' ').ToList();
                        //    var entryID = keywords.Last();
                        //    keywords.RemoveAt(keywords.Count - 1);
                        //    var item = string.Join(" ", keywords);
                        //    var entryPrice = (prices == null ? "0" : prices[i]);
                        //    var entryUrl = productURLs[i];
                        //    var thumb = Thumnails[i];
                        //    //BuildURL(domain, item, entryID);
                        //    var sku = (skus == null ? "0" : skus[i]);
                        //    catalog.Add((item, entryID, entryPrice, entryUrl, sku.Trim(), thumb));
                        //}

                        page_number++;
                    }
                    catch
                    {
                        page_number = 999;
                        break;
                    }
                }

            }

            return Products;
        }

        static List<Product> DownloadViaSitemap(string domain)
        {
            var sitemapUrl = $"https://{domain}/sitemap.xml";

            List<Product> Products = new List<Product>();
            // Download the sitemap XML content
            string xmlContent = DownloadSitemapAsync(sitemapUrl).Result;

            if (!string.IsNullOrEmpty(xmlContent))
            {
                // Parse the XML content
                XDocument sitemapXml = XDocument.Parse(xmlContent);
                // Using the namespace from your XML
                XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

                // You can now use the sitemapXml variable to access the sitemap's XML data
                // For example, printing out all the loc tags:
                List<string> locs = new List<string>();
                foreach (var loc in sitemapXml.Descendants(ns + "loc"))
                {
                    locs.Add(loc.Value);
                }

                // Display the loc values
                Console.WriteLine("List of <loc> elements:");
                foreach (string loc in locs)
                {
                    Console.WriteLine(loc);
                }
                var url = locs.Find(x => x.Contains("product-sitemap.xml"));
                if (url != null)
                {

                    //this is the impportant one this will show all products
                    xmlContent = DownloadSitemapAsync(url).Result;
                    if (!string.IsNullOrEmpty(xmlContent))
                    {
                        // Parse the XML content
                        sitemapXml = XDocument.Parse(xmlContent);
                        // Using the namespace from your XML
                        ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
                        XNamespace nsImage = "http://www.google.com/schemas/sitemap-image/1.1";
                        var urls = from _url in sitemapXml.Descendants(ns + "url")
                                   let imageLoc = _url.Element(nsImage + "loc")
                                   select new
                                   {
                                       Loc = _url.Element(ns + "loc").Value,
                                       ImageLoc = _url.Descendants().FirstOrDefault(e => e.Name.LocalName == "loc" && e.Parent.Name.LocalName == "image")?.Value ?? string.Empty
                                   };

                        Console.WriteLine($"Found {urls.Count()} products");
                        int id = 0;
                        foreach (var _url in urls)
                        {
                            Product product = new Product();
                            Console.WriteLine($"Downloading product {id + 1} / {urls.Count()}");
                            id++;
                            //Console.WriteLine($"Product URL: {_url.Loc}");
                            if (_url.ImageLoc != null)
                            {
                                //Console.WriteLine($"Image URL: {_url.ImageLoc}");
                            }
                            var shortDesc = "";
                            var metaDesc = "";
                            try
                            {
                                using (HttpClient client = new HttpClient())
                                {
                                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537");

                                    var productPageResponse = client.GetStringAsync(_url.Loc).Result;
                                    var productPageDoc = new HtmlDocument();
                                    productPageDoc.LoadHtml(productPageResponse);

                                    // Extract product short description
                                    var shortDescNode = productPageDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'product-short-description')]");
                                    if (shortDescNode != null)
                                    {
                                        shortDesc = WebUtility.HtmlDecode(shortDescNode.InnerText.Trim());

                                    }


                                    // Extract description from meta tag
                                    var metaDescNode = productPageDoc.DocumentNode.SelectSingleNode("//meta[@property='og:description']");
                                    if (metaDescNode != null)
                                    {
                                        metaDesc = metaDescNode.GetAttributeValue("content", "");
                                        metaDesc = WebUtility.HtmlDecode(metaDesc.Trim());

                                    }
                                    //Get The Product Title
                                    var metatitleNode = productPageDoc.DocumentNode.SelectSingleNode("//meta[@property='og:title']");
                                    if (metatitleNode != null)
                                    {
                                        var metatitle = metatitleNode.GetAttributeValue("content", "");
                                        metatitle = WebUtility.HtmlDecode(metatitle.Trim());
                                        product.Name = metatitle;
                                    }
                                    //Get Price 
                                    var priceNode = productPageDoc.DocumentNode.SelectSingleNode(".//span[@class='woocommerce-Price-amount']");
                                    var price = priceNode?.InnerText;

                                    // Fetch product SKU
                                    var skuNodes = productPageDoc.DocumentNode.SelectSingleNode(".//div[contains(@class, 'product') and contains(@class, 'product_cat')]");
                                    var sku = skuNodes?.GetAttributeValue("class", "");
                                    if (sku != null)
                                    {
                                        var match1 = Regex.Match(sku, @"product_cat-(\w+)");
                                        if (match1.Success && match1.Groups.Count > 1)
                                        {
                                            sku = match1.Groups[1].Value;
                                            product.Sku = string.Join("; ", sku);
                                        }
                                    }
                                    // Fetch product type
                                    var productType = "unknown";
                                    var classAttr = productPageDoc.DocumentNode.SelectSingleNode(".//div[contains(@class, 'product type-product')]")?.GetAttributeValue("class", "");
                                    var match = Regex.Match(classAttr, @"product-type-(\w+)");
                                    if (match.Success && match.Groups.Count > 1)
                                    {
                                        productType = match.Groups[1].Value;
                                    }

                                    var jsonLdNode = productPageDoc.DocumentNode.SelectSingleNode("//script[@type='application/ld+json' and contains(@class, 'yoast-schema-graph')]");
                                    if (jsonLdNode != null)
                                    {
                                        var jsonContent = jsonLdNode.InnerText;
                                        try
                                        {
                                            var structuredData = JsonConvert.DeserializeObject<dynamic>(jsonContent);

                                            // Now you can access different parts of the structured data
                                            var webPageName = structuredData["@graph"][0]["name"]?.ToString();

                                            var primaryImageUrl = structuredData["@graph"][0]["primaryImageOfPage"]["@id"]?.ToString();
                                            product.PrimaryImage = primaryImageUrl;
                                            product.Images.Add(primaryImageUrl);
                                            var primaryImageUrlAlt = structuredData["@graph"][0]["image"]["@id"]?.ToString();
                                            product.PrimaryImageAlt = primaryImageUrlAlt;
                                            var thumbnailUrl = structuredData["@graph"][0]["thumbnailUrl"]?.ToString();
                                            product.Thumbnail = thumbnailUrl;
                                            //... and so on for other fields


                                            //... print other fields as required

                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine($"Error parsing JSON-LD: {e.Message}");
                                        }
                                    }

                                    //Set the url of the product
                                    product.Url = _url.Loc;

                                    Decimal.TryParse(price, out decimal _price);
                                    product.Price = _price;
                                    product.MinPrice = 0;

                                    product.ShortDescription = shortDesc;
                                    product.Description = metaDesc;

                                    Products.Add(product);
                                }
                            }
                            catch (Exception ex)
                            {

                            }


                        }


                    }
                }

            }
            return Products;
        }

        static async System.Threading.Tasks.Task<string> DownloadSitemapAsync(string url)
        {
            using HttpClient client = new HttpClient();
            try
            {
                return await client.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading the sitemap: {ex.Message}");
                return null;
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("WooForAll - The simple woocommerce scraper V0.1");

            string domain;
            if (args == null || args.Length == 0)
            {
                DisplayUsage();

                return;
            }

            domain = args[0];
            bool useSitemap = args.Contains("-useSitemap");
            bool useCustomShopPage = args.Contains("-useCustomShopPage");
            bool outputJson = args.Contains("-outputJson");
            bool outputCSV = args.Contains("-outputCSV");

            if (outputJson == false && outputCSV == false)
            {
                Console.WriteLine("Please spesify an output type iether -outputJson or -outputCSV");
                return;
            }


            List<Product> Products = new List<Product>();
            if (useSitemap == false && useCustomShopPage == false)
            {
                Products = DownloadWooDefault(domain);
            }
            else if (useSitemap == true)
            {
                Products = DownloadViaSitemap(domain);
            }
            else if (useCustomShopPage == true)
            {

            }

            if (outputCSV)
            {
                // Build CSV File
                Console.WriteLine("Building CSV File...");
                var csvFile = $"{domain}_{DateTime.Now.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.csv";
                using (var writer = new StreamWriter(csvFile))
                {
                    Console.WriteLine($"Loading Products ({Products.Count})");
                    int Counter = 0;
                    writer.WriteLine("url,type,name,price,minPrice,maxPrice,priceCurrency,sku,short_description,description,primaryImage,primaryImageAlt,inStock,averageRating,reviewCount,images,categories,attributes,tags"); // header)
                    for (int i = 0; i < Products.Count; i++)
                    {
                        var product = Products[i];
                        Console.WriteLine($"Writing Product {(i + 1)} / {Products.Count}");
                        writer.WriteLine($"{product.Url},{product.Type},{product.Name},{product.Price},{product.MinPrice},{product.MaxPrice},{product.PriceCurrency},{product.Sku},{WebUtility.HtmlEncode(product.ShortDescription)},{WebUtility.HtmlEncode(product.Description)},{product.PrimaryImage},{product.PrimaryImageAlt},{product.InStock},{product.AverageRating},{product.ReviewCount},{string.Join(";", product.Images)},{string.Join(";", product.Categories)},{string.Join(";", product.Attributes)},{string.Join(";", product.Tags)}");
                    }
                }
            }
            if (outputJson)
            {
                var json = $"{domain}_{DateTime.Now.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.json";
                File.WriteAllText(json, JsonConvert.SerializeObject(Products));
            }
        }


    }
}
