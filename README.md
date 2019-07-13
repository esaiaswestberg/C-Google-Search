# C# Google Search
Simple C# library for making simple Google searches without the use of any API.

##### Search one page
```csharp
GoogleSearch.Engine search = new GoogleSearch.Engine();
GoogleSearch.Engine.Result[] results = search.Search("Your search query here...");
Console.WriteLine("We found theese websites:");
foreach(var result in results)
    Console.WriteLine(result.title + " - " + result.description + "\n    " + result.url);
```

##### Search multiple pages
```csharp
GoogleSearch.Engine search = new GoogleSearch.Engine();
GoogleSearch.Engine.Result[] results = search.SearchPages("Your search query here...", 5);
Console.WriteLine("We found theese websites:");
foreach(var result in results)
    Console.WriteLine(result.title + " - " + result.description + "\n    " + result.url);
```
