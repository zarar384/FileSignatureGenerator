using FileSignatureGenerator;

try
{
    if (args.Length < 1)
    {
        FileSignatureProcessor.DisplayUsage();
        return;
    }

    FileSignatureProcessor.GenerateFileSignature(args[0]);
}
catch (Exception ex)
{
    FileSignatureProcessor.DisplayError(ex);
}

