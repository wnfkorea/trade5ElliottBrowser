# ElliottBrowser for Quandl.Com

ElliottBrowser is a tool for Elliott wave analysis, mainly based on the methods described in the Glenn Neely's book, 'Mastering Elliott Wave'.
ElliottBrowser for Quandl.Com uses free data gathered from Quandl.Com, especially its WIKI database.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

### Prerequisites

ElliottBrowser is a .Net form application, and requires a database for storing stock chart data which will be gathered using Quandl.Com API over the web.

```
- .Net framework 4.5 or above
- Visual Studio Community 2015, or a compatible version of Visual Studio
- Microsoft SQL Server Compact 4.0, or SQLite, or MySQL
- Quandl.Com account and an API Key for testing and running the application
```

### Installing

There are no specific instructions to get a development environment running except a database system.

```
- In the source tree, included are dll files for Microsoft SQL Server Compact 4.0 and SQLite.
- Anyone who want to use other DBMS must have an active DBMS instance and implement an DB connection wrapper for his own.
```

## Deployment

Add additional notes about how to deploy this on a live system

## Built With

* Visual Studio Community 2015

## Authors

* **Kyungseog Kim**

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

