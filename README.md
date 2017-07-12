# ContosoUniversity2

## Description 

Contoso university Code First EF MVC site following and extending the tutorial from [Tom Dykstra](https://docs.microsoft.com/en-us/aspnet/mvc/overview/getting-started/getting-started-with-ef-using-mvc/creating-an-entity-framework-data-model-for-an-asp-net-mvc-application)

Main extension currently being the Seminar Table and associated Views and Controller with extensive validation built into the Controller. 

Stored procedures have also been employed to perform SQL queries in Seminar Search

## Installation

Clone the repo, then change the connection string to reflect your SQL Server instance in web.config as in:

``` <add name="SchoolContext" connectionString="Data Source=DESKTOP-PJG1KCI\SQLEXPRESS;Initial Catalog=ContosoUniversity;Integrated Security=SSPI;" providerName="System.Data.SqlClient" /> ```

Build the solution and call update-database from the Package Manager Console - the site should then run

