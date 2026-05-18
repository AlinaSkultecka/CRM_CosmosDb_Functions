# CRM Minimal API with Cosmos DB and Azure Functions

This project is a simple CRM system built with **ASP.NET Core Minimal API**, **Azure Cosmos DB Emulator**, and **Azure Functions**.

The system stores customer information together with the responsible seller for each customer.  
When a customer is added or updated, an Azure Function is triggered and sends an email notification to the responsible seller using **Mailtrap**.

## How It Works

The project has two main parts:

```text
CrmApi        -> Minimal API for managing customers
CrmFunctions  -> Azure Function for email notifications
```


#### The Project Flow
```text
User adds or updates a customer through the API
        ↓
Customer is saved in Cosmos DB
        ↓
Cosmos DB Trigger detects the change
        ↓
Azure Function runs
        ↓
Email is sent to the responsible seller
        ↓
Email appears in Mailtrap inbox
```
Mailtrap is used for testing emails, so the emails are shown in the Mailtrap sandbox inbox instead of being sent to a real inbox.


#### Cosmos DB ScreenShot
<img width="1907" height="787" alt="image" src="https://github.com/user-attachments/assets/db9adbfc-8660-4082-8d1a-eae6e240faad" />


#### Main API Endpoints
```text
GET /customers
GET /customers/{id}
POST /customers
PUT /customers/{id}
DELETE /customers/{id}
GET /customers/search/name/{name}
GET /customers/search/seller/{sellerName}
```

## How to Run Locally
1. Start **Cosmos DB Emulator**
2. Start **Azurite**
   Run this command in the terminal:
   ```text
   azurite
   ```
3. Start both projects:
   ```text
   CrmApi + CrmFunctions runs simantenuasly.
   ```
4. To **test** the **Azure Function**, use this endpoint
   ```text
   POST /customers
   ```
   Example **JSON**:
   ```text
   {
      "name": "Lisa Bergström",
      "title": "Operations Manager",
      "phone": "0731234567",
      "email": "lisa.bergstrom@example.com",
      "address": "Kungsgatan 15, Göteborg",
      "responsibleSeller": {
        "name": "Alina Skultecka",
        "phone": "0709999999",
        "email": "alina.skultecka@gmail.com"
      }
   }
    ```
5. After a customer is added or updated, the Azure Function sends an email notification through **Mailtrap**.

