using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace ServerChat
{
    public class Logger
    {
        private readonly ILogger<Logger> _logger;
        private StreamWriter _streamWriter;

        public Logger(ILogger<Logger> logger)
        {
            _logger = logger;
        }
        public void Log(string message)
        {
            _streamWriter =
                new StreamWriter($@"logs\Log{DateTime.Today.Day}_{DateTime.Today.Month}_{DateTime.Today.Year}.log",
                    true);
            _logger.LogInformation($"{DateTime.Now.TimeOfDay} >> {message}");
            _streamWriter.WriteLine($"{DateTime.Now.TimeOfDay} >> {message}");
            _streamWriter.Close();
        }
    }
}