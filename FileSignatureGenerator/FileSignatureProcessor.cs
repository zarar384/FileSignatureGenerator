using System.Security.Cryptography;

namespace FileSignatureGenerator;

public class FileSignatureProcessor
{
    public static void DisplayUsage()
    {
        Console.WriteLine("Usage: FileSignatureGenearator <file_path>");
    }

    public static void DisplayError(Exception ex)
    {
        Console.WriteLine("Error: {0}", ex.Message);
        Console.WriteLine("StackTrace: {0}", ex.StackTrace);
    }

    public static void GenerateFileSignature(string filePath)
    {
        const int defBlockSize = 4096;

        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            var fileSize = fileStream.Length;
            var blockSize = CalculateBlockSize(fileSize, defBlockSize); //determing block size
            var blocks = fileSize / blockSize;
            var remainder = (int)(fileSize % blockSize); //remainder for processing after 

            //for simultaneous processing of blocks
            Thread[] threads = new Thread[blocks];

            for (long i = 0; i < blocks; i++)
            {
                byte[] buffer = ReadBlock(fileStream, blockSize);
                long blockNumber = i;//copy for each iteration

                //create a thread to process the block and add it to the arr of threads
                threads[i] = new Thread(() =>
                {
                    if (buffer != null)
                    {
                        byte[] blockBuffer = buffer; //copy of buffer
                        ProcessBlock(blockBuffer, blockNumber + 1);
                    }
                    else
                    {
                        Console.WriteLine($"Block {blockNumber} is null");
                    }
                });
            }

            //run threads in parallel
            foreach (var thread in threads)
            {
                thread.Start();
            }

            //wait for all threads to complete
            foreach (var thread in threads)
            {
                thread.Join();
            }

            if (remainder > 0)
            {
                byte[] buffer = ReadBlock(fileStream, remainder);
                ProcessBlock(buffer, blocks + 1);
            }
        }
    }

    //determing block size 
    //return effective block size for processing a file
    public static int CalculateBlockSize(long fileSize, int defaultBlockSize)
    {
        int availableMemory = (int)(Environment.WorkingSet / 2);
        int blockSize = (int)Math.Min(defaultBlockSize, fileSize / 10); // 1/10 file size otherwise default
        return blockSize == 0 ? defaultBlockSize : Math.Min(blockSize, availableMemory);
    }

    //read block from a file
    static byte[] ReadBlock(FileStream fileStream, int blockSize)
    {
        byte[] buffer = new byte[blockSize];
        fileStream.Read(buffer, 0, blockSize);
        return buffer;
    }

    //write a hash to the console for a specific block
    static void ProcessBlock(byte[] buffer, long blockNumber)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(buffer);
            string hash = BitConverter.ToString(hashBytes).Replace("-", "");
            Console.WriteLine($"Block {blockNumber}: {hash}");
        }
    }
}
