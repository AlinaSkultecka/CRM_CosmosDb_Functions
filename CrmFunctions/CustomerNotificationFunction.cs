using CrmFunctions.Entities;
using MailKit.Net.Smtp;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace CrmFunctions;

public class CustomerNotificationFunction
{
    private readonly ILogger<CustomerNotificationFunction> _logger;
    private readonly IConfiguration _configuration;

    public CustomerNotificationFunction(
        ILogger<CustomerNotificationFunction> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [Function("CustomerNotificationFunction")]
    public async Task Run(
        [CosmosDBTrigger(
            databaseName: "CrmDb",
            containerName: "Customers",
            Connection = "CosmosDbConnection",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true)]
        IReadOnlyList<Customer> customers)
    {
        if (customers is null || customers.Count == 0)
        {
            return;
        }

        foreach (Customer customer in customers)
        {
            if (string.IsNullOrWhiteSpace(customer.ResponsibleSeller.Email))
            {
                _logger.LogWarning("Customer {CustomerId} has no responsible seller email.", customer.Id);
                continue;
            }

            await SendEmailToSellerAsync(customer);

            _logger.LogInformation(
                "Email notification sent to {SellerEmail} for customer {CustomerName}",
                customer.ResponsibleSeller.Email,
                customer.Name);
        }
    }

    private async Task SendEmailToSellerAsync(Customer customer)
    {
        string smtpHost = _configuration["MailSettings:SmtpHost"]!;
        int smtpPort = int.Parse(_configuration["MailSettings:SmtpPort"]!);
        string username = _configuration["MailSettings:Username"]!;
        string password = _configuration["MailSettings:Password"]!;
        string fromEmail = _configuration["MailSettings:FromEmail"]!;

        var message = new MimeMessage();

        message.From.Add(MailboxAddress.Parse(fromEmail));

        // The email is sent to the responsible seller
        message.To.Add(MailboxAddress.Parse(customer.ResponsibleSeller.Email));

        message.Subject = $"New or updated customer assigned: {customer.Name}";

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

        using var smtpClient = new SmtpClient();

        await smtpClient.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
        await smtpClient.AuthenticateAsync(username, password);
        await smtpClient.SendAsync(message);
        await smtpClient.DisconnectAsync(true);
    }
}