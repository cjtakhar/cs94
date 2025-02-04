# Structure of finished solution and directories

**HW1NotekeeperSolution**
├── *HW1NotekeeperSolution.sln*
├── **HW1Notekeeper**
│   └── *HW1Notekeeper.csproj*
└── **FuncationalTests**
    └── *FuncationalTests.csproj*



## 0. Install the nswag tool for generating the client REST sdk
   `dotnet tool install -g NSwag.ConsoleCore`              




## 1. Create a folder
## 2. Open folder using VS Code
## 3. Launch a terminal (View Terminal)
## 4. Run from terminal in that folder:
`dotnet new sln -n WeatherForecastDemoTesting`

## 5. Create the web API Project:
`dotnet new webapi --framework net9.0 --use-controllers --use-program-main -n WeatherForecastDemoTesting`

## 6. Create the functional Test project
`dotnet new xunit -n FunctionalTests  --framework net9.0`

## 7. Add the Web API Project to the solution file
`dotnet sln WeatherForecastDemoTesting.sln add WeatherForecastDemoTesting/WeatherForecastDemoTesting.csproj`
**7.1 Add the necessary swagger code so the swagger ui will work**

## 8. Add the functional test project to the solution file
`dotnet sln WeatherForecastDemoTesting.sln add FunctionalTests/FunctionalTests.csproj`

## 9. If the explorer doesn't show the solution then close and reopen the folder
## 9.a Wait a few moments for the DevKit to find the solution
## 9.b You will see SOLUTION EXPLORER in your explorer to the left once it does
## 9.c Open a terminal if one is not already open

## 10. Change into the functional test directory

## 11. Run the Web API app, choosing C# and HTTPS
## 12. Verify it is working
## 13. Get the URL to the swagger .json file, in this example the url is listed below but yours will be different
`https://localhost:7125/swagger/v1/swagger.json`


## 13. Run the following command to generate the client sdk
`nswag openapi2csclient /input:https://localhost:7125/swagger/v1/swagger.json /output:WeatherForecastRestClient.cs /classname:WeatherForecastRestClient /namespace:FunctionalTests`

## 14. Add your test
## 15. Rebuild your functional test project
## 16. Go to the Test icon (Looks like a chemistry beaker)
## 17. Run your test