using System.Security.Cryptography;

namespace FileSignatureGenerator.Tests;

[TestFixture]
public class ProgramTests
{
    private const string _testFilePath = "testfile.txt";

    [SetUp]
    public void Setup()
    {
        File.WriteAllText(_testFilePath, "test text");
    }

    [TearDown]
    public void Cleanup()
    {
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }

    [Test]
    public void GenerateFileSignature_ValidFile_ReturnExpextedFirstSignature()
    {
        string expectedFirstSignature = GenerateExpectedFirstSignature(_testFilePath);
        string generatedFirstSignature = CaptureConsoleOutput(() => FileSignatureProcessor.GenerateFileSignature(_testFilePath));

        Assert.AreEqual(expectedFirstSignature, generatedFirstSignature);
    }

    [Test]
    public void CalculateBlockSize_ValidFileSize_ReturnExpectedBlockSize()
    {
        long fileSize = 100000; //example file size
        int expectedBlockSize = 4096;

        int calculatedBlockSize = FileSignatureProcessor.CalculateBlockSize(fileSize, expectedBlockSize);

        Assert.AreEqual(expectedBlockSize, calculatedBlockSize);
    }

    private string GenerateExpectedFirstSignature(string filePath)
    {
        using (var fileStream = File.OpenRead(filePath))
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(fileStream);
                var hash = BitConverter.ToString(hashBytes).Replace("-", "");
                return string.Format($"Block 1: {hash}");
            }
        }
    }

    private string CaptureConsoleOutput(Action action)
    {
        using (StringWriter sw = new StringWriter())
        {
            Console.SetOut(sw);
            action.Invoke();
            return sw.ToString().Split(Environment.NewLine).FirstOrDefault().Trim();
        }
    }
}