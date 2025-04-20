# Party Search API

A .NET microservice for searching sanctioned parties. This project is used as part of the 
Serenity Dojo BDD training workshops.

## Overview

The Party Search API provides a secure, RESTful interface for searching parties by name or ID, 
with filtering capabilities for party type and sanctions status. 
The API provides endpoints for comprehensive filtering, search, and pagination.

## Features

- **Full-text search**: Search parties by name or ID, supporting both exact and partial matches
- **Filtering**: Filter results by party type (Individual or Organization) or sanctions status
- **Pagination**: Navigate through large result sets with built-in pagination support
- **Sorting**: Results are sorted alphabetically by name
- **Swagger Documentation**: Interactive API documentation with Swagger UI
- **Comprehensive Testing**: Complete test coverage with unit and integration tests

## Project Structure

- **PartySearchApi.Api**: Core API implementation with controllers, models, and services
- **PartySearchApi.UnitTests**: Unit tests for individual components
- **PartySearchApi.AcceptanceTests**: Integration tests using ReqnRoll

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Git

### Building the Application

1. Clone the repository
   ```
   git clone https://github.com/serenitydojo/party-search-api.git
   cd party-search-api
   ```

2. Build the solution
   ```
   dotnet build
   ```

### Running the Application

#### Standard Startup

```
cd PartySearchApi.Api
dotnet run
```

#### With Sample Data

Create a sample JSON file or use the provided `sample-parties.json`, then run:

```
cd PartySearchApi.Api
dotnet run --seed-data "./sample-parties.json"
```

The application will display URLs it's running on in the console. Open your browser and navigate to:

```
https://localhost:{port}/swagger
```

To view and interact with the API through Swagger UI.

## API Endpoints

### Search Parties

```
GET /api/parties
```

#### Query Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| searchTerm | string | Full or partial name or ID to search for |
| type | string | Filter by party type: "Individual" or "Organization" |
| sanctionsStatus | string | Filter by sanctions status: "Approved", "PendingReview", "Escalated", "ConfirmedMatch", or "FalsePositive" |
| page | string | Page number (starting from 1) |
| pageSize | string | Number of results per page |

#### Example Requests

Search for parties with "Smith" in the name:
```
GET /api/parties?searchTerm=Smith
```

Search for individual parties with pending review:
```
GET /api/parties?searchTerm=&type=Individual&sanctionsStatus=PendingReview
```

Get the second page of results with 5 items per page:
```
GET /api/parties?searchTerm=&page=2&pageSize=5
```

## Running Tests

### Unit Tests

```
cd PartySearchApi.UnitTests
dotnet test
```

### Acceptance Tests

```
cd PartySearchApi.AcceptanceTests
dotnet test
```

## Development Notes

### Adding New Sample Data

You can create custom JSON files for testing different scenarios. The sample data must follow this format:

```json
[
  {
    "partyId": "P12345678",
    "name": "Acme Corporation",
    "type": "Organization",
    "sanctionsStatus": "Approved",
    "matchScore": "95%"
  },
  ...
]
```

You can then run the application with the `--seed-data` option to load this data.

### Architecture

The API follows a layered architecture:
- **Controller Layer**: Handles HTTP requests/responses
- **Service Layer**: Implements business logic
- **Repository Layer**: Manages data access
