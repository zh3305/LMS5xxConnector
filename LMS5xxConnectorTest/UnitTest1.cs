using System.Text;
using BinarySerialization;
using LMS5xxConnector;
using LMS5xxConnector.Telegram;
using LMS5xxConnector.Telegram.CommandContainers;
using LMS5xxConnector.Telegram.CommandContents;

namespace LMS5xxConnectorTest;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        
        var telegram = new TelegramContent
        {
            CommandTypes = CommandTypes.Sen,
            Payload = new SenCommandContainer()
            {
                Command = Commands.LMDscandata,
                CommandConnent = new ConfirmationRequestModeCommandBase
                {
                    StopStart = StopStart.Start
                }
            }
        };
        var packet = new CoLaA
        {
            Content = telegram
        };
        
        var serializer = new BinarySerializer();
        MemoryStream stream = new MemoryStream();
        serializer.Serialize(stream, packet);
        var array = stream.ToArray();
        TestContext.Out.WriteLine(Encoding.ASCII.GetString(array));
        TestContext.Out.WriteLine(BitConverter.ToString(array).Replace("-",""));
        
        
        // Assert.Pass();
    }
}