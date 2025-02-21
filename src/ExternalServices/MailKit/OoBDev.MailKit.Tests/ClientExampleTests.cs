using OoBDev.TestUtilities;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MimeKit;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.MailKit.Tests;

[TestClass]
public class ClientExampleTests
{
    public required TestContext TestContext { get; set; }

    [DataTestMethod]
    [TestCategory(TestCategories.DevLocal)]
    [DataRow("example-smtp4dev.d6cke7dpbbesb5ga.eastus.azurecontainer.io")]
    [DataRow("localhost")]
    public async Task SendSmtpTest(string host, int port = 25)
    {
        var config = new
        {
            Value = new
            {
                Uri = (Uri?)null,
                Host = host,
                Port = port,
                SecureSocketOption = SecureSocketOptions.None,

                UserName = (string?)null,
                Password = (string?)null,
            }
        };

        using var client = new SmtpClient();

        if (config.Value.Uri != null)
        {
            await client.ConnectAsync(config.Value.Uri);
        }
        else
        {
            await client.ConnectAsync(config.Value.Host, config.Value.Port, config.Value.SecureSocketOption);
        }

        if (!string.IsNullOrWhiteSpace(config.Value.UserName) && !string.IsNullOrWhiteSpace(config.Value.Password))
        {
            await client.AuthenticateAsync(config.Value.UserName, config.Value.Password);
        }

        var result = await client.SendAsync(new MimeMessage
        {
            From = {
                InternetAddress.Parse("fake@email.com"),
            },
            To = {
                InternetAddress.Parse("to-fake@email.com"),
            },

            Body = new BodyBuilder()
            {
                TextBody = "Hello World! " + Guid.NewGuid(),
            }.ToMessageBody(),
        });

        TestContext.WriteLine($"Sent: {result}");
    }

    [DataTestMethod]
    [TestCategory(TestCategories.DevLocal)]
    [DataRow("example-smtp4dev.d6cke7dpbbesb5ga.eastus.azurecontainer.io")]
    [DataRow("localhost")]
    public async Task GetImapTest(string host, int port = 143)
    {
        var config = new
        {
            Value = new
            {
                Uri = (Uri?)null,
                Host = host,
                Port = port,
                SecureSocketOption = SecureSocketOptions.None,

                UserName = "Hi",
                Password = "There",
            }
        };

        using var client = new ImapClient();

        if (config.Value.Uri != null)
        {
            await client.ConnectAsync(config.Value.Uri);
        }
        else
        {
            await client.ConnectAsync(config.Value.Host, config.Value.Port, config.Value.SecureSocketOption);
        }

        if (!string.IsNullOrWhiteSpace(config.Value.UserName) && !string.IsNullOrWhiteSpace(config.Value.Password))
        {
            await client.AuthenticateAsync(config.Value.UserName, config.Value.Password);
        }

        var namespaces = client.PersonalNamespaces.Cast<FolderNamespace>().Select(ns => new
        {
            Group = "Personal",
            Namespace = ns,
        }).Concat(client.OtherNamespaces.Cast<FolderNamespace>().Select(ns => new
        {
            Group = "Other",
            Namespace = ns,
        })).Concat(client.SharedNamespaces.Cast<FolderNamespace>().Select(ns => new
        {
            Group = "Shared",
            Namespace = ns,
        }));

        foreach (var @namespace in namespaces)
        {
            TestContext.WriteLine($"Namespace: {@namespace.Group}: \"{@namespace.Namespace.Path}\" [{@namespace.Namespace.DirectorySeparator}]");

            foreach (var folder in await client.GetFoldersAsync(@namespace.Namespace))
            {
                TestContext.WriteLine($"Folder: {folder.Name} ({folder.Id})");
                await folder.OpenAsync(FolderAccess.ReadOnly);

                foreach (var item in folder)
                {
                    TestContext.WriteLine($@"Message> 
From:    {string.Join(';', item.From.Cast<InternetAddress>())} 
To:      {string.Join(';', item.To.Cast<InternetAddress>())}
Subject: {item.Subject}
Text:    {item.TextBody}
HTML:    {item.HtmlBody}
");
                }
            }
        }
    }
}
