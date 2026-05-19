// Entity model used by the function (Customer and ResponsibleSeller)
using CrmFunctions.Entities;
// MailKit SMTP client used to send emails
using MailKit.Net.Smtp;
// Azure Functions attributes and types
using Microsoft.Azure.Functions.Worker;
// Used to read values from appsettings.json or local.settings.json.
using Microsoft.Extensions.Configuration;
// Used for logging information, warnings, and errors.
using Microsoft.Extensions.Logging;
// MimeKit is used to create the email message.
using MimeKit;

namespace CrmFunctions;

public class CustomerNotificationFunction
{
    // Logger is used to write messages to the console/output window.
    private readonly ILogger<CustomerNotificationFunction> _logger;

    // Configuration is used to read settings like SMTP host, username, password, etc.
    private readonly IConfiguration _configuration;

    // Constructor
    // Azure Functions automatically injects logger and configuration here.
    public CustomerNotificationFunction(
        ILogger<CustomerNotificationFunction> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    // This attribute gives the function its name in Azure Functions.
    [Function("CustomerNotificationFunction")]
    public async Task Run(
        // Cosmos DB trigger.
        // This function will run when a customer is added or updated in the Customers container inside the CrmDb 
        [CosmosDBTrigger(
            databaseName: "CrmDb",
            containerName: "Customers",
            Connection = "CosmosDbConnection",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true)]
        IReadOnlyList<Customer> customers)
    {
        // If no customers were received from the trigger, stop the function.
        if (customers is null || customers.Count == 0)
        {
            return;
        }

        // The trigger can send one or more changed customers at the same time.
        foreach (Customer customer in customers)
        {
            // Check that the customer has a responsible seller email.
            // Without an email address, we cannot send a notification.
            if (string.IsNullOrWhiteSpace(customer.ResponsibleSeller.Email))
            {
                _logger.LogWarning("Customer {CustomerId} has no responsible seller email.", customer.Id);
                continue;
            }

            // Send email to the responsible seller.
            await SendEmailToSellerAsync(customer);

            // Log that the email was sent successfully.
            _logger.LogInformation(
                "Email notification sent to {SellerEmail} for customer {CustomerName}",
                customer.ResponsibleSeller.Email,
                customer.Name);
        }
    }

    // This private helper method creates and sends the email.
    private async Task SendEmailToSellerAsync(Customer customer)
    {
        // Read SMTP settings from configuration.
        string smtpHost = _configuration["MailSettings:SmtpHost"]!;
        int smtpPort = int.Parse(_configuration["MailSettings:SmtpPort"]!);
        string username = _configuration["MailSettings:Username"]!;
        string password = _configuration["MailSettings:Password"]!;
        string fromEmail = _configuration["MailSettings:FromEmail"]!;

        // Create a new email message.
        var message = new MimeMessage();

        // Set sender email address.
        message.From.Add(MailboxAddress.Parse(fromEmail));

        // Set receiver email address.
        message.To.Add(MailboxAddress.Parse(customer.ResponsibleSeller.Email));

        // Set email subject.
        message.Subject = $"New or updated customer assigned: {customer.Name}";

        // Set email body as plain text.
        // This includes the customer information and seller information.
        message.Body = new TextPart("plain")
        {
            Text =
                $"""
                Hello {customer.ResponsibleSeller.Name},

                You are responsible for this customer.

                Customer information:
                Name: {customer.Name}
                Title: {customer.Title}
                Phone: {customer.Phone}
                Email: {customer.Email}
                Address: {customer.Address}

                Responsible seller:
                Name: {customer.ResponsibleSeller.Name}
                Phone: {customer.ResponsibleSeller.Phone}
                Email: {customer.ResponsibleSeller.Email}

                This message was sent automatically from the CRM system.
                """
        };

        // Create SMTP client.
        using var smtpClient = new SmtpClient();

        // Connect to the SMTP server.
        // StartTls means the connection will use encryption when available.
        await smtpClient.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);

        // Login to the SMTP server using username and password.
        await smtpClient.AuthenticateAsync(username, password);

        // Send the email message.
        await smtpClient.SendAsync(message);

        // Disconnect from the SMTP server.
        await smtpClient.DisconnectAsync(true);
    }
}