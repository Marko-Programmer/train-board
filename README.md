# TrainBoard 🚆

A **WPF desktop application** for viewing train schedules and detailed route information in Poland. It uses the **Polish Railway API (PLK PDP API)** to fetch real-time data and provides an easy-to-use interface for selecting stations, searching trains, and viewing train stop details.



## Features ✨

- Search for trains between any two stations in Poland.
- Auto-complete suggestions for stations using a local station dictionary.
- Display train schedules with departure and arrival times, platforms, and track numbers.
- View detailed train stops with arrival/departure times and waiting durations.
- Reverse “From” and “To” stations with one click.


## Screenshots 🖼️

*You can add screenshots here:*

- Main window with station search and train list.
- Train details window with stop-by-stop schedule.



## Technologies 🛠️

- **C#** with **.NET WPF**
- **HttpClient** for API requests
- **System.Text.Json** for JSON parsing
- **Microsoft.Extensions.Configuration** for API key management
- **MVVM-inspired structure** (Services + Models + Views)



## Getting Started 🚀

### Prerequisites

- Windows OS
- [.NET](https://dotnet.microsoft.com/)
- API Key for PLK PDP API  

### Setup

1. Clone the repository:

```bash
git clone https://github.com/YOUR_USERNAME/TrainBoard.git
cd TrainBoard
```


2. Add your API key in appsettings.json:

```json
{
  "ApiSettings": {
    "ApiKey": "YOUR_API_KEY_HERE"
  }
}
```


3. Build and run the project using Visual Studio or dotnet CLI:

```bash
dotnet build
dotnet run
```


## How It Works ⚙️

- **Station Dictionary**: On first run, the app downloads the list of all Polish stations from the API and caches it locally (`stations.json`) to minimize API calls.

- **Searching Trains**: User selects "From" and "To" stations. The app fetches available train routes for the current day.

- **Train Details**: Clicking "View Details" opens a new window showing all stops of the selected train with times and durations.

- **Reverse Stations**: Swap “From” and “To” stations while preserving search history and dropdown list.
