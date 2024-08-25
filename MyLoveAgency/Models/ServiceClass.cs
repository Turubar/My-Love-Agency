using MimeKit;
using MailKit.Net.Smtp;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;

namespace MyLoveAgency.Models
{
    public static class ServiceClass
    {
        public static bool GetPassword()
        {
            try
            {
                if (File.Exists(DataClass.pathToPasswordFile))
                {
                    string data = File.ReadAllText(DataClass.pathToPasswordFile);
                    if (data.Length <= 10 || data.Length > 32) throw new Exception("Invalid password length!");

                    DataClass.password = data;
                    return true;
                }
                else throw new Exception("The password file was not found!");
            }
            catch (Exception e)
            {
                throw new Exception("The password could not be set!\r\n" + e);
            }
        }

        public static int GetMailing()
        {
            try
            {
                if (File.Exists(DataClass.pathToMailingFile))
                {
                    List<string> data = File.ReadAllLines(DataClass.pathToMailingFile).ToList();
                    if (data.Count() <= 0) throw new Exception("The mailing data is not set!");

                    for (int i = 0; i < data.Count(); i++)
                    {
                        List<string> partData = data[i].Split("|").ToList();

                        if (partData.Count() != 2) throw new Exception("The mailing data is incorrect!");
                        if (partData[0].Length <= 0 || partData[1].Length <= 0) throw new Exception("The mailing data is incorrect!");

                        DataClass.Mailing.Add((email: partData[0], key: partData[1]));
                    }

                    return DataClass.Mailing.Count();
                }
                else throw new Exception("The mailing file was not found!");
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't get the mailing data!\r\n" + e);
            }
        }

        public static bool GetEmailForNotifications()
        {
            try
            {
                if (File.Exists(DataClass.pathToEmailNotification))
                {
                    List<string> emails = File.ReadAllLines(DataClass.pathToEmailNotification).ToList();
                    if (emails.Count() <= 0) throw new Exception("The email for notification file was empty!");

                    for (int i = 0; i < emails.Count(); i++)
                    {
                        if (IsValidEmail(emails[i])) DataClass.EmailForNotifications.Add(emails[i]);
                    }

                    if (DataClass.EmailForNotifications.Count() >= 1) return true;
                    else return false;
                }
                else throw new Exception("The email for notification file was not found!");
            }
            catch (Exception e)
            {
                throw new Exception("The email for notifcation could not be set!\r\n" + e);
            }
        }

        public static bool IsValidEmail(string email)
        {
            try
            {
                MailAddress m = new MailAddress(email);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static async Task<bool> SendEmailAsync(string emailFrom, string emailTo, string subject, string message, string key)
        {
            try
            {
                using var emailMessage = new MimeMessage();

                emailMessage.From.Add(new MailboxAddress("Lovely Love Agency", emailFrom));
                emailMessage.To.Add(new MailboxAddress("", emailTo));
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = message
                };

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 25, false);
                    await client.AuthenticateAsync(emailFrom, key);
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }

                return true;
            }
            catch (Exception e)
            {
                WriteLog("Не удалось отправить код подтверждения! Время: " + DateTime.Now + "\nИнформация: " + e + "\n\r\n\r");
                return false;
            }
        }

        public static void WriteLog(string text)
        {
            try
            {
                File.AppendAllText(DataClass.pathToLogFile, text);
            }
            catch
            {
                
            }
        }
    }
}
